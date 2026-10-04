using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PathCreation;

public class PlayerInfo : PlayerController
{
    private Vector3 currentPosition;
    [HideInInspector] public byte playerId; //プレイヤーのID（例：０ → マリオ）
    public byte maxHPSegment = 6;             //最大ＨＰ
    [HideInInspector] public uint maxHP = 600; //最大ＨＰ
    [HideInInspector] public uint HP;         //ＨＰ
    public float defenseRate = 1.0f;         //防御補正
    private float hpRate;

    // [HideInInspector] public byte lives = 0; //残機
    public int localScore = 0; //プレイヤーのスコア
    // public float maxAirAmount = 24f;
    [HideInInspector] public float airAmount = 24f;
    [HideInInspector] public sbyte airCountdown = 5;

    [Header("エフェクト等のオブジェクト")]
    public GameObject OneUpObj; //1UPアピール用オブジェクト
    private GameObject OneUpObjControl;
    public GameObject hitEffect;
    public GameObject giantTransformEffect;
    public GameObject giantTransformEffect2;
    private Transform currentGiantEffect;
    public GameObject target;
    public GameObject ScorePopUp;
    public byte rate = 1;
    public float rateTime = 0f;

    [Header("７大パワーアップ")]
    public byte powerUpActive = 0; //0＝なし、1＝ファイア、2＝アイス、3＝サンダー
    public GameObject fireBall;
    public GameObject iceBall;
    public GameObject thunderBeam;
    public GameObject boomerang;
    public GameObject bubble;
    public GameObject windStorm;
    public GameObject bounceBall;
    [Header("４大バリア")]
    public GameObject shieldGroup;
    public Vector3 shieldOffset;
    public sbyte shieldActive = 0; //0＝なし、1＝バリア、2＝フレイム、3＝アクア、4＝サンダー
    private ShieldManager shieldManager;
    [Header("バリアで守られる属性ダメージ")]
    public bool fireProof;
    public bool underwaterProof;
    public bool elecProof;
    public bool poisonProof;
    [HideInInspector] public bool disableDamage;

    [Header("アイテム風船")]
    public GameObject itemBalloon;

    private byte cookiesAmount;
    // private byte ringsAmount;

    private PlayerVoiceManager voices;

    [HideInInspector] public float damageTime;
    [Header("効果音ソース")]
    public AudioSource source;

    [Header("データ")]
    public GameData data;
    public Options options;

    private float swapLockTime = 0.1f;
    private bool teleportedToPartner = false;

    [Header("相方復帰フラグ")]
    public PlayerPrefabs playerPrefs;
    public bool partnerRevival;

    void Start()
    {
        // source = GetComponent<AudioSource>();
        StartCoroutine("setPlayerInfo");

        shieldManager = Instantiate(shieldGroup, this.transform.position + shieldOffset, Quaternion.identity, this.transform).GetComponent<ShieldManager>();

        maxHP = (uint)maxHPSegment * 100;
        if (data.HPs[playerNumber] <= 0)
        {
            HP = maxHP;
        }
        else
        {
            HP = data.HPs[playerNumber];
        }

        // lives = data.lives[playerNumber];
        GameManager.Lives = data.lives;

        if (this.GetComponent<_16MSonic>() == null) powerUpActive = data.powerUps[playerNumber];
        shieldActive = data.shields[playerNumber];

        voices = GetComponent<PlayerVoiceManager>();

        currentPosition = this.transform.position;

        //バッジの効果
        GetBadgeEffect();

        //metalSkin.gameObject.SetActive(true);
    }
    void Update()
    {
        //最大HPの比率（エクストラの場合：60%）
        hpRate = GameManager.extra ? 0.6f : 1f;

        //最大HPセグメント（1～10）
        maxHPSegment = (byte)Mathf.Clamp(maxHPSegment, 1, 10);
        //最大HP（5～100）
        maxHP = (uint)maxHPSegment * 100;
        //HP（0～最大HP）
        HP = (uint)Mathf.Clamp(HP, 0, (int)((float)maxHP * hpRate));

        //スコア（0～999,999,999）
        localScore = (int)Mathf.Clamp(localScore, 0, 999999999);

        // if (HP > 0)
        // {
        //     data.powerUps[playerNumber] = powerUpActive;
        //     data.shields[playerNumber] = shieldActive;
        // }
        // else
        // {
        //     data.powerUps[playerNumber] = 0;
        //     data.shields[playerNumber] = 0;
        // }

        if (GameManager.players.Count > playerNumber)
        {
            //プレイヤークラスの値を設定
            if (playerId != 16 && playerType != 2 && primary)
            {
                // GameManager.players[playerNumber].setStatus(playerId, maxHP, HP, lives, localScore);
                GameManager.players[playerNumber].setStatus(playerId, (ushort)((float)maxHP * hpRate), HP, (uint)localScore);
            }
        }
        Controller();

        RaycastHit crushRaycast;
        Physics.SphereCast(transform.position, 0.4f, transform.up, out crushRaycast, ColliderHeight, GroundLayer);
        if (crushRaycast.distance > 0 && crushRaycast.distance < ((ColliderHeight / 2) - 1f) && HP > 0 && Grounded && activeCollision)
        {
            //上から挟まれた！！
            //Debug.Log("Oh no");
            HP = 0;
            Death();
        }

        //バリア機能
        SetProof();

        //タイムアップ
        if (GameManager.TotalTime <= 0 && HP > 0 && GameManager.extra)
        {
            HP = 0;
            Death();
        }

        if (this.transform.position.y <= -100f && HP > 0)
        {
            //落下
            Fallen();
        }

        if (HP > 0)
        {
            //ダメージを受けた後の点滅
            if (damageTime > 0 && !tookDamage)
            {
                damageTime -= Time.deltaTime;
                BlinkingSkin((damageTime % 0.1f) / 0.05f < 1);
            }
            else
            {
                BlinkingSkin(true);
            }
        }
        //MetalSkinRender();

        //1UPアピール用オブジェクト
        if (OneUpObjControl != null)
        {
            OneUpObjControl.transform.position = transform.position + (Vector3.up * 2);
        }

        //水中
        // if (maxAirAmount > 0)
        // {
        if (underwater && !underwaterProof && !metal && HP > 0)
        {
            if ((shieldActive == 2 && !underwaterButLava) || shieldActive == 4)
            {
                //フレイムバリア及びサンダーバリアが無効となる。
                shieldActive = 0;
            }
            if (airAmount <= 0)
            {
                //カウントダウン
                if (12 + airAmount <= airCountdown * 2)
                {
                    airCountdown--;
                }
            }
            if (airAmount <= -12 && airAmount > -13)
            {
                Drown();
                airAmount = -13f;
            }
            else
            {
                if (!underwaterButLava)
                {
                    airAmount -= Time.deltaTime;
                    if (airAmount <= 0 && airCountdown == 5 && playerType == 3)
                    {
                        MusicManager.musicIndex = 3;
                    }
                }
                else
                {
                    if (shieldActive != 2 && damageTime <= 0 && !tookDamage)
                    {
                        //フレイムバリアを纏っていない状態でマグマの中にいると、即死。
                        TakeDamage(99, this.transform.position, (int)DamageType.Fire);
                    }
                }
            }
        }
        else
        {
            airAmount = 24f;
            airCountdown = 5;
        }
        GetComponent<AudioLowPassFilter>().enabled = underwater;

        if (MusicManager.musicIndex == 3 && (!underwater || airAmount > 0))
        {
            MusicManager.musicIndex = 0;
        }

        //スピードアップ
        if (speedUpTime > 0)
        {
            speedUpTime -= Time.deltaTime;
            if (speedUpTime <= 0)
            {
                speedUp = false;
                SoundPlay(powerDownSound);
                if (MusicManager.musicIndex == 4)
                {
                    MusicManager.musicIndex = 0;
                }
            }
        }

        var invincibleEmission0 = marioInvincibleEffect.emission;
        var invincibleEmission1 = pacInvincibleEffect.emission;
        var invincibleEmission2 = sonicInvincibleEffect.emission;
        if (invincibleTime > 0)
        {
            //無敵エフェクト
            invincibleEmission0.rateOverTime = (invincibleTrigger == 0) ? 100f : 0f;
            invincibleEmission1.rateOverTime = (invincibleTrigger == 1) ? 100f : 0f;
            invincibleEmission2.rateOverTime = (invincibleTrigger == 2) ? 100f : 0f;

            //エフェクトタイム
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0)
            {
                invincible = false;
                SoundPlay(powerDownSound);
                if (MusicManager.musicIndex == 5 || MusicManager.musicIndex == 6)
                {
                    MusicManager.musicIndex = 0;
                }
            }
        }
        else
        {
            invincibleEmission0.rateOverTime = 0f;
            invincibleEmission1.rateOverTime = 0f;
            invincibleEmission2.rateOverTime = 0f;
        }
        StarSkin();

        if (currentGiantEffect != null)
        {
            currentGiantEffect.position = this.transform.position;
        }

        if (giantTime > 0)
        {
            giantTime -= Time.deltaTime;
            if (giantTime <= 0)
            {
                GetComponent<AudioSource>().pitch = 1f;
                if (giantTrigger == 1)
                {
                    SoundPlay(powerPelletEnd);
                }
                else
                {
                    SoundPlay(megaMushroomEnd);
                }
                if (MusicManager.musicIndex == 9)
                {
                    MusicManager.musicIndex = 0;
                }
                giant = false;
            }
        }
        if (giant)
        {
            if (scaleRate < 1f)
            {
                scaleRate += 1.5f * Time.deltaTime;
                if (scaleRate > 1f)
                {
                    scaleRate = 1f;
                }
            }
        }
        else
        {
            if (scaleRate > 0f)
            {
                scaleRate -= 1.5f * Time.deltaTime;
                if (scaleRate < 0f)
                {
                    scaleRate = 0f;
                }
            }
        }
        this.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 4f, scaleRate);

        if (metalTime > 0)
        {
            //エフェクトタイム
            metalTime -= Time.deltaTime;
            if (metalTime <= 0)
            {
                metal = false;
                if (MusicManager.musicIndex == 7)
                {
                    MusicManager.musicIndex = 0;
                }
            }
        }

        if (invincible && invincibleTrigger == 0)
        {
            invincibleSpeedRate = 1.5f;
        }
        else
        {
            invincibleSpeedRate = 1.0f;
        }

        if (invincible && invincibleTrigger == 1 && !invulSound.isPlaying)
        {
            invulSound.Play();
        }
        else if ((!invincible && invincibleTrigger == 1 && invulSound.isPlaying) || invincibleTrigger != 1)
        {
            invulSound.Stop();
        }

        //上を見上げている状態
        LookingUp = (
            XZmag <= 1f &&
            ((Axises["Vertical"] > 0.5) || (Axises["Alt Vertical"] > 0.5)) &&
            GameManager.dimension != DimensionType.Normal3D
        );

        if (rateTime > 0)
        {
            if (rateTime > 10)
            {
                rateTime = 10f;
            }
            rateTime -= Time.deltaTime;
            if (rateTime <= 0)
            {
                rate = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            UseStockedItem(0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UseStockedItem(1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            UseStockedItem(2);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseStockedItem(3);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            UseStockedTank(0);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            UseStockedTank(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            UseStockedTank(2);
        }

        if (primary)
        {
            if (swapLockTime > 0)
            {
                swapLockTime -= Time.deltaTime;
            }
        } else
        {
            swapLockTime = 0.1f;
        }
        if (ButtonsDown["Back"] && primary && swapLockTime <= 0f)
        {
            swapLockTime = 0.1f;
            Swap();
        }

        if (partner != null)
        {
            if (!primary && !teleportedToPartner && Vector3.Distance(partner.gameObject.transform.position, this.transform.position) > 40f && HP > 0)
            {
                TeleportToPartner();
            }
        }
        if (teleportedToPartner)
        {
            if (Grounded)
            {
                teleportedToPartner = false;
            }
        }
    }
    IEnumerator setPlayerInfo()
    {
        //例）P2の場合、0.2秒待ってから追加する
        yield return new WaitForSeconds(0.05f * (playerNumber + 1));
        if (primary)
        {
            GameManager.players.Add(new PlayerStatus());
        }
    }

    void BlinkingSkin(bool enabled)
    {
        Renderer[] childrenRenderer = skin.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < childrenRenderer.Length; i++)
        {
            //childrenRenderer[i].enabled = enabled && !metal;
            childrenRenderer[i].enabled = enabled;
        }
    }
    void StarSkin()
    {
        Renderer[] childrenRenderer = skin.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < childrenRenderer.Length; i++)
        {
            try
            {
                childrenRenderer[i].material.SetFloat("StarTrigger", (invincible ? 1f : 0f));
                childrenRenderer[i].material.SetFloat("MetalTrigger", (metal ? 1f : 0f));
            }
            catch { }
        }
    }
    // void MetalSkinRender() {
    //     if (metalSkin == null) {
    //         return;
    //     }
    //     Renderer[] childrenRenderer = metalSkin.GetComponentsInChildren<Renderer>();

    //     for (int i = 0; i < childrenRenderer.Length; i++){
    // 		childrenRenderer[i].enabled = metal;
    // 	}
    // }

    public void Jump()
    {
        //ジャンプ
        if (metal)
        {
            SoundPlay(jumpSoundMetal);
        }
        else
        {
            SoundPlay(jumpSound);
        }
        if (speedPassed)
        {
            velocity = finalVelocity + AirborneC["jmp"] * (metal ? 1.35f : 1) * transform.up * (giant ? 1.5f : 1f);
        }
        else
        {
            velocity.y = AirborneC["jmp"] * (metal ? 1.35f : 1) * (giant ? 1.5f : 1f);
        }

        GroundNormal = Vector3.up;

        Grounded = false;
    }
    public void Swim()
    {
        velocity.y = AirborneC["jmp"] * (giant ? 1.5f : 1f);
        GroundNormal = Vector3.up;
        Grounded = false;
    }
    public void Stomp(bool notBeated = false)
    {
        //弱い敵を踏んだ時
        attacked = true;

        switch (playerType)
        {
            case 0:
                //マリオ
                Grounded = false;

                if (this.GetComponent<MarioActions>().actionId != 5 || notBeated)
                {
                    velocity.y = 30f;
                    this.GetComponent<MarioActions>().actionId = 0;
                }
                axisInput = true;
                break;

            case 1:
                //パックマン
                ;
                break;

            case 2:
                //ロックマン
                // velocity.y = 36.63f;
                Grounded = false;

                if (this.GetComponent<CommonActions>().actionId != 5 || notBeated)
                {
                    velocity.y = 36.63f;
                    this.GetComponent<CommonActions>().actionId = 0;
                }
                axisInput = true;
                break;

            case 3:
                //ソニック
                if (this.GetComponent<SonicActions>().actionId != 2)
                {
                    if (velocity.y < 0 && !Grounded)
                    {
                        velocity.y *= -1;
                        if (velocity.y < 30f)
                        {
                            velocity.y = 30f;
                        }
                    }
                }
                activePhysics = true;
                break;
        }
    }
    public void BigStomp()
    {
        //敵を踏んだ時（マリオ）
        attacked = true;

        switch (playerType)
        {
            case 0:
                //マリオ
                Grounded = false;
                velocity.y = 40f;

                this.GetComponent<MarioActions>().actionId = 0;
                axisInput = true;
                break;

            case 1:
                //パックマン
                Grounded = true;
                break;

            case 2:
                //ロックマン
                velocity.y = 48.84f;
                break;

            case 3:
                //ソニック
                velocity.x *= -1;
                velocity.y *= -1;
                if (velocity.y < 30f)
                {
                    velocity.y = 30f;
                }
                velocity.z *= -1;
                break;
        }

    }
    public byte ComboIncrease()
    {
        //コンボ上昇（8以上で1UP）
        if (giant)
        {
            combo = 1;
        }
        else if (this.GetComponent<_16MSonic>() != null)
        {
            // if (combo < 2) combo++;
            combo = 1;
        }
        else
        {
            if (combo < 99) combo++;
            if (combo > GameManager.ghostCombo)
            {
                GameManager.ghostCombo = (byte)combo;
            }
            if (GameManager.maxCombo < combo) GameManager.maxCombo = (byte)combo;

            // if ((combo >= 8 && !GameManager.extra) || (combo == 8 && GameManager.extra))
            if (combo == 8)
            {
                OneUp();
            }
        }

        // return (byte)Math.Min(5, combo);
        return (byte)combo;
    }
    public byte GetCombo()
    {
        //コンボの取得
        // return (byte)Math.Min(5, combo);
        return (byte)combo;
    }
    public byte GhostComboIncrease()
    {
        //コンボ上昇（8以上で1UP）
        if (GameManager.ghostCombo < 99) GameManager.ghostCombo++;
        if (GameManager.maxCombo < GameManager.ghostCombo) GameManager.maxCombo = GameManager.ghostCombo;

        // if ((GameManager.ghostCombo >= 8 && !GameManager.extra) || (GameManager.ghostCombo == 8 && GameManager.extra))
        if (GameManager.ghostCombo == 8)
        {
            OneUp();
        }

        return GameManager.ghostCombo;
    }
    public byte GetGhostCombo()
    {
        //コンボの取得
        return GameManager.ghostCombo;
    }
    public void ComboReset()
    {
        if (!invincible)
        {
            //コンボの初期化
            combo = 0;
        }
    }
    public void RateUp()
    {
        rateTime = 10f;
        if (rate < 100)
        {
            rate++;
            // } else {
            //     scorePopUp(1000, false, this.transform.position);
        }
    }

    public bool getNormalAttack()
    {
        //通常攻撃か否かを取得
        return true;
    }
    public void TakeDamage(ushort value, Vector3 colPos, int damageType = 0)
    {
        value *= 20;
        if (!primary)
        {
            value = 0;
        }

        if (metal || giant || (disableDamage && damageType != 5))
        {
            //メタルまたは巨大でダメージ無効化
            return;
        }
        if (damageType == 1 && fireProof)
        {
            //フレイムバリアで火によるダメージを防ぐ
            return;
        }
        if (damageType == 3 && elecProof)
        {
            //サンダーバリアで電撃によるダメージを防ぐ
            return;
        }
        if (damageType == 6 && poisonProof)
        {
            //ポイズンバリアで毒によるダメージを防ぐ
            return;
        }
        grabbing = false;
        Vector3 thisPos = this.transform.position;
        (thisPos.y, colPos.y) = (0, 0);
        Vector3 direction = this.transform.position - colPos;
        float dMagnitude;

        //防御補正
        value = (ushort)Math.Round((float)value * defenseRate);

        //アシスト
        if (options.difficulty == Options.Difficulty.Easy)
        {
            value = (ushort)Math.Round((float)value / 2f);
        }

        //防御アップ
        if (defenseUp)
        {
            value = (ushort)Math.Round((float)value / 2f);
        }

        switch (GameManager.dimension)
        {
            case DimensionType.XWay2D:
                //2D(X方向、Z固定)
                dMagnitude = direction.magnitude * Math.Sign(direction.x);
                direction = new Vector3(dMagnitude, 0f, 0f);
                break;

            case DimensionType.ZWay2D:
                //2D(Z方向、X固定)
                dMagnitude = direction.magnitude * Math.Sign(direction.z);
                direction = new Vector3(0f, 0f, dMagnitude);
                break;
        }

        if ((damageTime <= 0 || (damageType == 5 && finalVelocity.y < 0f)) && HP > 0 && !invincible)
        {
            bool powerDown = false;

            //ダメージの処理
            if (shieldActive > 0 || shieldActive < 0)
            {
                if (value > 0)
                {
                    if (shieldActive == 6)
                    {
                        //フォースバリア残り1回
                        shieldActive = -1;
                    }
                    else
                    {
                        if (shieldActive == 7)
                        {
                            //アルマゲドンバリアによる大爆発
                            AllOutAttack();
                        }
                        shieldActive = 0;
                    }
                }
            }
            else
            {
                if (playerNumber == 0 && GameManager.damageTaken < 99 && primary) GameManager.damageTaken++;
                if (HP >= 2 && HP <= value && value < maxHP)
                {
                    HP = 1;
                }
                else
                {
                    if (HP < value)
                    {
                        HP = 0;
                    }
                    else
                    {
                        HP -= value;
                    }
                }
                if (GetComponent<_16MSonic>() != null && GameManager.courseMode != CourseMode.RingAttack && HP > 0 && value > 0)
                {
                    GetComponent<_16MSonic>().RingSpread();
                }
                if (GameManager.courseMode == CourseMode.RingAttack && HP > 0 && value > 0)
                {
                    GetComponent<CommonActions>().RingSpreadRingAttackOnly();
                }
                if (powerUpActive > 0 && value > 0)
                {
                    powerDown = true;
                    StartCoroutine(PowerDown(powerUpActive));
                    if (HP <= 0) HP = 1;
                } else
                {
                    if (HP > 0 && primary)
                    {
                        StartCoroutine("DamageTimeStop");
                    }
                }
                if (autoEnergyTank && data.stockItems[4].amounts > 0 && HP <= 0)
                {
                    HP = 1;
                    UseStockedTank(0);
                }
            }
            if (HP <= 0)
            {
                Death(direction);
            }
            else
            {
                if (damageType == 5)
                {
                    voices.Burning();
                }
                else
                {
                    voices.Hurt();
                }
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                switch (damageType)
                {
                    case 1:
                    case 5:
                        //火によるダメージ
                        SoundPlay(damageSound);
                        SoundPlay(fireDamageSound);
                        break;

                    case 2:
                        //氷によるダメージ
                        SoundPlay(damageSound);
                        SoundPlay(iceDamageSound);
                        break;

                    case 3:
                        //電撃によるダメージ
                        SoundPlay(damageSound);
                        SoundPlay(elecDamageSound);
                        break;

                    case 4:
                        //トゲによるダメージ
                        SoundPlay(spikeDamageSound);
                        break;

                    default:
                        //通常ダメージ
                        SoundPlay(damageSound);
                        break;
                }
                damageTime = 2f;
                tookDamage = true;

                if (damageType == 5)
                {
                    if (GetComponent<MarioActions>() != null)
                    {
                        //マリオ
                        if (!powerDown) SoundPlay(powerDownSound);
                    }
                    damageByLava = 2;
                    JumpFromSprings(80f, Vector3.up);
                }
                else
                {
                    if (GetComponent<MarioActions>() != null)
                    {
                        //マリオ
                        if (!powerDown) SoundPlay(powerDownSound);
                        GetComponent<MarioActions>().StartCoroutine("DamageAnimation", direction);
                    }
                    else if (GetComponent<PacManActions>() != null)
                    {
                        //パックマン
                        GetComponent<PacManActions>().StartCoroutine("DamageAnimation");
                    }
                    else if (GetComponent<MegaManActions>() != null)
                    {
                        //ロックマン
                        GetComponent<MegaManActions>().StartCoroutine("DamageAnimation", direction);
                    }
                    else if (GetComponent<SonicActions>() != null)
                    {
                        //ソニック
                        GetComponent<SonicActions>().StartCoroutine("DamageAnimation", direction);
                    }
                }
            }
        }
    }
    public void HPRestore(float value, bool self = true)
    {
        if (partner != null && self)
        {
            partner.HPRestore(value, false);
        }
        //回復の処理
        HP += (uint)(value * 20f);
    }
    public void SpeedUp(bool self = true)
    {
        if (partner != null && self)
        {
            partner.SpeedUp(false);
        }
        //スピードアップ
        MusicManager.musicIndex = 4;
        speedUp = true;
        speedUpTime = 20f;
    }
    public void Invincible(float time, byte trigger, bool self = true)
    {
        if (partner != null && self)
        {
            partner.Invincible(time, trigger, false);
        }
        //無敵
        switch (trigger)
        {
            case 0:
                MusicManager.musicIndex = 5;
                break;

            case 2:
                MusicManager.musicIndex = 6;
                break;
        }
        invincible = true;
        invincibleTime = time;

        invincibleTrigger = trigger;
    }
    public void Giant(float time, byte trigger, bool self = true)
    {
        if (partner != null && self)
        {
            partner.Giant(time, trigger, false);
        }
        //巨大化
        if (trigger == 1)
        {
            //パワークッキー
            currentGiantEffect = Instantiate(giantTransformEffect2, this.transform.position, Quaternion.identity).transform;
        }
        else
        {
            //巨大キノコ
            currentGiantEffect = Instantiate(giantTransformEffect, this.transform.position, Quaternion.identity).transform;
            MusicManager.musicIndex = 9;
        }
        giant = true;
        giantTime = time;
        giantTrigger = trigger;
        GetComponent<AudioSource>().pitch = 0.75f;
    }
    public void MetalCookie(bool self = true)
    {
        if (partner != null && self)
        {
            partner.MetalCookie(false);
        }
        //メタル
        metal = true;
        metalTime = 20f;
        if (playerType == 0)
        {
            MusicManager.musicIndex = 7;
        }
    }
    public void setPlayerId(byte id)
    {
        //プレイヤーIDの設定
        playerId = id;
    }
    public void OneUp()
    {
        if (!options.lives)
        {
            return;
        }
        // if (options.difficulty != Options.Difficulty.Easy && !data.timeAttack)
        // {
        SoundPlay(oneUpSound);
        if (OneUpObjControl != null) Destroy(OneUpObjControl);
        OneUpObjControl = Instantiate(OneUpObj, transform.position + (Vector3.up * 2), Quaternion.identity);

        if (partnerRevival)
        {
            partnerRevival = false;
            PlayerInfo pl2 = Instantiate(playerPrefs.playerData[data.characterIdsSub[playerNumber]].prefab, this.transform.position - skin.forward * 4f + Vector3.up * 30f, Quaternion.identity).GetComponent<PlayerInfo>();
            pl2.playerNumber = this.playerNumber;
            pl2.primary = false;
            pl2.partner = this;
            pl2.TeleportToPartner();

            this.partner = pl2;
        } else
        {
            GameManager.Lives++;
        }
        // }
    }
    public void scorePopUp(int num, bool doubleRate, Vector3 position, int pCombo = 0)
    {
        if (!primary)
        {
            partner.scorePopUp(num, doubleRate, position, pCombo = 0);
            return;
        }
        if (GameManager.ScoreShow)
        {
            int points = (int)Math.Round(num * rate * (scoreUp ? 1.2 : 1.0) * GameManager.scoreRate);

            GameManager.ChainTimeRecovery();

            localScore += (int)(points * (doubleRate ? 2 : 1));

            bool sonicStage = this.GetComponent<_16MSonic>() != null;
            if (
                options.scorePopUp == Options.ScorePopUp.Normal || 
                options.scorePopUp == Options.ScorePopUp.Full || 
                (options.scorePopUp == Options.ScorePopUp.NormalExceptSonic && !sonicStage) || 
                (options.scorePopUp == Options.ScorePopUp.FullExceptSonic && !sonicStage)
                )
            {
                GameObject popup = Instantiate(ScorePopUp, position, Quaternion.identity);
                popup.GetComponent<ScoringController>().points = points;
                popup.GetComponent<ScoringController>().combo = pCombo;
                popup.GetComponent<ScoringController>().noScore = GameManager.noScore;
            
                if (doubleRate)
                {
                    popup.GetComponent<ScoringController>().rate = 2;
                }
            }
        }
    }
    public void scoreIncrease(int num, Vector3 position)
    {
        if (!primary)
        {
            partner.scoreIncrease(num, position);
            return;
        }
        if (GameManager.ScoreShow)
        {
            if (num < 0)
            {
                localScore -= (int)Math.Round(-num * rate * (scoreUp ? 1.2 : 1.0) * GameManager.scoreRate);
            }
            else
            {
                GameManager.ChainTimeRecovery();

                if (options.scorePopUp == Options.ScorePopUp.Full || options.scorePopUp == Options.ScorePopUp.FullExceptSonic)
                {
                    scorePopUp(num, false, position);
                } else
                {
                    localScore += (int)Math.Round(num * rate * (scoreUp ? 1.2 : 1.0) * GameManager.scoreRate);
                }
            }
        }
    }

    void Death(Vector3? direction = null)
    {
        if (direction == null)
        {
            direction = Vector3.zero;
        }

        canInput = false;
        SoundPlay(deathSound);

        if (GetComponent<MarioActions>() != null)
        {
            //マリオ
            GetComponent<MarioActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<PacManActions>() != null)
        {
            //パックマン
            GetComponent<PacManActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<MegaManActions>() != null)
        {
            //ロックマン
            GetComponent<MegaManActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<SonicActions>() != null)
        {
            //ソニック
            GetComponent<SonicActions>().StartCoroutine("DeathAnimation");
        }
        GetComponent<PlayerAnimManager>().actionId = -2;

        StartCoroutine("LostLife");
    }
    public IEnumerator DamageTimeStop()
    {
        GameManager.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.2f);
        GameManager.timeScale = 1f;
    }

    IEnumerator LostLife()
    {
        //やられた
        if (playerNumber == 0 && primary) GameManager.noMiss = false;

        // if (options.difficulty == Options.Difficulty.Easy && GameManager.Lives < 1)
        // {
        // GameManager.Lives = 1;
        // }
        if (!options.lives && GameManager.Lives < 1)
        {
            GameManager.Lives = 1;
        }

        bool partnerExists = false;
        if (partner != null)
        {
            if (partner.HP > 0)
            {
                partnerExists = true;
            }
        }

        if (areTheyPerished() && primary && !partnerExists)
        {
            //全滅した時の処理
            if (GetComponent<MarioActions>() != null)
            {
                MusicManager.musicIndex = 1;
            }
            else if (GetComponent<MegaManActions>() != null)
            {
                MusicManager.musicIndex = 2;
            }

            //スコア8割減少
            data.checkpointScore = GameManager.Score;
            //タイム継続
            if (!GameManager.extra)
            {
                data.totalTime = GameManager.TotalTime;
            }
            //コイン100枚減少
            data.checkpointCoins = GameManager.Coins;

            GameManager.death = true;

            yield return new WaitForSeconds(2f);

            //ワイプ
            WipeManager.WipeSetUp(true, 1.5f);
            // FadeManager.FadeInstantly(true);
            MusicManager.musicFade = true;

            yield return new WaitForSeconds(1.67f);

            // data.ResetExceptTime();
            SavePlayerInfo();
            if (GameManager.Lives <= 0 && (GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS))
            {
                SceneManager.LoadScene("GameOver");
            }
            else
            {
                // if (options.difficulty != Options.Difficulty.Easy && !data.timeAttack)
                if (GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS)
                {
                    yield return new WaitUntil(() => GameManager.restartTrigger);
                    //残機を減らす
                    if (options.lives)
                    {
                        // data.LostLife(playerNumber);
                        data.Lives();
                    }
                }
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            bool primaryDead = false;
            if (primary)
            {
                primaryDead = true;
                Swap(true);
                partner.scorePopUp((int)((float)partner.localScore * -0.1f), false, partner.gameObject.transform.position);
            }
            if (partner == null)
            {
                primaryDead = true;
            }
            //単体時の処理
            if (GameManager.Lives > 0)
            {
                yield return new WaitForSeconds(5f);

                if (GameManager.death)
                {
                    yield break;
                }

                if (!primary && this == partner.partner)
                {
                    TeleportToPartner();
                } else
                {
                    //泡状態になり、他のプレイヤーに追従
                    this.transform.position = currentPosition;
                }

                // if (options.difficulty != Options.Difficulty.Easy && !data.timeAttack) data.LostLife(playerNumber);
                if (primaryDead)
                {
                    if (options.lives && (GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS)) {
                        data.LostLife(playerNumber);
                        GameManager.Lives = data.lives;
                    }
                }
                HP = maxHP;
                SavePlayerInfo();

                canInput = true;
                GetComponent<PlayerAnimManager>().actionId = 0;

                if (GetComponent<MarioActions>() != null)
                {
                    //マリオ
                    GetComponent<MarioActions>().Reset();
                }
                else if (GetComponent<PacManActions>() != null)
                {
                    //パックマン
                    GetComponent<PacManActions>().Reset();
                }
                else if (GetComponent<MegaManActions>() != null)
                {
                    //ロックマン
                    GetComponent<MegaManActions>().Reset();
                }
                else if (GetComponent<SonicActions>() != null)
                {
                    //ソニック
                    GetComponent<SonicActions>().Reset();
                }
            }
        }
    }
    void Fallen()
    {
        //落ちた
        // if (options.beginnerMode && HP >= 8) {
        if (options.difficulty == Options.Difficulty.Easy)
        {
            if (HP > 1 && HP <= 160)
            {
                HP = 1;
            } else
            {
                HP -= 160;
            }
            if (GetComponent<_16MSonic>() != null && HP > 0)
            {
                GetComponent<_16MSonic>().RingSpread();
            }
        }
        else
        {
            HP = 0;
        }
        if (HP <= 0)
        {
            GameManager.falling = true;
            SoundPlay(deathSound);
            voices.Fallen();

            if (GetComponent<MegaManActions>() != null)
            {
                GetComponent<MegaManActions>().StartCoroutine("DeathAnimation");
            }

            StartCoroutine("LostLife");
        }
        else
        {
            SoundPlay(damageSound);
            this.transform.position = previousLandedPos;
        }
    }
    void Drown()
    {
        //溺れた
        SoundPlay(drownSound);

        if (GetComponent<MarioActions>() != null)
        {
            GetComponent<MarioActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<PacManActions>() != null)
        {
            GetComponent<PacManActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<MegaManActions>() != null)
        {
            GetComponent<MegaManActions>().StartCoroutine("DeathAnimation");
        }
        else if (GetComponent<SonicActions>() != null)
        {
            MusicManager.musicIndex = 2;
            GetComponent<SonicActions>().StartCoroutine("DrownAnimation");
        }

        HP = 0;
        StartCoroutine("LostLife");
    }

    bool areTheyPerished()
    {
        // GameManager.players[playerNumber].setStatus(playerId, maxHP, HP, lives, localScore);
        GameManager.players[playerNumber].setStatus(playerId, maxHP, HP, (uint)localScore);

        foreach (PlayerStatus player in GameManager.players)
        {
            if (player.HP > 0)
            {
                return false;
            }
        }
        return true;
    }
    public void LookFront()
    {
        GameObject cam = Camera.main.gameObject;
        //カメラの前方向
        Vector3 lookDir = -cam.transform.forward;
        lookDir.y = 0;
        lookDir = lookDir.normalized;

        skin.transform.forward = lookDir;
    }

    public void GotCookie(byte amounts)
    {
        cookiesAmount += amounts;
        while (cookiesAmount >= 50)
        {
            cookiesAmount -= 50;
            HPRestore(8);
        }
    }
    public void GotRing(byte amounts)
    {
        HPRestore(0.5f * amounts);
        // ringsAmount += amounts;
        // while (ringsAmount >= 2)
        // {
        //     ringsAmount -= 2;
        //     HPRestore(1);
        // }
    }
    public bool GotShroom(Vector3 itemPos, bool isMini = false)
    {
        if (HP == (int)(maxHP * hpRate))
        {
            if (
                (data.stockItems[0].amounts >= 1 && !isMini) ||
                (data.stockItems[0].amounts >= 1.75f && isMini)
            )
            {
                // scoreIncrease(-100, this.transform.position);
                scorePopUp(1000, false, itemPos);
                return true;
            }
            else
            {
                StartCoroutine(ItemStock(0, 1, isMini ? 2 : 1));
                return false;
            }
            // if (data.stockItems[0].amounts < 1) {
            //     StartCoroutine(ItemStock(0, 1));
            //     data.stockItems[0].amounts = 1;
            // }
        }
        return false;
    }
    public bool GotRestoreWP(byte value, Vector3 itemPos, bool isMini = false)
    {
        if (this.GetComponent<MegaManActions>() == null)
        {
            // scoreIncrease(-100, this.transform.position);
            // scorePopUp(1000, false, itemPos);
            return true;
        }
        else
        {
            return this.GetComponent<MegaManActions>().WPRestore(value, itemPos, isMini);
        }
        // if (this.GetComponent<MegaManActions>() != null) {
        //     this.GetComponent<MegaManActions>().WPRestore(value);
        // }
    }
    public bool GotETank(byte index, Vector3 itemPos)
    {
        if (index == 1 && this.GetComponent<MegaManActions>() == null)
        {
            //チームロックマン以外がＷ缶を獲得した時
            // scoreIncrease(-100, this.transform.position);
            scorePopUp(3000, false, itemPos);
            return true;
        }

        if (data.stockItems[index + 4].amounts == data.stockItems[index + 4].limit)
        {
            // scoreIncrease(-100, this.transform.position);
            scorePopUp(3000, false, itemPos);
            UseStockedTank((int)index, true);
            return true;
        }
        else
        {
            StartCoroutine(SpecialStock(index));
        }
        // if (data.stockItems[index + 4].amounts != data.stockItems[index + 4].limit) {
        //     StartCoroutine(SpecialStock(index));
        // }
        return false;
    }
    public bool GotYashichi(Vector3 itemPos)
    {
        MegaManActions mega = this.GetComponent<MegaManActions>();

        bool areAllWeaponsFull = true;
        if (mega != null)
        {
            foreach (float weaponEnergy in mega.weaponRemainsList)
            {
                if (weaponEnergy < 28 && weaponEnergy >= 0)
                {
                    areAllWeaponsFull = false;
                }
            }
        }

        if (HP == (int)(maxHP * hpRate) && areAllWeaponsFull)
        {
            // scoreIncrease(-100, this.transform.position);
            scorePopUp(3000, false, itemPos);
            return true;
        }
        else
        {
            UseStockedTank(2, true);
        }
        return false;
    }

    public bool PowerUpAnim(byte index, bool self = true)
    {
        if (partner != null && self)
        {
            partner.PowerUpAnim(index, false);
        }

        bool mega = this.GetComponent<MegaManActions>() != null;

        if (allPowerUp > 0)
        {
            index = (byte)allPowerUp;
        }

        if (this.GetComponent<_16MSonic>() != null)
        {
            return true;
        }

        if (powerUpActive == index)
        {
            if (powerUpActive == data.stockItems[2].amounts)
            {
                return true;
            }
            else
            {
                StartCoroutine(ItemStock(2, index));
            }
        }
        else if (powerUpActive > 0)
        {
            StartCoroutine(ItemStock(2, powerUpActive));
            if (mega)
            {
                powerUpActive = index;
            }
            else
            {
                StartCoroutine(PowerUp(index));
            }
        }
        else
        {
            if (mega)
            {
                powerUpActive = index;
            }
            else
            {
                StartCoroutine(PowerUp(index));
            }
        }
        return false;
    }
    IEnumerator PowerUp(byte index)
    {
        GameManager.timeScale = 0f;
        if (index == 7)
        {
            SoundPlay(landPowerUpSound);
        } else
        {
            SoundPlay(powerUpSound);
        }

        for (int i = 0; i < 3; i++)
        {
            powerUpActive = index;
            yield return new WaitForSecondsRealtime(0.1f);
            powerUpActive = 0;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        GameManager.timeScale = 1;
        powerUpActive = index;
    }

    IEnumerator PowerDown(byte index)
    {
        GameManager.timeScale = 0;
        if (index == 7)
        {
            SoundPlay(landPowerDownSound);
        } else
        {
            SoundPlay(powerDownSound);
        }

        for (int i = 0; i < 3; i++)
        {
            powerUpActive = 0;
            yield return new WaitForSecondsRealtime(0.1f);
            powerUpActive = index;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        GameManager.timeScale = 1;
        powerUpActive = 0;
    }

    public void GotShield(sbyte index, bool self = true)
    {
        if (partner != null && self)
        {
            partner.GotShield(index, false);
        }
        if (allShield > 0)
        {
            index = (sbyte)allShield;
        }

        SoundPlay(shieldManager.shieldSounds[index - 1]);

        if (shieldActive == index)
        {
            // if (data.stockItems[3].amounts == shieldActive) {
            //     scoreIncrease(-1000);
            //     scorePopUp(1000, false, this.transform.position);
            // } else {
            //     if (data.stockItems[3].amounts > 0) {
            //         scoreIncrease(-1000);
            //         scorePopUp(1000, false, this.transform.position);
            //     }
            //     StartCoroutine(ItemStock(3, index));
            // }
            if (data.stockItems[3].amounts != shieldActive)
            {
                // if (data.stockItems[3].amounts > 0) {
                //     scoreIncrease(-1000);
                //     scorePopUp(1000, false, this.transform.position);
                // }
                StartCoroutine(ItemStock(3, (byte)index));
            }
        }
        else if (shieldActive > 0)
        {
            // if (data.stockItems[3].amounts > 0) {
            //     scoreIncrease(-1000);
            //     scorePopUp(1000, false, this.transform.position);
            // }
            StartCoroutine(ItemStock(3, (byte)shieldActive));
        }
        shieldActive = index;
    }

    public void SetProof()
    {
        fireProof = shieldActive == 2;
        underwaterProof = shieldActive == 3;
        elecProof = shieldActive == 4;
        poisonProof = shieldActive == 5;
    }

    public void Breath()
    {
        // airAmount = maxAirAmount;
        airAmount = 24f;
        airCountdown = 5;

        if (playerType == 3)
        {
            rolling = false;
            VelocitySetUp(Vector3.zero);
        }
    }

    public IEnumerator PipeEnter(Vector3 pipePos, int exitIndex)
    {
        activeCollision = false;
        activePhysics = false;
        GetComponent<CapsuleCollider>().isTrigger = true;
        // GetComponent<CapsuleCollider>().enabled = false;
        canInput = false;
        Grounded = true;
        float keepY = pipePos.y;

        pipePos.y = this.transform.position.y;
        this.transform.position = pipePos;
        rb.position = pipePos;
        VelocitySetUp(Vector3.zero);

        SoundPlay(pipeSound);

        float pipeRate = 0f;

        while (pipeRate < 1f)
        {
            yield return null;
            pipeRate += Time.deltaTime;
            if (pipeRate > 1f)
            {
                pipeRate = 1f;
            }
            this.transform.position = Vector3.Lerp(pipePos, pipePos - Vector3.up * 6.4f, pipeRate);
            rb.position = this.transform.position;
        }
        float difference = this.transform.position.y - keepY;
        this.transform.position = pipePos - Vector3.up * 6.4f;
        rb.position = this.transform.position;

        yield return new WaitForSeconds(0.5f);

        Vector3 pipeBeginPos = Vector3.zero;

        // float difference = this.transform.position.y - keepY;
        WarpPipe[] pipes = FindObjectsByType<WarpPipe>(FindObjectsSortMode.None);
        foreach (WarpPipe pipe in pipes)
        {
            if (pipe.exitIndex == exitIndex)
            {
                pipeBeginPos = pipe.gameObject.transform.position + (Vector3.up * difference);
                this.transform.position = pipeBeginPos;
                rb.position = this.transform.position;
                break;
            }
        }

        SoundPlay(pipeSound);

        // for (int i = 0; i < 64; i++) {
        //     yield return null;
        //     this.transform.position += Vector3.up * 0.1f;
        // }
        pipeRate = 0f;

        while (pipeRate < 1f)
        {
            yield return null;
            pipeRate += Time.deltaTime;
            if (pipeRate > 1f)
            {
                pipeRate = 1f;
            }
            this.transform.position = Vector3.Lerp(pipeBeginPos, pipeBeginPos + Vector3.up * 6.4f, pipeRate);
        }
        this.transform.position = pipeBeginPos + Vector3.up * 6.4f;
        rb.position = this.transform.position;

        activeCollision = true;
        activePhysics = true;
        GetComponent<CapsuleCollider>().isTrigger = false;
        // GetComponent<CapsuleCollider>().enabled = true;
        Debug.Log(pipeBeginPos);
        canInput = true;
    }
    public IEnumerator ItemStock(int stockIndex, byte powerNo, int restoreTrigger = 0)
    {
        yield return new WaitForSeconds(0.3f);

        SoundPlay(stockSound);
        switch (restoreTrigger)
        {
            case 1:
                //回復アイテム（大）
                data.stockItems[stockIndex].amounts += 1f;
                break;

            case 2:
                //回復アイテム（小）
                data.stockItems[stockIndex].amounts += 0.25f;
                break;

            default:
                data.stockItems[stockIndex].amounts = powerNo;
                break;
        }
    }
    public void UseStockedItem(int stockIndex)
    {
        ItemBalloon balloon = Instantiate(itemBalloon, this.transform.position + Vector3.up * 5f, Quaternion.identity).GetComponent<ItemBalloon>();
        switch (stockIndex)
        {
            case 1:
                //アミーボ武器
                balloon.itemType = ItemBalloonType.RestoreWP;
                break;

            case 2:
                //マリオのパワーアップ
                balloon.itemType = (ItemBalloonType)(data.stockItems[2].amounts + 1);
                break;

            case 3:
                //ソニックのバリア
                balloon.itemType = (ItemBalloonType)(data.stockItems[3].amounts + 8);
                break;

            default:
                //回復アイテム
                balloon.itemType = ItemBalloonType.RestoreHP;
                break;
        }
        if (stockIndex < 2)
        {
            if (data.stockItems[stockIndex].amounts >= 1f)
            {
                data.stockItems[stockIndex].amounts -= 1f;
            }
            else
            {
                data.stockItems[stockIndex].amounts -= 0.25f;
                if (stockIndex == 1)
                {
                    balloon.itemType = ItemBalloonType.RestoreWPMini;
                }
                else
                {
                    balloon.itemType = ItemBalloonType.RestoreHPMini;
                }
            }
        }
        else
        {
            data.stockItems[stockIndex].amounts = 0;
        }
    }
    public void UseStockedTank(int stockIndex, bool gotYashichi = false)
    {
        MegaManActions mega = this.GetComponent<MegaManActions>();

        void RestoreMaxHP()
        {
            if (this.GetComponent<_16MSonic>() != null)
            {
                GameManager.Coins = 100;
            }
            else
            {
                HP = maxHP;
            }
        }
        bool isHPMax()
        {
            if (this.GetComponent<_16MSonic>() != null)
            {
                return GameManager.Coins > 0;
            }
            else
            {
                return HP == maxHP;
            }
        }

        bool areAllWeaponsFull = true;
        if (mega != null)
        {
            // foreach (SpecialWeapon weaponData in mega.weapons.weapons)
            // {
            //     if (weaponData.Energy < 28)
            //     {
            //         areAllWeaponsFull = false;
            //     }
            // }
            foreach (float weaponEnergy in mega.weaponRemainsList)
            {
                if (weaponEnergy < 28 && weaponEnergy >= 0)
                {
                    areAllWeaponsFull = false;
                }
            }
        }
        bool areTheyAllFull = isHPMax() && areAllWeaponsFull;

        switch (stockIndex)
        {
            case 0:
                //E缶（ＨＰを満タンにする）
                if (isHPMax())
                {
                    return;
                }
                RestoreMaxHP();
                break;

            case 1:
                //W缶（一つの武器エネルギーを満タンにする）
                if (mega != null)
                {
                    // //単一の武器エネルギーを満タンにする
                    // if (mega.weaponRemainsList[mega.weaponId] >= 28)
                    // {
                    //     return;
                    // }
                    // mega.WeaponRestore(28);

                    //全ての武器エネルギーを満タンにする
                    if (areAllWeaponsFull)
                    {
                        return;
                    }
                    mega.RestoreAllWeapons(28);
                }
                else
                {
                    return;
                }
                break;

            case 2:
                //M缶
                if (areTheyAllFull)
                {
                    if (gotYashichi)
                    {
                        return;
                    }
                    //敵を全て50コインにする
                    byte localCombo = 1;
                    foreach (EnemyManager enemy in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
                    {
                        if (enemy.ReplaceTo50Coins(this, localCombo))
                        {
                            // if ((localCombo >= 8 && !GameManager.extra) || (localCombo == 8 && GameManager.extra))
                            if (localCombo == 8)
                            {
                                OneUp();
                            }
                            if (GameManager.maxCombo < localCombo) GameManager.maxCombo = localCombo;
                            localCombo++;
                        }
                    }
                }
                else
                {
                    //（ＨＰを満タンにする）
                    RestoreMaxHP();

                    if (mega != null)
                    {
                        //全ての武器エネルギーを満タンにする
                        mega.RestoreAllWeapons(28);
                    }
                }
                break;
        }

        if (!gotYashichi)
        {
            if (data.stockItems[4 + stockIndex].amounts > 0)
            {
                data.stockItems[4 + stockIndex].amounts--;
            }
        }
    }

    public IEnumerator SpecialStock(byte stockIndex)
    {
        yield return new WaitForSeconds(0.3f);

        SoundPlay(stockSound);
        if (data.stockItems[stockIndex + 4].amounts < data.stockItems[stockIndex + 4].limit)
        {
            data.stockItems[stockIndex + 4].amounts++;
        }
    }

    public void GrindRailStart(PathCreator rail)
    {
        if (this.GetComponent<PlayerGrindRail>() != null)
        {
            PlayerGrindRail grindRail = this.GetComponent<PlayerGrindRail>();
            grindRail.rail = rail;
            grindRail.speed = XZmag;
            grindRail.enabled = true;
            axisInput = false;
            Grounded = true;
            activePhysics = false;
        }
    }

    public IEnumerator GoalPole(MarioGoal goal, bool isTop)
    {
        MusicManager.musicIndex = 2;

        canInput = false;
        VelocitySetUp(Vector3.zero);
        activeCollision = false;
        activePhysics = false;

        Vector3 goalPos = goal.gameObject.transform.position;
        float yPos = Math.Min(goal.upperPos, this.transform.position.y);

        Vector3 goalStartPos = new Vector3(goalPos.x, yPos, goalPos.z);
        Vector3 goalEndPos = new Vector3(goalPos.x, goal.bottomPos, goalPos.z);
        this.transform.position = goalStartPos;

        if (isTop)
        {
            yield return new WaitForSeconds(1.5f);
            goal.step = 2;
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        float maxLength = goal.upperPos - goal.bottomPos;
        float thisLength = yPos - goal.bottomPos;
        float lengthRate = Math.Min(1f, thisLength / maxLength);
        float goalRate = 0f;

        goal.PlayDownSound();

        while ((goalRate / lengthRate) < 1f)
        {
            goalRate += Time.deltaTime;
            if (goalRate > 1f)
            {
                goalRate = 1f;
            }
            goal.rate = goalRate;

            this.transform.position = Vector3.Lerp(goalStartPos, goalEndPos, (goalRate / lengthRate));

            yield return null;
        }

        goal.PlayFlagChangeSound(isTop);

        StartCoroutine(CourseCleared(0));
    }
    public IEnumerator CourseCleared(int index)
    {
        GameManager.CalcItemCollectedRate();

        if (index != 2)
        {
            canInput = false;
            ForwardSetUp(Vector3.zero, 0f);

            yield return new WaitForSeconds(1f);

            GameManager.stageClearIndex = index;
        } else
        {
            yield return null;
        }

        data.ResetCheckPoint();
        SavePlayerInfo();
        // SceneManager.LoadScene("ResultScreen");
    }

    public void SavePlayerInfo()
    {
        //ＨＰ
        if (this.GetComponent<_16MSonic>() == null)
        {
            data.HPs[playerNumber] = (uint)((HP == (int)(maxHP * hpRate)) ? 1000 : HP);
            if (HP > 0)
            {
                data.powerUps[playerNumber] = powerUpActive;
            }
        }
        data.lives = GameManager.Lives;
        //パワーアップ・バリア
        if (HP > 0)
        {
            data.shields[playerNumber] = shieldActive;
        }
        else
        {
            data.powerUps[playerNumber] = 0;
            data.shields[playerNumber] = 0;
        }
    }

    public void AllOutAttack()
    {
        // byte combo = 1;

        foreach (EnemyManager enemy in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
        {
            if (enemy.IsVisible())
            {
                enemy.TakeDamage(true, this, 10, 0, false, null, WeaponTypes.Bomb);
            }
        }
    }
    public void Swap(bool force = false)
    {
        if (partner != null)
        {
            if ((Vector3.Distance(partner.gameObject.transform.position, this.transform.position) > 12f || partner.HP <= 0) && !force)
            {
                SoundPlay(swapFailSound);
            } else
            {
                SoundPlay(swapSound);

                partner.primary = true;
                partner.localScore = localScore;
                
                primary = false;
                localScore = 0;
            }
        }
    }
    public void TeleportToPartner()
    {
        this.transform.position = partner.gameObject.transform.position - partner.skin.forward * 4f + Vector3.up * 30f;
        teleportedToPartner = true;
    }

    public void GetBadgeEffect()
    {
        speedUpByBadge = false;
        attackUp = false;
        defenseUp = false;
        allPowerUp = 0;
        allShield = 0;
        scoreUp = false;
        coinUp = false;
        autoEnergyTank = false;
        jetRun = false;
        superBoost = false;
        reduceBoost = false;
        awakenerChip = false;

        foreach (int badgeIndex in data.allBadges[playerNumber].badges)
        {
            float value = data.GetBadgeValue(badgeIndex);
            switch (data.GetBadgeEffect(badgeIndex))
            {
                case BadgeEffect.FloatingHighJump:
                break;

                case BadgeEffect.SpeedUp:
                speedUpByBadge = true;
                break;

                case BadgeEffect.AttackPlus:
                attackUp = true;
                break;

                case BadgeEffect.DefensePlus:
                defenseUp = true;
                break;

                case BadgeEffect.AllPowerUp:
                allPowerUp = (int)value;
                break;

                case BadgeEffect.AutoShield:
                if (shieldActive < 1)
                {
                    GotShield(1);
                }
                break;

                case BadgeEffect.AllShield:
                allShield = (int)value;
                break;

                case BadgeEffect.ScoreUp:
                scoreUp = true;
                break;

                case BadgeEffect.CoinUp:
                coinUp = true;
                break;

                case BadgeEffect.SafetyBounce:
                break;

                case BadgeEffect.Sensor:
                break;

                case BadgeEffect.Weakness:
                break;

                case BadgeEffect.SpikeBoots:
                break;

                case BadgeEffect.AutoRestore:
                autoEnergyTank = true;
                break;

                case BadgeEffect.Invisible:
                break;

                case BadgeEffect.DashSpeedUp:
                jetRun = true;
                break;

                case BadgeEffect.SuperBoost:
                superBoost = true;
                break;

                case BadgeEffect.SpringFeet:
                break;

                case BadgeEffect.ReduceBoost:
                reduceBoost = true;
                break;

                case BadgeEffect.AutoRings:
                GameManager.Coins +=  (uint)10;
                break;

                case BadgeEffect.EnergyBalancer:
                break;

                case BadgeEffect.EnergyBalancerNeo:
                break;

                case BadgeEffect.AutoChargeChip:
                break;

                case BadgeEffect.AwakenerChip:
                awakenerChip = true;
                break;
            }
        }
    }

    public IEnumerator Excellent()
    {
        yield return new WaitForSeconds(0.25f);

        voices.Excellent();
    }
}
