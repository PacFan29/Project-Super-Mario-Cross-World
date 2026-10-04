using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _02Peach : MarioActions
{
    private bool floating = false;
    private bool floatingTrigger = false;
    void Update()
    {
        if (info.Grounded)
        {
            floatingTrigger = true;
        }
        if (!info.Grounded && (info.ButtonsDown["A"] || (info.Buttons["A"] && info.finalVelocity.y <= 0f)) && !floating && floatingTrigger && actionId == 0)
        {
            floating = true;
            floatingTrigger = false;
        }
        if (floating)
        {
            info.YvelSetUp(-1.5f);

            if (!info.Buttons["A"])
            {
                floating = false;
            }
        }

        //共通アクションの実行
        actions();
        
        /*
        ピーチ姫でしかできない技
        ・空中で少しの間浮く

        声の出演：?
        */
        if (info != null){
            //プレイヤーIDを2に設定
            info.setPlayerId(2);
        }
    }
}
