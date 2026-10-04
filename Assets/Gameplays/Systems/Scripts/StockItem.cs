using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockItem : MonoBehaviour
{
    public bool special;
    public float amounts;
    [HideInInspector] public Sprite[] sprites;
    [HideInInspector] public int index;
    [Header("背景")]
    public Image background;
    [Header("アイコン")]
    public Image icon;
    public Image[] miniIcons = new Image[3];
    [Header("個数")]
    public Text amountText;

    // Update is called once per frame
    void Update()
    {
        if (special) {
            background.color = new Color(1f, 1f, 0f, 0.55f);
            amountText.text = amounts.ToString();
            amountText.gameObject.SetActive(amounts > 1);
            icon.sprite = sprites[0];
        } else {
            background.color = new Color(0f, 0f, 0f, 0.55f);
            amountText.gameObject.SetActive(false);
            int sprI = Math.Max(0, Math.Min(sprites.Length - 1, (int)Math.Floor(amounts) - 1));
            icon.sprite = sprites[sprI];
            miniIcons[0].sprite = sprites[sprI];
            miniIcons[1].sprite = sprites[sprI];
            miniIcons[2].sprite = sprites[sprI];
        }

        icon.gameObject.SetActive(amounts >= 1);
        miniIcons[0].gameObject.SetActive(amounts % 1f >= 0.25f);
        miniIcons[1].gameObject.SetActive(amounts % 1f >= 0.5f);
        miniIcons[2].gameObject.SetActive(amounts % 1f >= 0.75f);
    }
}
