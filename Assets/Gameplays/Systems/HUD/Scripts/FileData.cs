using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileData : MonoBehaviour
{
    public int fileNo;
    [Header("セーブデータ")]
    public SaveManager saveManager;
    [Header("データなし")]
    public GameObject newTxt;
    [Header("データあり")]
    public GameObject usedFile;
    [Header("UI")]
    public Text fileNumText;
    public Text currentWorld;
    public PlayerPrefabs playerInfos;
    public Image livesIcon;
    public Text lives;
    public Text greenStarsCount;
    public Text sRankCount;

    private SaveManager.GlobalData data;
    private List<SaveManager.StageData> stageData;

    // Start is called before the first frame update
    void Awake()
    {
        data = saveManager.LoadAndGetGlobal(fileNo);
        stageData = saveManager.LoadAndGetEntireStages(fileNo);

        fileNumText.text = "ファイル " + (fileNo + 1).ToString();

        if (data != null) {
            newTxt.SetActive(false);
            usedFile.SetActive(true);

            livesIcon.sprite = playerInfos.playerData[data.playerID].livesIcon;
            lives.text = data.playerLives.ToString();

            int g = 0;
            int s = 0;
            foreach (SaveManager.StageData stage in stageData) {
                foreach (bool star in stage.greenStars) {
                    if (star) {
                        g++;
                    }
                }
                if (stage.bestRank == 6) {
                    s++;
                }
            }

            greenStarsCount.text = g.ToString();
            sRankCount.text = s.ToString();
        } else {
            newTxt.SetActive(true);
            usedFile.SetActive(false);
        }
    }
}
