using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _04Rosalina : MarioActions
{
    private float spinTime = 0f;
    private bool spinTrigger = true;
    public GameObject spinHitMesh;

    void Update()
    {
        //共通アクションの実行
        actions();
        
        /*
        ピーチ姫でしかできない技
        ・空中スピン

        声の出演：?
        */
        if (info != null){
            //プレイヤーIDを2に設定
            info.setPlayerId(4);
        }

        if (spinTime > 0)
        {
            spinTime -= Time.deltaTime;
        }
        if (info.Grounded)
        {
            spinTrigger = true;
        }
        if (((info.ButtonsDown["X"] && info.powerUpActive < 1) || info.ButtonsDown["Y"]) && spinTime <= 0f && spinTrigger && actionId == 0)
        {
            spinTime = 0.5f;
            if (!info.Grounded)
            {
                //2段ジャンプ
                info.YvelSetUp(info.JumpForce);
            }
            spinTrigger = false;
        }
        spinHitMesh.SetActive(spinTime > 0f);
    }
}
