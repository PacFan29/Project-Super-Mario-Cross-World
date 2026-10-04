using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class MazeManager : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    public Options options;
    public ObjectRecordSaver saver;
    [Header("READY!")]
    public GameObject readyTxt;
    public AudioSource readySource;
    public AudioClip[] readyMusics = new AudioClip[3];
    [Header("メインBGM")]
    public AudioSource mainMusic;
    private float musicOpacity = 1f;
    [Header("プレイヤー表記")]
    public GameObject[] playerGroups = new GameObject[3];
    public GameObject onePDisplay;

    private MazePlayer[] players = new MazePlayer[4];

    [Header("残機")]
    public GameObject[] p1LivesIcons = new GameObject[3];
    public GameObject[] p2LivesIcons = new GameObject[3];
    public GameObject[] p3LivesIcons = new GameObject[3];
    public GameObject[] p4LivesIcons = new GameObject[3];
    [Header("スコア")]
    public LocalizedStringTable scoreHeaderTable;
    public Text ScoreHeader;
    public Text ScoreValue;
    private uint currentScore;
    private float[] alert = new float[2] { 1f, 1f };

    private int score1up = 0;
    [Header("メイズ失敗")]
    public GameObject mazeFailedTxt;

    public enum GhostState
    {
        Scatter,
        Chase
    }
    [Header("ゴーストのステータス")]
    public GhostState ghostState;
    private float ghostStatetime;
    private int ghostStateIndex;
    public bool ghostRateUp;

    private bool clearTrigger = false;

    [Serializable]
    public class FruitTargetItem
    {
        public int remainedCookies;
        public MazeItems.ItemType itemType;
        public int index;
    }
    public GameObject invulnableSound;
    public GameObject eyesSound;
    [Header("フルーツターゲット")]
    public List<FruitTargetItem> fruitTargetList = new List<FruitTargetItem>();
    public MazeItems fruitTarget;
    public GameObject fruitDisp;
    public Text fruitTxt;
    public Image fruitIcon;
    public Sprite[] fruitSprites = new Sprite[10];
    public Sprite doubleCookieSprite;
    public Sprite superCookieSprite;
    public Image fruitTime;

    [HideInInspector] public int giantCount;
    [HideInInspector] public int ghostCombo;
    [Header("カメラ制御")]
    public CinemachineCamera mainCam;
    public CinemachineCamera zoomCam;
    private bool cameraZoomTrigger = false;
    public CinemachineCamera clearCam;
    [Header("メインHUD／メイズクリア")]
    public GameObject mainHUD;
    public GameObject mazeClear;
    public GameObject mazeClearBonuses;
    public Text clearedScore;
    public GameObject[] livesGroups = new GameObject[4];
    public Image[] livesIcons = new Image[4];
    public Text[] livesAmms = new Text[4];
    public Text[] livesBonusValues = new Text[4];
    public Text totalScore;

    private byte[] livesAmounts = new byte[4];
    private uint[] livesBonuses = new uint[4];

    private uint total;
    
    [HideInInspector] public bool almostThere = false;
    [HideInInspector] public int clearedPlayerNo = 0;

    [Header("ワイプ")]
    public Image wipe;
	public Sprite normalWipe;
	public Sprite pacManWipe; //メイズクリア（パックマン）
	public Sprite ghostWipe; //メイズ失敗（ゴースト）
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpecialStageManager.ready = false;
        SpecialStageManager.death = false;
        SpecialStageManager.cleared = false;
        SpecialStageManager.Score = 0;
        SpecialStageManager.Coins = 0;
        ghostState = GhostState.Scatter;
        ghostStatetime = 7f;
        ghostStateIndex = 0;
        clearTrigger = false;

        musicOpacity = 1f;

        StartCoroutine("Ready");
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤー情報
        giantCount = 0;
        players = new MazePlayer[4];
        livesAmounts = new byte[4];
        MazePlayer[] allPl = FindObjectsByType<MazePlayer>(FindObjectsSortMode.None);
        foreach (MazePlayer pl in allPl)
        {
            players[(int)pl.playerNumber] = pl;
            livesAmounts[(int)pl.playerNumber] = pl.lives;
            if (pl.isInvincible)
            {
                giantCount++;
            }
        }
        onePDisplay.SetActive(FindObjectsByType<MazePlayer>(FindObjectsSortMode.None).Length > 1);

        SetPlayerLives(0, null, p1LivesIcons);
        SetPlayerLives(1, playerGroups[0], p2LivesIcons);
        SetPlayerLives(2, playerGroups[1], p3LivesIcons);
        SetPlayerLives(3, playerGroups[2], p4LivesIcons);

        //スコア
        UnityEngine.Localization.Tables.StringTable scoreHeadPairs = scoreHeaderTable.GetTable();

        //もうすぐ１ＵＰするよ！の点滅
        alert[0] += 7f * alert[1] * Time.deltaTime;
        if (alert[1] > 0f && alert[0] >= 1f || alert[1] < 0f && alert[0] <= 0f)
        {
            alert[1] *= -1f;
        }
        if (currentScore < SpecialStageManager.Score)
        {
            currentScore += (uint)Math.Max(1, ((float)(SpecialStageManager.Score - currentScore) * 8f * Time.unscaledDeltaTime));
        }
        else if (currentScore > SpecialStageManager.Score)
        {
            currentScore -= (uint)Math.Max(1, ((float)(currentScore - SpecialStageManager.Score) * 8f * Time.unscaledDeltaTime));
        }
        currentScore = (uint)Mathf.Clamp(currentScore, 0, SpecialStageManager.Score);

        float oneUpProgress = currentScore / 25000f;
        if (data.courseMode == CourseMode.NoScoreAndRankS)
        {
            ScoreHeader.text = scoreHeadPairs["EN-HUD-1UPProgress"].Value;
            if (currentScore >= 250000)
            {
                oneUpProgress = 1f;
            } else if (currentScore >= 100000)
            {
                oneUpProgress = (currentScore - 100000) / 150000f;
            } else if (currentScore >= 25000 && currentScore < 100000)
            {
                oneUpProgress = (currentScore - 25000) / 75000f;
            }
            ScoreValue.text = oneUpProgress.ToString("###,###,##0.00%");
        }
        else
        {
            if (GameManager.players.Count >= 2)
            {
                //２プレイヤー以降
                ScoreHeader.text = scoreHeadPairs["EN-HUD-TotalScore"].Value;
            }
            else
            {
                //１プレイヤーのみ
                ScoreHeader.text = scoreHeadPairs["EN-HUD-Score"].Value;
            }
            ScoreValue.text = currentScore.ToString("###,###,##0");
        }
        ScoreValue.text = ScoreValue.text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");
        if (
            (
                (
                    (currentScore >= 20000 && currentScore < 25000) || 
                    (currentScore >= 90000 && currentScore < 100000) || 
                    (currentScore >= 200000 && currentScore < 250000)
                ) && data.courseMode != CourseMode.NoScoreAndRankS
            ) || (
                oneUpProgress >= 0.9f && oneUpProgress < 1f && data.courseMode == CourseMode.NoScoreAndRankS
            )
        ){
            //もうすぐ１ＵＰ！
            ScoreValue.color = new Color(1.0f, 1.0f, alert[0], 1.0f);
        }
        else
        {
            ScoreValue.color = Color.white;
        }

        if (
            (currentScore >= 25000 && score1up == 0) || 
            (currentScore >= 100000 && score1up == 1) || 
            (currentScore >= 250000 && score1up == 2)
        ){
            //25,000点、100,000点、250,000点で１ＵＰ
            allPl = FindObjectsByType<MazePlayer>(FindObjectsSortMode.None);
            foreach (MazePlayer pl in allPl)
            {
                pl.OneUp();
            }
            score1up++;
        }

        //ゴーストのステータス
        if (ghostStatetime > 0f && SpecialStageManager.ready)
        {
            ghostStatetime -= Time.deltaTime;
        }
        if (ghostStatetime <= 0f)
        {
            switch (ghostStateIndex)
            {
                case 0:
                case 2:
                case 4:
                case 6:
                //追いかけ
                ghostStatetime = 20f;
                ghostState = GhostState.Chase;
                TurnAroundAllGhosts();
                break;

                case 1:
                //解散（7秒間）
                ghostStatetime = 7f;
                ghostState = GhostState.Scatter;
                TurnAroundAllGhosts();
                break;

                case 3:
                case 5:
                //解散（5秒間）
                ghostStatetime = 5f;
                ghostState = GhostState.Scatter;
                TurnAroundAllGhosts();
                break;
            }
            ghostStateIndex++;
        }

        //ゴーストコンボ
        if (ghostCombo > 0 && giantCount <= 0)
        {
            ghostCombo = 0;
        }

        //ゴースト効果音
        int eyesCount = 0;
        foreach (MazeGhost g in FindObjectsByType<MazeGhost>(FindObjectsSortMode.None))
        {
            if (g.ghostMode == MazeGhost.GhostMode.Eaten)
            {
                eyesCount++;
            }
        }
        invulnableSound.SetActive(giantCount > 0 && !SpecialStageManager.cleared && !SpecialStageManager.death);
        eyesSound.SetActive(eyesCount > 0 && !SpecialStageManager.cleared && !SpecialStageManager.death);

        //残りクッキー数
        int cookieCount = 0;
        foreach (MazeCookie c in FindObjectsByType<MazeCookie>(FindObjectsSortMode.None))
        {
            if (c.enable)
            {
                cookieCount++;
            }
        }
        almostThere = cookieCount <= 20;

        //フルーツターゲット出現
        foreach (FruitTargetItem item in fruitTargetList)
        {
            if (cookieCount <= item.remainedCookies)
            {
                fruitTargetList.Remove(item);
                fruitTarget.type = item.itemType;
                fruitTarget.index = item.index;
                fruitTarget.timeLimit = 10f;
                fruitTarget.manager = this;
                break;
            }
        }
        fruitDisp.SetActive(fruitTarget.timeLimit > 0);
        switch (fruitTarget.type)
        {
            case MazeItems.ItemType.FruitTarget:
            fruitIcon.sprite = fruitSprites[fruitTarget.index];
            fruitTxt.text = scoreHeadPairs["EN-Maze-FruitTarget"].Value;
            break;

            case MazeItems.ItemType.DoubleCookie:
            fruitIcon.sprite = doubleCookieSprite;
            fruitTxt.text = scoreHeadPairs["EN-Maze-SpecialItem"].Value;
            break;

            case MazeItems.ItemType.SuperCookie:
            fruitIcon.sprite = superCookieSprite;
            fruitTxt.text = scoreHeadPairs["EN-Maze-SpecialItem"].Value;
            break;
        }
        fruitTime.fillAmount = fruitTarget.timeLimit / 10f;

        //カメラのズーム変更
        if (Input.GetButtonDown("X") && SpecialStageManager.ready && !SpecialStageManager.death && !SpecialStageManager.cleared)
        {
            cameraZoomTrigger = !cameraZoomTrigger;
            if (cameraZoomTrigger)
            {
                mainCam.Priority.Enabled = false;
                mainCam.Priority.Value = 0;
                zoomCam.Priority.Enabled = true;
                zoomCam.Priority.Value = 1;
            } else
            {
                mainCam.Priority.Enabled = true;
                mainCam.Priority.Value = 1;
                zoomCam.Priority.Enabled = false;
                zoomCam.Priority.Value = 0;
            }
        }

        //クリア判定
        if (cookieCount <= 0 && !clearTrigger)
        {
            clearTrigger = true;
            StartCoroutine("MazeClear");
        }
        mazeClearBonuses.SetActive(data.courseMode != CourseMode.NoScoreAndRankS);

        //メイズクリア
        clearedScore.text = SpecialStageManager.Score.ToString("###,###,##0");
        clearedScore.text = clearedScore.text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");

        total = SpecialStageManager.Score;
        //表示・非表示
        for (int l = 0; l < livesGroups.Length; l++)
        {
            livesGroups[l].SetActive(allPl.Length > l);
        }
        //アイコン
        for (int l = 0; l < allPl.Length; l++)
        {
            livesIcons[l].sprite = allPl[l].icon;
        }
        //残機数／ボーナス
        for (int l = 0; l < livesAmounts.Length; l++)
        {
            livesAmms[l].text = "×" + livesAmounts[l].ToString("###,###,##0");
            livesBonuses[l] = (uint)(5000f * GetEqualityRate()[l]) * livesAmounts[l];
            livesBonusValues[l].text = livesBonuses[l].ToString("###,###,##0");
            livesBonusValues[l].text = livesBonusValues[l].text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");
            total += livesBonuses[l];
        }

        totalScore.text = total.ToString("###,###,##0");
        totalScore.text = totalScore.text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");


        //ワイプ
        if (WipeManager.scale < 1f && WipeManager.isWipeOut)
		{
			if (SpecialStageManager.cleared)
			{
                wipe.sprite = pacManWipe;
			} else
			{
				wipe.sprite = ghostWipe;
			}
			wipe.gameObject.transform.localScale = Vector3.one * 1.5f;
		} else
		{
			wipe.sprite = normalWipe;
			wipe.gameObject.transform.localScale = Vector3.one;
		}

        //ワイプによるＢＧＭの音量調整
        if (WipeManager.isWipeOut)
        {
            musicOpacity -= 2 * Time.deltaTime;
        }
        musicOpacity = Mathf.Clamp(musicOpacity, 0f, 1f);
        mainMusic.volume = 0.6f * musicOpacity;

        mazeClear.GetComponent<AudioSource>().volume = 0.7f * musicOpacity;
    }

    public IEnumerator Reset()
    {
        SpecialStageManager.death = false;
        SpecialStageManager.ready = false;

        ghostState = GhostState.Scatter;
        ghostStatetime = 7f;
        ghostStateIndex = 0;
        fruitTarget.timeLimit = 0f;
        ghostRateUp = false;

        foreach (MazeCharacter c in FindObjectsByType<MazeCharacter>(FindObjectsSortMode.None))
        {
            // Debug.Log(c);
            bool notPlayer = c.gameObject.GetComponent<MazePlayer>() == null;
            bool isPlayer = c.gameObject.GetComponent<MazePlayer>() != null;
            isPlayer = isPlayer && !c.gameObject.GetComponent<MazePlayer>().gameOver;

            if (notPlayer || isPlayer)
            {
                c.Initialize();
            }
        }

        yield return new WaitForSeconds(1f);

        SpecialStageManager.ready = true;
    }

    void SetPlayerLives(byte playerNo, GameObject group, GameObject[] livesIcons)
    {
        if (group != null)
        {
            group.SetActive(players[playerNo] != null);
        }
        if (players[playerNo] == null)
        {
            return;
        }
        for (int l = 0; l < livesIcons.Length; l++)
        {
            livesIcons[l].SetActive(players[playerNo].lives > l);
        }

        for (int i = 0; i < livesIcons.Length; i++)
        {
            livesIcons[i].GetComponent<Image>().sprite = players[playerNo].icon;
        }
    }

    IEnumerator MazeClear()
    {
        SpecialStageManager.cleared = true;

        mainCam.Priority.Enabled = false;
        mainCam.Priority.Value = 0;
        zoomCam.Priority.Enabled = false;
        zoomCam.Priority.Value = 0;

        clearCam.Priority.Enabled = true;
        clearCam.Priority.Value = 1;

        yield return new WaitForSeconds(0.5f);

        mainHUD.SetActive(false);
        mazeClear.SetActive(true);

        //最後にクッキーを回収したプレイヤーがガッツポーズ
        players[clearedPlayerNo].Clear();

        yield return new WaitForSeconds(5f);

        // MazePlayer[] allPl = FindObjectsByType<MazePlayer>(FindObjectsSortMode.None);
        // for (int i = 0; i < allPl.Length; i++)
        // {
        //     SpecialStageManager.Score += (uint)(5000f * GetEqualityRate()[i] * allPl[i].lives);
        // }

        // saver.mazeScore = SpecialStageManager.Score;
        saver.mazeScore = total;
        saver.mazeCookies = SpecialStageManager.Coins;
        saver.mazeState = 2;
        saver.clearedPlayerNo = clearedPlayerNo;

        WipeManager.WipeSetUp(true, 1.5f);
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(saver.previousSceneIndex);
    }
    public IEnumerator MazeFailed()
    {
        mazeFailedTxt.SetActive(true);

        saver.mazeState = 1;

        yield return new WaitForSeconds(3f);

        WipeManager.WipeSetUp(true, 1.5f);
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(saver.previousSceneIndex);
    }

    void TurnAroundAllGhosts()
    {
        foreach (MazeGhost g in FindObjectsByType<MazeGhost>(FindObjectsSortMode.None))
        {
            g.TurnAround();
        }
    }
    public static float[] GetEqualityRate()
    {
        int playerLength = FindObjectsByType<MazePlayer>(FindObjectsSortMode.None).Length;
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

    public IEnumerator Ready()
    {
        yield return new WaitForSeconds(0.5f);

        int mIndex = 0;

        MazePlayer[] allPl = FindObjectsByType<MazePlayer>(FindObjectsSortMode.None);
        foreach (MazePlayer pl in allPl)
        {
            if (pl.playerNumber == 0)
            {
                mIndex = pl.readyMusicIndex;
                readySource.PlayOneShot(readyMusics[mIndex]);
            }
        }

        readyTxt.SetActive(true);

        yield return new WaitForSeconds(readyMusics[mIndex].length);

        readyTxt.SetActive(false);

        mainMusic.Play();
        SpecialStageManager.ready = true;
    }
}
