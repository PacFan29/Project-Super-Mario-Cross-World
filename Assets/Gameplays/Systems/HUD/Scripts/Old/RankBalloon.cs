using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankBalloon : MonoBehaviour
{
    [Header("パーセント")]
    public Image barFill;
    public int rankIndex;
    // public float linePercent = 0f; //0~100まで
    private float linePercent = 0f; //0~100まで
    [Header("吹き出し")]
    public Transform balloon;
    public GameObject rank;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // linePercent = Mathf.Clamp(linePercent, 0f, 100f);

        switch (rankIndex)
        {
            case 6:
            //Ｓランク
            linePercent = 100f;
            break;

            case 5:
            //Ａランク
            linePercent = GameManager.aRankLine * 1.0f / GameManager.sRankLine * 100f;
            break;

            case 4:
            //Ｂランク
            linePercent = GameManager.aRankLine * 0.8f / GameManager.sRankLine * 100f;
            break;

            case 3:
            //Ｃランク
            linePercent = GameManager.aRankLine * 0.6f / GameManager.sRankLine * 100f;
            break;

            case 2:
            //Ｄランク
            linePercent = GameManager.aRankLine * 0.4f / GameManager.sRankLine * 100f;
            break;

            case 1:
            //Ｅランク
            linePercent = GameManager.aRankLine * 0.2f / GameManager.sRankLine * 100f;
            break;

            case 0:
            //Ｆランク
            linePercent = 0f;
            break;
        }

        this.transform.localPosition = new Vector3(-183f + 366f * linePercent / 100, 0f, 0f);

        if (barFill.fillAmount * 100 >= linePercent) {
            balloon.localScale = Vector3.one;
            rank.SetActive(true);
        } else {
            balloon.localScale = Vector3.one * 0.5f;
            rank.SetActive(false);
        }
    }
}
