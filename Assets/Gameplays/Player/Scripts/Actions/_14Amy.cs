using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _14Amy : SonicActions
{
    private float jumpTime;
    private bool sliding = false;
    [Header("ハンマーの当たり判定")]
    public GameObject spinHammer;
    [Header("効果音")]
    public AudioClip spinSound;
    public AudioClip slidingSound;
    // public AudioClip stompSound;
    // public AudioClip stompLandSound;
    public LoopingSoundManager lManager;

    // Update is called once per frame
    void Update()
    {
        //共通アクションの実行
        actions();

        /*
        エミーでしかできない技
        ・ハンマーで攻撃
        */
        if (info != null){
            //プレイヤーIDを2に設定
            info.setPlayerId(14);
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
                spinHammer.SetActive(true);
                info.rolling = true;
            }
        }

        if (jumpAction > 0 && info.ButtonsDown["A"]) {
            info.SoundPlay(info.jumpSound);
            spinHammer.SetActive(true);
            if (!info.rolling) {
                info.SoundPlay(spinSound);
                info.rolling = true;
            }

            info.YvelSetUp(info.AirborneC["jmp"]);
            jumpAction = 0;
        }
        if (info.Grounded) {
            spinHammer.SetActive(false);
            jumpAction = -1;
        }

        if (
            (jumpAction > 0 && info.finalVelocity.y < 0) || 
            (jumpAction == 0 && !info.Buttons["A"])
        ) {
            spinHammer.SetActive(false);
        }

        //スライディング
        if ((info.GetCrouchButton("RB") || info.GetCrouchButton("B")) && info.Grounded && !sliding && !info.rolling && actionId != 5) {
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
        } else if (sliding && ((!(info.GetCrouchButton("RB") || info.GetCrouchButton("B")) && transform.up == Vector3.up) || !info.Grounded || info.Crouching)) {
            info.constantChange("rollfrc", info.RollFrc);
            if (info.Grounded || (!info.Grounded && info.finalVelocity.y <= 0)) info.rolling = false;
            sliding = false;

            lManager.Stop();
        }

        // if ((info.GetCrouchButtonDown("RB") || info.GetCrouchButtonDown("B")) && !info.Grounded && actionId <= 0) {
        //     //ストンピング
        //     actionId = 2;
        //     info.SoundPlay(stompSound);
        //     info.groundEvent = true;
        //     info.attacking = true;
        //     info.ForwardSetUp(Vector3.zero, 0f);
        //     info.rolling = false;
        //     info.YvelSetUp(-80f);
        //     info.groundAttack = true;
        // } else if (info.Grounded && actionId == 2) {
        //     actionId = 0;
        //     info.groundAttack = false;
        //     info.groundEvent = false;
        //     info.attacking = false;
        //     info.StopAllSounds();
        //     info.SoundPlay(stompLandSound);

        //     if (info.GroundNormal != Vector3.zero) {
        //         info.rolling = true;
        //         sliding = true;
        //     }
        // }
    }
}
