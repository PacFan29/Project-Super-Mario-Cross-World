using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SonicActions : CommonActions
{
    [Header("速度定数（スーパー）")]
    public float S_Acceleration = 0.9375f;
    public float S_Deceleration = 5f;
    public float S_TopSpeed = 50f;
    
    [Header("空中速度定数（スーパー）")]
    public float S_AirAcc = 1.875f;
    public float S_JumpForce = 40f;

    [Header("速度定数（スーパー、水中）")]
    public float S_U_Acceleration = 0.46875f;
    public float S_U_Deceleration = 2.5f;
    public float S_U_TopSpeed = 25f;
    
    [Header("空中速度定数（スーパー、水中）")]
    public float S_U_AirAcc = 0.9375f;
    public float S_U_JumpForce = 17.5f;

    [Header("スキン")]
    public GameObject normalSkin;
    public GameObject spinSkin;
    protected bool enableSpin = false;
    [Header("スーパー状態かどうか")]
    public bool isSuper;
    private bool superPrevious;
    private float superTime;

    //操作等
    protected bool inputA;
    protected bool canJump = true;
    protected bool jumped = false;
    protected sbyte jumpAction = -1;

    [HideInInspector] public bool bubbleShieldDrop = false;

    [Header("摩擦")]
    public GameObject frictionWeapon;
    private float fricInterval = 0f;


    [Header("共通エフェクト")]
    public ParticleSystem spindashEffect;
    [Header("共通効果音")]
    public AudioClip spinDashSound;
    public AudioClip[] spinDashStepSounds;
    public AudioClip spinDashReleaseSound;
    public AudioClip stompSound;
    public AudioClip stompLandSound;

    protected int spinstep = 0;
    protected float spinrev = 0f;
    protected bool spinDashed = false;
    protected bool enabledStomping = true;
    protected bool enabledRTSpinDash = true;
    [Header("毒しぶき（ポイズンバリア取得時のみ）")]
    public GameObject poisonBurst;
    
    void Start()
    {
        info = GetComponent<PlayerInfo>();
        info.Stompable = false;
        initialStompable = false;
        info.playerType = 3;
        info.desacelerar = false;

        voices = GetComponent<PlayerVoiceManager>();
    }

    void FixedUpdate() {
        if (this.GetComponent<PlayerAnimManager>() != null) {
            this.GetComponent<PlayerAnimManager>().skin.speed = 1 + (info.Grounded ? (info.XZmag / 30f) : 0);
        }

        normalSkin.SetActive(!enableSpin);
        spinSkin.SetActive(enableSpin);
        // if (info.XZmag >= 60) {
        //     info.scorePopUp((int)info.XZmag - 59, this.transform.position);
        // }
    }

    public void actions()
    {
        enabledDash = false;
        enabledGroundPound = false;
        enabledFireball = true;
        enabledToSwim = false;
        gravityControl = true;
        commonActions();

        var sdEmission = spindashEffect.emission;
        var sdShape = spindashEffect.shape;

        sdEmission.rateOverTime = 0f;
        Vector3 sdRotation = new Vector3(-15f, 180 + info.skin.eulerAngles.y, 0f);
        sdShape.rotation = sdRotation;

        bool crouchButton = info.GetCrouchButton("RB") || info.GetCrouchButton("B");

        // if (info.XZmag <= 1f && (info.GetCrouchButton("RB") || info.GetCrouchButton("B")) && info.Grounded && !info.Crouching && !info.rolling)
        if (info.XZmag <= 1f && crouchButton && info.Grounded && !info.Crouching && !info.rolling)
        {
            info.Crouching = true;
        }
        // else if ((!info.GetCrouchButton("RB") && !info.GetCrouchButton("B")) && info.Grounded && info.Crouching)
        else if (!crouchButton && info.Grounded && info.Crouching)
        {
            info.Crouching = false;
        }

        if (info.ButtonsDown["A"] && info.Grounded && !info.Crouching && canJump)
        {
            info.Crouching = false;
            inputA = true;
            jumped = true;
            info.Jump();

            voices.Jump();
        }
        if (inputA && info.finalVelocity.y > 0)
        {
            //ジャンプの勢い
            float jumpReleaseStrength = 0f;
            if (info.underwater)
            {
                //水中
                jumpReleaseStrength = 10f;
            }
            else
            {
                //通常
                jumpReleaseStrength = 20f;
            }
            //Aを押し続けるとより高く跳べる。
            if (info.finalVelocity.y > jumpReleaseStrength && info.ButtonsUp["A"])
            {
                //途中でAを離したとき
                // info.YvelSetUp(jumpReleaseStrength);
                inputA = false;
            }
            else if (info.finalVelocity.y <= jumpReleaseStrength)
            {
                //高く跳びきったとき
                inputA = false;
            }
        }
        if (!info.Grounded && !info.Buttons["A"] && jumpAction == -1)
        {
            jumpAction = 1;
        }
        else if ((info.Grounded || !info.rolling) && jumpAction == 0)
        {
            jumpAction = -1;
        }

        if (info.Crouching || info.rolling)
        {
            this.GetComponent<CapsuleCollider>().height = 2.4f;
        }
        else
        {
            // this.GetComponent<CapsuleCollider>().height = 3.5f;
            this.GetComponent<CapsuleCollider>().height = info.InitialColliderHeight;
        }

        this.GetComponent<PlayerAnimManager>().actionId = actionId;

        if (info.underwater != underwaterPrevious)
        {
            if (info.underwater)
            {
                Vector3 setvel = info.finalVelocity / 2f;
                setvel.y /= 2f;
                info.VelocitySetUp(setvel);
            }
            else
            {
                info.YvelSetUp(info.finalVelocity.y * 2);
            }
        }
        underwaterPrevious = info.underwater;

        //摩擦
        if (info.rolling && info.Grounded && info.XZmag > 0f && info.powerUpActive > 0 && info.powerUpActive <= 3)
        {
            fricInterval += Time.deltaTime;
            if (fricInterval >= 0.1f)
            {
                fricInterval = 0f;
                FrictionWeapons fr = Instantiate(frictionWeapon, transform.position + (transform.up * -0.3f), this.transform.rotation).GetComponent<FrictionWeapons>();
                fr.index = info.powerUpActive - 1;
                fr.player = info;
            }
        }

        //スピンダッシュ
        if (this.GetComponent<_16MSonic>() == null)
        {
            if (actionId == 1)
            {
                canJump = false;

                spinrev -= ((float)Math.Floor(spinrev / 0.125f) / 256);
                if (info.ButtonsDown["A"])
                {
                    info.StopAllSounds();
                    info.SoundPlay(spinDashStepSounds[spinstep]);
                    if (spinstep < 3)
                    {
                        spinstep++;
                    }
                    spinrev += Math.Min(2f, 8f - spinrev);
                }
                else if (!info.GetCrouchButton("RB"))
                {
                    canJump = true;

                    actionId = 0;
                    info.StopAllSounds();
                    CameraLock();
                    voices.Attack();
                    info.SoundPlay(spinDashReleaseSound);
                    // float spinRelease = 40f + (float)(Math.Floor(spinrev) * 2.5f);
                    float spinRelease = 50f + (float)(Math.Floor(spinrev) * 3.75f);
                    info.ForwardSetUp(Vector3.zero, spinRelease);

                    info.rolling = true;
                    spinDashed = true;
                }

                sdEmission.rateOverTime = 125f;
            }
            if (info.Crouching && info.Grounded && info.ButtonsDown["A"] && actionId != 1 && !info.grabbing)
            {
                //スピンダッシュ準備
                actionId = 1;
                info.rolling = true;
                info.StopAllSounds();
                info.SoundPlay(spinDashSound);
                spinstep = 0;
                spinrev = 0f;
            }
        }
        if ((!info.Grounded || !info.rolling) && spinDashed)
        {
            spinDashed = false;
        }

        //長押し式スピンダッシュ
        if (enabledRTSpinDash)
        {
            if (info.Grounded && info.Axises["LT/RT"] >= 0.25f && !info.rolling && actionId != 3) {
                //スピンダッシュ準備
                info.VelocitySetUp(Vector3.zero);
                actionId = 3;
                info.StopAllSounds();
                info.SoundPlay(spinDashSound);
                spinrev = 0f;
            }
            if (actionId == 3)
            {
                canJump = false;

                info.rolling = true;
                info.Crouching = true;
                
                spinrev += 16 * Time.deltaTime;
                if (spinrev > 8) spinrev = 8f;

                if (!(info.Axises["LT/RT"] >= 0.25f)) {
                    canJump = true;

                    actionId = 0;
                    info.StopAllSounds();
                    CameraLock();
                    voices.Attack();
                    info.SoundPlay(spinDashReleaseSound);
                    float spinRelease = 50f + (float)(Math.Floor(spinrev) * 3.75f);

                    spinDashed = true;
                    info.ForwardSetUp(Vector3.zero, spinRelease);
                }

                sdEmission.rateOverTime = 125f;
                spindashEffect.transform.position = this.transform.position - new Vector3(0, 1.2f, 0f);
            }
        }

        //スーパー
        if (!info.Grounded && info.ButtonsDown["Y"] && !isSuper && false && GameManager.Coins >= 50)
        {
            isSuper = true;
        }

        if (isSuper)
        {
            if (!superPrevious)
            {
                SuperConstantChange();
            }

            superTime += Time.deltaTime;
            if (superTime >= 1f)
            {
                GameManager.Coins--;
                if (info.playerId == 16) info.scoreIncrease(100, this.transform.position);
                superTime -= 1f;
            }

            if (GameManager.Coins <= 0)
            {
                isSuper = false;
                info.constantSetUp();
            }

            info.speedUp = false;
            info.invincible = true;
        }
        else
        {
            if (superPrevious)
            {
                info.constantSetUp();
            }
            superTime = 0f;
        }
        superPrevious = isSuper;

        enabledFireball = (actionId == 0);

        // if (info.ButtonsDown["X"] && info.powerUpActive == 1 && actionId == 0) {
        //     info.SoundPlay(throwFireball);

        //     FireBall fire = Instantiate(info.fireBall, transform.position + info.skin.forward, info.skin.rotation).GetComponent<FireBall>();
        //     fire.player = info;
        // }
        // if (info.ButtonsDown["X"] && info.powerUpActive == 2 && actionId == 0) {
        //     info.SoundPlay(throwIceball);

        //     IceBall ice = Instantiate(info.iceBall, transform.position + info.skin.forward, info.skin.rotation).GetComponent<IceBall>();
        //     ice.player = info;
        // }
        // if (info.ButtonsDown["X"] && info.powerUpActive == 3 && actionId == 0) {
        //     //info.SoundPlay(throwIceball);

        //     ThunderBeam beam = Instantiate(info.thunderBeam, transform.position + info.skin.forward, info.skin.rotation).GetComponent<ThunderBeam>();
        //     beam.player = info;
        // }

        if (enabledStomping)
        {
            if (((info.GetCrouchButtonDown("RB") && GetComponent<_16MSonic>() == null) || (info.GetCrouchButtonDown("B") && GetComponent<_16MSonic>() != null)) && !info.Grounded && actionId <= 0)
            {
                //ストンピング
                actionId = 2;
                info.SoundPlay(stompSound);
                info.groundEvent = true;
                info.attacking = true;
                info.ForwardSetUp(Vector3.zero, 0f);
                info.rolling = false;
                info.YvelSetUp(-80f);
                info.groundAttack = true;
            }
            else if (info.Grounded && actionId == 2)
            {
                actionId = 0;
                info.groundAttack = false;
                info.groundEvent = false;
                info.attacking = false;
                info.StopAllSounds();
                info.SoundPlay(stompLandSound);

                if (info.GroundNormal != Vector3.zero)
                {
                    info.rolling = true;
                    // sliding = true;
                }
            }
        }
    }
    
    protected bool AorY() {
        return (info.Buttons["A"] && info.shieldActive < 2) || info.Axises["LT/RT"] >= 0.25f;
    }
    protected bool AorYDown() {
        return (info.ButtonsDown["A"] && info.shieldActive < 2) || info.Axises["LT/RT"] >= 0.25f;
    }
    protected bool AorYUp() {
        return (info.ButtonsUp["A"] && info.shieldActive < 2) || !(info.Axises["LT/RT"] >= 0.25f);
    }

    public void SuperConstantChange()
    {
        if (info.underwater)
        {
            info.constantChange("acc", S_U_Acceleration);
            info.constantChange("dec", S_U_Deceleration);
            info.constantChange("top", S_U_TopSpeed);

            info.constantChange("airacc", S_U_AirAcc);
            info.constantChange("jmp", S_U_JumpForce);
        }
        else
        {
            info.constantChange("acc", S_Acceleration);
            info.constantChange("dec", S_Deceleration);
            info.constantChange("top", S_TopSpeed);

            info.constantChange("airacc", S_AirAcc);
            info.constantChange("jmp", S_JumpForce);
        }
    }

    public IEnumerator DamageAnimation(Vector3 direction) {
        info.canInput = false;
        info.rolling = false;
        actionId = -1;

        Vector3 damageVelocity = direction * 10f;
        damageVelocity.y = 20f;
        info.gravityLock = true;
        info.VelocitySetUp(damageVelocity);
        info.constantChange("grv", 0.9375f);

        Vector3 XZvel = new Vector3(damageVelocity.x, 0, damageVelocity.z) * -1;
        if (XZvel.magnitude > 20f) {
            XZvel = info.SpeedReachToTop(XZvel, 20f);
        }

        while (!info.Grounded) {
            info.skin.transform.forward = XZvel.normalized;
            yield return new WaitForSeconds(0);
        }
        
        actionId = 0;
        info.VelocitySetUp(Vector3.zero);
        info.constantChange("grv", info.Gravity);
        info.tookDamage = false;
        info.canInput = true;
        info.axisInput = true;
        info.gravityLock = false;
        canJump = true;
    }
    public IEnumerator DeathAnimation() {
        info.LookFront();
        GetChildren(info.skin.gameObject, "Top Priority");

        voices.Death();
        
        actionId = -2;
        info.gravityLock = true;
        info.activeCollision = false;
        info.ForwardSetUp(-Vector3.forward, 0);
        info.YvelSetUp(35);
        info.constantChange("grv", 1.09375f);
        GetComponent<CapsuleCollider>().isTrigger = true;

        this.gameObject.layer = LayerMask.NameToLayer("Top Priority");

        yield return null;
    }
    public IEnumerator DrownAnimation() {
        info.LookFront();
        GetChildren(info.skin.gameObject, "Top Priority");
        
        info.activeCollision = false;
        info.ForwardSetUp(-Vector3.forward, 0);
        info.YvelSetUp(0);
        info.constantChange("grv", info.Gravity);
        GetComponent<CapsuleCollider>().isTrigger = true;

        this.gameObject.layer = LayerMask.NameToLayer("Top Priority");

        yield return null;
    }

    // void GetChildren(GameObject obj, string layer) {
    //     Transform children = obj.GetComponentInChildren<Transform>();
    //     //子要素がいなければ終了
    //     if (children.childCount == 0) {
    //         return;
    //     }
    //     foreach(Transform ob in children) {
    //         //ここに何かしらの処理
    //         ob.gameObject.layer = LayerMask.NameToLayer(layer);
    //         GetChildren(ob.gameObject, layer);
    //     }
    // }

    public void Reset() {
        info.activeCollision = true;
        actionId = 0;
        GetComponent<CapsuleCollider>().isTrigger = false;
        GetChildren(info.skin.gameObject, "Ignore Raycast");
    }

    protected void CameraLock()
    {
        FindAnyObjectByType<CameraManager>().XZLockTime = 0.2f;
    }

    protected void PoisonSplash()
    {
        ParticleWeapon p = Instantiate(poisonBurst, this.transform.position, Quaternion.identity).GetComponent<ParticleWeapon>();
        p.player = info;
        ParticleSystem first = p.gameObject.GetComponent<ParticleSystem>();
        var firstShapeModule = first.shape;

        p.gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleWeapon>().player = info;
        ParticleSystem second = p.gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        var secondMainModule = second.main;
        var secondShapeModule = second.shape;

        p.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ParticleWeapon>().player = info;
        p.gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleWeapon>().player = info;
        
        if (GameManager.dimension == DimensionType.XWay2D)
        {
            p.gameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            firstShapeModule.shapeType = ParticleSystemShapeType.Circle;
            secondShapeModule.shapeType = ParticleSystemShapeType.Circle;
            secondMainModule.startSpeed = 16f;
        }
        if (GameManager.dimension == DimensionType.ZWay2D)
        {
            p.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            firstShapeModule.shapeType = ParticleSystemShapeType.Circle;
            secondShapeModule.shapeType = ParticleSystemShapeType.Circle;
            secondMainModule.startSpeed = 16f;
        }
    }
}
