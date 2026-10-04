using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    public Options options;
    [Header("HP")]
    public int HP = 3;
    protected int latestHP = 3;
    public int HitsPerHP = 5;
    //private int hits;
    [HideInInspector] public int singleDamage;
    public float totalHp;
    [HideInInspector] public int maxHp;
    // protected AudioSource sound;
    protected PlayerInfo player;
    protected int actionId = 0;
    [Header("コイン")]
    public int coins = 20; //※弱点を突いたら2倍

    [Header("アニメーション")]
    public Animator animator;
    [Header("段階ごとのHP")]
    public int[] PhasePerHP; //昇順（例：1,2,3）
    public bool multiPhase; //複数形態有無
    [HideInInspector] public int phase = 0;
    [Header("弱点")]
    public WeaknessDamage[] weaknesses;
    [Header("ボスゲージ表示")]
    public bool showBossHP;
    [Header("ボスボーナス")]
    public bool enableBossBonus = false;
    public uint Score = 10000;
    [Header("ステージクリア")]
    public int clearIndex = 0;
    public bool stopMusic;
    public bool stageClear;

    private Rigidbody rb;
    protected Vector3 velocity;
    protected float gravity = 0f;
    protected GameObject targetPlayer;

    protected bool defended = false;

    float colliderRadius;
    float colliderHeight;
    protected float damageTime;
    [HideInInspector] public float damageInterval;
    protected int damageTrigger = 0;
    protected bool invincible = false;
    protected bool weaknessExploited = false;

    private float damageOpac = 0f;

    [Header("エフェクト")]
    public GameObject hitEffect;
    public GameObject damageEffect;
    [Header("効果音")]
    public AudioSource soundSource;
    public AudioClip stompedSound;
    public AudioClip[] fireDamageSound;
    public AudioClip blowDamageSound;
    public AudioClip defeatedSound;
    [Header("ボイス")]
    public AudioSource voiceSource;
    public AudioClip[] damageVoices;
    public AudioClip[] blowDamageVoices;
    [Header("状態")]
    public bool Grounded = true;
    public bool Flying = false;
    [Header("着地判定")]
    public LayerMask GroundLayer;

    private RaycastHit hit;
    // Start is called before the first frame update
    void OnEnable()
    {
        //HPの設定
        SetHP();

        //当たり判定
        colliderRadius = GetComponent<CapsuleCollider>().radius;
        colliderHeight = GetComponent<CapsuleCollider>().height;

        rb = GetComponent<Rigidbody>();

        //複数形態判定
        foreach (int phaseHP in PhasePerHP)
        {
            if (phaseHP >= HP)
            {
                multiPhase = true;
                break;
            }
        }
        if (!multiPhase)
        {
            //１形態のみ、段階設定
            Array.Sort(PhasePerHP);
        }
        phase = PhasePerHP.Length;

        if (showBossHP)
        {
            GameManager.bossInfo = this;
        }

        GameManager.bossCount++;

        StartCoroutine("Attack");
    }
    public virtual void SetHP()
    {
        // switch (options.difficulty)
        // {
        //     case Options.Difficulty.Easy:
        //         if (HP > 3)
        //         {
        //             HP = (int)Math.Round((float)HP * 0.8f);

        //             for (int i = 0; i < PhasePerHP.Length; i++)
        //             {
        //                 PhasePerHP[i] = (int)Math.Round((float)PhasePerHP[i] * 0.8f);
        //             }
        //         }
        //         HitsPerHP = (int)Math.Ceiling((float)HitsPerHP / 2f);
        //         break;

        //     case Options.Difficulty.Hard:
        //         if (HP < 4)
        //         {
        //             HP++;
        //         }
        //         else
        //         {
        //             HP = (int)((float)HP * 1.25f);

        //             for (int i = 0; i < PhasePerHP.Length; i++)
        //             {
        //                 PhasePerHP[i] = (int)((float)PhasePerHP[i] * 1.25f) + 1;
        //             }
        //         }
        //         HitsPerHP = (int)Math.Round((float)HitsPerHP * 1.2f);
        //         // HitsPerHP = HitsPerHP * 2;
        //         break;
        // }
        if (HP <= 0)
        {
            HP = PhasePerHP[phase];
        }
        totalHp = HitsPerHP * 2f * HP;
        singleDamage = HitsPerHP * 2;
        maxHp = (int)totalHp;

        latestHP = HP;
    }

    // Update is called once per frame
    void Update()
    {
        if (showBossHP)
        {
            if (!GameManager.bossHPShow && HP > 0)
            {
                GameManager.bossHPShow = true;
            }
        }

        if (damageTime > 0)
        {
            damageTime -= Time.deltaTime;
            if (damageTime <= 0)
            {
                damageTrigger = 0;
            }
        }
        if (damageInterval > 0)
        {
            damageInterval -= Time.deltaTime;
        }

        if (animator != null)
        {
            animator.SetInteger("Actions", actionId);
            animator.SetBool("IsAlive", HP > 0);
            animator.SetInteger("DamageTrigger", damageTrigger);
        }

        OverrideUpdate();

        SkinManager();
    }

    public virtual void OverrideUpdate()
    {
        ;
    }

    void FixedUpdate()
    {
        // if (gravity > 0)
        // {
        //     velocity.y -= gravity;
        // }
        float maxRay = Math.Max(colliderRadius, colliderHeight);
        if (!Flying)
        {
            Physics.BoxCast(transform.position, Vector3.one * 0.75f, -transform.up, out hit, Quaternion.identity, maxRay + 0.2f, GroundLayer);
            if (Grounded)
            {
                if (hit.distance <= 0f)
                {
                    Grounded = false;
                }
                else
                {
                    transform.position += Vector3.up * (maxRay - hit.distance);
                }
            }
            else
            {
                if (gravity > 0)
                {
                    velocity.y -= gravity;
                }

                if (hit.distance > 0 && velocity.y < 0)
                {
                    Grounded = true;
                    velocity.y = 0f;
                }
            }
        }

        if (HP <= 0)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = velocity;
        }

        if (this.GetComponent<MegaManBossManager>() != null)
        {
            damageOpac = (damageTime % 0.1f >= 0.05f) ? 1f : 0f;
        } else
        {
            if (damageOpac > 0f)
            {
                damageOpac -= 3 * Time.fixedDeltaTime;
                if (damageOpac <= 0f)
                {
                    damageOpac = 0f;
                }
            }
        }

        OverrideFixedUpdate();
    }

    public virtual void OverrideFixedUpdate()
    {
        ;
    }

    void OnTriggerStay(Collider col)
    {
        if (HP > 0 && damageTime <= 0)
        {
            if (col.gameObject.GetComponent<InstaShield>() != null)
            {
                //ダブル回転アタックの攻撃が当たった場合
                player = col.GetComponent<InstaShield>().player;
                if (player.playerType == 3) player.BigStomp();
                Damage(player, singleDamage, false);
            }
        }
    }
    void OnCollisionStay(Collision col)
    {
        if (HP > 0 && damageTime <= 0)
        {
            if (col.gameObject.tag == "Player")
            {
                player = col.gameObject.GetComponent<PlayerInfo>();
                if (player.invincible || player.giant)
                {
                    if (player.playerType == 3) player.BigStomp();
                    Instantiate(hitEffect, col.gameObject.transform.position, Quaternion.identity);
                    Damage(player, singleDamage, false);
                }
                else if (player.rolling)
                {
                    if (player.playerType != 0) player.BigStomp();
                    Instantiate(hitEffect, col.gameObject.transform.position, Quaternion.identity);
                    Damage(player, singleDamage, Stomped(col, false));
                }
                else if (Stomped(col, true))
                {
                    player.BigStomp();
                    Instantiate(hitEffect, col.gameObject.transform.position, Quaternion.identity);
                    Damage(player, singleDamage, true);
                }
                else if (player.attacking)
                {
                    Instantiate(hitEffect, col.gameObject.transform.position, Quaternion.identity);
                    Damage(player, singleDamage, Stomped(col, true));
                }
                else
                {
                    player.TakeDamage(6, this.transform.position);
                }
            }
        }
    }
    public virtual void Damage(PlayerInfo playerObj, int damage, bool stomped, WeaponTypes weapon = WeaponTypes.None, bool canPenetrate = false)
    {
        if (damageInterval > 0f)
        {
            return;
        }
        player = playerObj;

        if (defended)
        {
            damageTime = 0.25f;
            damageTrigger = 2;
            return;
        }

        if (canPenetrate && damageTime <= 0f)
        {
            damageInterval = 0.1f;
        }

        int wDamage = GetWeaponDamage(damage, weapon);

        if (!invincible)
        {
            if (HP > 0)
            {
                if (stomped)
                {
                    soundSource.PlayOneShot(stompedSound);
                    damageTrigger = 1;
                    damageTime = 1f;

                    VoicePlay(damageVoices);
                }

                float attackRate = playerObj.attackUp ? 1.5f : 1f;

                float totalDamage = (weapon != WeaponTypes.None ? wDamage : damage) * attackRate;
                totalHp -= totalDamage;
                if (weaknessExploited)
                {
                    GameManager.weaknessRate += totalDamage * 100f / (float)maxHp;
                }
                HP = (int)Math.Ceiling((float)totalHp / singleDamage);

                damageOpac = 1f;

                if (latestHP > HP)
                {
                    latestHP = HP;
                    if (!stomped)
                    {
                        damageTime = 1f;
                        soundSource.PlayOneShot(blowDamageSound);
                        damageTrigger = 2;
                        VoicePlay(blowDamageVoices);
                    }

                    Instantiate(damageEffect, this.transform.position, Quaternion.identity);
                    if (HP > 0)
                    {
                        StartCoroutine("DamageTimeStop");
                    }
                    StartCoroutine("DamageAnimation");
                } else
                {
                    if (!stomped) {
                        SoundPlay(fireDamageSound);
                    }
                }

                if (phase > 0 && !multiPhase)
                {
                    if (HP <= PhasePerHP[phase - 1])
                    {
                        phase--;
                        StartCoroutine("PhaseChange");
                    }
                }

                if (totalHp <= 0)
                {
                    if (multiPhase && phase > 0)
                    {
                        phase--;
                        StartCoroutine("PhaseChange");
                    } else
                    {
                        if (stageClear) GameManager.timeIncrease = false;
                        if (enableBossBonus)
                        {
                            if (weaknessExploited)
                            {
                                player.scorePopUp((int)Score, true, this.transform.position, 0);
                            } else
                            {
                                player.scorePopUp((int)Score, false, this.transform.position, 0);
                            }
                        }
                        if (weaknessExploited)
                        {
                            coins *= 2;
                        }
                        StopCoroutine("Attack");
                        if (stopMusic)
                        {
                            MusicManager.musicIndex = 2;
                            StartCoroutine("SlowMotion");
                        }
                        else
                        {
                            StartCoroutine("DamageTimeStop");
                            StartCoroutine("DefeatedAnimation");
                        }
                    }
                }
            }
        }

        if (showBossHP)
        {
            GameManager.bossInfo = this;
        }
    }
    public virtual int GetWeaponDamage(int damage, WeaponTypes weapon)
    {
        // int wDamage = 2;
        int wDamage = damage;
        weaknessExploited = false;
        foreach (WeaknessDamage weakness in weaknesses)
        {
            if (weakness.weakness == weapon && weapon != WeaponTypes.None)
            {
                switch (weakness.damageType)
                {
                    case WeaponDamageType.Effective:
                        //効果は抜群だ！
                        wDamage = (int)((float)singleDamage * 1.5f);
                        weaknessExploited = true;
                        break;

                    case WeaponDamageType.Weak:
                        //効果はいま一つのようだ・・・
                        wDamage = 1;
                        break;

                    case WeaponDamageType.NoEffective:
                        //効果がないみたいだ・・・
                        wDamage = 0;
                        break;

                    case WeaponDamageType.Heal:
                        wDamage = -singleDamage;
                        break;
                }
                break;
            }
        }
        // if (wDamage == 2 && wDamage < damage)
        // {
        //     wDamage = damage;
        // }
        return wDamage;
    }

    public virtual IEnumerator Attack()
    {
        yield return null;
    }
    public virtual IEnumerator PhaseChange()
    {
        yield return null;
    }
    public IEnumerator DamageTimeStop()
    {
        GameManager.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.1f);
        GameManager.timeScale = 1f;
    }
    public virtual IEnumerator DamageAnimation()
    {
        yield return null;
    }
    public IEnumerator SlowMotion()
    {
        soundSource.PlayOneShot(defeatedSound);
        GameManager.timeScale = 0.5f;

        yield return new WaitForSeconds(0.875f);
        
        GameManager.timeScale = 1f;
        StartCoroutine("DefeatedAnimation");
    }
    public virtual IEnumerator DefeatedAnimation()
    {
        yield return null;
    }
    public void VoicePlay(AudioClip[] voiceClips)
    {
        if (voiceSource == null)
        {
            return;
        }
        if (voiceClips.Length > 0)
        {
            int voiceIndex = UnityEngine.Random.Range(0, voiceClips.Length);

            //効果音の再生
            voiceSource.clip = voiceClips[voiceIndex];
            voiceSource.Play();
        }
    }

    protected void OnDestroy()
    {
        if (HP <= 0)
        {
            GameManager.bossHPShow = false;

            if (stageClear)
            {
                // data.ResetCheckPoint();
                // SceneManager.LoadScene("ResultScreen");
                if (this.GetComponent<MegaManBossManager>() != null)
                {
                    GameManager.stageClearIndex = 2;
                } else
                {
                    GameManager.stageClearIndex = clearIndex;
                }
            }
        }
    }

    bool Stomped(Collision col, bool stompable)
    {
        //float playerY = col.transform.position.y - 2f + (colliderRadius * this.transform.localScale.y);
        float playerY = col.transform.position.y;
        float thisY = this.transform.position.y;

        if (stompable)
        {
            return player.Stompable && playerY >= thisY;
        }
        return playerY >= thisY;
    }

    public void SoundPlay(AudioClip[] soundClips)
    {
        if (soundClips != null)
        {
            int soundIndex = UnityEngine.Random.Range(0, soundClips.Length);

            //効果音の再生
            soundSource.PlayOneShot(soundClips[soundIndex]);
        }
    }

    public Vector3 FacePlayer(bool yLock = false)
    {
        SetPlayer();
        if (targetPlayer == null)
        {
            return Vector3.forward;
        }
        else
        {
            Vector3 direction = (targetPlayer.transform.position - this.transform.position).normalized;
            if (yLock)
            {
                direction.y = 0;
            }
            return direction;
        }
    }

    public void SetPlayer()
    {
        float minDistance = Mathf.Infinity;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerInfo>() != null && Vector3.Distance(player.transform.position, this.transform.position) < minDistance)
            {
                minDistance = Vector3.Distance(player.transform.position, this.transform.position);
                targetPlayer = player;
            }
        }
    }
    
    public Vector3 SpeedReachToTop(Vector3 velocity, float top){
        //特定の速度を超えた時の処理
        Vector3 ReducedSpeed = velocity;
        float keepY = velocity.y;
        ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, top);
        ReducedSpeed.y = keepY;
        return ReducedSpeed;
    }

    public void SkinManager()
    {
        Material[] currentMat;

        if (this.GetComponent<SkinnedMeshRenderer>() != null)
        {
            currentMat = this.GetComponent<SkinnedMeshRenderer>().materials;

            currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

            this.GetComponent<SkinnedMeshRenderer>().materials = currentMat;
        }

        if (this.GetComponent<MeshRenderer>() != null)
        {
            currentMat = this.GetComponent<MeshRenderer>().materials;

            currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

            this.GetComponent<MeshRenderer>().materials = currentMat;
        }

        ChangeSkin(this.transform);
    }
    void ChangeSkin(Transform p)
    {
        Material[] currentMat;
        foreach (Transform child in p)
        {
            if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
            {
                currentMat = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;

                currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

                child.gameObject.GetComponent<SkinnedMeshRenderer>().materials = currentMat;
            }

            if (child.gameObject.GetComponent<MeshRenderer>() != null)
            {
                currentMat = child.gameObject.GetComponent<MeshRenderer>().materials;

                currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

                child.gameObject.GetComponent<MeshRenderer>().materials = currentMat;
            }

            ChangeSkin(child);
        }
    }
}
