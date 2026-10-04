using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePlayer : MazeCharacter
{
    [Header("プレイヤー情報")]
    public GameData gameData;
    public PlayerPrefabs playerPrefabs;
    public byte playerNumber;
    public byte lives = 2;
    public Sprite icon;

    [HideInInspector] public int readyMusicIndex = 0;

    private int playerTypeIndex;

    [Header("エフェクト")]
    // public GameObject invincibleEffect;

    float invincibleTime = 0f;
    public bool isInvincible = false;

    private int revRollStep = 0;
    private float revRollTime = 0f;

    private float speedUpTime = 0f;

    [Header("スコアポップアップなど")]
    public GameObject ScorePopUp;
    public GameObject OneUpObj; //1UPアピール用オブジェクト
    private GameObject OneUpObjControl;
    public Options options;
    [Header("ソーラーブリット（チームロックマンのみ）")]
    public GameObject solarBrit;
    private List<GameObject> britList = new List<GameObject>();
    private float britReloadTime = 0f;
    [Header("ティウンティウン（チームロックマンのみ）")]
    public GameObject deathEffect;
    [Header("スキン切り替え（チームソニックのみ）")]
    public GameObject normalSkin;
    public GameObject spinSkin;
    [Header("スピンダッシュエフェクト")]
    public ParticleSystem spindashEffect;
    [Header("効果音")]
    public AudioSource source;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip shootSound;
    public AudioClip powerPelletSound;
    public AudioClip powerPelletEndSound;
    [Header("効果音（外部連携）")]
    public AudioClip revRollPrepareSound;
    public AudioClip revRollReleaseSound;
    public AudioClip deathSound;
    public AudioClip oneUpSound;

    private PlayerVoiceManager voice;

    [Header("ゲームオーバー判定")]
    public bool gameOver = false;

    private float animSpeedRate = 1f;
    private bool control = true;

    void OnEnable()
    {
        Destroy(skin.GetChild(0).gameObject);

        GameObject playerPref = playerPrefabs.playerData[gameData.characterIds[playerNumber]].prefab;
        
        if (playerPref.GetComponent<MarioActions>() != null)
        {
            playerTypeIndex = 0;
        } else if (playerPref.GetComponent<PacManActions>() != null)
        {
            playerTypeIndex = 1;
        } else if (playerPref.GetComponent<MegaManActions>() != null)
        {
            playerTypeIndex = 2;
        } else if (playerPref.GetComponent<SonicActions>() != null)
        {
            playerTypeIndex = 3;
        }
        icon = playerPrefabs.playerData[gameData.characterIds[playerNumber]].livesIcon;

        //スキン
        Transform s = Instantiate(playerPref.GetComponent<PlayerInfo>().skin.gameObject, skin.position - Vector3.up * 0.9f + playerPref.GetComponent<PlayerInfo>().skin.localPosition, Quaternion.identity, skin).transform;
        PlayerFootsteps f = GetFootsteps(s);
        // f.parentTrans = this.transform;
        if (playerPref.GetComponent<SonicActions>() != null)
        {
            StartCoroutine("SetSkin");
        }
        
        //効果音
        deathSound = playerPref.GetComponent<PlayerInfo>().deathSound;
        oneUpSound = playerPref.GetComponent<PlayerInfo>().oneUpSound;
        if (playerPref.GetComponent<PacManActions>() != null)
        {
            revRollPrepareSound = playerPref.GetComponent<PacManActions>().dashReadySound;
            revRollReleaseSound = playerPref.GetComponent<PacManActions>().dashReleaseSound;
        }
        if (playerPref.GetComponent<SonicActions>() != null)
        {
            revRollPrepareSound = playerPref.GetComponent<SonicActions>().spinDashSound;
            revRollReleaseSound = playerPref.GetComponent<SonicActions>().spinDashReleaseSound;
        }

        readyMusicIndex = 0;
        if (playerPref.GetComponent<_06MsPacMan>() != null)
        {
            readyMusicIndex = 1;
        }
        if (playerPref.GetComponent<_07PacJr>() != null)
        {
            readyMusicIndex = 2;
        }

        britList = new List<GameObject>();

        //ボイス
        PlayerVoiceManager prefV = playerPref.GetComponent<PlayerVoiceManager>();
        voice = this.GetComponent<PlayerVoiceManager>();
        voice.JumpVoices = new AudioClip[prefV.JumpVoices.Length];
        Array.Copy(prefV.JumpVoices, voice.JumpVoices, prefV.JumpVoices.Length);
        voice.AttackVoices = new AudioClip[prefV.AttackVoices.Length];
        Array.Copy(prefV.AttackVoices, voice.AttackVoices, prefV.AttackVoices.Length);
        voice.DeathVoices = new AudioClip[prefV.DeathVoices.Length];
        Array.Copy(prefV.DeathVoices, voice.DeathVoices, prefV.DeathVoices.Length);
        voice.ClearVoices = new AudioClip[prefV.ClearVoices.Length];
        Array.Copy(prefV.ClearVoices, voice.ClearVoices, prefV.ClearVoices.Length);
    }
    IEnumerator SetSkin()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject playerPref = playerPrefabs.playerData[gameData.characterIds[playerNumber]].prefab;
        string normalSkinName = playerPref.GetComponent<SonicActions>().normalSkin.name;
        string spinSkinName = playerPref.GetComponent<SonicActions>().spinSkin.name;

        Debug.Log("N : " + normalSkinName);
        Debug.Log("S : " + spinSkinName);
        foreach (Transform sk in skin.GetChild(0))
        {
            if (sk.gameObject.name.Equals(normalSkinName))
            {
                normalSkin = sk.gameObject;
            }
            else if (sk.gameObject.name.Equals(spinSkinName))
            {
                spinSkin = sk.gameObject;
            }
            Debug.Log(sk.gameObject.name);
        }
    }

    void Update()
    {
        if (playerAnim == null)
        {
            playerAnim = GetAnimator(skin);
        }
        switch (playerTypeIndex)
        {
            case 3:
            //チームソニック
            switch (revRollStep)
            {
                case 2:
                speed = 55f + Math.Max(0f, revRollTime - 1f) * 25f;
                break;

                case 1:
                speed = 0f;
                break;

                default:
                speed = 30f;
                break;
            }
            if (playerAnim != null) playerAnim.speed = 1 + (new Vector3(velocity.x, 0, velocity.z).magnitude / 30f);
            break;

            default:
            switch (revRollStep)
            {
                case 2:
                speed = 55f;
                break;

                case 1:
                speed = 0f;
                break;

                default:
                speed = 22.5f;
                break;
            }
            if (playerAnim != null) playerAnim.speed = 1f;
            break;
        }
        if (!control && animSpeedRate > 0)
        {
            speed = 0f;
        }
        if (speedUpTime > 0f)
        {
            speedUpTime -= Time.deltaTime;
            if (speedUpTime <= 0f)
            {
                SoundPlay(powerPelletEndSound);
            }
            speed *= 4f / 3f;
        }
        if (playerAnim != null) playerAnim.speed *= animSpeedRate;

        if (isInvincible) {speed *= 1.2f;}

        if (invincibleTime > 0)
        {
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0)
            {
                SoundPlay(powerPelletEndSound);
                isInvincible = false;
            }
        }
        // invincibleEffect.SetActive(isInvincible);

        if (control && SpecialStageManager.ready && !stop)
        {
            if (Input.GetAxisRaw("Horizontal") >= 0.5f && (canRotate(Vector2.right) || direction == Vector2.left) && direction != Vector2.right)
            {
                if (direction != Vector2.left) gridPos();
                direction = Vector2.right;
            }
            else if (Input.GetAxisRaw("Horizontal") <= -0.5f && (canRotate(Vector2.left) || direction == Vector2.right) && direction != Vector2.left)
            {
                if (direction != Vector2.right) gridPos();
                direction = Vector2.left;
            }
            else if (Input.GetAxisRaw("Vertical") >= 0.5f && (canRotate(Vector2.up) || direction == Vector2.down) && direction != Vector2.up)
            {
                if (direction != Vector2.down) gridPos();
                direction = Vector2.up;
            }
            else if (Input.GetAxisRaw("Vertical") <= -0.5f && (canRotate(Vector2.down) || direction == Vector2.up) && direction != Vector2.down)
            {
                if (direction != Vector2.up) gridPos();
                direction = Vector2.down;
            }

            //スペシャル技
            GameObject sb;
            MegaBuster bust;
            foreach (GameObject b in britList)
            {
                if (b == null)
                {
                    britList.Remove(b);
                    break;
                }
            }
            if (britReloadTime > 0f)
            {
                britReloadTime -= Time.deltaTime;
                if (britList.Count <= 0f)
                {
                    britReloadTime = 0f;
                }
            }
            if (Input.GetButtonDown("A") && Grounded && !isInvincible)
            {
                switch (playerTypeIndex)
                {
                    case 0:
                    //チームマリオ：ジャンプ
                    voice.Jump();
                    SoundPlay(jumpSound);
                    Grounded = false;
                    jumpVel = 40f;
                    //ファンキー＆スパンキーもジャンプする
                    foreach (MazeGhost g in FindObjectsByType<MazeGhost>(FindObjectsSortMode.None))
                    {
                        g.GhostJump();
                    }
                    break;

                    case 1:
                    case 3:
                    if (revRollStep == 0)
                    {
                        //チームパックマン：パックダッシュ
                        //チームソニック：スピンダッシュ
                        actionId = 3;
                        SoundPlay(revRollPrepareSound);
                        revRollStep = 1;
                    }
                    break;

                    case 2:
                    //チームロックマン：バスターでゴーストを怯ませる
                    if (britReloadTime <= 0f)
                    {
                        SoundPlay(shootSound);
                        sb = Instantiate(solarBrit, transform.position, skin.rotation);
                        bust = sb.GetComponent<MegaBuster>();
                        bust.level = 0;
                        bust.power = 1f;
                        britList.Add(sb);
                        britReloadTime = 0.25f;
                    }
                    break;
                }
            }

            if (revRollStep == 2)
            {
                revRollTime -= Time.deltaTime;
                if (revRollTime <= 0f)
                {
                    revRollStep = 0;
                }
            } else if (revRollStep == 1)
            {
                revRollTime += 3 * Time.deltaTime;
                if (revRollTime > 2f)
                {
                    revRollTime = 2f;
                }
                if (Input.GetButtonUp("A"))
                {
                    actionId = 0;
                    voice.Attack();
                    revRollStep = 2;
                    source.Stop();
                    SoundPlay(revRollReleaseSound);
                }
            }
            rolling = revRollStep == 2;
        }
        if (normalSkin != null && spinSkin != null)
        {
            bool spinSkinActive = false;
            
            if (spinSkin.GetComponent<SpindashBall>() != null)
            {
                spinSkinActive = actionId == 3;
                spinSkin.GetComponent<SpindashBall>().isSpindash = true;
            } else
            {
                spinSkinActive = actionId == 3 || (rolling && actionId != -2);
                Transform spinball = spinSkin.transform.GetChild(0);
                if (revRollStep == 1)
                {
                    spinball.localRotation = Quaternion.Euler(45, 0, 0);
                    spinball.localScale = new Vector3(1f, 1.2f, 0.9f);
                } else
                {
                    spinball.localRotation = Quaternion.Euler(0, 0, 0);
                    spinball.localScale = Vector3.one;
                }
            }

            normalSkin.SetActive(!spinSkinActive);
            spinSkin.SetActive(spinSkinActive);
        }

        //スピンダッシュエフェクト
        var sdEmission = spindashEffect.emission;
        var sdShape = spindashEffect.shape;

        sdEmission.rateOverTime = 0f;
        Vector3 sdRotation = new Vector3(-15f, 180 + skin.eulerAngles.y, 0f);
        sdShape.rotation = sdRotation;

        if (actionId == 3)
        {
            sdEmission.rateOverTime = 125f;
            spindashEffect.transform.position = this.transform.position - new Vector3(0, 1.2f, 0f);
        }

        //1UPアピール用オブジェクト
        if (OneUpObjControl != null)
        {
            OneUpObjControl.transform.position = transform.position + (Vector3.up * 2);
        }

        if (SpecialStageManager.cleared)
        {
            control = false;
            if (invincibleTime > 0.01f)
            {
                invincibleTime = 0.01f;
            }
        }
    }

    public void PowerUp(bool isShort = false) {
        SoundPlay(powerPelletSound);
        isInvincible = true;
        invincibleTime = isShort ? 4f : 7.5f;
        revRollStep = 0;
    }
    public void SpeedUp()
    {
        SoundPlay(powerPelletSound);
        speedUpTime = 10f;
    }

    Animator GetAnimator(Transform skin)
    {
        Animator a = null;
        foreach (Transform child in skin)
        {
            if (child.gameObject.GetComponent<Animator>() != null)
            {
                return child.gameObject.GetComponent<Animator>();
            } else
            {
                Animator got = GetAnimator(child);
                if (got != null)
                {
                    a = got;
                }
            }
        }
        return a;
    }
    PlayerFootsteps GetFootsteps(Transform skin)
    {
        PlayerFootsteps f = null;
        foreach (Transform child in skin)
        {
            if (child.gameObject.GetComponent<PlayerFootsteps>() != null)
            {
                return child.gameObject.GetComponent<PlayerFootsteps>();
            } else
            {
                PlayerFootsteps got = GetFootsteps(child);
                if (got != null)
                {
                    f = got;
                }
            }
        }
        return f;
    }

    public void scorePopUp(uint num, Vector3 position, int pCombo = 0)
    {
        if (pCombo > 6)
        {
            pCombo = 6;
        }
        if (GameManager.ScoreShow)
        {
            SpecialStageManager.Score += num;

            if (
                options.scorePopUp == Options.ScorePopUp.Normal || 
                options.scorePopUp == Options.ScorePopUp.Full || 
                options.scorePopUp == Options.ScorePopUp.NormalExceptSonic || 
                options.scorePopUp == Options.ScorePopUp.FullExceptSonic
                )
            {
                GameObject popup = Instantiate(ScorePopUp, position, Quaternion.identity);
                popup.GetComponent<ScoringController>().points = (int)num;
                popup.GetComponent<ScoringController>().combo = pCombo;
                popup.GetComponent<ScoringController>().noScore = gameData.courseMode == CourseMode.NoScoreAndRankS;

                popup.GetComponent<ScoringController>().scale = 2.5f;
            }
        }
    }

    public void OneUp()
    {
        SoundPlay(oneUpSound);
        if (OneUpObjControl != null) Destroy(OneUpObjControl);
        OneUpObjControl = Instantiate(OneUpObj, transform.position + (Vector3.up * 2), Quaternion.identity);
        OneUpObjControl.GetComponent<ScoringController>().scale = 2.5f;
        lives++;
    }
    public void SoundPlay(AudioClip sound){
        //効果音の再生
        if (sound != null) source.PlayOneShot(sound);
    }

    public IEnumerator LostLife()
    {
        SpecialStageManager.death = true;
        control = false;
        animSpeedRate = 0f;

        foreach (MazeCharacter c in FindObjectsByType<MazeCharacter>(FindObjectsSortMode.None))
        {
            c.stop = true;
        }

        yield return new WaitForSeconds(1f);

        foreach (MazeCharacter c in FindObjectsByType<MazeCharacter>(FindObjectsSortMode.None))
        {
            if (c != this.GetComponent<MazeCharacter>())
            {
                c.hide = true;
            }
        }

        jumpVel = 0f;
        animSpeedRate = 1f;
        SoundPlay(deathSound);
        actionId = -2;
        speedUpTime = 0f;
        revRollStep = 0;
        revRollTime = 0f;
        stop = false;

        StartCoroutine("DeathVoices");
        if (playerTypeIndex == 2)
        {
            //ティウンティウン
            StartCoroutine("ThiunThiun");
        }

        yield return new WaitForSeconds(2.5f);

        if (lives == 0)
        {
            gameOver = true;
        } else
        {
            lives--;
            control = true;
        }

        bool allGameOver = true;
        foreach (MazePlayer p in FindObjectsByType<MazePlayer>(FindObjectsSortMode.None))
        {
            allGameOver = allGameOver && p.gameOver;
        }

        if (allGameOver)
        {
            //メイズ失敗
            StartCoroutine(manager.MazeFailed());
        } else
        {
            //リセット
            StartCoroutine(manager.Reset());
        }
    }

    public IEnumerator DeathVoices()
    {
        if (playerTypeIndex == 0)
        {
            yield return new WaitForSeconds(0.8f);
            voice.Death();
        } else if (playerTypeIndex == 1 || playerTypeIndex == 3)
        {
            yield return null;
            voice.Death();
        }

        yield return null;
    }
    public void Clear()
    {
        voice.Clear();
    }
    public IEnumerator ThiunThiun()
    {
        yield return new WaitForSecondsRealtime(0.7f);

        source.Stop();
        voice.Death();
        hide = true;
        GameObject d_effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        d_effect.transform.rotation = Quaternion.Euler(90, 0, 0);
        d_effect.GetComponent<AudioSource>().spatialBlend = 0f;
    }
}
