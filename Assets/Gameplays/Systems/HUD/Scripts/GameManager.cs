using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DimensionType {
	Normal3D,
	XWay2D,
	ZWay2D,
    FreeWay3D
}
public class PlayerStatus
{
    public byte ID;
    public uint HP;
    public uint MaxHP;
    // public byte Lives;
    public uint Score;
    public bool skillShow;
    public Sprite skillIcon;
    public Color skillColor;
    public float skillAmounts;
    public float skillConsumption;

    public PlayerStatus(){
        ID = 0;
        MaxHP = 40;
        HP = MaxHP;
        // Lives = 2;
        Score = 0;
        skillShow = false;
        skillIcon = null;
        skillColor = Color.white;
        skillAmounts = 28f;
        skillConsumption = 1f;
    }
    // public void setStatus(byte setID, ushort setMaxHP, ushort setHP, byte setLives, uint setScore, bool showSkill = false, byte skills = 28){
    public void setStatus(
        byte setID, uint setMaxHP, uint setHP,
        uint setScore, bool showSkill = false,
        Sprite icon = null, Color? sColor = null,
        float skills = 28f, float consumption = 1f)
    {
        ID = setID;
        MaxHP = setMaxHP;
        HP = setHP;
        // Lives = setLives;
        Score = setScore;

        skillShow = showSkill;
        skillIcon = icon;
        skillColor = sColor ?? Color.white;
        skillAmounts = skills;
        skillConsumption = consumption;
    }
}

public class GameManager : MonoBehaviour
{
    public static uint Score = 0;
    public static uint Coins = 0;
    public static float TotalTime = 0;
    public static uint Gold = 0; //標準目標：100,000ドル
    public static bool[] keyItems = new bool[7];
    public static byte Lives = 5;

    // public static uint targetScore = 50000; //標準目標：50,000点
    public static uint aRankLine = 45000; //標準目標：45,000点
    public static uint sRankLine = 50000; //標準目標：50,000点
    public static uint targetTime = (600) * 1000; //標準目標：10分

    public static bool timeAction = true;
    public static bool gameOver = false;
    public static bool coins1up = false;
    public static bool death = false;
    public static bool falling = false;
    public static DimensionType dimension = DimensionType.Normal3D; //０＝3D １＝2D(X方向) ２＝2D(Z方向)

    public static byte maxCombo = 0;
    public static byte ghostCombo = 0;
    public static uint checkPointCount = 0;
    public static float currentSpeed = 0f; //速度60 → 1200km/h（20倍）
    public static float totalSpeed = 0f; //速度60 → 1200km/h（20倍）
    public static float averageSpeed = 0f; //速度60 → 1200km/h（20倍）
    public static uint allRings = 0;

    public static int actionChainCount = -1;
    public static float actionChainTime = 0f;

    public static List<int> stopwatches = new List<int>();

    public static List<PlayerStatus> players = new List<PlayerStatus>();

    public static byte damageTaken = 0;
    public static bool noMiss = true;

    public static BossManager bossInfo;

    public static bool extra = false;
    public static bool ready = false;
    public static bool restartTrigger = false;

    public static bool HUDshow = true;
	public static bool bossHPShow = false;
	public static bool ScoreShow = true;
	public static bool noScore = false;
	public static bool timeIncrease = true;

    public static byte[] fruits = new byte[10];
    // public static byte hudAnimNo = 0;

    public static bool isPaused = false;

    public static ushort chain = 0;
    public static float chainTime = 0f;
    public static ushort maxChain = 0;
    public static float scoreRate = 1f;

    public static float timeScale = 1f;
    public static float timeRate = 1f;

    public bool isShooting = false;

    public static float greenStarPitch = 1f;

    public static bool sonicStage = false;

    public static int stageClearIndex = -1;
    private bool clearTrigger = false;

    public static int itemCount = 0;
    public static float itemCollectedRate;

    public static float weaknessRate;
    public static int bossCount;

    public static int shootCount;
    public static int hitCount;

    public static CourseMode courseMode;

    public static float ringRushProgress = 0f;
    public static bool ringRush = false;
    public static bool forceRingRush = false;

    void Start()
    {
        players = new List<PlayerStatus>();

        bossHPShow = false;
        timeIncrease = true;
        
        maxChain = 0;

        // //リング総数（ソニックステージ＋ボスのみ）
        // allRings = 0;
        // CoinManager[] rings = FindObjectsByType<CoinManager>(FindObjectsSortMode.None);
        // foreach (CoinManager ring in rings)
        // {
        //     allRings += (uint)ring.amounts;

        //     if (allRings > 100)
        //     {
        //         allRings = 100;
        //     }
        // }

        actionChainCount = -1;
        actionChainTime = 0f;

        greenStarPitch = 1f;
        
        restartTrigger = false;

        stageClearIndex = -1;
        clearTrigger = false;
    }

    void Update()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        //スコア（0点～9億9999万9999点まで）
        Score = 0;
        for (int i = 0; i < players.Count; i++){
            //プレイヤー全員分のスコアを総計
            Score += players[i].Score;
        }
        Score = (uint)Mathf.Clamp(Score, 0, 999999999);
        //タイム（0分0秒00から99分59秒99まで）
        TotalTime = Mathf.Clamp(TotalTime, 0, 5999999);
        //コイン（0枚～999枚まで）
        Coins = (uint)Mathf.Clamp(Coins, 0, 999999);
        //ゴールド（0ドル～9億9999万9999ドルまで）
        Gold = (uint)Mathf.Clamp(Gold, 0, 999999999);
        //残機（0～99）
        // if (extra)
        // {
        //     Lives = (byte)Mathf.Clamp(Lives, 0, 9);
        // }
        // else
        // {
        //     Lives = (byte)Mathf.Clamp(Lives, 0, 99);
        // }
        Lives = (byte)Mathf.Clamp(Lives, 0, 99);

        if ((Coins >= 100 && !coins1up && !GameManager.extra) || (Coins >= 200 && !coins1up && GameManager.extra))
        {
            //1UP（1回のみ）
            // GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            // for (int i = 0; i < allPlayers.Length; i++)
            // {
            //     //プレイヤー全員分1UP
            //     if (allPlayers[i].GetComponent<PlayerInfo>() != null)
            //     {
            //         allPlayers[i].GetComponent<PlayerInfo>().OneUp();
            //     }
            // }
            if (allPlayers[0].GetComponent<PlayerInfo>() != null)
            {
                allPlayers[0].GetComponent<PlayerInfo>().OneUp();
            }
            coins1up = true;
        }

        ready = (FadeManager.alpha <= 0) && (WipeManager.scale >= 1);

        if (checkPointCount > 0) {
            averageSpeed = totalSpeed / (float)checkPointCount;
        } else {
            averageSpeed = 0f;
        }

        //アクションチェーン
        if (actionChainTime > 0f) {
            actionChainTime -= Time.deltaTime;
            if (actionChainTime <= 0f && actionChainCount >= 0) {
                // GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < allPlayers.Length; i++){
                    //プレイヤー全員分1UP
                    if (allPlayers[i].GetComponent<PlayerInfo>() != null){
                        if (allPlayers[i].GetComponent<PlayerInfo>().playerNumber == 0) {
                            // allPlayers[i].GetComponent<PlayerInfo>().scoreIncrease((int)Math.Round(200 * actionChainCount * (1f + (actionChainCount - 1) * 0.05f)));
                            int actionChainBonus = (int)Math.Round((250 * actionChainCount * (1f + (float)(actionChainCount - 1) / 99f)) / 5f) * 5;
                            
                            allPlayers[i].GetComponent<PlayerInfo>().scorePopUp(actionChainBonus, false, allPlayers[i].transform.position);
                            if (allPlayers[i].GetComponent<_16MSonic>() != null) {
                                allPlayers[i].GetComponent<_16MSonic>().BoostIncrease(2f * actionChainCount, false);
                            }
                        }
                    }
                }
                actionChainCount = -1;
            }
        }

        // //HUDアニメーション専用
        // if (gameOver){
		// 	//ゲームオーバー
		// 	hudAnimNo = 2;
		// } else {
		// 	if (HUDshow && !bossHPShow && hudAnimNo != 1 && !(hudAnimNo >= 3 && hudAnimNo <= 5)){
		// 		//表示
		// 		hudAnimNo = 1;
		// 	} else if (!HUDshow && !bossHPShow && hudAnimNo == 1){
		// 		//非表示
		// 		hudAnimNo = 2;
		// 	} else if (HUDshow && bossHPShow && (hudAnimNo == 1 || hudAnimNo == 4)){
		// 		//ボスゲージ表示（全表示状態）
		// 		hudAnimNo = 3;
		// 	} else if (HUDshow && !bossHPShow && (hudAnimNo == 3 || hudAnimNo == 5)){
		// 		//ボスゲージ非表示（全表示状態）
		// 		hudAnimNo = 4;
		// 	} else if (HUDshow && bossHPShow && hudAnimNo != 3 && hudAnimNo != 5){
		// 		//ボスゲージ表示
		// 		hudAnimNo = 5;
		// 	} else if (!HUDshow && !bossHPShow && hudAnimNo == 5){
		// 		//ボスゲージ非表示
		// 		hudAnimNo = 6;
		// 	}
		// }

        //チェーン（シューティングのみ）
        chain = (ushort)Mathf.Clamp(chain, 0, 1110);
        if (chainTime > 0f) {
            chainTime -= Time.deltaTime;

            if (chainTime <= 0f) {
                chain = 0;
            }
        }
        if (chain > maxChain) {
            maxChain = chain;
        }
        if (isShooting) {
            if (chain >= 900) {
                scoreRate = 10f;
            } else {
                scoreRate = 1f + (chain / 5) * 0.05f;
            }
        }

        bool leastOneGiant = false;
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<PlayerInfo>() != null)
            {
                if (player.GetComponent<PlayerInfo>().giant || player.GetComponent<PlayerInfo>().invincible)
                {
                    leastOneGiant = true;
                }
            }
        }
        if (!leastOneGiant && ghostCombo > 0)
        {
            ghostCombo = 0;
        }

        //タイムスケール
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = timeScale * timeRate;
        }

        //ステージクリア
        if (stageClearIndex >= 0 && !clearTrigger)
        {
            MusicManager.musicIndex = 8;
            clearTrigger = true;
            StartCoroutine("CourseClear");
        }

        //リングラッシュ
        /*
        通常：+10%（敵、グリーンスター）
        リングラッシュ中：
        敵、グリーンスター：+10%
        リング：+1%
        減少：-20%毎秒
        */
        if (courseMode == CourseMode.RingAttack) {
            if (leastOneGiant) {
                forceRingRush = true;
                ringRush = true;
                ringRushProgress = 1f;
            } else {
                if (forceRingRush) {
                    ringRushProgress = 0f;
                    ringRush = false;
                    forceRingRush = false;
                }
            }

            if (ringRush) {
                if (!forceRingRush) {
                    ringRushProgress -= 0.2f * Time.deltaTime;
                }
                if (ringRushProgress <= 0f) {
                    ringRush = false;
                }
            } else {
                if (ringRushProgress >= 1f) {
                    ringRush = true;
                }
            }
        }
        ringRushProgress = Mathf.Clamp(ringRushProgress, 0f, 1f);
    }

    // public void Reset(uint sc_tg, uint tm_tg, bool sonicBeginner, bool sonic)
    public void Reset(uint sc_aRank, uint sc_sRank, uint tm_tg, bool sonicBeginner, bool sonic)
    {
        death = false;
        falling = false;
        Score = 0;
        Coins = (uint)(sonicBeginner ? 10 : 0);
        Gold = 0;
        // targetScore = sc_tg;
        aRankLine = sc_aRank;
        sRankLine = sc_sRank;
        targetTime = tm_tg * 1000;
        maxCombo = 0;
        timeAction = true;

        checkPointCount = 0;
        totalSpeed = 0f;

        keyItems = new bool[7];
        fruits = new byte[10];

        greenStarPitch = 1f;

        sonicStage = sonic;
        
        restartTrigger = false;

        stageClearIndex = -1;
        clearTrigger = false;
        
        itemCount = 0;
        CoinManager[] coins = FindObjectsByType<CoinManager>(FindObjectsSortMode.None);
        itemCount += coins.Length;
        QuestionBlockManager[] blocks = FindObjectsByType<QuestionBlockManager>(FindObjectsSortMode.None);
        foreach (QuestionBlockManager block in blocks)
        {
            if (block.blockType != BlockType.None)
            {
                itemCount++;
            }
        }
        ItemManager[] items = FindObjectsByType<ItemManager>(FindObjectsSortMode.None);
        itemCount += items.Length;
        MonitorManager[] monitors = FindObjectsByType<MonitorManager>(FindObjectsSortMode.None);
        itemCount += monitors.Length;
        ItemCapsule[] capsules = FindObjectsByType<ItemCapsule>(FindObjectsSortMode.None);
        itemCount += capsules.Length;

        weaknessRate = 0f;
        bossCount = 0;

        shootCount = 0;
        hitCount = 0;

        ringRushProgress = 0f;
        ringRush = false;
        forceRingRush = false;
    }
    public static void ResetEntire() {
        keyItems = new bool[7];
        stopwatches = new List<int>();

        GameManager.noMiss = true;
        GameManager.damageTaken = 0;
    }

    public static bool is3D() {
        return dimension == DimensionType.Normal3D;
    }

    public static void ChainTimeRecovery() {
        chainTime = 7f;
    }
    public static void ChainIncrease(bool isEnemy = false) {
        if (isEnemy || (!isEnemy && chain > 0)) {
            chain++;
        }
        ChainTimeRecovery();
    }

    public static void IncreaseActionChain(bool isEnemy = false) {
        if (!sonicStage) {
            return;
        }
        if (isEnemy || (!isEnemy && actionChainCount > 0)) {
            actionChainCount++;
            if (actionChainCount > 100) {
                actionChainCount = 100;
            }
        }
        if (actionChainCount < 0) {
            actionChainCount = 0;
        }
        actionChainTime = 2f;
    }

    public static bool StageClear()
    {
        return stageClearIndex >= 0;
    }
    public static float[] GetEqualityRate()
    {
        int playerLength = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int p = 0; p < players.Length; p++) {
            if (players[p].GetComponent<PlayerInfo>().primary) {
                playerLength++;
            }
        }
        switch (playerLength)
        {
            case 4:
            return new float[4]{0.25f, 0.25f, 0.25f, 0.25f};
            
            case 3:
            return new float[4]{0.50f, 0.25f, 0.25f, 0.00f};
            
            case 2:
            return new float[4]{0.50f, 0.50f, 0.00f, 0.00f};
        }

        return new float[4]{1.00f, 0.00f, 0.00f, 0.00f};
    }

    public IEnumerator CourseClear()
    {
        switch (stageClearIndex)
        {
            case 0:
            //マリオ
            yield return new WaitForSeconds(6f);
            break;

            case 1:
            //パックマン
            yield return new WaitForSeconds(5f);
            break;

            case 2:
            //ロックマン
            yield return new WaitForSeconds(8f);
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                PlayerInfo pi;
                if (p.GetComponent<PlayerInfo>() != null)
                {
                    pi = p.GetComponent<PlayerInfo>();
                    pi.StartCoroutine(pi.CourseCleared(2));
                }
            }
            break;

            case 3:
            //ソニック
            yield return new WaitForSeconds(6f);
            break;
        }

        yield return null;

        StartCoroutine("GoToResultScreen");
    }

    public IEnumerator GoToResultScreen()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        bool fruitBonus = false;

        //フルーツボーナス
        for (int f = 0; f < fruits.Length; f++)
        {
            while (fruits[f] > 0)
            {
                fruits[f]--;
                foreach (GameObject p in allPlayers)
                {
                    PlayerInfo pi;
                    if (p.GetComponent<PlayerInfo>() != null)
                    {
                        pi = p.GetComponent<PlayerInfo>();
                        int score = (int)(200 * GetEqualityRate()[pi.playerNumber]);
                        pi.scoreIncrease(score, p.transform.position);
                    }
                    if (courseMode == CourseMode.RingAttack) {
                        Coins++;
                    }
                }
                if (this.GetComponent<HUDManager>() != null && this.GetComponent<AudioSource>() != null)
                {
                    this.GetComponent<AudioSource>().PlayOneShot(this.GetComponent<HUDManager>().fruitBonusSound);
                }

                fruitBonus = true;

                yield return new WaitForSeconds(0.1f);
            }
        }
        if (fruitBonus)
        {
            yield return new WaitForSeconds(0.75f);
        }

        stageClearIndex = 4;

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("ResultScreen");
    }

    public static void CalcItemCollectedRate()
    {
        int collectedItemCount = 0;
        CoinManager[] coins = FindObjectsByType<CoinManager>(FindObjectsSortMode.None);
        collectedItemCount += coins.Length;
        QuestionBlockManager[] blocks = FindObjectsByType<QuestionBlockManager>(FindObjectsSortMode.None);
        foreach (QuestionBlockManager block in blocks)
        {
            if (block.blockType != BlockType.None && !block.isEmpty)
            {
                collectedItemCount++;
            }
        }
        ItemManager[] items = FindObjectsByType<ItemManager>(FindObjectsSortMode.None);
        collectedItemCount += items.Length;
        MonitorManager[] monitors = FindObjectsByType<MonitorManager>(FindObjectsSortMode.None);
        collectedItemCount += monitors.Length;
        ItemCapsule[] capsules = FindObjectsByType<ItemCapsule>(FindObjectsSortMode.None);
        collectedItemCount += capsules.Length;

        itemCollectedRate = Math.Max(0, itemCount - collectedItemCount) * 100f / (float)itemCount;
    }
}
