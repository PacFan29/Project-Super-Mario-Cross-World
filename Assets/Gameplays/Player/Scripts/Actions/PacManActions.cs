using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PacManActions : CommonActions
{
    protected bool canJump;
    protected bool inputA;
    
    private Dictionary<string, bool> pacActions = new Dictionary<string, bool>() {
        {"bounce", false},
        {"dashReady", false}
    };
    private bool canKick = false;
    private int bounceCount;
    private float dashTime;
    private AudioSource source;
    [Header("エフェクト")]
    public GameObject rollingEffect;
    public GameObject bounceEffect;
    public GameObject flipKickEffect;
    [Header("効果音")]
    public AudioClip bounceSound;
    public AudioClip flipkickSound;
    public AudioClip dashReadySound;
    public AudioClip dashReleaseSound;
    [Header("効果音(メタル状態)")]
    public AudioClip bounceSoundMetal;
    public AudioClip flipkickSoundMetal;
    // Start is called before the first frame update
    void Start()
    {
        info = GetComponent<PlayerInfo>();
        info.Stompable = false;
        gravityControl = true;
        // initialStompable = false;
        info.playerType = 1;
        info.desacelerar = true;
    }

    // Update is called once per frame
    public void actions()
    {
        enabledDash = true;
        enabledGroundPound = false;
        enabledFireball = true;
        enabledToSwim = true;
        commonActions();

        if (info.underwater)
        {
            info.rolling = false;
        }
        else
        {
            if (!info.onlyTurningDirection)
            {
                //しゃがむ
                if (info.GetCrouchButton("RB") && info.Grounded && !info.Crouching && !info.rolling) {
                    info.Crouching = true;
                } else if (!info.GetCrouchButton("RB") && info.Grounded && info.Crouching && !info.rolling && !info.isNarrow() && actionId != 3) {
                    info.Crouching = false;
                }
            }

            if (info.ButtonsDown["A"] && info.Grounded && actionId != 3)
            {
                if (info.onlyTurningDirection)
                {
                    info.onlyTurningDirection = false;
                }
                info.rolling = false;
                info.Crouching = false;
                inputA = true;
                info.Jump();
            }
            if (inputA && info.finalVelocity.y > 0)
            {
                // //ジャンプの勢い
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
        }
        if (info.Grounded && (actionId == 1 || actionId == 2)) {
            actionId = 0;
        }

        if (info.underwater != underwaterPrevious && info.Buttons["A"]) {
            /*ドルフィンジャンプ */
            info.YvelSetUp(50);
        }
        underwaterPrevious = info.underwater;

        if (info.Crouching || info.rolling) {
            this.GetComponent<CapsuleCollider>().height = 2.4f;
        } else {
            // this.GetComponent<CapsuleCollider>().height = 3.5f;
            this.GetComponent<CapsuleCollider>().height = info.InitialColliderHeight;
        }

        this.GetComponent<PlayerAnimManager>().actionId = actionId;

        if (info != null){
            //プレイヤーIDを5に設定
            info.setPlayerId(5);
        }
        if (info.Grounded) {
            if (dashTime > 0 && !info.rolling && actionId != 3)
            {
                info.rolling = true;
            }
            canKick = true;
        // } else {
        //     if (info.ButtonsDown["Y"] && !info.rolling && canKick && !info.giant) {
        //         canKick = false;
        //         if (info.metal) {
        //             info.SoundPlay(flipkickSoundMetal);
        //         } else {
        //             info.SoundPlay(flipkickSound);
        //         }
        //         StartCoroutine("FlipKick");
        //     }
        }
        if (info.ButtonsDown["B"] && !info.rolling && canKick && !info.giant) {
            canKick = false;
            if (info.metal) {
                info.SoundPlay(flipkickSoundMetal);
            } else {
                info.SoundPlay(flipkickSound);
            }
            StartCoroutine("FlipKick");
        }
        
        if (info.underwater) {
            dashTime = 0f;
            info.onlyTurningDirection = false;
            bounceCount = 0;
            pacActions["bounce"] = false;
            pacActions["dashReady"] = false;

            info.axisInput = true;
        } else {
            /* ヒップアタック */
            if (info.axisInput && (info.ButtonsDown["A"] || info.GetCrouchButtonDown("RB")) && !inputA && !info.Grounded && !info.groundEvent && !info.rolling && dashTime <= 0 && !info.giant)
            {
                //急降下
                actionId = 0;
                info.attacked = false;
                info.groundAttack = true;

                if (info.finalVelocity.y > 0) info.YvelSetUp(0f);
                info.rolling = true;
                gravityControl = false;
                info.constantChange("grv", info.Gravity * 5f);

                pacActions["bounce"] = true;
            }
            else if (pacActions["bounce"] && (info.Grounded || info.attacked))
            {
                //バウンド
                if (!info.attacked) info.ComboReset();
                info.attacked = false;
                info.groundAttack = false;
                gravityControl = false;

                if (bounceCount < 3) bounceCount++;
                info.constantSetUp();
                if (info.metal)
                {
                    info.SoundPlay(bounceSoundMetal);
                }
                else
                {
                    info.SoundPlay(bounceSound);
                }
                info.rolling = false;
                // info.YvelSetUp(info.JumpForce * (1.2f - ((float)(bounceCount - 1) / 40)));
                info.YvelSetUp(Math.Max(54f, info.JumpForce * 1.5f));

                pacActions["bounce"] = false;
                actionId = (sbyte)(bounceCount == 3 ? 2 : 1);

                RaycastHit hit;
                Physics.BoxCast(transform.position, Vector3.one * 0.5f, -Vector3.up, out hit, Quaternion.identity, 1.85f, info.GroundLayer);
                Vector3 pos = transform.position - (transform.up * 1.35f);
                Instantiate(bounceEffect, pos, Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(-90, 0, 0));
            }
            else if (!pacActions["bounce"] && info.Grounded)
            {
                bounceCount = 0;
                gravityControl = true;
            }

            /* パックダッシュ */
            if (info.ButtonsDown["Y"] && dashTime <= 0 && info.Grounded && !info.giant) {
                //パックダッシュ開始
                info.VelocitySetUp(Vector3.zero);
                info.onlyTurningDirection = true;
                info.Crouching = false;
                actionId = 3;

                canJump = false;
                info.StopAllSounds();

                source = GetComponent<AudioSource>();
                source.clip = dashReadySound;
                // source.pitch = 0f;
                source.loop = true;
                source.Play();

                dashTime += Time.deltaTime;
                pacActions["dashReady"] = true;
            } else if (pacActions["dashReady"] && dashTime < 1f && info.Buttons["Y"]) {
                //チャージ（効果音のピッチ：0～1.9）
                dashTime += Time.deltaTime;
                if (dashTime > 1f) dashTime = 1f;
                // source.pitch = Math.Min(1.9f, 3.8f * dashTime);
            } else if (!info.Buttons["Y"] && pacActions["dashReady"]) {
                // info.onlyTurningDirection = false;
                source.clip = null;
                source.pitch = 1f;
                source.loop = false;
                source.Stop();

                pacActions["dashReady"] = false;
                
                if (dashTime >= 0.25f) {
                    //突進
                    actionId = 0;
                    // info.axisInput = false;
                    info.rolling = true;
                    info.constantChange("airfrc", 0f);
                    info.ForwardSetUp(Vector3.zero, 55f);
                    info.SoundPlay(dashReleaseSound);
                    dashTime *= 2f;
                    dashTime -= Time.deltaTime;

                    info.constantChange("slprollup", 0f);
                } else {
                    //キャンセル
                    actionId = 0;
                    dashTime = 0f;
                    info.onlyTurningDirection = false;
                }
            } else if (!pacActions["dashReady"] && dashTime > 0f) {
                //ダッシュ中の時間減少
                if (info.Grounded) dashTime -= Time.deltaTime;

                if (dashTime < 0) {
                    dashTime = 0f;
                    info.onlyTurningDirection = false;
                    info.constantChange("slprollup", info.SlopeRollUp);
                }
            }
            if (inputA && (dashTime > 0 || info.rolling) && !info.isNarrow()) {
                //パックダッシュ中にジャンプ（キャンセル）
                // info.axisInput = true;
                info.rolling = false;
                info.onlyTurningDirection = false;
                dashTime = 0f;
                if (info.input != Vector3.zero) {
                    info.ForwardSetUp(Vector3.zero, info.TopSpeed, true);
                } else
                {
                    info.ForwardSetUp(Vector3.zero, 0f, true);
                }
            }

            if (dashTime <= 0f && Vector3.Angle(Vector3.up, info.GroundNormal) <= 15 && info.Grounded && !info.isNarrow()) {
                if (info.rolling) info.constantSetUp();
                // info.axisInput = true;
                info.rolling = false;
            } else if (Vector3.Angle(Vector3.up, info.GroundNormal) >= 40 && dashTime <= 0f && info.Grounded) {
                if (!info.rolling) info.constantChange("airfrc", 0f);
                info.axisInput = false;
                info.rolling = true;
            }

            if (!info.axisInput && !info.Grounded && !info.rolling) {
                info.axisInput = true;
            }

            if (!pacActions["bounce"] && info.rolling && info.attacked) {
                info.VelocitySetUp(info.finalVelocity * -1);
                info.attacked = false;
            }

            info.groundEvent = pacActions["bounce"];

            if (info.rolling) {
                Instantiate(rollingEffect, transform.position, Quaternion.identity);
            }
        }
    }

    public IEnumerator DamageAnimation() {
        info.tookDamage = false;
        yield return null;
    }
    public IEnumerator DeathAnimation() {
        actionId = -2;
        yield return null;
    }

    public void Reset() {
        actionId = 0;
    }

    IEnumerator FlipKick() {
        actionId = 4;
        if (info.finalVelocity.y < 25f || info.Grounded)
        {
            info.YvelSetUp(25);
        }
        flipKickEffect.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        flipKickEffect.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        actionId = 0;
    }
}
