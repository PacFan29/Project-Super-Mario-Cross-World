using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _13Tails : SonicActions
{
    private float jumpTime;
    private bool sliding = false;
    private float flyingTime = 8f;
    private bool flying = false;
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
        テイルスでしかできない技
        ・飛行
        */
        if (info != null){
            //プレイヤーIDを2に設定
            info.setPlayerId(13);
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

        //スライディング
        if ((info.GetCrouchButton("RB") || info.GetCrouchButton("B")) && info.Grounded && !sliding && !info.rolling && actionId != 5) {
            if (info.XZmag <= 0 && info.input != Vector3.zero) {
                info.ForwardSetUp(Vector3.zero, 15f);
                info.Crouching = false;
            }
            if (info.XZmag >= 2.5f) {
                if (!spinDashed) {
                    info.constantChange("rollfrc", info.RollFrc * 3);
                }
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

        //飛行
        if (flying) {
            if (info.finalVelocity.y < -15f) {
                info.YvelSetUp(-15f);
            }

            if (flyingTime > 0) {
                flyingTime -= Time.deltaTime;

                if (info.Buttons["A"]) {
                    if (info.finalVelocity.y < 15f) {
                        if (info.finalVelocity.y > 0) {
                            info.YvelSetUp(info.finalVelocity.y + 0.5f);
                        } else {
                            info.YvelSetUp(info.finalVelocity.y + 5f);
                        }
                    }
                }

                if (info.GetCrouchButton("RB") || info.GetCrouchButton("B")) {
                    info.YvelSetUp(info.finalVelocity.y - 0.5f);
                }
            }

            if (info.Grounded) {
                jumpAction = -1;
                flying = false;
                info.constantChange("grv", info.Gravity);
            }
        } else {
            flyingTime = 8f;

            if (jumpAction > 0 && info.ButtonsDown["A"]) {
                info.rolling = false;
                info.constantChange("grv", 0.15625f);
                jumpAction = 0;
                flying = true;
            }

            if (info.Grounded) {
                jumpAction = -1;
            }
        }

        // if (
        //     ((info.GetCrouchButtonDown("RB") || info.GetCrouchButtonDown("B")) && !info.Grounded && actionId <= 0 && !flying) || 
        //     ((info.GetCrouchButton("RB") || info.GetCrouchButton("B")) && flying && info.ButtonsDown["A"])
        // ) {
        //     //ストンピング
        //     jumpAction = -1;
        //     info.constantChange("grv", info.Gravity);
        //     flying = false;

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
