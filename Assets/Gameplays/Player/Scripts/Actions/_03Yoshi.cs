using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _03Yoshi : MarioActions
{
    public AudioClip[] flutterVoices;
    public AudioClip[] swallowVoices;
    public AudioClip[] layEggVoices;
    public AudioClip[] aimVoices;

    private float flutterTime = 1f;
    private bool fluttering = false;
    void Update()
    {
        if (info.Grounded)
        {
            flutterTime = 1f;
        } else
        {
            if (fluttering)
            {
                flutterTime -= Time.deltaTime;

                if (!(info.Buttons["A"] || info.Buttons["B"]) || flutterTime <= 0f)
                {
                    fluttering = false;
                    if (flutterTime > 0)
                    {
                        voices.VoiceStop();
                    }
                }
            }

            if ((info.Buttons["A"] || info.Buttons["B"]) && !fluttering && flutterTime > 0f && info.finalVelocity.y <= 0f && actionId == 0)
            {
                fluttering = true;
                voices.VoicePlay(flutterVoices);
                if (info.finalVelocity.y < 0f)
                {
                    info.YvelSetUp(0f);
                }
            }
        }

        //共通アクションの実行
        actions();
        
        /*
        ヨッシーでしかできない技
        ・敵を食べる
        ・卵を産む
        ・卵を投げる
        ・ヒップドロップ

        声の出演：?
        */
        if (info != null){
            //プレイヤーIDを3に設定
            info.setPlayerId(3);
        }
    }

    void FixedUpdate()
    {
        if (fluttering)
        {
            info.YvelSetUp(info.finalVelocity.y + info.Gravity * 1.45f * Math.Min(1f, (1 - flutterTime) * 4f));
        }
    }
}
