using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _05PacMan : PacManActions
{
    [Header("隠しスキル：フルーツターゲット")]
    public bool hiddenSkill;
    public GameObject fruitTarget;
    private SmashFruitTarget currentFruit;
    public AudioClip[] fruitTargetChargeSounds = new AudioClip[8];
    public AudioClip fruitTargetThrowSound;

    private bool fruitPreparing = false;
    private int fruitIndex = -1;
    private float fruitChargeTime = 0f;
    private bool LTTrigger = false;

    public void Update()
    {
        //共通アクションの実行
        actions();
        
        /*
        パックマンでしかできない技
        ・パックダッシュ
        */

        if (LTTrigger)
        {
            if (info.Axises["LT/RT"] > -0.25f)
            {
                LTTrigger = false;
            }
        }

        if (info.Axises["LT/RT"] <= -0.25f && !LTTrigger && !fruitPreparing && hiddenSkill)
        {
            currentFruit = Instantiate(fruitTarget, this.transform.position + Vector3.up * 4f, Quaternion.identity).GetComponent<SmashFruitTarget>();
            currentFruit.player = info;
            LTTrigger = true;
            if (fruitIndex == 7)
            {
                //もしカギならすぐ投げる
                currentFruit.index = 7;
                currentFruit.gameObject.transform.forward = info.skin.forward;
                currentFruit.gameObject.GetComponent<Rigidbody>().rotation = currentFruit.gameObject.transform.rotation;
                ThrowFruit();
                fruitPreparing = false;
            } else
            {
                fruitPreparing = true;
                fruitChargeTime = 0f;

                if (fruitIndex == -1)
                {
                    fruitIndex = 0;
                }
                info.SoundPlay(fruitTargetChargeSounds[fruitIndex]);
            }
        }

        if (fruitPreparing)
        {
            currentFruit.gameObject.transform.position = this.transform.position + Vector3.up * 4f;
            currentFruit.gameObject.transform.forward = info.skin.forward;
            currentFruit.gameObject.GetComponent<Rigidbody>().position = currentFruit.gameObject.transform.position;
            currentFruit.gameObject.GetComponent<Rigidbody>().rotation = currentFruit.gameObject.transform.rotation;
            currentFruit.index = fruitIndex;

            fruitChargeTime += Time.deltaTime;
            if (
                (fruitChargeTime >= 0.2f && fruitIndex < 2) || 
                (fruitChargeTime >= 0.375f && fruitIndex >= 2 && fruitIndex < 7)
            )
            {
                fruitChargeTime = 0f;
                fruitIndex++;
                info.SoundPlay(fruitTargetChargeSounds[fruitIndex]);
            }

            if (fruitChargeTime >= 0.25f && fruitIndex == 7)
            {
                fruitPreparing = false;
                Destroy(currentFruit.gameObject);
            }

            if (info.ButtonsDown["X"] || (info.Axises["LT/RT"] <= -0.25f && !LTTrigger))
            {
                ThrowFruit();
                fruitPreparing = false;
                if (info.Axises["LT/RT"] <= -0.25f)
                {
                    LTTrigger = true;
                }
            }

            if (info.Crouching)
            {
                fruitPreparing = false;
                Destroy(currentFruit.gameObject);
            }
        }
    }

    void ThrowFruit()
    {
        info.SoundPlay(fruitTargetThrowSound);
        fruitPreparing = false;
        fruitIndex = -1;

        currentFruit.gameObject.transform.position = this.transform.position + info.skin.forward * 2f;
        currentFruit.gameObject.GetComponent<Rigidbody>().position = currentFruit.gameObject.transform.position;
        currentFruit.Thrown();
    }
}
