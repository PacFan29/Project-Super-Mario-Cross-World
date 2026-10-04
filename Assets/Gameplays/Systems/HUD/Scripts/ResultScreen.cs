using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;

public class ResultScreen : MonoBehaviour
{
    public AudioSource source;

    [Header("データ")]
    public GameData data;
    public StageInfos stageInfos;
    public Options options;
    public SaveManager saveManager;
    [Header("リザルト表示")]
    public Text resultHeader;
    [Header("クラス一覧")]
    public List<ResultBonus.BonusItem> bonusList = new List<ResultBonus.BonusItem>();
    [Header("項目一覧")]
    public List<int> normalStageItems = new List<int>();
    public List<int> sonicStageItems = new List<int>();
    public List<int> bossStageItems = new List<int>();
    public List<int> sonicBossStageItems = new List<int>();
    public List<int> shootingStageItems = new List<int>();
    public List<int> ringAttackNormalItems = new List<int>();
    public List<int> ringAttackSonicItems = new List<int>();
    [Header("項目")]
    public GameObject bonusItem;
    private List<ResultBonus> bonusItemList = new List<ResultBonus>();
    [Header("多言語対応")]
    public LocalizedStringTable JPScoreHeaderTable;
    public LocalizedStringTable ENScoreHeaderTable;
	UnityEngine.Localization.Tables.StringTable jpScoreHeadPairs;
	UnityEngine.Localization.Tables.StringTable enScoreHeadPairs;
    [Header("合計スコア")]
    public Text jpScoreHeader;
    public Text enScoreHeader;
    public Text scoreAmm;
    public Image progressFToA;
    public Image progressS;
    public Color[] progressColors = new Color[6];
    [Header("新記録")]
    public GameObject newRecord;
    [Header("タイムアタックのみ")]
    public Sprite[] stopwatchSprites = new Sprite[5];
    public Transform stopwatchesParent;
    public GameObject stopwatch;
    public AudioClip guageSound;
    public AudioClip timeDecreased;
    [Header("アニメーションカーブ")]
    public AnimationCurve increaseAnim;
    [Header("ランク")]
    public AudioClip commonRankSound;
    public AudioClip commonRankSound2;
    public AudioClip FRankSound;
    public AudioClip ARankSound;
    public AudioClip SRankSound;
    public AudioClip SRankSound2;
    public Sprite[] RankSprites = new Sprite[7];
    public GameObject RankObj;

    private StageInfos.Stage stageInfo;
    private int step = 0;
    private float waitTime = 0.5f;

    private float timeDisplay;
    private float timeDispAdd = 0f;

    private int bonusCoins;
    
    [Header("コインゲット")]
    public RectTransform coinGetGroup;
    public Text earnedCoins;
    public Text rankBonusCoins;
    public Image currentRank;
    public Text totalCoins;
    public Text coinGet;
    public Text coinsAmount;
    public MessageBox totalCoinsBox;
    public MessageBox coinGetBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int world = Int32.Parse(data.stageInfo["worldNo"]);
        int stage = Int32.Parse(data.stageInfo["stageNo"]);

        saveManager.LoadSingleStage(world, stage);

        stageInfo = stageInfos.worlds[world].stages[stage];

        List<int> items = new List<int>();
        switch (data.courseMode) {
            case CourseMode.ScoreAttack:
                switch (data.HUDType)
                {
                    case GameType.Normal:
                        items = new List<int>(normalStageItems);
                        break;
                        
                    case GameType.Boss:
                        items = new List<int>(bossStageItems);
                        break;

                    case GameType.Sonic:
                        items = new List<int>(sonicStageItems);
                        break;

                    case GameType.SonicBoss:
                        items = new List<int>(sonicBossStageItems);
                        break;

                    case GameType.Shooting:
                        items = new List<int>(shootingStageItems);
                        break;
                }
            break;

            case CourseMode.RingAttack:
                switch (data.HUDType)
                {
                    case GameType.Normal:
                        items = new List<int>(ringAttackNormalItems);
                        break;
                        
                    case GameType.Sonic:
                        items = new List<int>(ringAttackSonicItems);
                        break;
                }
            break;
        }

        if (items.Count < 1)
        {
            return;
        }
        Vector3 itemPos = bonusItem.transform.localPosition;
        bonusItem.GetComponent<ResultBonus>().info = bonusList[items[0]];
        bonusItem.GetComponent<ResultBonus>().targetTime = (int)stageInfo.targetTime;
        bonusItem.GetComponent<ResultBonus>().maxTimeBonus = (int)stageInfo.maxTimeBonus;
        bonusItem.GetComponent<ResultBonus>().maxDamageCount = (int)stageInfo.maxDamageCount;
        bonusItem.GetComponent<ResultBonus>().isRingAttack = (data.courseMode == CourseMode.RingAttack);
        bonusItemList.Add(bonusItem.GetComponent<ResultBonus>());
        
        GameObject it;
        ResultBonus itB;

        itemPos -= Vector3.up * 40f;
        for (int i = 1; i < items.Count; i++)
        {
            it = Instantiate(bonusItem, Vector3.zero, Quaternion.identity, this.transform);
            it.transform.localPosition = itemPos;

            itB = it.GetComponent<ResultBonus>();
            itB.info = bonusList[items[i]];
            itB.targetTime = (int)stageInfo.targetTime;
            itB.maxTimeBonus = (int)stageInfo.maxTimeBonus;
            itB.maxDamageCount = (int)stageInfo.maxDamageCount;
            itB.isRingAttack = (data.courseMode == CourseMode.RingAttack);
            
            if (!bonusList[items[i]].hideZeroValue || (itB.GetBonus() > 0 && bonusList[items[i]].hideZeroValue))
            {
                bonusItemList.Add(itB);
                itemPos -= Vector3.up * 40f;
            }
        }

        step = 0;
        waitTime = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        int world = Int32.Parse(data.stageInfo["worldNo"]);
        int stage = Int32.Parse(data.stageInfo["stageNo"]);

        jpScoreHeadPairs = JPScoreHeaderTable.GetTable();
        enScoreHeadPairs = ENScoreHeaderTable.GetTable();

        // resultHeader.text = "WORLD " + (world + 1).ToString() + "-" + (stage + 1).ToString() + " Result";
        resultHeader.text = enScoreHeadPairs["EN-HUD-World"].Value + " " + 
            (world + 1).ToString() + "-" + (stage + 1).ToString() + 
            " " + enScoreHeadPairs["EN-HUD-Result"].Value;

        int allBonuses = 0;
        foreach (ResultBonus b in bonusItemList)
        {
            allBonuses += b.bonusDisp;
        }
        
        int totalScore = 0;
        switch (data.courseMode) {
            case CourseMode.ScoreAttack:
                totalScore = (int)(GameManager.Score + allBonuses);
                jpScoreHeader.text = jpScoreHeadPairs["Results-JP-Score"].Value;
                enScoreHeader.text = enScoreHeadPairs["Results-EN-Score"].Value;
                scoreAmm.text = totalScore.ToString("###,###,##0");
                break;

            case CourseMode.TimeAttack:
                jpScoreHeader.text = jpScoreHeadPairs["Results-JP-Time"].Value;
                enScoreHeader.text = enScoreHeadPairs["Results-EN-Time"].Value;
                // scoreAmm.text = getTimeFormat((uint)GameManager.TotalTime);
                scoreAmm.text = getTimeFormat((uint)timeDisplay);
                break;

            case CourseMode.RingAttack:
                totalScore = (int)Math.Round(GameManager.Coins * (1f + (float)allBonuses / 100f));
                jpScoreHeader.text = jpScoreHeadPairs["Results-JP-Rings"].Value;
                enScoreHeader.text = enScoreHeadPairs["Results-EN-Rings"].Value;
                scoreAmm.text = totalScore.ToString("###,###,##0");
                break;
        }

        //ランク
        int rankIndex = 0;
        uint sRankLine = stageInfo.sRankLine;
        uint aRankLine = stageInfo.aRankLine;
        float timeAttackTargetTime = stageInfo.timeAttackTargetTime * 1000f;
        if (data.courseMode == CourseMode.RingAttack) {
            sRankLine = stageInfo.ringsSRankLine;
            aRankLine = stageInfo.ringsARankLine;
        }
        
        int rankInterval = 0;
        if (data.courseMode == CourseMode.TimeAttack) {
            rankInterval = (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss) ? 5 : 10;
            if (timeDisplay <= timeAttackTargetTime)
            {
                rankIndex = 6;
            } else if (timeDisplay <= timeAttackTargetTime + rankInterval * 1000)
            {
                rankIndex = 5;
            } else if (timeDisplay <= timeAttackTargetTime + rankInterval * 3000)
            {
                rankIndex = 4;
            } else if (timeDisplay <= timeAttackTargetTime + rankInterval * 5000)
            {
                rankIndex = 3;
            } else if (timeDisplay <= timeAttackTargetTime + rankInterval * 7000)
            {
                rankIndex = 2;
            } else if (timeDisplay <= timeAttackTargetTime + rankInterval * 9000)
            {
                rankIndex = 1;
            } else
            {
                rankIndex = 0;
            }
        } else {
            if (totalScore >= sRankLine)
            {
                rankIndex = 6;
            } else if (totalScore >= aRankLine)
            {
                rankIndex = 5;
            } else if (totalScore >= (aRankLine * 0.8))
            {
                rankIndex = 4;
            } else if (totalScore >= (aRankLine * 0.6))
            {
                rankIndex = 3;
            } else if (totalScore >= (aRankLine * 0.4))
            {
                rankIndex = 2;
            } else if (totalScore >= (aRankLine * 0.2))
            {
                rankIndex = 1;
            } else
            {
                rankIndex = 0;
            }
        }

        //ボーナスコイン
        if (data.HUDType == GameType.Boss || data.HUDType == GameType.SonicBoss) {
            if (rankIndex > 0) {
                bonusCoins = Math.Min(1000, (int)Math.Pow(2, rankIndex - 1) * 50);
            } else {
                bonusCoins = 0;
            }
        } else {
            bonusCoins = rankIndex * 50;
        }
        
        //バー
        progressFToA.fillAmount = Math.Min(1f, (float)totalScore / (float)aRankLine);
        progressFToA.color = progressColors[Math.Min(5, rankIndex)];
        progressS.fillAmount = Math.Min(1f, Math.Max(0f, (float)(totalScore - aRankLine) / (float)(sRankLine - aRankLine)));
        if (data.courseMode == CourseMode.TimeAttack) {
            progressFToA.fillAmount = Mathf.Clamp((timeAttackTargetTime + rankInterval * 11000 - timeDisplay) / (rankInterval * 10000), 0f, 1f);
            progressS.fillAmount = Mathf.Clamp((timeAttackTargetTime + rankInterval * 1000 - timeDisplay) / (rankInterval * 1000), 0f, 1f);
        }

        if (data.courseMode == CourseMode.TimeAttack && step == 0 && GameManager.stopwatches.Count < 1) {
            source.clip = guageSound;
            source.Play();
            step = 2;
            waitTime = 2.5f;

            if (GameManager.TotalTime > timeAttackTargetTime + rankInterval * 9000) {
                timeDispAdd = 5999999f - GameManager.TotalTime;
            } else {
                timeDispAdd = timeAttackTargetTime + rankInterval * 9000 - GameManager.TotalTime;
            }
        }
        if (data.courseMode == CourseMode.TimeAttack) {
            switch (step) {
                case 0:
                    timeDisplay = GameManager.TotalTime;
                    break;

                case 2:
                    timeDisplay = GameManager.TotalTime + timeDispAdd * (1f - increaseAnim.Evaluate((2.5f - waitTime) / 2.5f));
                    break;
                    
                case 3:
                    timeDisplay = GameManager.TotalTime + timeDispAdd * (1f - increaseAnim.Evaluate((2f - waitTime) / 0.5f));
                    break;
            }
        }

        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        if (waitTime <= 0f)
        {
            if (data.courseMode == CourseMode.TimeAttack) {
                switch (step)
                {
                    case 0:
                        step = 1;
                        waitTime = 9f;
                        StartCoroutine("ShowThoseStopwatches");
                        break;
                    
                    case 1:
                        step = 3;
                        waitTime = 2f;
                        source.PlayOneShot(timeDecreased);
                        break;
                        
                    case 2:
                        source.Stop();
                        step = 3;
                        waitTime = 1f;
                        break;
                    
                    case 3:
                        step = 6;
                        StartCoroutine(ShowRank(rankIndex));
                        break;
                }
            } else {
                switch (step)
                {
                    case 0:
                        if (bonusItemList.Count > 0)
                        {
                            step = 1;
                            bonusItemList[0].gameObject.SetActive(true);
                        } else
                        {
                            step = 5;
                        }
                        waitTime = 1f;
                        break;
                    
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        if (step < bonusItemList.Count)
                        {
                            bonusItemList[step].gameObject.SetActive(true);
                            step++;
                        } else
                        {
                            step = 5;
                        }
                        waitTime = 1f;
                        break;
                    
                    case 5:
                        step = 6;
                        StartCoroutine(ShowRank(rankIndex));
                        break;
                }
            }
        }
        switch (step) {
            case 6:
                if (Input.GetButtonDown("A")) {
                    coinGetGroup.SetAsLastSibling();
                    step = 7;
                    totalCoinsBox.display = 1;
                }
                break;
                
            case 7:
                if (Input.GetButtonDown("A")) {
                    if (data.courseMode == CourseMode.RingAttack) {
                        data.coins += (uint)totalScore;
                    } else {
                        data.coins += GameManager.Coins;
                    }
                    data.coins += (uint)bonusCoins;
                    if (data.coins > 999999) data.coins = 999999;

                    totalCoinsBox.display = -1;
                    coinGetBox.display = 1;
                    step = 8;
                }
                break;
            
            case 8:
                if (Input.GetButtonDown("A")) {
                    saveManager.SaveGlobal();
                    saveManager.SaveStageInfo(world, stage);
                    
                    GameManager.ResetEntire();
                    data.HUDType = GameType.Normal;
                    SceneManager.LoadScene("WorldMap");
                }
                break;
        }
        
        //コインゲット
        earnedCoins.text = GameManager.Coins.ToString("###,###,##0");
        rankBonusCoins.text = (bonusCoins).ToString("###,###,##0");
        currentRank.sprite = RankObj.GetComponent<Image>().sprite;
        totalCoins.text = (GameManager.Coins + bonusCoins).ToString("###,###,##0");
        coinGet.text = (GameManager.Coins + bonusCoins).ToString("###,###,##0") + "コイン ゲット!";
        coinsAmount.text = data.coins.ToString("###,###,##0");
        if (data.courseMode == CourseMode.RingAttack) {
            earnedCoins.text = totalScore.ToString("###,###,##0");
            totalCoins.text = (totalScore + bonusCoins).ToString("###,###,##0");
            coinGet.text = (totalScore + bonusCoins).ToString("###,###,##0") + "コイン ゲット!";
        }

        /* ハイスコア更新等 */
        if (step < 6) {
            return;
        }
        StageInfos.Stage info = stageInfos.worlds[world].stages[stage];

        switch (data.courseMode) {
            case CourseMode.ScoreAttack:
                info.cleared = true;
                if (totalScore > info.highScore) {
                    //ハイスコア
                    info.highScore = (uint)totalScore;
                    newRecord.SetActive(true);
                }
                if (GameManager.Coins + bonusCoins > info.bestCoins) {
                    //ベストコイン
                    info.bestCoins = (uint)(GameManager.Coins + bonusCoins);
                }
                if (rankIndex > info.bestRank) {
                    //ランク
                    info.bestRank = rankIndex;
                }
                if (GameManager.TotalTime < info.bestTime) {
                    //ベストタイム
                    info.bestTime = (uint)GameManager.TotalTime;
                }

                //グリーンスターの記録
                for (int i = 0; i < info.greenStars.Length; i++) {
                    if (GameManager.keyItems[i]) {
                        info.greenStars[i] = GameManager.keyItems[i];
                    }
                }
                break;
                
            case CourseMode.TimeAttack:
                info.timeCleared = true;
                if (timeDisplay < info.timeAttackBestTime) {
                    //ベストタイム
                    info.timeAttackBestTime = (uint)timeDisplay;
                    newRecord.SetActive(true);
                }
                if (rankIndex > info.timeBestRank) {
                    //ランク
                    info.timeBestRank = rankIndex;
                }
                break;
                
            case CourseMode.RingAttack:
                info.ringCleared = true;
                if (totalScore > info.ringAttackBestRings) {
                    //ベストリング
                    info.ringAttackBestRings = (uint)totalScore;
                    newRecord.SetActive(true);
                }
                if (rankIndex > info.ringBestRank) {
                    //ランク
                    info.ringBestRank = rankIndex;
                }
                break;
        }
    }
    void FixedUpdate()
    {
        if (RankObj.activeInHierarchy)
        {
            RankObj.transform.localScale = Vector3.Lerp(RankObj.transform.localScale, Vector3.one, 0.2f);
        }
    }

    IEnumerator ShowRank(int rankIndex)
    {
        source.PlayOneShot(commonRankSound);
        RankObj.GetComponent<Image>().sprite = RankSprites[rankIndex];
        RankObj.SetActive(true);

        switch (rankIndex)
        {
            case 0:
                //Ｆランク
                source.PlayOneShot(FRankSound);
                break;
                
            case 3:
            case 4:
                //Ｃ～Ｂランク
                source.PlayOneShot(commonRankSound2);
                break;

            case 5:
                //Ａランク
                source.PlayOneShot(commonRankSound2);
                source.PlayOneShot(ARankSound);
                break;

            case 6:
                //Ｓランク
                source.PlayOneShot(commonRankSound2);
                source.PlayOneShot(SRankSound);
                break;
        }

        yield return new WaitForSeconds(0.8f);

        if (rankIndex == 6)
        {
            //Ｓランク獲得時のみ
            source.PlayOneShot(SRankSound2);
        }
    }

    public string getTimeFormat(uint totalTime){
        double minutes = totalTime / 60000;
        double seconds = (totalTime / 1000) % 60;
        double milli = (totalTime % 1000) / 10;
        return minutes.ToString("00") 
            + "\'" + seconds.ToString("00") 
            + "\"" + milli.ToString("00");
    }

    IEnumerator ShowThoseStopwatches() {
        List<Animator> anims = new List<Animator>();
        int totalDecreaseTime = 0;

        int swIndex = 0;
        while (swIndex < GameManager.stopwatches.Count) {
            if (swIndex == 0) {
                stopwatch.SetActive(true);
                stopwatch.GetComponent<Image>().sprite = stopwatchSprites[GameManager.stopwatches[swIndex]];
                anims.Add(stopwatch.GetComponent<Animator>());

                totalDecreaseTime += GameManager.stopwatches[swIndex] + 1;
            } else {
                Transform sw = Instantiate(stopwatch, stopwatchesParent.position, Quaternion.identity, stopwatchesParent).transform;
                sw.localPosition = stopwatch.transform.localPosition + Vector3.right * (swIndex % 16) * 24f + Vector3.down * (swIndex / 16) * 30f;
                sw.gameObject.GetComponent<Image>().sprite = stopwatchSprites[GameManager.stopwatches[swIndex]];
                anims.Add(sw.gameObject.GetComponent<Animator>());

                totalDecreaseTime += GameManager.stopwatches[swIndex] + 1;
            }
            waitTime = 9f;
            float waitSec = Math.Min(0.2f, 1.5f / (float)GameManager.stopwatches.Count);
            yield return new WaitForSeconds(waitSec);
            swIndex++;
        }
        waitTime = 0f;

        foreach (Animator anim in anims) {
            anim.Play("StopwatchHide", 0);
        }

        timeDispAdd = Math.Min(GameManager.TotalTime, totalDecreaseTime * 1000f);
        GameManager.TotalTime -= totalDecreaseTime * 1000f;
        if (GameManager.TotalTime < 0f) {
            GameManager.TotalTime = 0f;
        }
    }
}
