using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComboManager : MonoBehaviour
{
    [Header("コンボ")]
    public byte combo = 0;
    [Header("コンボ上限")]
    public byte comboLimit = 99;
    [Header("プレイヤー情報")]
    public PlayerInfo player;

    public byte ComboIncrease(){
        //コンボ上昇（ただし5まで）
        if (combo < 99 && combo < comboLimit) combo++;
        if (combo > GameManager.ghostCombo)
        {
            GameManager.ghostCombo = combo;
        }
        if (GameManager.maxCombo < combo) GameManager.maxCombo = combo;

        // if (combo >= 8) {
        if (combo == 8) {
            player.OneUp();
        }

        // return (byte)Math.Min(5, (int)combo);
        return (byte)combo;
    }
    public byte GetCombo(){
        //コンボの取得
        // return (byte)Math.Min(5, (int)combo);
        return (byte)combo;
    }
    public byte GhostComboIncrease()
    {
        //コンボ上昇（8以上で1UP）
        if (GameManager.ghostCombo < 99) GameManager.ghostCombo++;
        if (GameManager.maxCombo < GameManager.ghostCombo) GameManager.maxCombo = GameManager.ghostCombo;

        // if (GameManager.ghostCombo >= 8)
        if (GameManager.ghostCombo == 8)
        {
            player.OneUp();
        }

        return GameManager.ghostCombo;
    }
    public byte GetGhostCombo()
    {
        //コンボの取得
        return GameManager.ghostCombo;
    }
    public void ComboReset()
    {
        //コンボの初期化
        combo = 0;
    }
}
