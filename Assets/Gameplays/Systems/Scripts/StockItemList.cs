using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItemList : MonoBehaviour
{
    public GameData data;
    public GameObject item;

    private bool leastOneTypeMega;
    private bool sonicStage;

    private List<StockItem> list = new List<StockItem>();
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine("SetUp");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (StockItem item in list) {
            item.sprites = data.stockItems[item.index].sprites;
            item.amounts = data.stockItems[item.index].amounts;
        }
    }

    IEnumerator SetUp() {
        yield return new WaitForSeconds(0.25f);

        int offset = 0;

        leastOneTypeMega = false;
        sonicStage = false;
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < allPlayers.Length; i++){
            if (allPlayers[i].GetComponent<PlayerInfo>() != null){
                if (allPlayers[i].GetComponent<PlayerInfo>().playerType == 2) {
                    leastOneTypeMega = true;
                }

                if (allPlayers[i].GetComponent<_16MSonic>() != null) {
                    sonicStage = true;
                }
            }
        }

        StockItem stk = item.GetComponent<StockItem>();
        for (int i = 0; i < data.stockItems.Length; i++) {
            stk.index = i;
            stk.special = data.stockItems[i].special;
            offset = i + 1;

            if ((!data.stockItems[i].megaManOnly || leastOneTypeMega) && (!data.stockItems[i].exceptSonic || !sonicStage)) {
                list.Add(stk);
                break;
            }
        }

        int count = 1;
        for (int i = offset; i < data.stockItems.Length; i++) {
            if ((!data.stockItems[i].megaManOnly || leastOneTypeMega) && (!data.stockItems[i].exceptSonic || !sonicStage)) {
                RectTransform scoreimage = (RectTransform)Instantiate(item).transform;
                scoreimage.SetParent(this.transform , false);
                scoreimage.localPosition = new Vector2(
                    scoreimage.localPosition.x + scoreimage.sizeDelta.x * count ,
                    scoreimage.localPosition.y);
                count++;

                stk = scoreimage.gameObject.GetComponent<StockItem>();
                stk.index = i;
                stk.special = data.stockItems[i].special;
                list.Add(stk);
            }
        }
    }
}
