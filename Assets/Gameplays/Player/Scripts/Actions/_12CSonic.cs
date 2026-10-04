using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ability {
    HomingDropDash,
	Mania,
	CD,
	ThreeAndKnuckles,
	Mix,
    Homing
}
public class _12CSonic : SonicActions
{
    private Dictionary<string, float> DropdashC = new Dictionary<string, float>() {
        // {"drpspd", 40f},
        // {"drpmax", 60f},
        // {"drpspdsup", 60f},
        // {"drpmaxsup", 65f},
        {"drpspd", 50f},
        {"drpmax", 80f},
        {"drpspdsup", 70f},
        {"drpmaxsup", 90f},
        {"drptime", 0f},
        {"drpmaxtime", 0.334f}
    };
    private Dictionary<string, bool> DropdashP = new Dictionary<string, bool>() {
        {"prepare", false},
        {"ready", false}
    };
    [Header("エフェクト")]
    public GameObject dropdashEffect;
    public GameObject instashieldEffect;
    public GameObject fShieldDash;
    public GameObject tShieldJump;

    [Header("効果音")]
    public AudioClip rollSound;
    public AudioClip dropDashSound;
    public AudioClip peeloutSound;
    public AudioClip peeloutReleaseSound;
    public AudioClip instashieldSound;
    public AudioClip homingSound;
    public AudioClip[] ShieldSounds = new AudioClip[3];
    public AudioClip chargeSound;
    [Header("技")]
    public Ability ability;
    private Dictionary<string, bool> abilityCheck = new Dictionary<string, bool>() {
        {"DropDash", true},
        {"SuperPeelout", false},
        {"InstaShield", false}
    };
    private bool lookedUp;
    private bool homing;
    private float homingTime;
    private GameObject hTargetObj;

    private float poisonCharge = 0f;
    
    void Update()
    {
        //共通アクションの実行
        actions();

        //ドロップダッシュの有効
        abilityCheck["DropDash"] = (ability == Ability.HomingDropDash || ability == Ability.Mania || ability == Ability.Mix);
        //チャージダッシュの有効
        abilityCheck["SuperPeelout"] = (ability == Ability.CD || ability == Ability.Mix);
        //ダブル回転アタックの有効
        abilityCheck["InstaShield"] = (ability == Ability.ThreeAndKnuckles || ability == Ability.Mix);
        //ホーミングアタックの有効
        if (ability == Ability.HomingDropDash || ability == Ability.Homing) HomingAttack();

        if (info != null){
            //プレイヤーIDを12に設定
            info.setPlayerId(12);
        }

        enabledRTSpinDash = false;

        if (jumped) {
            if (!info.grabbing)
            {
                info.rolling = true;
            }
            jumped = false;
        }

        if (info.XZmag >= 2.5f && info.GetCrouchButton("RB") && !info.rolling && info.Grounded) {
            //転がる
            info.SoundPlay(rollSound);
            info.rolling = true;
        }
        if (info.GetCrouchButton("RB") && !info.rolling && !info.Grounded) {
            //転がる
            if (!info.grabbing)
            {
                info.rolling = true;
            }
        }
        if (!info.GetCrouchButton("RB") && info.rolling && info.Grounded && !spinDashed) {
            info.rolling = false;
        }
        if (
            // ((info.Grounded && info.ButtonsDown["Y"] && ((!abilityCheck["SuperPeelout"] && !info.rolling) || abilityCheck["SuperPeelout"])) ||
            ((info.Grounded && info.Axises["LT/RT"] >= 0.25f && ((!abilityCheck["SuperPeelout"] && !info.rolling) || abilityCheck["SuperPeelout"])) ||
            (info.LookingUp && info.ButtonsDown["A"] && abilityCheck["SuperPeelout"])) && 
            actionId != 3 && 
            !DropdashP["ready"]
        ) {
            //スピンダッシュ準備
            lookedUp = info.ButtonsDown["A"];
            
            info.VelocitySetUp(Vector3.zero);
            actionId = 3;
            info.StopAllSounds();
            if (abilityCheck["SuperPeelout"]) {
                info.SoundPlay(peeloutSound);
            } else {
                info.SoundPlay(spinDashSound);
            }
            spinrev = 0f;
        }

        if (!info.Grounded && !info.rolling && info.ButtonsDown["A"]) {
            if (!info.grabbing)
            {
                info.rolling = true;
            }
        }

        switch (actionId) {
            case 3:
            //チャージダッシュ
            canJump = false;

            info.rolling = !abilityCheck["SuperPeelout"];
            info.Crouching = true;
            
            spinrev += 16 * Time.deltaTime;
            if (spinrev > 8) spinrev = 8f;

            if (
                (!(info.Axises["LT/RT"] >= 0.25f) && !abilityCheck["SuperPeelout"]) ||
                (!lookedUp && !(info.Axises["LT/RT"] >= 0.25f) && abilityCheck["SuperPeelout"] && spinrev >= 8f) ||
                (lookedUp && !info.LookingUp && abilityCheck["SuperPeelout"] && spinrev >= 8f)
            ) {
                canJump = true;

                actionId = 0;
                info.StopAllSounds();
                CameraLock();
                voices.Attack();
                if (abilityCheck["SuperPeelout"]) {
                    info.SoundPlay(peeloutReleaseSound);
                } else {
                    info.SoundPlay(spinDashReleaseSound);
                }
                // float spinRelease = 40f + (float)(Math.Floor(spinrev) * 2.5f);
                float spinRelease = 50f + (float)(Math.Floor(spinrev) * 3.75f);

                spinDashed = true;
                info.ForwardSetUp(Vector3.zero, spinRelease);
            } else if (
                ((!lookedUp && !(info.Axises["LT/RT"] >= 0.25f)) ||
                (lookedUp && !info.LookingUp)) && abilityCheck["SuperPeelout"] && spinrev < 8f
            ) {
                canJump = true;

                actionId = 0;
                info.StopAllSounds();
            }

            var sdEmission = spindashEffect.emission;
            sdEmission.rateOverTime = 125f;
            spindashEffect.transform.position = this.transform.position - new Vector3(0, 1.2f, 0f);
            break;

            case 4:
            //ホーミングアタック
            homingTime -= Time.deltaTime;
            if (!info.rolling || homingTime <= 0) {
                actionId = 0;
                homingTime = 0f;
                info.groundEvent = false;
                info.axisInput = true;
                info.attacking = false;
                return;
            }

            info.groundEvent = !(hTargetObj == null || info.attacked);
            if (!info.groundEvent) {
                info.axisInput = true;
                info.attacked = false;
                info.attacking = false;
                Vector3 velSet = info.finalVelocity * 0.5f;
                velSet.y = 42f;
                info.VelocitySetUp(velSet);
                actionId = 0;
            } else {
                Vector3 aim = hTargetObj.transform.position - this.transform.position;
                info.skin.localRotation = Quaternion.LookRotation(aim);

                float homingSpeed = isSuper ? 6000f : 4500f;
                info.VelocitySetUp(info.skin.forward * (homingSpeed * Time.fixedDeltaTime));
            }
            break;
        }

        if (jumpAction > 0 && !info.grabbing && !info.giant) {
            if (abilityCheck["DropDash"] && !homing) {
                /* ドロップダッシュ */ 
                if (
                    // (AorYDown() && !info.Grounded && !DropdashP["prepare"]) ||
                    // (AorY() && !info.Grounded && DropdashC["drptime"] > 0f && DropdashP["prepare"])
                    (((info.ButtonsDown["A"] && info.shieldActive < 2) || info.Axises["LT/RT"] >= 0.25f) && !info.Grounded && !DropdashP["prepare"]) ||
                    (((info.Buttons["A"] && info.shieldActive < 2) || info.Axises["LT/RT"] >= 0.25f) && !info.Grounded && DropdashC["drptime"] > 0f && DropdashP["prepare"])
                ) {
                    //チャージ
                    info.rolling = true;
                    jumpAction = 1;

                    DropdashP["prepare"] = true;
                    DropdashC["drptime"] -= Time.deltaTime;
                    if (DropdashC["drptime"] <= 0 && !DropdashP["ready"]) {
                        DropdashP["ready"] = true;
                        info.groundEvent = true;
                        info.SoundPlay(dropDashSound);
                    }
                }
                if (DropdashC["drptime"] > -1 && DropdashP["prepare"]) {
                    if ((!info.Buttons["A"] && info.shieldActive < 2 && !(info.Axises["LT/RT"] >= 0.25f)) || actionId != 0) {
                        //キャンセル
                        Debug.Log("Cancel");
                        info.groundEvent = false;
                        jumpAction = 0;
                        DropdashP["ready"] = false;
                    }
                }
            }
            if (info.Grounded) {
                jumpAction = -1;

                if (DropdashP["ready"]) {
                    //ドロップダッシュ実行
                    spinDashed = true;
                    info.rolling = true;
                    info.groundEvent = false;
                    info.StopAllSounds();
                    CameraLock();
                    info.SoundPlay(spinDashReleaseSound);
                    float dropRelease;

                    float dropSpeed;
                    float dropMax;

                    if (isSuper) {
                        dropSpeed = DropdashC["drpspdsup"];
                        dropMax = DropdashC["drpmaxsup"];
                    } else {
                        dropSpeed = DropdashC["drpspd"];
                        dropMax = DropdashC["drpmax"];
                    }

                    Vector3 pos = transform.position - (transform.up * 1.2f);
                    GameObject d_dash = Instantiate(dropdashEffect, pos, transform.rotation);
                    d_dash.transform.rotation *= info.skin.transform.rotation;

                    if (info.skidding) {
                        if (info.GroundNormal == Vector3.up) {
                            //後方へダッシュ
                            dropRelease = -dropSpeed;
                        } else {
                            //坂かよ・・・
                            dropRelease = Math.Min((info.XZmag / 2f) + dropSpeed, dropMax);
                        }
                    } else {
                        //前方へダッシュ
                        dropRelease = Math.Min((info.XZmag / 4f) + dropSpeed, dropMax);
                    }
                    info.ForwardSetUp(Vector3.zero, dropRelease);
                }
            }
            if (info.ButtonsDown["A"] && DropdashC["drptime"] > -1 && !homing) {
                info.rolling = true;

                switch (info.shieldActive) {
                    case 0:
                    case 1:
                    if (abilityCheck["InstaShield"]){
                        /* ダブル回転アタック */
                        info.SoundPlay(instashieldSound);
                        StartCoroutine("InstaShield");
                        if (!abilityCheck["DropDash"]) jumpAction = 0;
                    }
                    break;

                    case 2:
                    /* 火の玉ダッシュ（フレイムバリア） */
                    GameObject f_dash = Instantiate(fShieldDash, transform.position, Quaternion.identity, info.skinGroup.transform);
                    f_dash.transform.rotation = info.skin.transform.rotation * Quaternion.Euler(0, -90, 0);

                    info.SoundPlay(ShieldSounds[0]);

                    info.YvelSetUp(0);
                    // info.ForwardSetUp(Vector3.zero, Math.Max(40f, info.XZmag + 10f));
                    info.ForwardSetUp(Vector3.zero, Math.Min((info.XZmag / 2f) + 50f, 80f));
                    jumpAction = 0;
                    break;

                    case 3:
                    /* 連続バウンド（アクアバリア） */
                    info.groundAttack = true;
                    info.groundEvent = true;
                    // info.ForwardSetUp(Vector3.zero, 0);
                    info.ForwardSetUp(Vector3.zero, info.XZmag / 2f);
                    info.YvelSetUp(-40f);
                    info.SoundPlay(ShieldSounds[1]);
                    jumpAction = 0;
                    bubbleShieldDrop = true;
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

                    // info.YvelSetUp(27.5f);
                    info.YvelSetUp(36f);
                    jumpAction = 0;
                    break;

                    case 5:
                    /* バースト（ポイズンバリア） */
                    info.SoundPlay(chargeSound);
                    poisonCharge = 0.01f;
                    // info.VelocitySetUp(Vector3.zero);
                    jumpAction = 0;
                    break;

                    case -1:
                    case 6:
                    /* 速度停止（フォースバリア） */
                    info.SoundPlay(ShieldSounds[4]);
                    info.VelocitySetUp(Vector3.zero);
                    jumpAction = 0;
                    break;

                    case 7:
                    /* 起爆（アルマゲドンバリア） */
                    info.AllOutAttack();
                    info.shieldActive = 0;
                    jumpAction = 0;
                    break;
                }
            }

            if (DropdashC["drptime"] <= -1) {
                //ドロップダッシュ初期化
                DropdashC["drptime"] = DropdashC["drpmaxtime"];
                DropdashP["prepare"] = false;
            }
        } else {
            DropdashC["drptime"] = -1f;
            DropdashP["ready"] = false;
            DropdashP["prepare"] = true;
        }

        if (info.shieldActive == 3 && info.Grounded && info.groundEvent) {
            info.groundAttack = false;
            // Vector3 normal = info.GroundNormal * 37.5f;
            Vector3 normal = info.GroundNormal * 50f;
            bubbleShieldDrop = false;

            info.SoundPlay(ShieldSounds[1]);
            info.VelocitySetUp(new Vector3(info.finalVelocity.x + normal.x, normal.y, info.finalVelocity.z + normal.z));
            info.groundEvent = false;

            info.rolling = true;

            jumpAction = 1;
        }

        if (info.attacked) info.attacked = false;

        if (poisonCharge > 0)
        {
            poisonCharge += Time.deltaTime;
            if (poisonCharge > 1f)
            {
                poisonCharge = 1f;
            }
            info.YvelSetUp(-2f);
            if (!info.Buttons["A"])
            {
                info.StopAllSounds();
                info.SoundPlay(ShieldSounds[3]);
                info.YvelSetUp(50f + poisonCharge * 30f);
                poisonCharge = 0f;
                PoisonSplash();
            }
        }


        enableSpin = (DropdashP["ready"] || actionId == 1 || (actionId == 3 && !abilityCheck["SuperPeelout"]));
        spinSkin.GetComponent<SpindashBall>().isSpindash = (actionId == 1 || actionId == 3);
    }

    void HomingAttack() {
        bool looking;
        GameObject[] homingTargets = GameObject.FindGameObjectsWithTag("HomingTarget");
        float enDistance;
        float MaxHDistance = 24;

        if (info.grabbing)
        {
            homing = false;
            return;
        }
        
        if (homing){
            if (hTargetObj == null) {
                looking = false;
            } else {
                looking = Vector3.Angle(info.skin.forward, (hTargetObj.transform.position - this.transform.position)) <= 90;
            }

            if (hTargetObj == null || info.Grounded || !looking){
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
                    && looking
                ){
                    //プレイヤーから敵までの距離
                    enDistance = Vector3.Distance(homingTargets[i].transform.position, transform.position);
                    if (enDistance <= MaxHDistance && enDistance < minDis && !info.Grounded){
                        //最短距離でターゲットを認識する
                        minDis = enDistance;
                        hTargetObj = homingTargets[i];
                        homing = true;
                    }
                }
            }
        }
        info.target.SetActive(homing);

        if (!info.Grounded) {
            if (info.ButtonsDown["A"] && DropdashC["drptime"] > -1) {
                info.rolling = true;

                if (homing) {
                    info.SoundPlay(homingSound);
                    info.attacking = true;
                    info.axisInput = false;
                    homingTime = 1f;
                    actionId = 4;
                } else {
                    if (info.shieldActive < 2 && !abilityCheck["DropDash"] && !abilityCheck["InstaShield"]) {
                        info.SoundPlay(homingSound);
                        info.ForwardSetUp(Vector3.zero, Math.Max(35f, info.XZmag + 5f));
                        info.YvelSetUp(0f);
                        jumpAction = 0;
                    }
                }
            }
        }
    }

    IEnumerator InstaShield() {
        info.disableDamage = true;
        instashieldEffect.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        instashieldEffect.SetActive(false);
        info.disableDamage = false;
    }
}
