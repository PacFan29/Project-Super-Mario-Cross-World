using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;

public class Results : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    public StageInfos stageInfos;
    public Options options;
    public SaveManager saveManager;
    [Header("移動グループ")]
    public RectTransform[] group1st;
    public RectTransform[] group2nd;
    public RectTransform[] group3rd;
    public RectTransform[] group4th;
    public RectTransform[] group5th;
    public RectTransform[] group6th;
    public RectTransform[] group7th;
    [Header("アニメーションカーブ")]
    public AnimationCurve increaseAnim;
    [Header("ワールドA-B")]
    public Text worldDisp;
    [Header("グループ")]
    public GameObject scoreAttackOnly;
    public GameObject bossStageOnly;
    public GameObject scoreGroup;
    public GameObject thirdGroup;

    [Header("ヘッダー")]
    public Text CoinsHeader;
    public Text CoinsHeaderEN;
    public Text MaxComboHeader;
    public Text MaxComboHeaderEN;

    [Header("[スコアアタック]")]
    public Text ScoreText;
    [Header("タイム")]
    public Text TimeHeader;
    public Text TimeHeaderEN;
    public Text TimeText;
    public Text TimeBonus;
    public GameObject timeNewRecord;
    [Header("チェーン")]
    public GameObject[] chainCrowns = new GameObject[3];
    [Header("コイン・クッキー・ネジ・リング")]
    public Text CoinsText;
    public Text CoinBonus;
    [Header("最大コンボ")]
    public Text MaxComboText;
    public Text MaxComboBonus;
    [Header("合計スコア")]
    public Text TotalScore;
    public Image scoreBar;
    public Image scoreBar2;
    public GameObject rankLinesEasy;
    public GameObject rankLinesNormal;
    public GameObject scoreNewRecord;
    [Header("[ボスステージ]")]
    public Text PerfectHeader;
    public Text PerfectHeaderEN;
    public Text PerfectBonus;
    public LocalizedStringTable perfectHeaderTable;
	UnityEngine.Localization.Tables.StringTable perfectHeadPairs;
    public LocalizedStringTable perfectHeaderTableEN;
	UnityEngine.Localization.Tables.StringTable perfectHeadPairsEN;

    [Header("ランク")]
    public Sprite[] RankSprites = new Sprite[7];
    public GameObject RankObj;
    public AudioSource rankSoundSource;
    public Text NextRank;
    private uint[] bonuses = new uint[3];
    private uint[] bonusDisps = new uint[3];
    private uint[] uppers = new uint[3];
    private uint[] currents = new uint[3];
    private float[] curves = new float[3];
    private bool[] minuses = new bool[3];

    private int animNo;
    private float time;
    private GameObject[] PopUps = new GameObject[6];
    private float[] totalDistances = new float[6];

    private bool bossStage;
    private bool sonicStage;
    private bool sonicBossStage;
    [Header("ランクのエフェクト")]
    public GameObject fRankSmokes;
    public GameObject sRankShine;
    public GameObject bRankSparkles;
    public GameObject aRankSparkles;
    public GameObject sRankSparkles;

    [Header("ＢＧＭ・効果音")]
    // public AudioClip[] musics = new AudioClip[3];
    public AudioClip[] sounds = new AudioClip[6];
    private AudioSource musicPlayer;

    private Animator rankAnim;
    private int rankIndex = 0;
    private int bonusCoins;

    [Header("コインゲット")]
    public Text earnedCoins;
    public Text rankBonusCoins;
    public Image currentRank;
    public Text totalCoins;
    public Text coinGet;
    public Text coinsAmount;
    public MessageBox totalCoinsBox;
    public MessageBox coinGetBox;
    // Start is called before the first frame update
    void Awake()
    {
        saveManager.LoadSingleStage(Int32.Parse(data.stageInfo["worldNo"]), Int32.Parse(data.stageInfo["stageNo"]));

        sonicStage = (data.HUDType == GameType.Sonic);
        bossStage = (data.HUDType == GameType.Boss) || (data.HUDType == GameType.BossRush);
        sonicBossStage = (data.HUDType == GameType.SonicBoss);

        musicPlayer = GetComponent<AudioSource>();
        rankAnim = GetComponent<Animator>();
        for (int i = 0; i < bonuses.Length; i++){
            bonuses[i] = 0;
        }
        if (data.HUDType == GameType.Shooting) {
            uppers[0] = (uint)GameManager.maxChain;
        } else {
            uppers[0] = (uint)GameManager.TotalTime;
            // if (options.hardMode) {
            if (options.difficulty == Options.Difficulty.Hard) {
                uint maxTime = (uint)(GameManager.targetTime * 0.5f);
                if (bossStage || sonicBossStage) {
                    maxTime = GameManager.targetTime + (uint)((float)Math.Floor(Math.Min(60000f, GameManager.targetTime / 2f) / 5000f) * 5000 * 2);
                }
                uppers[0] = maxTime - (uint)GameManager.TotalTime;
                if (uppers[0] < 0) {
                    uppers[0] = 0;
                }
            }
        }
        if (sonicBossStage) {
            uppers[1] = GameManager.Coins;
        } else if (bossStage) {
            if (GameManager.noMiss) {
                if (GameManager.damageTaken > 0) {
                    uppers[1] = 5000;
                } else {
                    uppers[1] = 10000;
                }
            } else {
                uppers[1] = 0;
            }
        } else {
            if (sonicStage){
                uppers[1] = GameManager.Coins;
                uppers[2] = (uint)Math.Ceiling(GameManager.averageSpeed * 10);
            } else {
                uppers[1] = GameManager.damageTaken;
                uppers[2] = GameManager.maxCombo;
            }
        }

        minuses[0] = data.HUDType != GameType.Shooting;
        minuses[1] = !(sonicStage || bossStage || sonicBossStage);
        minuses[2] = false;

        currents[0] = (uint)(minuses[0] ? 5999999 : 0);
        currents[1] = (uint)(minuses[1] ? 99 : 0);
        currents[2] = 0;

        animNo = 0;

        //ボスステージ判定
        scoreAttackOnly.SetActive(!bossStage);
        bossStageOnly.SetActive(bossStage);
        scoreGroup.SetActive(!sonicBossStage);
        thirdGroup.SetActive(!sonicBossStage);

        // if (sonicStage || sonicBossStage){
        //     musicPlayer.clip = musics[1];
        // } else {
        //     musicPlayer.clip = musics[0];
        // }
        //musicPlayer.clip = musics[2];
        musicPlayer.Play();
        // musicPlayer.volume = options.musicVolume * 0.7f;
        // musicPlayer.volume = 0.7f;

        data.LevelComplete();

        GroupInit(group1st, 0f);
        GroupInit(group2nd, 1000f);
        GroupInit(group3rd, 2000f);
        GroupInit(group4th, 3000f);
        GroupInit(group5th, 4000f);
        GroupInit(group6th, 5000f);
        GroupInit(group7th, 6000f);
    }

    // Update is called once per frame
    void Update()
    {
        int world = Int32.Parse(data.stageInfo["worldNo"]);
        int stage = Int32.Parse(data.stageInfo["stageNo"]);

        //ワールドA-B
        worldDisp.text = "WORLD " + (world + 1).ToString() + "-" + (stage + 1).ToString();

        string timeFormat = getTimeFormat(currents[0]);
        uint left = 0;
        
        //タイムボーナス(1 - (現在のタイム ÷ 目標タイム) × 20,000点)
        int maxTimeBonus = sonicStage ? 60000 : 30000;
        // if (options.hardMode) {
        // if (options.difficulty == Options.Difficulty.Hard) {
        //     maxTimeBonus = sonicStage ? 99999 : 50000;
        //     bonuses[0] = (uint)Math.Max(0, (int)Math.Ceiling((1f - ((float)currents[0] / (GameManager.targetTime * 0.5f))) * maxTimeBonus));
        // } else {
        //     bonuses[0] = (uint)Math.Max(0, (int)Math.Ceiling((1f - ((float)currents[0] / GameManager.targetTime)) * maxTimeBonus));
        // }
        if (options.difficulty == Options.Difficulty.Hard) {
            maxTimeBonus = sonicStage ? 99999 : 50000;
            bonuses[0] = (uint)Math.Max(0, (int)Math.Ceiling((1f - ((float)uppers[0] / (GameManager.targetTime * 0.5f))) * maxTimeBonus));
        } else {
            bonuses[0] = (uint)Math.Max(0, (int)Math.Ceiling((1f - ((float)uppers[0] / GameManager.targetTime)) * maxTimeBonus));
        }

        if (bossStage || sonicBossStage) {
            // if (currents[0] > GameManager.targetTime) {
            //     float timeRemained = ((float)Math.Floor(Math.Min(60000f, GameManager.targetTime / 2f) / 5000f) * 5000 * 4);
            //     // if (options.hardMode) {
            //     if (options.difficulty == Options.Difficulty.Hard) {
            //         timeRemained /= 2f;
            //     }
            //     bonuses[0] = (uint)Math.Max(0, Math.Ceiling((1f - (((float)currents[0] - GameManager.targetTime) / timeRemained)) * 40000));
            // } else {
            //     bonuses[0] = (uint)Math.Max(0, Math.Ceiling((1f - ((float)currents[0] / GameManager.targetTime)) * 10000)) + 40000;
            // }
            if (uppers[0] > GameManager.targetTime) {
                float timeRemained = ((float)Math.Floor(Math.Min(60000f, GameManager.targetTime / 2f) / 5000f) * 5000 * 4);
                // if (options.hardMode) {
                if (options.difficulty == Options.Difficulty.Hard) {
                    timeRemained /= 2f;
                }
                bonuses[0] = (uint)Math.Max(0, Math.Ceiling((1f - (((float)uppers[0] - GameManager.targetTime) / timeRemained)) * 40000));
            } else {
                bonuses[0] = (uint)Math.Max(0, Math.Ceiling((1f - ((float)uppers[0] / GameManager.targetTime)) * 10000)) + 40000;
            }
        }
        //最大チェーンボーナス
        if (data.HUDType == GameType.Shooting) {
            // if (currents[0] == 1110) {
            //     bonuses[0] = 99999;
            // } else {
            //     bonuses[0] = currents[0] * 50;
            // }
            if (uppers[0] == 1110) {
                bonuses[0] = 99999;
            } else {
                bonuses[0] = uppers[0] * 50;
            }
        }
        
        perfectHeadPairs = perfectHeaderTable.GetTable();
        perfectHeadPairsEN = perfectHeaderTableEN.GetTable();

        /* 出力 */
        if (data.HUDType == GameType.Shooting) {
            // TimeHeader.text = "最大チェーン";
            TimeHeader.text = "最大チェーン";
            TimeHeaderEN.text = "最大チェーン";
            if (currents[0] >= 1110) {
                TimeText.text = "";
            } else if (currents[0] >= 1100) {
                TimeText.text = "    " + (currents[0] % 10).ToString();
            } else if (currents[0] >= 1000) {
                TimeText.text = "  " + (currents[0] % 100).ToString("00");
            } else {
                TimeText.text = currents[0].ToString("###,###,##0");
            }
            
            chainCrowns[0].SetActive(currents[0] >= 1000);
            chainCrowns[1].SetActive(currents[0] >= 1100);
            chainCrowns[2].SetActive(currents[0] >= 1110);
        } else {
            // TimeHeader.text = "タイム";
            TimeHeader.text = perfectHeadPairs["Results-JP-Time"].Value;
            TimeHeaderEN.text = perfectHeadPairsEN["Results-EN-Time"].Value;
            TimeText.text = timeFormat;

            chainCrowns[0].SetActive(false);
            chainCrowns[1].SetActive(false);
            chainCrowns[2].SetActive(false);
        }
        // TimeBonus.text = bonuses[0].ToString("###,###,##0");
        TimeBonus.text = bonusDisps[0].ToString("###,###,##0");

        if (!bossStage) {
            if (sonicBossStage) {
                if (GameManager.allRings <= 0) {
                    bonuses[1] = 0;
                } else {
                    // bonuses[1] = (uint)Math.Min(Math.Floor((float)currents[1] / (float)GameManager.allRings * 100f) * 100, 10000);
                    bonuses[1] = (uint)Math.Min(Math.Floor((float)uppers[1] / (float)GameManager.allRings * 100f) * 100, 10000);
                }
            } else if (sonicStage){
                //リングボーナス(獲得リング枚数 × 100点)
                // bonuses[1] = (uint)Math.Min(currents[1] * 100, 99999);
                bonuses[1] = (uint)Math.Min(uppers[1] * 100, 99999);

                //平均スピードボーナス(平均スピード × 1.0点)
                // bonuses[2] = (uint)Math.Ceiling((float)currents[2] * 1f);
                bonuses[2] = (uint)Math.Ceiling((float)uppers[2] * 1f);
            } else {
                //被ダメージボーナス(10000点 - ダメージ数 × 1000点)
                // if (currents[1] > 10 || !GameManager.noMiss) {
                if (uppers[1] > 10 || !GameManager.noMiss) {
                    bonuses[1] = 0;
                } else {
                    // bonuses[1] = (uint)(10000 - currents[1] * 1000);
                    bonuses[1] = (uint)(10000 - uppers[1] * 1000);
                }

                //最大コンボボーナス（(最大コンボ * (最大コンボ - 1)) * 100点）
                // bonuses[2] = (uint)Math.Min(Math.Pow(currents[2], 2) * 50, 5000);
                // if (currents[2] >= 12)
                if (uppers[2] >= 12)
                {
                    // bonuses[2] = (uint)(currents[2] * 1000);
                    bonuses[2] = (uint)(uppers[2] * 1000);
                } else
                {
                    // bonuses[2] = (uint)(currents[2] * (currents[2] - 1) * 100);
                    bonuses[2] = (uint)(uppers[2] * (uppers[2] - 1) * 100);
                }
            }

            /* 出力 */
            //スコア
            ScoreText.text = GameManager.Score.ToString("###,###,##0");
            //被ダメージボーナス・リングボーナス
            if (sonicStage || sonicBossStage){
                // CoinsHeader.text = "リング";
                CoinsHeader.text = perfectHeadPairs["Results-JP-Rings"].Value;
                CoinsHeaderEN.text = perfectHeadPairsEN["Results-EN-Rings"].Value;
            } else {
                // CoinsHeader.text = "被ダメージ";
                CoinsHeader.text = perfectHeadPairs["Results-JP-DamageTaken"].Value;
                CoinsHeaderEN.text = perfectHeadPairsEN["Results-EN-DamageTaken"].Value;
            }
            if (sonicBossStage) {
                CoinsText.text = currents[1].ToString("###,###,##0") + "/" + GameManager.allRings.ToString("###,###,##0");
            } else {
                CoinsText.text = currents[1].ToString("###,###,##0");
            }
            // CoinBonus.text = bonuses[1].ToString("###,###,##0");
            CoinBonus.text = bonusDisps[1].ToString("###,###,##0");
            if (sonicStage) {
                //平均スピードボーナス
                // MaxComboHeader.text = "平均スピード";
                MaxComboHeader.text = perfectHeadPairs["Results-JP-AverageSpeed"].Value;
                MaxComboHeaderEN.text = perfectHeadPairsEN["Results-EN-AverageSpeed"].Value;
                MaxComboText.text = ((float)currents[2] / 10f).ToString("###,###,##0.0");
            } else {
                //最大コンボボーナス
                // MaxComboHeader.text = "最大コンボ";
                MaxComboHeader.text = perfectHeadPairs["Results-JP-MaxCombo"].Value;
                MaxComboHeaderEN.text = perfectHeadPairsEN["Results-EN-MaxCombo"].Value;
                MaxComboText.text = currents[2].ToString("###,###,##0");
            }
            // MaxComboBonus.text = bonuses[2].ToString("###,###,##0");
            MaxComboBonus.text = bonusDisps[2].ToString("###,###,##0");
        } else {
            //ノーダメージ・ノーミスボーナス
            // bonuses[1] = currents[1];
            bonuses[1] = uppers[1];

            if (GameManager.noMiss) {
                if (GameManager.damageTaken <= 0) {
                    // PerfectHeader.text = "ノーダメージ";
                    PerfectHeader.text = perfectHeadPairs["Results-JP-NoDamage"].Value;
                    PerfectHeaderEN.text = perfectHeadPairsEN["Results-EN-NoDamage"].Value;
                } else {
                    // PerfectHeader.text = "ノーミス";
                    PerfectHeader.text = perfectHeadPairs["Results-JP-NoMiss"].Value;
                    PerfectHeaderEN.text = perfectHeadPairsEN["Results-EN-NoMiss"].Value;
                }
                // PerfectBonus.text = bonuses[1].ToString("###,###,##0");
                PerfectBonus.text = bonusDisps[1].ToString("###,###,##0");
            } else {
                PerfectHeader.gameObject.SetActive(false);
            }
        }
        //合計スコア
        uint total = GameManager.Score;
        // foreach(uint bonus in bonuses){
        foreach(uint bonus in bonusDisps){
            total += bonus;
        }
        if (total > 999999999) { 
            total = 999999999;
        }
        TotalScore.text = total.ToString("###,###,##0");
        // float scorePercent = ((float)total / GameManager.targetScore) * 100;
        // scoreBar.fillAmount = scorePercent / 100f;
        float scorePercent = ((float)total / GameManager.aRankLine) * 100;
        scoreBar.fillAmount = (float)total / GameManager.sRankLine;
        scoreBar2.fillAmount = (float)GameManager.Score / GameManager.sRankLine;
        // //ランクライン
        if (total >= GameManager.sRankLine){
            //ランクＳ（通常：300コイン　ボス：1,000コイン）
            rankIndex = 6;
        } else if (scorePercent >= 100){
            //ランクＡ（通常：250コイン　ボス：800コイン）
            rankIndex = 5;
            left = GameManager.sRankLine - total;
        } else if (scorePercent >= 80){
            //ランクＢ（通常：200コイン　ボス：400コイン）
            rankIndex = 4;
            left = (uint)Math.Round(GameManager.aRankLine * 1.0f) - total;
        } else if (scorePercent >= 60){
            //ランクＣ（通常：150コイン　ボス：200コイン）
            rankIndex = 3;
            left = (uint)Math.Round(GameManager.aRankLine * 0.8f) - total;
        } else if (scorePercent >= 40){
            //ランクＤ（通常：100コイン　ボス：100コイン）
            rankIndex = 2;
            left = (uint)Math.Round(GameManager.aRankLine * 0.6f) - total;
        } else if (scorePercent >= 20){
            //ランクＥ（通常：50コイン　ボス：50コイン）
            rankIndex = 1;
            left = (uint)Math.Round(GameManager.aRankLine * 0.4f) - total;
        } else {
            //ランクＦ（通常：0コイン　ボス：0コイン）
            rankIndex = 0;
            left = (uint)Math.Round(GameManager.aRankLine * 0.2f) - total;
        }
        if (bossStage) {
            if (rankIndex > 0) {
                bonusCoins = Math.Min(1000, (int)Math.Pow(2, rankIndex - 1) * 50);
            } else {
                bonusCoins = 0;
            }
        } else {
            bonusCoins = rankIndex * 50;
        }
        // }
        RankObj.GetComponent<Image>().sprite = RankSprites[rankIndex];
        NextRank.text = left.ToString("###,###,##0");

        /* ハイスコア更新等 */
        StageInfos.Stage info = stageInfos.worlds[world].stages[stage];

        info.cleared = true;

        if (total > info.highScore) {
            //ハイスコア
            info.highScore = total;
            scoreNewRecord.SetActive(true);
        }
        if (GameManager.Coins + bonusCoins > info.bestCoins) {
            //ベストコイン
            info.bestCoins = (uint)(GameManager.Coins + bonusCoins);
        }
        if (rankIndex > info.bestRank) {
            //ランク
            info.bestRank = rankIndex;
        }
        if (data.HUDType == GameType.Shooting) {
            if (GameManager.TotalTime < info.bestTime) {
                //ベストタイム
                info.bestTime = (uint)GameManager.TotalTime;
            }
            timeNewRecord.SetActive(false);
        } else {
            if (currents[0] < info.bestTime) {
                //ベストタイム
                info.bestTime = uppers[0];
                timeNewRecord.SetActive(true);
            }
        }

        //グリーンスターの記録
        for (int i = 0; i < info.greenStars.Length; i++) {
            if (GameManager.keyItems[i]) {
                info.greenStars[i] = GameManager.keyItems[i];
            }
        }

                /* アニメーション */
        time += Time.deltaTime;
        switch (animNo){
            case 0:
            case 2:
            case 4:
            //1秒待つ
            if (animNo == 0 && time >= 2f || animNo != 0 && time >= 1f) {
                if (bossStage && animNo == 2){
                    animNo = 7;
                } else {
                    animNo += 1;
                }
            }
            break;

            case 1:
            //タイムボーナス
            curves[0] += 0.5f * Time.deltaTime;
            if (curves[0] >= 1f || (!minuses[0] && uppers[0] <= 0))
            {
                curves[0] = 1f;
                animNo = 2;
                time = 0f;
            }
            if (minuses[0])
            {
                currents[0] = 5999999 - (uint)(increaseAnim.Evaluate(curves[0]) * (5999999 - uppers[0]));
            } else
            {
                currents[0] = (uint)(increaseAnim.Evaluate(curves[0]) * uppers[0]);
            }
            bonusDisps[0] = (uint)(increaseAnim.Evaluate(curves[0]) * bonuses[0]);
            // currents[0] = valueAnim((int)currents[0], (int)uppers[0]);
            // if (currents[0] == uppers[0]){
            //     animNo = 2;
            //     time = 0f;
            // }
            break;

            case 3:
            //被ダメージボーナス・リングボーナス
            curves[1] += 1f * Time.deltaTime;
            if (curves[1] >= 1f || (!minuses[1] && uppers[1] <= 0))
            {
                curves[1] = 1f;
                if (sonicBossStage) {
                    animNo = 6;
                } else {
                    animNo = 4;
                }
                time = 0f;
            }
            if (minuses[1])
            {
                currents[1] = 99 - (uint)(increaseAnim.Evaluate(curves[1]) * (99 - uppers[1]));
            } else
            {
                currents[1] = (uint)(increaseAnim.Evaluate(curves[1]) * uppers[1]);
            }
            bonusDisps[1] = (uint)(increaseAnim.Evaluate(curves[1]) * bonuses[1]);
            // currents[1] = valueAnim((int)currents[1], (int)uppers[1]);
            // if (currents[1] == uppers[1]){
            //     if (sonicBossStage) {
            //         animNo = 6;
            //     } else {
            //         animNo = 4;
            //     }
            //     time = 0f;
            // }
            break;

            case 5:
            //最大コンボボーナス・平均スピードボーナス
            curves[2] += 0.5f * Time.deltaTime;
            if (curves[2] >= 1f || uppers[2] <= 0f)
            {
                animNo = 6;
                time = 0f;
            }
            currents[2] = (uint)(increaseAnim.Evaluate(curves[2]) * uppers[2]);
            bonusDisps[2] = (uint)(increaseAnim.Evaluate(curves[2]) * bonuses[2]);
            // currents[2] = valueAnim((int)currents[2], (int)uppers[2]);
            // if (currents[2] == uppers[2]){
            //     animNo = 6;
            //     time = 0f;
            // }
            break;

            case 6:
            if (Input.GetButtonDown("A")) {
                animNo = 8;
                totalCoinsBox.display = 1;
            }
            break;

            case 7:
            //ノーダメージ・ノーミスボーナス
            curves[1] += 1f * Time.deltaTime;
            if (curves[1] >= 1f || (!minuses[1] && uppers[1] <= 0))
            {
                curves[1] = 1f;
                animNo = 6;
                time = 0f;
            }
            if (minuses[1])
            {
                currents[1] = 99 - (uint)(increaseAnim.Evaluate(curves[1]) * (99 - uppers[1]));
            } else
            {
                currents[1] = (uint)(increaseAnim.Evaluate(curves[1]) * uppers[1]);
            }
            bonusDisps[1] = (uint)(increaseAnim.Evaluate(curves[1]) * bonuses[1]);
            // currents[1] = valueAnim((int)currents[1], (int)uppers[1]);
            // if (currents[1] == uppers[1]){
            //     animNo = 6;
            //     time = 0f;
            // }
            break;
            
            case 8:
            if (Input.GetButtonDown("A")) {
                data.coins += GameManager.Coins;
                data.coins += (uint)bonusCoins;
                if (data.coins > 999999) data.coins = 999999;

                totalCoinsBox.display = -1;
                coinGetBox.display = 1;
                animNo = 9;
            }
            break;
            
            case 9:
            if (Input.GetButtonDown("A")) {
                saveManager.SaveGlobal();
                saveManager.SaveStageInfo(world, stage);
                
                GameManager.noMiss = true;
                GameManager.damageTaken = 0;
                GameManager.ResetEntire();
                data.HUDType = GameType.Normal;
                SceneManager.LoadScene("WorldMap");
            }
            break;
        }
        if (Input.GetButtonDown("A") && animNo != 6 && animNo < 8){
            curves[0] = 1f;
            curves[1] = 1f;
            curves[2] = 1f;
            currents[0] = uppers[0];
            currents[1] = uppers[1];
            currents[2] = uppers[2];
            bonusDisps[0] = bonuses[0];
            bonusDisps[1] = bonuses[1];
            bonusDisps[2] = bonuses[2];
            animNo = 6;

            GroupSkip(group1st);
            GroupSkip(group2nd);
            GroupSkip(group3rd);
            GroupSkip(group4th);
            GroupSkip(group5th);
            GroupSkip(group6th);
            GroupSkip(group7th);
        }
        rankAnim.SetBool("rankShow", (animNo == 6));

        //コインゲット
        earnedCoins.text = GameManager.Coins.ToString("###,###,##0");
        rankBonusCoins.text = (bonusCoins).ToString("###,###,##0");
        currentRank.sprite = RankObj.GetComponent<Image>().sprite;
        totalCoins.text = (GameManager.Coins + bonusCoins).ToString("###,###,##0");
        coinGet.text = (GameManager.Coins + bonusCoins).ToString("###,###,##0") + "コイン ゲット!";
        coinsAmount.text = data.coins.ToString("###,###,##0");
    }
    void FixedUpdate()
    {
        GroupMove(group1st);
        GroupMove(group2nd);
        GroupMove(group3rd);
        GroupMove(group4th);
        GroupMove(group5th);
        GroupMove(group6th);
        GroupMove(group7th);
    }

    // public uint valueAnim(int current, int goal){
    //     int up = (int)Math.Ceiling(((float)(goal - current) * 5) * Time.deltaTime);
    //     if (Math.Abs(up) <= 0 && goal != current){
    //         if ((int)(goal - current) > 0){
    //             up = 1;
    //         } else {
    //             up = -1;
    //         }
    //     }
    //     current += up;
    //     return (uint)current;
    // }
    public string getTimeFormat(uint totalTime){
        double minutes = totalTime / 60000;
        double seconds = (totalTime / 1000) % 60;
        double milli = (totalTime % 1000) / 10;
        return minutes.ToString("00") 
            + "\'" + seconds.ToString("00") 
            + "\"" + milli.ToString("00");
    }
    public void rankSound(){
        rankSoundSource.PlayOneShot(sounds[2]);
        switch (rankIndex){
            case 0:
                //ランクＦ
                fRankSmokes.SetActive(true);
                rankSoundSource.PlayOneShot(sounds[3]);
                break;
            case 4:
                //ランクＢ
                bRankSparkles.SetActive(true);
                break;
            case 5:
            //ランクＡ
                rankSoundSource.PlayOneShot(sounds[4]);
                aRankSparkles.SetActive(true);
                break;
            case 6:
                //ランクＳ
                rankSoundSource.PlayOneShot(sounds[5]);
                sRankShine.SetActive(true);
                sRankSparkles.SetActive(true);
                break;
        }
    }

    void GroupInit(RectTransform[] groups, float offset)
    {
        foreach (RectTransform group in groups)
        {
            Vector3 pos = group.anchoredPosition;
            group.anchoredPosition = new Vector3(1200 + offset, pos.y, pos.z);
        }
    }
    void GroupMove(RectTransform[] groups)
    {
        foreach (RectTransform group in groups)
        {
            Vector3 pos = group.anchoredPosition;
            group.anchoredPosition = new Vector3(pos.x * (45f*Time.fixedDeltaTime), pos.y, pos.z);
        }
    }
    void GroupSkip(RectTransform[] groups)
    {
        foreach (RectTransform group in groups)
        {
            Vector3 pos = group.anchoredPosition;
            group.anchoredPosition = new Vector3(0f, pos.y, pos.z);
        }
    }
}
