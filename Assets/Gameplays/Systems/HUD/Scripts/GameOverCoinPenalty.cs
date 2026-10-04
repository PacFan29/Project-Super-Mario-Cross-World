using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

public class GameOverCoinPenalty : MonoBehaviour
{
    [Header("データ")]
    public GameData gameData;

    [Header("カンマ多言語対応")]
	public Options options;
    public LocalizedStringTable commaHeaderTable;
    UnityEngine.Localization.Tables.StringTable commaHeadPairs;
    [Header("コイン枚数")]

    public Text coins;
    public Animator coinsAnim;
    public Text penalty;

    private uint penaltyCoins;
    private uint currentCoins;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        StartCoroutine("DecreaseCoins");
        currentCoins = gameData.coins;
    }

    // Update is called once per frame
    void Update()
    {
        commaHeadPairs = commaHeaderTable.GetTable();

        if (penaltyCoins > 0)
        {
            penalty.text = "-" + penaltyCoins.ToString("###,###,##0");
        } else
        {
            penalty.text = "";
        }
        if (currentCoins > gameData.coins)
		{
			//コイン減少
			currentCoins -= (uint)Math.Max(1, ((float)(currentCoins - gameData.coins) * 5f * Time.unscaledDeltaTime));
			if (currentCoins == gameData.coins)
			{
				coinsAnim.Play("CoinsDec", 0, 0);
			} else
			{
				coinsAnim.Play("CoinsDec", 0, Time.time % 0.075f);
			}

		}
        coins.text = currentCoins.ToString("###,###,##0");

        penalty.text = penalty.text.Replace(",", options.comma ? commaHeadPairs["EN-HUD-Comma"].Value : "");
        coins.text = coins.text.Replace(",", options.comma ? commaHeadPairs["EN-HUD-Comma"].Value : "");
    }

    IEnumerator DecreaseCoins()
    {
        penaltyCoins = (uint)Math.Min(10000, Math.Ceiling((float)gameData.coins * 0.25f));

        yield return new WaitForSeconds(2.75f);

        gameData.coins -= penaltyCoins;
    }
}
