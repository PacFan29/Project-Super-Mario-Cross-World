using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TankType {
    Energy,
    Weapon,
    Mystery
}

public class ETankManager : MonoBehaviour
{
    public TankType tankType;

    [Header("スキン")]
    public GameObject goldShroom;
    public GameObject[] tankSkins = new GameObject[3];

    [Header("エフェクト")]
    public GameObject effect;
    public GameObject gotTankEffect;

    private bool typeMega;

    [Header("コイン")]
    public GameObject coinSpawner;

    // Update is called once per frame
    void Update()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (allPlayers.Length == 1) {
            typeMega = allPlayers[0].GetComponent<PlayerInfo>().playerType == 2 || tankType != TankType.Energy;
        } else {
            typeMega = false;
        }

        for (int i = 0; i < tankSkins.Length; i++) {
            tankSkins[i].SetActive((int)tankType == i);
        }

        if (!typeMega && tankType == TankType.Energy) {
            //ゴールドキノコ
            goldShroom.SetActive(true);
            tankSkins[0].SetActive(false);
        } else {
            goldShroom.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            if (player.GotETank((byte)tankType, this.transform.position)) {
                CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                c.SetUp(10, player);
            }

            if (!typeMega) {
                //ゴールドキノコのみ
                Instantiate(effect, this.transform.position, Quaternion.identity);
            } else {
                Instantiate(gotTankEffect, this.transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}
