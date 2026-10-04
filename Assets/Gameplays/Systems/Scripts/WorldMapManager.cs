using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapManager : MonoBehaviour
{
    [Header("データ")]
	public GameData data;
    public Options options;
    public SaveManager saveManager;
    [Header("クリアリスト")]
    public Text clearListTitle;
    public RectTransform clearListParent;
    public Transform clearData;
    private int clearListWorldNum = 0;

    private int currentStageCount = -1;

    public bool canPause = true;
    public bool canPlayerInput = true;
    // Start is called before the first frame update
    void Awake()
    {
        saveManager.LoadOptions();
        saveManager.LoadGlobal();
        saveManager.LoadEntireStages();

        saveManager.SaveGlobal();

        bool finalCleared = false;

        for (int w = 0; w < saveManager.stageInfos.worlds.Length; w++) {
            for (int s = 0; s < saveManager.stageInfos.worlds[w].stages.Length; s++) {
                if (finalCleared && !saveManager.stageInfos.worlds[w].stages[s].cleared) {
                    if (!saveManager.stageInfos.worlds[w].stages[s].unlocked)
                    {
                        saveManager.stageInfos.worlds[w].stages[s].unlocked = true;
                        saveManager.SaveStageInfo(w, s);
                    }
                    break;
                }

                if (saveManager.stageInfos.worlds[w].stages[s].cleared) {
                    finalCleared = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // clearListTitle.text = "WORLD " + (clearListWorldNum + 1).ToString() + " クリアリスト";
        clearListTitle.text = "WORLD " + (clearListWorldNum + 1).ToString();

        if (saveManager.stageInfos.worlds[clearListWorldNum].stages.Length != currentStageCount) {
            clearData.gameObject.GetComponent<ClearListData>().worldNum = clearListWorldNum;
            clearData.gameObject.GetComponent<ClearListData>().stageNum = 0;

            currentStageCount = saveManager.stageInfos.worlds[clearListWorldNum].stages.Length;

            if (currentStageCount > 0) {
                foreach (Transform obj in clearListParent) {
                    if ( 0 <= obj.gameObject.name.LastIndexOf("Clone") ) {
                        Destroy(obj.gameObject);
                    }
                }
                    
                for ( int i = 1 ; i < currentStageCount; i++ ) {
                    RectTransform keyRect = (RectTransform)Instantiate(clearData).transform;
                    keyRect.SetParent(clearListParent, false);
                    keyRect.localPosition = clearData.localPosition;

                    keyRect.localPosition = new Vector2(
                        keyRect.localPosition.x,
                        keyRect.localPosition.y - ((keyRect.sizeDelta.y + 7f) * i)
                    );

                    keyRect.gameObject.GetComponent<ClearListData>().worldNum = clearListWorldNum;
                    keyRect.gameObject.GetComponent<ClearListData>().stageNum = i;
                }
            }
        }

        canPause = true;
        foreach (StagePin pin in FindObjectsByType<StagePin>(FindObjectsSortMode.None))
        {
            if (pin.placed)
            {
                canPause = false;
            }
        }

        // if (Input.GetButtonDown("Start") && !GameManager.isPaused && canPause)
        if (Input.GetButtonDown("Start") && canPause)
		{
			//ポーズ時にタイムを表示
			GameManager.isPaused = !GameManager.isPaused;
		}

        canPlayerInput = WorldMapMainUI.step == 0;
    }
}
