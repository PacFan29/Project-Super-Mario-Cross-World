using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearListData : MonoBehaviour
{
    [Header("データ")]
    public StageInfos stageInfos;
    [Header("ＵＩ")]
    public Text stage;
    public RectTransform greenStarParent;
    public Transform greenStar;
    public Text bestTime;
    public Text highScore;
    public Image rank;
    [Header("スプライト")]
    public Sprite[] greenStarSprites = new Sprite[27];
    public Sprite[] rankSprites = new Sprite[7];

    public int worldNum;
    public int stageNum;

    // Update is called once per frame
    void Update()
    {
        StageInfos.Stage stageData = stageInfos.worlds[worldNum].stages[stageNum];

        stage.text = "W" + (worldNum + 1).ToString() + "-" + (stageNum + 1).ToString();

        greenStar.gameObject.SetActive(stageData.greenStars.Length >= 1);

        foreach (Transform obj in greenStarParent) {
            if ( 0 <= obj.gameObject.name.LastIndexOf("Clone") ) {
                Destroy(obj.gameObject);
            }
        }

        if (stageData.greenStars.Length > 0) {
            float offset = greenStar.GetComponent<RectTransform>().sizeDelta.x * -0.5f * (stageData.greenStars.Length - 1);

            if (stageData.greenStars[0]) {
                greenStar.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            } else {
                greenStar.gameObject.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 1);
            }

            greenStar.transform.localPosition = Vector2.right * offset;
                    
            for ( int i = 1 ; i < stageData.greenStars.Length; i++ ) {
                RectTransform keyRect = (RectTransform)Instantiate(greenStar).transform;
                keyRect.SetParent(greenStarParent, false);
                keyRect.localPosition = greenStar.localPosition;

                keyRect.localPosition = new Vector2(
                    keyRect.localPosition.x + (keyRect.sizeDelta.x * i),
                    keyRect.localPosition.y
                );

                if (stageData.greenStars[i]) {
                    keyRect.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                } else {
                    keyRect.gameObject.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 1);
                }
            }
        }

        if (stageData.cleared) {
            double minutes = stageData.bestTime / 60000;
            double seconds = (stageData.bestTime / 1000) % 60;
            double milli = (stageData.bestTime % 1000) / 10;
            bestTime.text = minutes.ToString("00") 
                + "\'" + seconds.ToString("00") 
                + "\"" + milli.ToString("00");

            highScore.text = stageData.highScore.ToString("###,###,##0");
        } else {
            bestTime.text = "--\'--\"--";
            highScore.text = "-";
        }
        
        rank.sprite = rankSprites[stageData.bestRank];
        rank.gameObject.SetActive(stageData.cleared);
    }
}
