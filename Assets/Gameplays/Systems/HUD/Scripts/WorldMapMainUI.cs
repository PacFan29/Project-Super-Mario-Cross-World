using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapMainUI : MonoBehaviour
{
    [Header("データ")]
    public StageInfos stageInfos;
    public GameData data;
    public Options options;
    public PlayerPrefabs playerPrefs;
    [Header("残機アイコン")]
    public Transform livesGroup;
    public Transform coinsGroup;
    public Image[] livesIcons = new Image[4];
    [Header("テキスト")]
    public Text worldAB;
    public Text percent;
    public Text lives;
    public Text totalCoins;
    public Text greenStars;
    public Text maxGreenStars;
    public Text sRanks;
    public Text maxSRanks;

    public static int step = 0;
    [Header("ステージ情報")]
    public GameObject stageDetailUI;
    // Start is called before the first frame update
    void Start()
    {
        if (!options.lives) {
            coinsGroup.localPosition = livesGroup.localPosition;
            livesGroup.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int world = 0;

        //ワールドA-B
        worldAB.text = "WORLD " + (world+1).ToString();
        //達成率
        percent.text = stageInfos.getPercent(world).ToString() + "% コンプリート";
        //残機
        for (int p = 0; p < 4; p++)
		{
			if (p < GameManager.players.Count)
			{
				livesIcons[p].sprite = playerPrefs.playerData[GameManager.players[p].ID].livesIcon;
				livesIcons[p].gameObject.SetActive(true);
			}
			else
			{
				livesIcons[p].gameObject.SetActive(false);
			}
		}
        lives.text = (data.lives).ToString("###,###,##0");
        lives.gameObject.transform.localPosition = new Vector3(40f + (GameManager.players.Count - 1) * 17.5f, 0f, 0f);
        //コイン
        totalCoins.text = (data.coins).ToString("###,###,##0");
        //グリーンスター
        greenStars.text = stageInfos.getCurrentGreenStars(world).ToString();
        maxGreenStars.text = "/" + stageInfos.getAllGreenStars(world).ToString();
        //Ｓランク
        sRanks.text = stageInfos.getCurrentBestRankCount(world).ToString();
        maxSRanks.text = "/" + stageInfos.getAllBestRankCount(world).ToString();

        stageDetailUI.SetActive(step > 0);
    }
}
