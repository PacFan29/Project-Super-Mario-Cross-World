using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _16MSonic : SonicActions
{
    public enum BoostMode {
        Normal,
        Frontiers
    }

    Vector3 contactNormal;
    private float boostAmount = 0f;
    private float jumpTime;
    [Header("ブースト")]
    public bool boost = false;
    private bool sliding = false;
    private bool boosted = false;
    public BoostMode boostMode = BoostMode.Normal;
    private bool boostDisabled = false;
    [Header("効果音")]
    public AudioClip spinSound;
    public AudioClip boostSound;
    public AudioClip homingSound;
    public AudioClip sweetSpotSound;
    // public AudioClip stompSound;
    // public AudioClip stompLandSound;
    public AudioClip quickStepSound;
    public AudioClip lightDashSound;
    public AudioClip slidingSound;
    public AudioClip driftingSound;
    // public AudioClip blueRingSound;
    public LoopingSoundManager lManager;
    public AudioClip[] ShieldSounds = new AudioClip[3];
    // [Header("リング散乱")]
    // public RingSpreader spreader;

    //ホーミングアタック
    private int homingActive = -1;
    private bool homing;
    private float homingTime;
    private float sweetSpotTime;
    private bool sweetSpot;
    // private uint sweetSpotCombo;
    private GameObject hTargetObj;
    //クイックステップ
    private float quickStepMag;
    private float qsSpeed;
    //ドリフト
    private int driftDirection;

    // private uint ringMemory;

    private int boundCount = 0;

    // [HideInInspector] public bool blueRing = false;
    /*
    ～ブーストの消費～
    ブースト：-10%
    エアーブースト：-20%
    ブースト中の1秒ごと：-5%（1ミリ秒ごと：-0.005%）

    ～ブーストの補充～
    リング：+1%
    ホーミングアタック時のスイートスポット：+5%
    敵：+20%（エネルギー × 4）
    ホワイトウィスプ：+30%

    ～ブーストの補充（100%突破可能）～
    トリック：+10%（エネルギー × 2）
    トリック終了：+10% × コンボ

    ホーミングアタック時のスイートスポット
    0.2秒～0.5秒
    */
    [Header("エフェクト")]
    public TrailRenderer homingEffect;
    public GameObject boostEffect;
    public GameObject stompingEffect;
    public GameObject fShieldDash;
    public GameObject tShieldJump;

    public bool trick;
    [Header("ホーミングターゲット")]
    public Animator hTargetAnim;
    [Header("トリック")]
    public GameObject trickUI;
    [HideInInspector] public SonicTrickManager trickManager;
    public AudioClip[] trickSuccessVoices;
    public AudioClip[] trickFailedVoices;

    void Awake() {
        trickManager = Instantiate(trickUI, this.transform.position, Quaternion.identity).GetComponent<SonicTrickManager>();
        trickManager.info = this.GetComponent<PlayerInfo>();

        if (boostMode == BoostMode.Frontiers) {
            boostAmount = 100f;
        }
    }
    void Update()
    {
        //声の出演：金丸 淳一

        if (isSuper && boostAmount < 100f) {
            boostAmount = 100f;
        }

        enabledRTSpinDash = false;

        if (trick) {
            actionId = 0;
            info.rolling = false;
            info.axisInput = false;
            info.groundEvent = false;
            homingTime = 0f;
        } else {
            Boost();
            HomingAttack();
            LightDash();

            if (info.Grounded) {
                if (info.dimension == DimensionType.Normal3D) {
                    QuickStep();
                    Drifting();
                } else {
                    if (Math.Abs(quickStepMag) > 0.5f) {
                        quickStepMag = 0;
                    }
                    if (actionId == 5) {
                        lManager.Stop();
                        info.axisInput = true;
                        canJump = true;
                        actionId = 0;
                    }
                }
                SpinDash();
            } else {
                WallJump();

                if (Math.Abs(quickStepMag) > 0.5f) {
                    quickStepMag = 0;
                }
                if (actionId == 5) {
                    lManager.Stop();
                    info.axisInput = true;
                    canJump = true;
                    actionId = 0;
                }
            }

            //共通アクションの実行
            actions();

            //スライディング
            if (info.GetCrouchButton("B") && info.Grounded && !sliding && !info.rolling && actionId != 5) {
                if (info.XZmag <= 0 && info.input != Vector3.zero) {
                    info.ForwardSetUp(Vector3.zero, 15f);
                    info.Crouching = false;
                }
                if (info.XZmag >= 2.5f) {
                    info.constantChange("rollfrc", info.RollFrc * 3);
                    info.rolling = true;
                    sliding = true;

                    lManager.SetUp(slidingSound, 1f, 2.595f);
                }
            } else if (sliding && ((!info.GetCrouchButton("B") && transform.up == Vector3.up) || !info.Grounded || info.Crouching)) {
                info.constantChange("rollfrc", info.RollFrc);
                if (info.Grounded || (!info.Grounded && info.finalVelocity.y <= 0)) info.rolling = false;
                sliding = false;

                lManager.Stop();
            }

            if (jumped) {
                info.rolling = false;
                jumpTime = 0.1f;
                jumped = false;
            }
            if (jumpTime > 0) {
                jumpTime -= Time.deltaTime;
                if (jumpTime <= 0 && info.Buttons["A"] && !info.Grounded) {
                    info.SoundPlay(spinSound);
                    info.rolling = true;
                }
            }
            // if (!info.Grounded && info.finalVelocity.y <= -25 && info.rolling && actionId == 0) {
            //     info.rolling = false;
            // }

            if (actionId == 2 && info.finalVelocity.y > 0 && false) {
                info.StopAllSounds();
                actionId = 0;
                info.groundAttack = false;
            }
            enabledStomping = (homingTime <= 0);
            // if (info.GetCrouchButtonDown("B") && !info.Grounded && actionId <= 0 && homingTime <= 0)
            // {
            //     //ストンピング
            //     actionId = 2;
            //     info.SoundPlay(stompSound);
            //     info.groundEvent = true;
            //     info.attacking = true;
            //     info.ForwardSetUp(Vector3.zero, 0f);
            //     info.rolling = false;
            //     info.YvelSetUp(-80f);
            //     info.groundAttack = true;
            // }
            // else if (info.Grounded && actionId == 2)
            // {
            //     actionId = 0;
            //     info.groundAttack = false;
            //     info.groundEvent = false;
            //     info.attacking = false;
            //     info.StopAllSounds();
            //     info.SoundPlay(stompLandSound);

            //     if (info.GroundNormal != Vector3.zero)
            //     {
            //         info.rolling = true;
            //         sliding = true;
            //     }
            // }
        }

        Effects();

        if (info != null && GameManager.players.Count > info.playerNumber){
            //プレイヤーIDを16に設定
            info.setPlayerId(16);
            // GameManager.players[info.playerNumber].setStatus(16, 100, (ushort)boostAmount, info.lives, info.localScore);
            GameManager.players[info.playerNumber].setStatus(16, 100, (uint)boostAmount, (uint)info.localScore);
        }

        if (GameManager.Coins > 0 && (info.HP > 0 && info.HP < info.maxHP)) {
            info.HP = info.maxHP;
        } else if (info.HP > 1 && GameManager.Coins <= 0) {
            info.HP = 1;
        }

        enableSpin = info.rolling && !sliding && (actionId < 2 || actionId == 4 || actionId == 5);
        Transform spinball = spinSkin.transform.GetChild(0);
        switch (actionId) {
            case 4:
            //スピンダッシュ
            spinball.localRotation = Quaternion.Euler(45, 0, 0);
            spinball.localScale = new Vector3(1f, 1.2f, 0.9f);
            break;

            case 5:
            //スピン中にドリフト
            spinball.localScale = new Vector3(1f, 1.2f, 0.9f);
            break;

            default:
            spinball.localRotation = Quaternion.Euler(0, 0, 0);
            spinball.localScale = Vector3.one;
            break;
        }

        // if (info.XZmag > 40f) {
        //     GameManager.timeRate = 1 - 0.5f * ((info.XZmag - 35f) / 45f);
        // } else {
        //     GameManager.timeRate = 1f;
        // }

        if (!info.canWallJump && info.lockWallCollision) {
            info.lockWallCollision = false;
        }
    }

    void SpinDash() {
        //万が一ブーストが不足した時のためのスピンダッシュ
        if (actionId == 4) {
            if (!info.GetCrouchButton("B")){
                actionId = 0;
                info.Crouching = false;
                info.StopAllSounds();
                CameraLock();
                voices.Attack();
                info.SoundPlay(spinDashReleaseSound);
                // float spinRelease = 40f + (float)(Math.Floor(spinrev) * 2.5f);
                float spinRelease = 50f + (float)(Math.Floor(spinrev) * 3.75f);
                info.ForwardSetUp(Vector3.zero, spinRelease);
            } else {
                // spinrev += 8 * Time.deltaTime;
                // if (spinrev > 8) spinrev = 8;
                if (info.ButtonsDown["A"]) {
                    info.StopAllSounds();
                    info.SoundPlay(spinDashStepSounds[spinstep]);
                    if (spinstep < 3)
                    {
                        spinstep++;
                    }
                    spinrev += Math.Min(2f, 8f - spinrev);
                }
            }
        }
        if (info.Crouching && info.ButtonsDown["A"] && actionId != 4) {
            //スピンダッシュ準備
            actionId = 4;
            info.Crouching = true;
            info.rolling = true;
            info.StopAllSounds();
            info.SoundPlay(spinDashSound);
            spinstep = 0;
            spinrev = 0f;
        }
    }
    void Boost() {
        /* ブースト */
        boostAmount = Mathf.Clamp(boostAmount, 0, 500);

        if (info.Grounded) {
            boosted = false;
        }

        // if (info.ButtonsDown["X"] && !boost && boostAmount > 0 && !boosted && !boostDisabled) {
        if (info.Axises["LT/RT"] >= 0.25f && !boost && boostAmount > 0 && !boosted && !boostDisabled) {
            CameraLock();
            boosted = true;
            voices.Attack();
            info.SoundPlay(boostSound);
            boostAmount -= info.reduceBoost ? 5f : 10f;

            info.constantSetUp();
            if (isSuper) {
                SuperConstantChange();

                info.constantChange("acc", S_Acceleration * 5);
                info.constantChange("frc", S_Acceleration * -5);
                // info.constantChange("top", 80);
                info.constantChange("top", info.superBoost ? 130 : 100);
                info.constantChange("airacc", S_AirAcc * 5);
            } else {
                info.constantChange("acc", info.Acceleration * 5);
                info.constantChange("frc", info.Acceleration * -5);
                // info.constantChange("top", 70);
                info.constantChange("top", info.superBoost ? 100 : 80);
                info.constantChange("airacc", info.AirAcc * 5);
            }

            if (info.XZmag < info.TopSpeed + 10f) info.ForwardSetUp(Vector3.zero, info.TopSpeed + 20f);
            float saveY = info.finalVelocity.y;
            if (!info.Grounded && info.finalVelocity.y < 20) {
                info.YvelSetUp(20f);
            } else {
                info.YvelSetUp(saveY);
            }
            
            boost = true;
            if (boostMode == BoostMode.Normal) {
                info.attacking = true;
            }

            // if (!isSuper) {
            //     if (info.Grounded) {
            //         //通常ブースト
            //         boostAmount -= 10f;
            //     } else {
            //         //エアーブースト
            //         boostAmount -= 20f;
            //     }
            // }
        // } else if (boost && (!info.Buttons["X"] || boostAmount <= 0 || !info.canInput)) {
        } else if (boost && (!(info.Axises["LT/RT"] >= 0.25f) || boostAmount <= 0 || !info.canInput)) {
            //ブースト解除
            info.constantSetUp();
            boost = false;
            info.attacking = false;

            if (boostAmount <= 0 && !boostDisabled && boostMode == BoostMode.Frontiers) {
                boostDisabled = true;
            }

            if (isSuper) {
                SuperConstantChange();
            }
        }

        if (boost && !isSuper) {
            //ブースト消費（１秒に5%ずつ）
            boostAmount -= (info.reduceBoost ? 2.5f : 5f) * Time.deltaTime;
        }
        if (!boost && boostMode == BoostMode.Frontiers && boostAmount < 100f) {
            //ブースト蓄積（フロンティア）
            boostAmount += 50 * Time.deltaTime;

            if (boostAmount > 100f) {
                boostAmount = 100f;
                boostDisabled = false;
            }
        }
    }
    void Drifting() {
        /* ドリフト */
        // if (Math.Abs(info.Axises["LT/RT"]) > 0 && Math.Abs(info.Axises["Horizontal"]) > 0 && actionId != 5 && info.XZmag > 30f) {
        if (info.Axises["LT/RT"] < 0 && Math.Abs(info.Axises["Horizontal"]) > 0 && actionId != 5 && info.XZmag > 30f) {
            actionId = 5;
            // driftDirection = (info.Axises["LT/RT"] > 0) ? 1 : -1;

            lManager.SetUp(driftingSound, 0.003f, 2.064f);
        }
        if (actionId == 5) {
            if (boostAmount < 100) {
                boostAmount += 10 * Time.deltaTime;

                if (boostAmount > 100) {
                    boostAmount = 100f;
                }
            }

            info.axisInput = false;
            canJump = false;

            driftDirection = (info.Axises["Horizontal"] > 0) ? 1 : -1;
            float control = (driftDirection == 1) ? ExtensionMethods.Remap(info.Axises["Horizontal"], -1, 1, 0, 2) : ExtensionMethods.Remap(info.Axises["Horizontal"], -1, 1, 2, 0);

            float steerControl = 55f * 0.0002f * Time.deltaTime;
            Vector3 turnVel = info.saveRotation * Vector3.right * Steer(driftDirection, 1 * control) * steerControl;

            spinSkin.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, Steer(driftDirection, 1 * control) * steerControl * -900f);

            info.ForwardSetUp((info.saveRotation * Vector3.forward + turnVel).normalized, info.XZmag);

            info.dustTrailEffect();

            if (Math.Abs(info.Axises["LT/RT"]) <= 0) {
                actionId = 0;
                info.axisInput = true;
                canJump = true;

                lManager.Stop();
            }
        }
    }
    float Steer(int direction, float amount) {
        return (80f * direction) * amount;
    }
    void HomingAttack() {
        /* ホーミングアタック */
        bool looking;
        GameObject[] homingTargets = GameObject.FindGameObjectsWithTag("HomingTarget");
        float enDistance;
        float MaxHDistance = 32;

        if (hTargetAnim.gameObject.activeInHierarchy) hTargetAnim.SetBool("SweetSpot", true);
        
        if (homing){
            if (hTargetObj == null) {
                looking = false;
            } else {
                looking = Vector3.Angle(info.skin.forward, (hTargetObj.transform.position - this.transform.position)) <= 90;
            }

            if (homingTime <= 0 && sweetSpotTime > 0) {
                sweetSpotTime -= Time.deltaTime;
                sweetSpot = (sweetSpotTime > 0 && sweetSpotTime <= 0.3f);
            }
            if (
                hTargetObj == null || info.Grounded || 
                info.canWallJump || !looking || 
                (hTargetObj.GetComponent<EnemyManager>() != null && !hTargetObj.GetComponent<EnemyManager>().isActive())
            ){
                homing = false;
            } else {
                if (Vector3.Distance(hTargetObj.transform.position, transform.position) > MaxHDistance){
                    homing = false;
                }
                info.target.transform.position = hTargetObj.transform.position;
            }
        } else {
            float minDis = Mathf.Infinity;
            for (int i = 0; i < homingTargets.Length; i++){
                //ターゲットの対象が空でないかつ、正常に動いていれば認識できる。
                looking = Vector3.Angle(info.skin.forward, (homingTargets[i].transform.position - this.transform.position)) <= 90;

                if (
                    homingTargets[i] != null && 
                    (
                        (homingTargets[i].GetComponent<EnemyManager>() != null && homingTargets[i].GetComponent<EnemyManager>().isActive()) ||
                        (homingTargets[i].GetComponent<BossManager>() != null && homingTargets[i].GetComponent<BossManager>().HP > 0) || 
                        (homingTargets[i].GetComponent<EnemyManager>() == null && homingTargets[i].GetComponent<BossManager>() == null)
                    )
                    && !info.canWallJump 
                    && looking
                ){
                    //プレイヤーから敵までの距離
                    enDistance = Vector3.Distance(homingTargets[i].transform.position, transform.position);
                    if (enDistance <= MaxHDistance && enDistance < minDis && !info.Grounded){
                        //最短距離でターゲットを認識する
                        sweetSpotTime = 0.5f;

                        minDis = enDistance;
                        hTargetObj = homingTargets[i];
                        homing = true;
                    }
                }
            }
        }
        info.target.SetActive(homing);

        if (!info.Grounded && !info.canWallJump) {
            if ((info.ButtonsDown["A"] || info.ButtonsDown["X"]) && ((homingActive == 0 && homing) || homingActive == 1)) {
                homingActive = 0;
                info.rolling = true;

                if (homing) {
                    info.SoundPlay(homingSound);
                    voices.Attack();
                    info.axisInput = false;
                    actionId = 1;
                    homingTime = 1f;
                    info.attacking = true;

                    if (sweetSpot) {
                        GameManager.IncreaseActionChain();
                        info.SoundPlay(sweetSpotSound);
                        BoostIncrease(2, false);
                        // sweetSpotCombo++;
                    }
                } else {
                    if (info.underwater) {
                        homingActive = 1;
                        info.SoundPlay(spinSound);
                        info.YvelSetUp(10f);
                    } else {
                        switch (info.shieldActive) {
                            case 2:
                            /* 火の玉ダッシュ（フレイムバリア） */
                            GameObject f_dash = Instantiate(fShieldDash, transform.position, Quaternion.identity, info.skinGroup.transform);
                            f_dash.transform.rotation = info.skin.transform.rotation * Quaternion.Euler(0, -90, 0);

                            info.SoundPlay(ShieldSounds[0]);

                            info.YvelSetUp(0);
                            // info.ForwardSetUp(Vector3.zero, Math.Max(50f, info.XZmag + 10f));
                            info.ForwardSetUp(Vector3.zero, Math.Min((info.XZmag / 2f) + 55f, 80f));
                            break;

                            case 3:
                            /* 連続バウンド（アクアバリア） */
                            info.groundAttack = true;
                            info.groundEvent = true;
                            // info.ForwardSetUp(Vector3.zero, 0);
                            info.YvelSetUp(-80f);
                            break;

                            case 4:
                            /* 二段ジャンプ（サンダーバリア） */
                            GameObject t_jump = Instantiate(tShieldJump, transform.position, Quaternion.identity);
                            if (GameManager.is3D()) {
                                //3Dの場合、X軸に90°回転する。
                                t_jump.transform.rotation = Quaternion.Euler(90, 0, 0);
                            } else if (GameManager.dimension == DimensionType.ZWay2D) {
                                t_jump.transform.rotation = Quaternion.Euler(0, 90, 0);
                            }

                            info.SoundPlay(ShieldSounds[2]);

                            info.YvelSetUp(40f);
                            break;
                            
                            case -1:
                            case 6:
                            /* 速度停止（フォースバリア） */
                            info.VelocitySetUp(Vector3.zero);
                            break;

                            case 7:
                            /* 起爆（アルマゲドンバリア） */
                            info.AllOutAttack();
                            info.shieldActive = 0;
                            break;

                            default:
                            info.SoundPlay(homingSound);
                            homingTime = 0.15f;
                            // info.ForwardSetUp(Vector3.zero, Math.Max(35f, (info.XZmag + 5f)));
                            info.ForwardSetUp(Vector3.zero, Math.Min((info.XZmag / 4f) + 35f, 50f));
                            info.YvelSetUp(0f);
                            break;
                        }
                    }
                }
            }
        }

        if (info.shieldActive == 3 && info.Grounded && info.groundEvent && actionId == 0) {
            info.groundAttack = false;
            Vector3 normal = info.GroundNormal * (50f + boundCount * 0.625f);
            if (boundCount < 5) {boundCount++;}

            info.SoundPlay(ShieldSounds[1]);
            info.VelocitySetUp(new Vector3(info.finalVelocity.x + normal.x, normal.y, info.finalVelocity.z + normal.z));
            info.groundEvent = false;

            info.rolling = true;

            homingActive = 1;
        }

        if (actionId == 1 && homingTime > 0) {
            homingTime -= Time.deltaTime;

            if (homingTime <= 0) {
                actionId = 0;
                info.groundEvent = false;
                info.axisInput = true;
                info.attacking = false;
                return;
            } else if (!info.rolling) {
                actionId = 0;
                homingActive = 1;
                homingTime = 0f;
                info.groundEvent = false;
                info.axisInput = true;
                info.attacking = false;
                return;
            }
            info.groundEvent = !(hTargetObj == null || info.attacked);
            if (!info.groundEvent) {
                homingActive = 1;
                homingTime = 0f;
                info.attacked = false;
                info.rolling = false;
                info.attacking = false;
                Vector3 velSet = info.finalVelocity * 0.5f;
                velSet.y = 42f;
                info.VelocitySetUp(velSet);
                actionId = 0;
                info.axisInput = true;
            } else {
                Vector3 aim = hTargetObj.transform.position - this.transform.position;
                info.skin.localRotation = Quaternion.LookRotation(aim);
                
                float homingSpeed = isSuper ? (sweetSpot ? 8000 : 6000) : (sweetSpot ? 6000 : 4500);
                info.VelocitySetUp(info.skin.forward * (homingSpeed * Time.fixedDeltaTime));
            }
        } else if (info.attacked) {
            info.attacked = false;
        } else if (actionId != 1 && homingTime > 0) {
            info.rolling = true;
            homingTime -= Time.deltaTime;
            info.YvelSetUp(0f);
            info.axisInput = false;

            if (homingTime <= 0) {
                info.axisInput = true;
                info.rolling = false;
            }
        }

        if (info.Grounded) {
            boundCount = 0;
            
            homingActive = -1;
            // if (sweetSpotCombo > 0) {
            //     TrickBonus(sweetSpotCombo);
            // }
            
            // sweetSpotCombo = 0;
        } else {
            if (!info.Grounded && !(info.Buttons["A"] || info.Buttons["X"]) && homingActive == -1) {
                homingActive = 1;
            }
        }
    }
    void QuickStep() {
        if (Math.Abs(quickStepMag) > 0.5f) {
            var sign = Math.Sign(quickStepMag);
            quickStepMag -= sign * Math.Min(Math.Abs(quickStepMag), qsSpeed / 1.5f) * 10f * Time.deltaTime;
            canJump = false;
        } else {
            canJump = true;
            qsSpeed = Math.Max(30f, info.XZmag * 2);
            qsSpeed = Math.Min(qsSpeed, 110);

            quickStepMag = 0;

            if (info.ButtonsDown["LB"]) {
                //左へクイックステップ
                info.SoundPlay(quickStepSound);
                quickStepMag = -qsSpeed;
            } else if (info.ButtonsDown["RB"]) {
                //右へクイックステップ
                info.SoundPlay(quickStepSound);
                quickStepMag = qsSpeed;
            }
        }
        info.extraVelocity = quickStepMag * info.skin.right;
    }

    void LightDash() {
        if (actionId == 0 && info.ButtonsDown["Y"] && GetClosestTarget(10f) != null) {
            //ライトダッシュ実行
            GameManager.IncreaseActionChain();
            info.SoundPlay(lightDashSound);
            info.axisInput = false;
            info.Grounded = false;
            info.rolling = false;
            actionId = 3;

            // ringMemory = GameManager.Coins;
        }
        if (actionId == 3) {
            if (GetClosestTarget(10f) == null) {
                //途切れた時
                actionId = 0;
                info.axisInput = true;
                // TrickBonus((uint)(GameManager.Coins - ringMemory) / 5);
            } else {
                Vector3 aim = GetClosestTarget(10f).transform.position - this.transform.position;
                info.skin.localRotation = Quaternion.LookRotation(aim);
                info.VelocitySetUp(info.skin.forward * (4000 * Time.fixedDeltaTime));
                GameManager.actionChainTime = 2f;
            }
        }
        // if (info.ButtonsDown["Y"] && GetClosestTarget(5f) != null) {
        //     info.SoundPlay(lightDashSound);
        //     info.axisInput = false;
        //     info.Grounded = false;
        //     info.rolling = false;
        //     actionId = 3;
        // }
        // if (actionId == 3) {
        //     if (GetClosestTarget(Mathf.Infinity) == null) {
        //         actionId = 0;
        //         info.VelocitySetUp(Vector3.zero);
        //         info.axisInput = true;
        //     } else {
        //         Vector3 aim = GetClosestTarget(Mathf.Infinity).transform.position - this.transform.position;
        //         info.skin.localRotation = Quaternion.LookRotation(aim);
        //         info.VelocitySetUp(info.skin.forward * (2000 * Time.fixedDeltaTime));
        //     }
        // }
    }
    void WallJump() {
        if (info.canWallJump) {
            info.constantChange("terminal", -30f);

            Vector3 pos = transform.position - contactNormal;
            Instantiate(info.dustTrail, pos, transform.rotation);

            if (info.ButtonsDown["A"]) {
                //壁キック
                info.ForwardSetUp(contactNormal, 35f);
                info.SoundPlay(info.jumpSound);
                info.YvelSetUp(info.JumpForce);
                homingActive = 1;
            }
        } else {
            info.constantChange("terminal", info.TerminalVelocity);
        }
    }

    GameObject GetClosestTarget(float maxDistance) {
        GameObject[] rings = GameObject.FindGameObjectsWithTag("Collectables");
        float ringDistance;
        float minDis = Mathf.Infinity;
        GameObject targetRing = null;

        for (int i = 0; i < rings.Length; i++) {
            ringDistance = Vector3.Distance(this.transform.position, rings[i].transform.position);
            bool looking = Vector3.Angle(info.skin.forward, (rings[i].transform.position - this.transform.position)) <= 90;

            if (ringDistance < maxDistance && ringDistance < minDis && looking) {
                minDis = ringDistance;
                targetRing = rings[i];
            }
        }

        return targetRing;
    }
    
    void Effects(){
        homingEffect.emitting = (homingTime > 0 || actionId == 3);


        var sdEmission = spindashEffect.emission;
        var sdShape = spindashEffect.shape;

        if (actionId == 4) {
            sdEmission.rateOverTime = 125f;
            Vector3 sdRotation = new Vector3(-15f, 180 + info.skin.eulerAngles.y, 0f);
            sdShape.rotation = sdRotation;
        } else {
            sdEmission.rateOverTime = 0f;
        }

        boostEffect.SetActive(boost && boostMode == BoostMode.Normal);
        boostEffect.transform.forward = info.finalVelocity.normalized;

        stompingEffect.SetActive(actionId == 2);
    }
    public void BoostIncrease(float increase, bool trick) {
        if (trick) {
            boostAmount += increase;
        } else {
            boostAmount += Math.Min(Math.Max(0, (100 - boostAmount)), increase);
        }
    }

    public void RingSpread() {
        if (GameManager.Coins > 0) {
            uint remained = (uint)Math.Floor(GameManager.Coins * Math.Min(0.2, ((double)GameManager.Coins / 1000)));
            if (GameManager.extra) remained = 0;
            
            if (info.defenseUp)
            {
                if (GameManager.extra)
                {
                    remained = GameManager.Coins >= 63 ? (uint)Math.Floor((float)GameManager.Coins * 0.2f) : Math.Max(0, GameManager.Coins - 50);
                } else
                {
                    remained = (uint)Math.Floor(GameManager.Coins * Math.Min(0.6, ((double)GameManager.Coins / 250)));
                }
            }

            if (info.options.difficulty == Options.Difficulty.Easy && GameManager.Coins > 20) {
                if (info.defenseUp)
                {
                    remained = Math.Max(0, GameManager.Coins - 10);
                } else
                {
                    remained = Math.Max(0, GameManager.Coins - 20);
                }
            }

            spreader.SpreadRings(GameManager.Coins - remained, blueRing, info.options.difficulty == Options.Difficulty.Easy);
            GameManager.Coins = remained;
            blueRing = false;
        }
    }

    // void TrickBonus(uint combo) {
    //     TrickBonusManager.player = info;
    //     TrickBonusManager.startBonus = (int)combo;
    // }

    /* 壁キック前の設定 */
    void OnCollisionStay (Collision hit)
    {
        if (info.isGroundLayerC(hit) && transform.up == Vector3.up) {
            ContactPoint contact = hit.contacts[0];
            if (!info.Grounded && contact.normal.y < 0.1f && !info.canWallJump && actionId == 0 && info.XZmag >= 12.5) {
                if (Vector3.Angle(info.input, contact.normal) >= 135f) {
                    Debug.DrawRay(contact.point, contact.normal, Color.red, 1.25f);
                    contactNormal = contact.normal;
                    if (!info.canWallJump) {
                        info.rolling = false;
                        info.ForwardSetUp(-contact.normal, 1f);
                        //info.VelocitySetUp(Vector3.zero);
                    }
                    info.lockWallCollision = true;
                    info.canWallJump = true;
                }
            }
        }
    }
    void OnCollisionExit(Collision hit) {
        if (info.isGroundLayerC(hit) || (info.canWallJump && Vector3.Angle(info.input, contactNormal) < 135f)) {
            info.canWallJump = false;
        }
    }

    // public void ActiveBlueRing() {
    //     info.SoundPlay(blueRingSound);
    //     blueRing = true;
    // }

    public void TrickSuccessVoice()
    {
        voices.VoicePlay(trickSuccessVoices);
    }
    public void TrickFailedVoice()
    {
        voices.VoicePlay(trickFailedVoices);
    }
}
