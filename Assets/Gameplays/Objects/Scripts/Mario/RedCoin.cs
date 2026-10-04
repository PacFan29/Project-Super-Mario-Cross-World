using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCoin : MonoBehaviour
{
    public GameObject[] skins = new GameObject[2];
    [Header("マネージャー")]
    public RedCoinRing manager;
    [Header("エフェクト")]
    public GameObject coinEffect;
    public GameObject countDisplay;
    // Start is called before the first frame update
    void Awake()
    {
        Update();
    }

    void Update()
    {
        skins[0].SetActive(!(GameManager.sonicStage || GameManager.courseMode == CourseMode.RingAttack));
        skins[1].SetActive(GameManager.sonicStage || GameManager.courseMode == CourseMode.RingAttack);
    }
    
    void FixedUpdate() {
        this.transform.Rotate(0f, -3f, 0f, Space.Self);
    }
    
    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            GameManager.Coins += 1;
            GameManager.ringRushProgress += 0.01f;
            player.scoreIncrease(100, this.transform.position + Vector3.up * 1.5f);

            if (col.GetComponent<_16MSonic>() != null) {
                col.GetComponent<_16MSonic>().BoostIncrease(1, false);
            }

            if (manager != null)
            {
                int count = manager.GotRedCoin(player);
                RedCoinCount c = Instantiate(countDisplay, this.transform.position + Vector3.up, Quaternion.identity).GetComponent<RedCoinCount>();

                c.count = count;

                if (count == 8)
                {
                    //8枚獲得でボーナス
                    manager.Bonus();
                }
            }

            Instantiate(coinEffect, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
