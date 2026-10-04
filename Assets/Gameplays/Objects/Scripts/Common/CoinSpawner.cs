using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("コイン枚数")]
    public int coinsAmount = 0;
    [Header("プレイヤー情報")]
    public PlayerInfo player;
    [Header("オブジェクト")]
    public GameObject coinObj;
    public GameObject coin10Obj;
    public GameObject coin30Obj;
    public GameObject coin50Obj;
    private bool startCoroutine = false;
    private bool dropTrigger = false;
    [Header("リングポップアップ")]
    public GameObject ringPopUp;

    // Update is called once per frame
    void Update()
    {
        if (!startCoroutine && coinsAmount > 0)
        {
            if (GameManager.courseMode == CourseMode.RingAttack) {
                GameObject popup = Instantiate(ringPopUp, this.transform.position, Quaternion.identity);
                popup.GetComponent<ScoringController>().points = coinsAmount;
            }
            StartCoroutine("SplitCoins");
            startCoroutine = true;
        }
    }

    public void SetUp(int coins, PlayerInfo info, bool drop = false)
    {
        dropTrigger = drop;
        coinsAmount = coins;
        player = info;
    }

    IEnumerator SplitCoins()
    {
        List<int> coinsList = new List<int>();
        while (coinsAmount >= 50)
        {
            coinsAmount -= 50;
            coinsList.Add(3);
        }
        while (coinsAmount >= 30)
        {
            coinsAmount -= 30;
            coinsList.Add(2);
        }
        while (coinsAmount >= 10)
        {
            coinsAmount -= 10;
            coinsList.Add(1);
        }
        while (coinsAmount > 0)
        {
            coinsAmount -= 1;
            coinsList.Add(0);
        }
        float waitSec = coinsList.Count <= 1 ? 0.1f : Math.Min(0.2f, 0.4f / (float)(coinsList.Count - 1));
        for (int i = 0; i < coinsList.Count; i++)
        {
            GameObject coin = coinObj;
            switch (coinsList[i])
            {
                case 1:
                    coin = coin10Obj;
                    break;
            
                case 2:
                    coin = coin30Obj;
                    break;
            
                case 3:
                    coin = coin50Obj;
                    break;
            }
            CoinManager c = Instantiate(coin, this.transform.position, Quaternion.identity).GetComponent<CoinManager>();
            if (dropTrigger)
            {
                c.DropCoin(0f, 0f, 50f);
            }
            else
            {
                c.targetPlayer = player.gameObject;
                StartCoroutine(c.SplitCoin(false));
            }

            yield return new WaitForSeconds(waitSec);
        }
        Destroy(gameObject);
    }
}
