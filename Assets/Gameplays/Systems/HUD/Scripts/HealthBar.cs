using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("値")]
    public float value;
    public float maxValue;
    private float valuePercent = 1f;
    private float currentValue;

    [Header("バー")]
    public Image barBack;
    public Image barFill;
    public Image barFillDamage;
    public bool pinch;
    [Header("バーの色（プレイヤーのみ）")]
	public Color[] barColors = new Color[4];
    public bool lowHP;
    [Header("バーの色（ボスのみ）")]
    public bool isBoss;
    public Gradient bossBarColors;
    public int bossPhase;
    public int maxBossPhase;

    private float valueFill = 1f;
    private float valueDamageFill = 1f;
    private float stopTime = 0f;
    private int animIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        valuePercent = 1f;
        currentValue = value;
        valueFill = 1f;
        valueDamageFill = 1f;
        stopTime = 0f;
        animIndex = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        valuePercent = value / maxValue;

        barFill.fillAmount = valueFill;
        barFillDamage.fillAmount = valueDamageFill;
        pinch = false;

        if (value > 0 && barFill.fillAmount <= 0.01f)
        {
            barFill.fillAmount = 0.01f;
        }
        if (value > 0 && barFillDamage.fillAmount <= 0.01f)
        {
            barFillDamage.fillAmount = 0.01f;
        }

        if (isBoss)
        {
            if (maxBossPhase >= 4)
            {
                barFill.color = bossBarColors.Evaluate((float)bossPhase / (float)maxBossPhase);
                pinch = (bossPhase == 0 && !GameManager.bossInfo.multiPhase) || (bossPhase == 0 && GameManager.bossInfo.multiPhase && valuePercent <= 0.5f);
            }
            else if (maxBossPhase > 0)
            {
                barFill.color = bossBarColors.Evaluate(0.25f * bossPhase);
                pinch = (bossPhase == 0 && !GameManager.bossInfo.multiPhase) || (bossPhase == 0 && GameManager.bossInfo.multiPhase && valuePercent <= 0.5f);
            }
            else
            {
                barFill.color = bossBarColors.Evaluate(0f);
                pinch = valuePercent <= 0.5f;
            }
        }
        else
        {
            int barColorIndex = 0;
            if (lowHP)
            {
                barColorIndex = Math.Max(
                    0,
                    Math.Min(
                        barColors.Length - 2,
                        (int)Math.Ceiling(valuePercent / (1f / 3f)) - 1
                    )
                );
            }
            else
            {
                barColorIndex = Math.Max(
                    0,
                    Math.Min(
                        barColors.Length - 1,
                        (int)Math.Ceiling(valuePercent / (1f / (barColors.Length))) - 1
                    )
                );
            }
            barFill.color = barColors[barColorIndex];
            pinch = barColorIndex == 0;
        }
        pinch = pinch && valuePercent > 0;

        //点滅
        if (pinch)
        {
            barFill.color = new Color(1.0f, HUDManager.alert[0] * 0.8f, HUDManager.alert[0] * 0.8f, 1.0f);
            barBack.color = new Color(HUDManager.alert[0] * 0.5f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            barBack.color = Color.black;
        }

        if (value != currentValue)
        {
            if (value > currentValue)
            {
                //回復
                stopTime = 0.5f;
                animIndex = 1;
            }
            else
            {
                //ダメージ
                stopTime = 1.5f;
                animIndex = 0;
            }
            currentValue = value;
        }
        if (stopTime > 0f) {
            stopTime -= Time.fixedDeltaTime;
        }
        if (animIndex == 1) {
            valueDamageFill = valuePercent;
            if (stopTime <= 0f) {
                valueFill = Mathf.Lerp(valueFill, valuePercent, 0.1f);
            }
        } else {
            valueFill = Mathf.Lerp(valueFill, valuePercent, 0.1f);
            if (stopTime <= 0f) {
                valueDamageFill = Mathf.Lerp(valueDamageFill, valuePercent, 0.1f);
            }
        }
    }
}
