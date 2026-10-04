using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRateInfo : MonoBehaviour
{
    public PlayerInfo info;
    public int targetPlayer = 0;
    [Header("出力")]
    public Image fill;
    public Text rate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players) {
            if (player.GetComponent<PlayerInfo>() != null) {
                if (player.GetComponent<PlayerInfo>().playerNumber == targetPlayer) {
                    info = player.GetComponent<PlayerInfo>();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (info != null) {
            this.GetComponent<CanvasGroup>().alpha = (info.rate > 1) ? 1f : 0f;

            this.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, info.transform.position);

            float percent = info.rateTime / 10f;
            fill.fillAmount = percent;

            rate.text = "×" + info.rate.ToString();
        }
    }
}
