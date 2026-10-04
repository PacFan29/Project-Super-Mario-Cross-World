using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _00Mario : MarioActions
{
    public void Update()
    {
        /*
        声の出演：Charles Martinet
        */
        if (info != null){
            //プレイヤーIDを0に設定
            info.setPlayerId(0);
        }

        //共通アクションの実行
        actions();
    }
}
