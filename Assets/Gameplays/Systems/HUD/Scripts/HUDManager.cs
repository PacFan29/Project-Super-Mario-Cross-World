using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;

public class HUDManager : MonoBehaviour
{

	[Header("データ")]
	public GameData data;
	public ObjectRecordSaver objectRecordSaver;
	public Options options;
	public PlayerPrefabs playerPrefs;
	[Header("メイン")]
	public GameObject mainGroup;
	[Header("ミス")]
	public GameObject miss;
	public GameObject[] missObjs = new GameObject[4];
	[Header("カウンター")]
	public Text[] Counters = new Text[4];
	public HUDToggle[] Groups = new HUDToggle[5];
	[Header("スコア")]
	public LocalizedStringTable scoreHeaderTable;
	public Text ScoreHeader;
	UnityEngine.Localization.Tables.StringTable scoreHeadPairs;
	[Header("タイム")]
	public GameObject TimeDisplay;
	[Header("コイン")]
	public GameObject CoinsDisplay;
	public GameObject coinsCross;
	public GameObject KeyItemsDisplay;
	public GameObject[] KeyItemDisplays = new GameObject[2];
	private List<KeyItemDisplay> gotKeyItems = new List<KeyItemDisplay>();
	public Image CollectableIcon;
	private bool[] currentGreenStars;

	[Header("残機")]
	public CanvasGroup livesGroup;
	public Image[] livesIcons = new Image[4];
	// public Sprite[] livesSprites = new Sprite[17];
	public GameObject livesCross;
	public Text lives;
	public GameObject livesDecreasedGroup;
	public Image[] livesDecreasedIcons = new Image[4];
	public Text livesDecreasedText;
	private bool livesDecreasedTrigger = false;
	private byte livesMinusAmount = 0;
	[Header("プレイヤー")]
	public Transform playerParent;
	public RectTransform playerHealth;
	public GameObject pinchSound;
	private bool mutePinchFirstTime = true;
	private int currentPlayerCount = 0;

	public static GameType HUDTypeGetter;

	//変数の宣言
	private double minutes;
	private double seconds;
	private double milli;

	public bool TimeType; //真＝減少、偽＝増加
	public int SecSetUp;

	private uint currentScore;
	private byte currentLives;
	private uint currentCoins;
	private float positionMemory;

	private int KeyItemAmounts;

	//画像のコントロール
	private RectTransform sec;
	private RectTransform min;
	public static float[] alert = new float[2] { 1f, 1f };

	[Header("ブースト")]
	public GameObject boostGroup;
	public Sprite boostFrontSprite;
	public Sprite boostFrontSpriteShadow;
	public Image boostFront;
	public Color boostFillSonicColor;
	public Color boostFillShadowColor;
	public Image boostFill;
	public Image boostFillExtended;
	public Image boostFillExtendedShade;
	[Header("ブースト（多言語対応）")]
    public Text boostName;
    public LocalizedStringTable boostNameTable;
	UnityEngine.Localization.Tables.StringTable boostNamePairs;
    public Font jpFont;
    public int jpFontSize;
    public Font enFont;
    public int enFontSize;
	[Header("スピードメーター")]
	public Sprite speedMeterSprite;
	public Sprite speedMeterSpriteShadow;
	public GameObject speedMeter;
	public RectTransform smArrow;
	[Header("アクションチェーン")]
	public GameObject actionChainGroup;
	public Text actionChainAmount;
	public Text actionChainRank;
	public Image actionChainTime;
	[Header("スピード測定")]
	public SpeedMeasure speedMeasure;
	private uint currentCheckpointCount = 0;

	[Header("青リング")]
	public GameObject blueRingIcon;
	public GameObject blueRingAmounts;

	[Header("画像")]
	public Sprite[] CollectableSprites = new Sprite[4];
	public Sprite[] KeyItemSprites = new Sprite[27];

	//グループのコントロール
	private bool[] displays = new bool[5];

	[Header("チェーン")]
	public CanvasGroup chainGroup;
	public GameObject chainShowOrHide;
	public Text chainAmount;
	public GameObject[] chainCrowns = new GameObject[3];
	public Image chainTime;
	public Text scoreRate;
	[Header("ボスゲージ")]
	public GameObject bossHealth;
	public HealthBar bossHealthBar;
	public Text bossName;
	public Transform bossHealthBorderLine;
	private float bossHealthHideTime = 0f;

	[Header("ポーズ")]
	public GameObject pauseMenu;
	[Header("フルーツボーナス効果音")]
	public AudioClip fruitBonusSound;
	// [Header("ゲームオーバー")]
	// public Animator GameOverManager;
	// private int GameOverAnimNo = 0;

	// private Animator anim;
	private Animator livesAnim;
	private Animator coinsAnim;

	public float toggleTime = 5f;
	[Header("ワイプ")]
	public Image wipe;
	public Sprite normalWipe;
	public Sprite bowserWipe; //通常ステージでミスした時（クッパ）
	public Sprite eggmanWipe; //ソニックステージでミスした時（Dr.エッグマン）
	public Sprite pacManWipe; //メイズ突入（パックマン）
	[Header("ステージクリア時のトランジション")]
	public RectTransform transitionBeforeResults;

	// Use this for initialization
	void Start()
	{
		// GameManager.extra = options.hardMode;
		GameManager.extra = options.difficulty == Options.Difficulty.Hard;
		// TimeType = GameManager.extra;
		TimeType = data.courseMode == CourseMode.RingAttack;

		Dictionary<string, string> stageInfo = data.stageInfo;

		int targetTime = Int32.Parse(stageInfo["targetTime"]);
		// targetTime = 140;

		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

		livesDecreasedTrigger = false;
		livesMinusAmount = 0;

		if (!data.isCpPassed)
		{
			if (TimeType)
			{
				if (data.HUDType == GameType.Boss || data.HUDType == GameType.SonicBoss)
				{
					GameManager.TotalTime = targetTime * 1000 + ((float)Math.Floor(Math.Min(60000f, targetTime * 500f) / 5000f) * 5000 * 2);
				}
				else
				{
					// GameManager.TotalTime = targetTime * 500;
					GameManager.TotalTime = targetTime * 1000;
					if (data.courseMode == CourseMode.RingAttack && targetTime > 600) {
						//上限10分
						GameManager.TotalTime = 600000;
					}
				}
			}
			else
			{
				GameManager.TotalTime = data.totalTime;
			}
		} else
		{
			GameManager.TotalTime = data.totalTime;
		}
		GameManager.Score = 0;

		GameManager.courseMode = data.courseMode;

		GameManager gm = gameObject.AddComponent<GameManager>();
		// gm.Reset((uint)Int32.Parse(stageInfo["targetScore"]), (uint)targetTime, options.beginnerMode && data.HUDType == GameType.Sonic, data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
		// gm.Reset((uint)Int32.Parse(stageInfo["targetScore"]), (uint)targetTime, options.difficulty == Options.Difficulty.Easy && data.HUDType == GameType.Sonic, data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
		gm.Reset((uint)Int32.Parse(
			stageInfo["aRankLine"]), 
			(uint)Int32.Parse(stageInfo["sRankLine"]), 
			(uint)targetTime, 
			options.difficulty == Options.Difficulty.Easy && (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss), 
			data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss
		);

		//メイズクリア・失敗後
		if (objectRecordSaver.mazeState > 0)
		{
			// GameManager.Score = objectRecordSaver.savedScore;
			for (int i = 0; i < allPlayers.Length; i++)
			{
				if (allPlayers[i].GetComponent<PlayerInfo>() != null)
				{
					PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
					player.scoreIncrease((int)objectRecordSaver.savedScores[player.playerNumber], allPlayers[i].transform.position);
				}
			}
			GameManager.Coins = objectRecordSaver.savedCoins;
			currentCoins = GameManager.Coins;
			if (GameManager.Coins >= 100)
			{
				GameManager.coins1up = false;
			}
			Array.Copy(objectRecordSaver.savedFruits, GameManager.fruits, GameManager.fruits.Length);

			if (objectRecordSaver.mazeState == 2)
			{
				//クリア
				StartCoroutine(IncreaseScoreFromMaze(objectRecordSaver.mazeScore, objectRecordSaver.clearedPlayerNo));
				GameManager.Coins += objectRecordSaver.mazeCookies;
				for (int f = 0; f < objectRecordSaver.savedFruits.Length; f++)
				{
					GameManager.fruits[f] += objectRecordSaver.mazeFruits[f];
				}
			}

			objectRecordSaver.mazeState = 0;
		} else
		{
			int cpScore = (int)Math.Floor((float)data.checkpointScore * 0.2f);
			for (int i = 0; i < allPlayers.Length; i++)
			{
				if (allPlayers[i].GetComponent<PlayerInfo>() != null)
				{
					PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
					if (player.playerNumber == 0 && player.primary)
					{
						player.scoreIncrease(cpScore, allPlayers[i].transform.position);
					}
				}
			}
			//コイン100枚ペナルティ（ソニックステージ以外）
			if (data.HUDType != GameType.Sonic && data.HUDType != GameType.SonicBoss)
			{
				GameManager.Coins = data.checkpointCoins - Math.Min(100, data.checkpointCoins);
			}
		}

		gm.isShooting = (data.HUDType == GameType.Shooting);

		min = GameObject.Find("MinutesArrow").GetComponent<RectTransform>();
		sec = GameObject.Find("SecondsArrow").GetComponent<RectTransform>();

		// positionMemory = TimeCoin[1].GetComponent<RectTransform>().anchoredPosition.y;
		positionMemory = CoinsDisplay.GetComponent<RectTransform>().anchoredPosition.y;

		// anim = GetComponent<Animator>();

		//残機の有無でコインとグリーンスターの配置を変更
		if (!options.lives || !(GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS))
		{
			livesGroup.gameObject.SetActive(false);
			KeyItemsDisplay.gameObject.transform.localPosition = CoinsDisplay.gameObject.transform.localPosition;
			CoinsDisplay.gameObject.transform.localPosition = livesGroup.gameObject.transform.localPosition;
		}

		livesAnim = Groups[4].transform.GetChild(0).gameObject.GetComponent<Animator>();
		coinsAnim = CoinsDisplay.transform.GetChild(0).gameObject.GetComponent<Animator>();

		currentLives = GameManager.Lives;

		Vector3 livesPosition = Groups[4].transform.localPosition;
		Vector3 coinsPosition = Groups[3].transform.localPosition;
		Vector3 keyItemsPosition = Groups[2].transform.localPosition;

		// if (options.difficulty == Options.Difficulty.Easy || data.timeAttack)
		// if (GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS)
		// {
		// 	Groups[2].transform.localPosition = coinsPosition;
		// 	Groups[3].transform.localPosition = livesPosition;
		// }
		// else
		// {
		// 	Groups[2].transform.localPosition = keyItemsPosition;
		// 	Groups[3].transform.localPosition = coinsPosition;
		// }

		//キーアイテム
		//値が0の場合は、隠す。
		KeyItemAmounts = data.greenStars.Length;
		if (KeyItemAmounts <= 0)
		{
			KeyItemDisplays[1].GetComponent<Image>().color = new Color(1, 1, 1, 0);
		}
		else
		{
			KeyItemDisplays[1].GetComponent<Image>().color = new Color(1, 1, 1, 1);

			RectTransform keyRect = (RectTransform)KeyItemDisplays[1].transform;
			gotKeyItems.Add(keyRect.gameObject.GetComponent<KeyItemDisplay>());
			// KeyItemDisplays[1].transform.localPosition = new Vector2(
			// 	keyRect.localPosition.x - (keyRect.sizeDelta.x * (KeyItemAmounts-1) / 2f),
			// 	keyRect.localPosition.y
			// );

			GameManager.keyItems[0] = data.greenStars[0];
			switch (data.keyItem)
			{
				case Character.Mario:
					KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[0];
					break;
				case Character.PacMan:
					if (KeyItemAmounts == 7)
					{
						KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[7];
					}
					else
					{
						KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[1];
					}
					break;
				case Character.RockMan:
					KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[14];
					break;
				case Character.Sonic:
					KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[26];
					break;
				case Character.Other:
					KeyItemDisplays[1].GetComponent<Image>().sprite = KeyItemSprites[27];
					break;
			}

			for (int i = 1; i < KeyItemAmounts; i++)
			{
				keyRect = (RectTransform)Instantiate(KeyItemDisplays[1]).transform;
				gotKeyItems.Add(keyRect.gameObject.GetComponent<KeyItemDisplay>());
				keyRect.SetParent(KeyItemDisplays[0].transform, false);
				keyRect.localPosition = KeyItemDisplays[1].transform.localPosition;

				keyRect.localPosition = new Vector2(
					keyRect.localPosition.x + (keyRect.sizeDelta.x * i),
					keyRect.localPosition.y
				);

				GameManager.keyItems[i] = data.greenStars[i];

				switch (data.keyItem)
				{
					case Character.Mario:
						keyRect.GetComponent<Image>().sprite = KeyItemSprites[0];
						break;
					case Character.PacMan:
						if (KeyItemAmounts == 7)
						{
							keyRect.GetComponent<Image>().sprite = KeyItemSprites[i + 7];
						}
						else
						{
							keyRect.GetComponent<Image>().sprite = KeyItemSprites[i + 1];
						}
						break;
					case Character.RockMan:
						keyRect.GetComponent<Image>().sprite = KeyItemSprites[14];
						break;
					case Character.Sonic:
						keyRect.GetComponent<Image>().sprite = KeyItemSprites[26];
						break;
				}
			}
		}
		currentGreenStars = new bool[data.greenStars.Length];
		for (int i = 0; i < data.greenStars.Length; i++)
		{
			currentGreenStars[i] = data.greenStars[i];
		}

		currentCheckpointCount = 0;

		mutePinchFirstTime = true;
	}

	// Update is called once per frame
	void Update()
	{
		// anim.speed = (GameManager.ready) ? 1f : 0f;

		bool sonicStage = (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
		bool playAsShadow = FindObjectsByType<_17Shadow>(FindObjectsSortMode.None).Length > 0;

		//グループの表示・非表示（0=スコア  1=タイム  2=コイン  3=ゴールド  4＝グリーンスター）
		switch (data.HUDType)
		{
			case GameType.Normal:
				//通常
				displays[0] = true;
				displays[1] = TimeType;
				displays[2] = true;
				displays[3] = false;
				displays[4] = true;
				break;

			case GameType.Boss:
			case GameType.BossRush:
				//vs. ボス
				displays[0] = false;
				displays[1] = true;
				displays[2] = true;
				displays[3] = false;
				displays[4] = false;
				break;

			case GameType.Sonic:
				//モダンソニックステージ
				displays[0] = true;
				displays[1] = true;
				displays[2] = true;
				displays[3] = false;
				displays[4] = true;
				break;

			case GameType.SonicBoss:
				//モダンソニック vs. ボス
				displays[0] = false;
				displays[1] = true;
				displays[2] = true;
				displays[3] = false;
				displays[4] = false;
				break;

			case GameType.Shooting:
				displays[0] = true;
				displays[1] = false;
				displays[2] = true;
				displays[3] = false;
				displays[4] = false;
				break;

			case GameType.Wario:
				//ワリオステージ
				displays[0] = false;
				displays[1] = false;
				displays[2] = false;
				displays[3] = true;
				displays[4] = true;
				break;

			case GameType.Trail:
				//おためし部屋
				displays[0] = true;
				displays[1] = false;
				displays[2] = false;
				displays[3] = false;
				displays[4] = false;
				break;
		}
		if (GameManager.courseMode == CourseMode.TimeAttack)
		{
			displays[0] = false;
			displays[1] = true;
			displays[2] = sonicStage;
			displays[3] = false;
			displays[4] = false;
		}
		if (GameManager.courseMode == CourseMode.RingAttack)
		{
			displays[0] = false;
			displays[1] = true;
			displays[2] = true;
			displays[3] = false;
			displays[4] = false;
		}
		GameManager.ScoreShow = displays[0];
		GameManager.noScore = GameManager.courseMode == CourseMode.NoScoreAndRankS;
		displays[0] = displays[0] && GameManager.courseMode != CourseMode.NoScoreAndRankS;

		// if (SceneManager.GetActiveScene().name == "WorldMap") {
		if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0)
		{
			//ワールドマップのみ、プレイヤーの情報以外を非表示にする。
			displays[0] = false;
			displays[1] = false;
			displays[2] = false;
			displays[3] = false;
			displays[4] = false;
		}

		HUDTypeGetter = data.HUDType;

		//サイズのセットアップ：ピクセルの2/9

		//点滅
		alert[0] += 7f * alert[1] * Time.deltaTime;
		if (alert[1] > 0f && alert[0] >= 1f || alert[1] < 0f && alert[0] <= 0f)
		{
			alert[1] *= -1f;
		}

		//残機
		for (int p = 0; p < 4; p++)
		{
			if (p < GameManager.players.Count)
			{
				// livesIcons[p].sprite = livesSprites[GameManager.players[p].ID];
				livesIcons[p].sprite = playerPrefs.playerData[GameManager.players[p].ID].livesIcon;
				livesIcons[p].gameObject.SetActive(true);
			}
			else
			{
				livesIcons[p].gameObject.SetActive(false);
			}
		}
		livesGroup.alpha = FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0 ? 0f : 1f;
		lives.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(47f + (GameManager.players.Count - 1) * 15f + (options.livesCoinsCross ? 9f : 0f), -0.9f, 0f);
		livesCross.SetActive(options.livesCoinsCross);
		lives.text = GameManager.Lives.ToString();
		Groups[4].value = (int)GameManager.Lives;
		Groups[4].alwaysShow = options.hudOptions == Options.HUDOptions.AlwaysShow;

		if (currentLives != GameManager.Lives)
		{
			if (currentLives < GameManager.Lives) livesAnim.Play("LivesUp", 0, 0);
			currentLives = GameManager.Lives;
		}

		for (int p = 0; p < 4; p++)
		{
			if (p < GameManager.players.Count)
			{
				// livesDecreasedIcons[p].sprite = livesSprites[GameManager.players[p].ID];
				livesDecreasedIcons[p].sprite = playerPrefs.playerData[GameManager.players[p].ID].livesIcon;
				livesDecreasedIcons[p].gameObject.SetActive(true);
			}
			else
			{
				livesDecreasedIcons[p].gameObject.SetActive(false);
			}
		}
		livesDecreasedGroup.GetComponent<RectTransform>().localPosition = new Vector3(-15f - (GameManager.players.Count - 1) * 7.5f, 0f, 0f);
		livesDecreasedText.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(44.5f + (GameManager.players.Count - 1) * 15f, 0f, 0f);
		livesDecreasedText.text = (GameManager.Lives - livesMinusAmount).ToString();

		// if (!livesDecreasedTrigger && WipeManager.scale <= 0 && GameManager.death && GameManager.Lives > 0 && options.difficulty != Options.Difficulty.Easy && !data.timeAttack)
		if (WipeManager.scale < 1f && GameManager.death)
		{
			if (sonicStage)
			{
				wipe.sprite = eggmanWipe;
			} else
			{
				wipe.sprite = bowserWipe;
			}
			wipe.gameObject.transform.localScale = Vector3.one * 1.5f;
		} else
		{
			wipe.sprite = normalWipe;
			wipe.gameObject.transform.localScale = Vector3.one;
		}
		// if (!livesDecreasedTrigger && WipeManager.scale <= 0 && GameManager.death && GameManager.Lives > 0 && !data.timeAttack)
		if (
			!livesDecreasedTrigger && WipeManager.scale <= 0 && 
			GameManager.death && GameManager.Lives > 0 && 
			(GameManager.courseMode == CourseMode.ScoreAttack || GameManager.courseMode == CourseMode.NoScoreAndRankS)
			)
		{
			livesDecreasedTrigger = true;
			if (!options.lives)
			{
				StartCoroutine("LifeLostPenalty");
			} else
			{
				livesDecreasedGroup.SetActive(true);
				StartCoroutine("LivesMinus");
			}
		}

		scoreHeadPairs = scoreHeaderTable.GetTable();

		ScoreDisp(sonicStage);
		TimeDisp(sonicStage);
		CoinsDisp();
		KeyItemsDisp();

		if (currentPlayerCount != GameManager.players.Count)
		{
			PlayerShow();
			currentPlayerCount = GameManager.players.Count;
		}
		bool pinchSoundTrigger = false;
		if (sonicStage)
		{
			pinchSoundTrigger = GameManager.Coins <= 0 && !mutePinchFirstTime;
			if (mutePinchFirstTime && GameManager.Coins > 0)
            {
                mutePinchFirstTime = false;
            }
		}
		else
		{
			if (GameManager.courseMode == CourseMode.RingAttack) {
				pinchSoundTrigger = GameManager.Coins <= 0 && !mutePinchFirstTime;
				if (mutePinchFirstTime && GameManager.Coins > 0)
				{
					mutePinchFirstTime = false;
				}
			} else {
				HealthBar[] allBars = GetComponentsInChildren<HealthBar>();
				List<HealthBar> playerBars = new List<HealthBar>();
				foreach (HealthBar bar in allBars)
				{
					if (!bar.isBoss)
					{
						playerBars.Add(bar);
					}
				}
				foreach (HealthBar bar in playerBars)
				{
					if (bar.pinch)
					{
						pinchSoundTrigger = true;
					}
				}
			}
		}
		pinchSound.SetActive(pinchSoundTrigger && !GameManager.isPaused);

		BossHealthBar();

		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < allPlayers.Length; i++)
		{
			if (allPlayers[i].GetComponent<PlayerInfo>() != null)
			{
				PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
				if (player.playerNumber == 0)
				{
					if (player.finalVelocity.magnitude > 0f)
					{
						toggleTime = 5f;
					}
				}
			}
		}
		if (toggleTime > 0f)
		{
			toggleTime -= Time.deltaTime;
		}

		//アクションチェーン
		actionChainGroup.SetActive(GameManager.actionChainCount > 0);
		actionChainAmount.text = Math.Min(99, GameManager.actionChainCount).ToString("00");
		string acRankTxt = "Good!";
		if (GameManager.actionChainCount >= 21)
		{
			acRankTxt = "Amazing!";
		}
		else if (GameManager.actionChainCount >= 19)
		{
			acRankTxt = "Outstanding!";
		}
		else if (GameManager.actionChainCount >= 17)
		{
			acRankTxt = "Extreme!";
		}
		else if (GameManager.actionChainCount >= 15)
		{
			acRankTxt = "Awesome!";
		}
		else if (GameManager.actionChainCount >= 13)
		{
			acRankTxt = "Tight!";
		}
		else if (GameManager.actionChainCount >= 11)
		{
			acRankTxt = "Radical!";
		}
		else if (GameManager.actionChainCount >= 9)
		{
			acRankTxt = "Cool!";
		}
		else if (GameManager.actionChainCount >= 7)
		{
			acRankTxt = "Jammin\'!";
		}
		else if (GameManager.actionChainCount >= 5)
		{
			acRankTxt = "Great!";
		}
		else if (GameManager.actionChainCount >= 3)
		{
			acRankTxt = "Nice!";
		}
		actionChainRank.text = acRankTxt;
		actionChainTime.fillAmount = GameManager.actionChainTime / 2f;

		playerHealth.gameObject.SetActive(data.HUDType != GameType.Sonic && data.HUDType != GameType.SonicBoss);
		boostGroup.SetActive(data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);

		if (GameManager.players.Count > 0)
		{
			//ブースト（ソニック・シャドウ配色）
			if (playAsShadow)
			{
				boostFront.sprite = boostFrontSpriteShadow;
				boostFill.color = boostFillShadowColor;
			} else {
				boostFront.sprite = boostFrontSprite;
				boostFill.color = boostFillSonicColor;
			}

			//ブースト
			boostFill.fillAmount = GameManager.players[0].HP / 100f;
			if (GameManager.players[0].HP > 100)
			{
				boostFillExtended.fillAmount = Math.Max(0f, (GameManager.players[0].HP - 100) / 400f);
			} else
			{
				boostFillExtended.fillAmount = 0f;
			}
			boostFillExtendedShade.fillAmount = boostFillExtended.fillAmount;
		}
		boostNamePairs = boostNameTable.GetTable();
        boostName.text = boostNamePairs["ブースト"].Value;
        if (Encoding.GetEncoding("Shift_JIS").GetByteCount(boostName.text) > boostName.text.Length)
        {
            boostName.font = jpFont;
            boostName.fontSize = jpFontSize;
        }
        else
        {
            boostName.font = enFont;
            boostName.fontSize = enFontSize;
        }

		//スピードメーター
		if (playAsShadow)
		{
			speedMeter.GetComponent<Image>().sprite = speedMeterSpriteShadow;
		} else
		{
			speedMeter.GetComponent<Image>().sprite = speedMeterSprite;
		}
		speedMeter.SetActive(data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
		if (allPlayers[0] != null)
		{
			if (allPlayers[0].GetComponent<PlayerInfo>() != null)
			{
				float smRot = Math.Max(-135f, 135f - (allPlayers[0].GetComponent<PlayerInfo>().XZmag / 100) * 270f);
				smArrow.transform.rotation = Quaternion.Euler(0, 0, smRot);
			}
		}

		// //アニメーターの処理
		// if (GameManager.gameOver)
		// {
		// 	//ゲームオーバー
		// 	GameOver();
		// }

		// anim.SetInteger("animNo", GameManager.hudAnimNo);
		// anim.updateMode = AnimatorUpdateMode.UnscaledTime;

		//チェーン（シューティングのみ）
		chainGroup.alpha = (data.HUDType == GameType.Shooting) ? 1f : 0f;
		// chainGroup.alpha = (true) ? 1f : 0f;
		chainShowOrHide.SetActive(GameManager.chain > 0);
		if (GameManager.chain >= 1110)
		{
			chainAmount.text = "";
		}
		else if (GameManager.chain >= 1100)
		{
			chainAmount.text = "    " + (GameManager.chain % 10).ToString();
		}
		else if (GameManager.chain >= 1000)
		{
			chainAmount.text = "  " + (GameManager.chain % 100).ToString("00");
		}
		else
		{
			chainAmount.text = GameManager.chain.ToString();
		}
		chainCrowns[0].SetActive(GameManager.chain >= 1000);
		chainCrowns[1].SetActive(GameManager.chain >= 1100);
		chainCrowns[2].SetActive(GameManager.chain >= 1110);

		chainTime.fillAmount = GameManager.chainTime / 7f;

		scoreRate.text = "SCORE ×" + GameManager.scoreRate.ToString("F2");

		//スピード測定
		if (currentCheckpointCount != GameManager.checkPointCount)
		{
			if (sonicStage)
			{
				StartCoroutine(speedMeasure.Begin());
			}
			currentCheckpointCount = GameManager.checkPointCount;
		}

		//ポーズ
		bool canPause = true;
		if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0)
		{
			if (!FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None)[0].canPause)
			{
				canPause = false;
			}
		}
		if (Input.GetButtonDown("Start") && !GameManager.isPaused && canPause && FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length <= 0)
		{
			//ポーズ時にタイムを表示
			GameManager.isPaused = !GameManager.isPaused;
		}
		if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length <= 0) {
			pauseMenu.SetActive(GameManager.isPaused);
		} else {
			pauseMenu.SetActive(false);
		}

		// GameOverManager.SetInteger("RouteID", GameOverAnimNo);

		mainGroup.SetActive(!GameManager.death);
		mainGroup.GetComponent<CanvasGroup>().alpha = options.hudOptions == Options.HUDOptions.AlwaysHide ? 0f : 1f;

		miss.SetActive(GameManager.death);
		missObjs[0].SetActive(data.HUDType != GameType.Sonic && data.HUDType != GameType.SonicBoss);
		missObjs[1].SetActive(data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
		// missObjs[0].SetActive(data.character == Character.Mario);
		// missObjs[1].SetActive(data.character == Character.PacMan);
		// missObjs[2].SetActive(data.character == Character.RockMan);
		// missObjs[3].SetActive(data.character == Character.Sonic);
	}

	void FixedUpdate()
	{
		if (GameManager.stageClearIndex == 4)
		{
			Vector3 pos = transitionBeforeResults.anchoredPosition;
            transitionBeforeResults.anchoredPosition = new Vector3(pos.x * (45f*Time.fixedDeltaTime), pos.y, pos.z);
		}
	}
	void ScoreDisp(bool sonicStage)
	{
		//スコアの表示・非表示
		if (displays[0] || displays[3])
		{
			Groups[0].gameObject.GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			Groups[0].gameObject.GetComponent<CanvasGroup>().alpha = 0f;
		}

		//スコア（出力のみ,９桁まで）
		if (displays[0])
		{
			if (currentScore < GameManager.Score)
			{
				currentScore += (uint)Math.Max(1, ((float)(GameManager.Score - currentScore) * 8f * Time.unscaledDeltaTime));
			}
			else if (currentScore > GameManager.Score)
			{
				currentScore -= (uint)Math.Max(1, ((float)(currentScore - GameManager.Score) * 8f * Time.unscaledDeltaTime));
			}
			currentScore = (uint)Mathf.Clamp(currentScore, 0, GameManager.Score);

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
			Counters[2].text = currentScore.ToString("###,###,##0");
		}
		else
		{
			GameManager.Score = (uint)currentScore;
		}
		//ゴールド（出力のみ、９桁まで）
		if (displays[3])
		{
			ScoreHeader.text = "GOLD";
			Counters[2].text = "$ " + GameManager.Gold.ToString("###,###,##0");
		}
		Groups[0].value = (int)GameManager.Score;
		Groups[0].alwaysShow = options.hudOptions == Options.HUDOptions.AlwaysShow;

		//カンマ変換
		Counters[2].text = Counters[2].text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");
	}
	void TimeDisp(bool sonicStage)
	{
		//タイム
		//60分（1時間）でタイムアップ
		if (GameManager.ready && GameManager.timeIncrease && FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length <= 0)
		{
			if (TimeType && GameManager.courseMode != CourseMode.TimeAttack)
			{
				if (GameManager.TotalTime > 0)
				{
					GameManager.TotalTime -= 1000 * Time.deltaTime;
				}
			}
			else
			{
				if (GameManager.TotalTime < 5999999)
				{
					GameManager.TotalTime += 1000 * Time.deltaTime;
				}
			}
		}
		//値のセットアップ（1分＝60,000ミリ秒、1秒＝1,000ミリ秒）
		bool milli3 = false;
		minutes = Math.Floor(GameManager.TotalTime / 60000);
		seconds = Math.Floor((GameManager.TotalTime / 1000) % 60);
		milli = Math.Floor((GameManager.TotalTime % 1000) / 10);
		if (
			options.timeFormat == Options.TimeFormat.QuotationsMilli3 || 
			options.timeFormat == Options.TimeFormat.ColonsOnlyMilli3 || 
			options.timeFormat == Options.TimeFormat.ColonsAndPeriodMilli3
			)
        {
            milli = Math.Floor(GameManager.TotalTime % 1000);
			milli3 = true;
        }
		//出力
		string minS = "\'";
		string secS = "\"";
		switch (options.timeFormat)
        {
            case Options.TimeFormat.ColonsOnly:
            case Options.TimeFormat.ColonsOnlyMilli3:
				minS = ":";
				secS = ":";
				break;
				
            case Options.TimeFormat.ColonsAndPeriod:
            case Options.TimeFormat.ColonsAndPeriodMilli3:
				minS = ":";
				secS = ".";
				break;
        }
		if (options.timeFormat == Options.TimeFormat.MinutesSecondsOnly)
        {
            Counters[1].text = minutes.ToString("00")
			+ ":" + seconds.ToString("00");
        } else
        {
			Counters[1].text = minutes.ToString("00")
			+ minS + seconds.ToString("00")
			+ secS + (milli3 ? milli.ToString("000") : milli.ToString("00"));
        }
		//時計の針の傾き
		sec.transform.rotation = Quaternion.Euler(0, 0, ((GameManager.TotalTime / -1000f) * 6f) % 360f);
		min.transform.rotation = Quaternion.Euler(0, 0, ((GameManager.TotalTime / -60000f) * 6f) % 360f);

		if (TimeType && GameManager.TotalTime <= 60000)
		{
			//タイムアップの１時間前（増加式）
			//タイムアップの１分前（減少式）
			Counters[1].color = new Color(1.0f, alert[0] * 0.8f, alert[0] * 0.8f, 1.0f);
		}
		else
		{
			Counters[1].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
		Groups[1].alwaysShow = displays[1];
	}
	void CoinsDisp()
	{
		if (displays[2])
		{
			Groups[3].gameObject.GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			Groups[3].gameObject.GetComponent<CanvasGroup>().alpha = 0f;
		}
		//コインなど（出力のみ、３桁まで）
		// if(displays[2]){
		switch (data.character)
		{
			case Character.Mario:
				CollectableIcon.sprite = CollectableSprites[0];
				break;
			case Character.PacMan:
				CollectableIcon.sprite = CollectableSprites[1];
				break;
			case Character.RockMan:
				CollectableIcon.sprite = CollectableSprites[2];
				break;
			case Character.Sonic:
				CollectableIcon.sprite = CollectableSprites[3];
				break;
			case Character.Other:
				CollectableIcon.sprite = CollectableSprites[4];
				break;
		}
		// Counters[0].text = GameManager.Coins.ToString("###,###,##0");
		Counters[0].text = currentCoins.ToString("###,###,##0");
		Counters[0].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(47f + (options.livesCoinsCross ? 9f : 0f), -0.9f, 0f);
		coinsCross.SetActive(options.livesCoinsCross);

		//カンマ変換
		Counters[0].text = Counters[0].text.Replace(",", options.comma ? scoreHeadPairs["EN-HUD-Comma"].Value : "");

		bool blueRingActive = false;
		if (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss || GameManager.courseMode == CourseMode.RingAttack)
		{
			if (GameManager.Coins <= 0)
			{
				//モダンソニックのみ
				Counters[3].text = Counters[0].text;
				Counters[3].color = new Color(1.0f, alert[0], alert[0], 1.0f);
				Counters[3].gameObject.SetActive(true);
			}
			else
			{
				Counters[3].gameObject.SetActive(false);
			}

			//青リング
			GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
			for (int i = 0; i < allPlayers.Length; i++)
			{
				if (allPlayers[i].GetComponent<PlayerInfo>() != null)
				{
					PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();

					// if (player.playerNumber == 0 && player.gameObject.GetComponent<_16MSonic>() != null)
					// {
					// 	blueRingActive = player.gameObject.GetComponent<_16MSonic>().blueRing;
					// }
					if (player.playerNumber == 0) {
						blueRingActive = player.gameObject.GetComponent<CommonActions>().blueRing;
					}
				}
			}
			blueRingIcon.SetActive(blueRingActive);
			blueRingAmounts.SetActive(blueRingActive);
		}
		else
		{
			blueRingIcon.SetActive(false);
			blueRingAmounts.SetActive(false);
		}
		// }

		blueRingAmounts.GetComponent<Text>().text = Counters[0].text;

		// if (currentCoins != GameManager.Coins)
		// {
		// 	if (currentCoins < GameManager.Coins) coinsAnim.Play("CoinsAdd", 0, 0);
		// 	currentCoins = GameManager.Coins;
		// }
		if (currentCoins > GameManager.Coins)
		{
			//コイン減少
			currentCoins -= (uint)Math.Max(1, ((float)(currentCoins - GameManager.Coins) * 5f * Time.unscaledDeltaTime));
			if (currentCoins == GameManager.Coins)
			{
				coinsAnim.Play("CoinsDec", 0, 0);
			} else
			{
				coinsAnim.Play("CoinsDec", 0, Time.time % 0.075f);
			}

		} else if (currentCoins < GameManager.Coins)
		{
			//コイン上昇
			currentCoins += (uint)Math.Max(1, ((float)(GameManager.Coins - currentCoins) * 5f * Time.unscaledDeltaTime));
			if (currentCoins == GameManager.Coins)
			{
				coinsAnim.Play("CoinsAdd", 0, 0);
			} else
			{
				coinsAnim.Play("CoinsAdd", 0, Time.time % 0.075f);
			}
		}
		// coinsAnim.SetBool("GotCoins", GameManager.Coins > 0);

		//タイム・コインの表示・非表示
		// TimeCoin[0].GetComponent<CanvasGroup>().alpha = alphaSet(displays[1]);
		// TimeCoin[1].GetComponent<CanvasGroup>().alpha = alphaSet(displays[2]);
		// TimeDisplay.GetComponent<CanvasGroup>().alpha = alphaSet(displays[1]);
		// CoinsDisplay.GetComponent<CanvasGroup>().alpha = alphaSet(displays[2]);
		// RectTransform coinPos = TimeDisplay.GetComponent<RectTransform>();
		// if (!displays[1]){
		// 	coinPos.anchoredPosition = CoinsDisplay.GetComponent<RectTransform>().anchoredPosition;
		// } else {
		// 	coinPos.anchoredPosition = new Vector2(coinPos.anchoredPosition.x, positionMemory);
		// }
		Groups[3].alwaysShow =
			((data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss || GameManager.courseMode == CourseMode.RingAttack) && GameManager.Coins <= 0)
			 || blueRingActive || options.hudOptions == Options.HUDOptions.AlwaysShow;
		Groups[3].value = (int)GameManager.Coins;
	}
	void KeyItemsDisp()
	{
		//キーアイテム
		for (int i = 0; i < gotKeyItems.Count; i++)
		{
			gotKeyItems[i].gotIt = GameManager.keyItems[i];
		}
		if (displays[4])
		{
			Groups[2].gameObject.GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			Groups[2].gameObject.GetComponent<CanvasGroup>().alpha = 0f;
		}

		int gStarCount = 0;
		for (int i = 0; i < data.greenStars.Length; i++)
		{
			if (GameManager.keyItems[i] && !currentGreenStars[i])
			{
				// int gStarCount = 0;
				foreach (bool g in GameManager.keyItems)
				{
					if (g) { gStarCount++; }
				}
				if (gStarCount == data.greenStars.Length && data.greenStars.Length > 1)
				{
					StartCoroutine("GreenStarPerfect");
				}
				currentGreenStars = new bool[data.greenStars.Length];
				for (int j = 0; j < data.greenStars.Length; j++)
				{
					currentGreenStars[j] = GameManager.keyItems[j];
				}
			}
		}
		Groups[2].value = gStarCount;
		Groups[2].alwaysShow = options.hudOptions == Options.HUDOptions.AlwaysShow;
	}
	IEnumerator GreenStarPerfect()
	{
		yield return new WaitForSeconds(0.5f);
		this.GetComponent<AudioSource>().Play();
	}

	void PlayerShow()
	{
		playerParent.gameObject.SetActive(GameManager.courseMode != CourseMode.RingAttack);

		foreach (Transform obj in playerParent)
		{
			if (0 <= obj.gameObject.name.LastIndexOf("Clone"))
			{
				Destroy(obj.gameObject);
			}
		}
		playerHealth.gameObject.GetComponent<PlayerUI>().alwaysShow = (options.hudOptions == Options.HUDOptions.AlwaysShow);
		for (int i = 1; i < GameManager.players.Count; i++)
		{
			//1の位の複製
			RectTransform ply = (RectTransform)Instantiate(playerHealth).transform;
			ply.SetParent(playerParent, false);
			ply.localPosition = new Vector2(
				ply.localPosition.x + ply.sizeDelta.x * i * 3.4f,
				ply.localPosition.y);

			// ply.gameObject.GetComponent<PlayerHealth>().playerNo = i;
			ply.gameObject.GetComponent<PlayerUI>().playerNo = i;
		}
	}

	void BossHealthBar()
	{
		if (GameManager.bossInfo == null || !GameManager.bossHPShow || !options.bossHealthBar)
		{
			if (bossHealthHideTime > 0f)
			{
				bossHealthHideTime -= Time.deltaTime;
			}
			bossHealth.SetActive(bossHealthHideTime > 0f);
			bossHealth.GetComponent<Animator>().SetBool("HideThis", true);
			return;
		}
		else
		{
			bossHealth.SetActive(true);
			bossHealthHideTime = 0.75f;
			bossHealth.GetComponent<Animator>().SetBool("HideThis", false);
		}

		bossHealthBar.value = (int)GameManager.bossInfo.totalHp;
		bossHealthBar.maxValue = GameManager.bossInfo.maxHp;
		bossHealthBar.bossPhase = GameManager.bossInfo.phase;
		bossHealthBar.maxBossPhase = GameManager.bossInfo.PhasePerHP.Length;
		bossName.text = GameManager.bossInfo.gameObject.name;

		if (GameManager.bossInfo.PhasePerHP.Length > 0 && !GameManager.bossInfo.multiPhase)
		{
			bossHealthBorderLine.gameObject.SetActive(true);

			float barSize = bossHealth.GetComponent<RectTransform>().sizeDelta.x - 12f;
			float emptyXpos = barSize / -2f;
			float rate = (float)(GameManager.bossInfo.PhasePerHP[0] * GameManager.bossInfo.HitsPerHP * 2) / GameManager.bossInfo.maxHp;

			bossHealthBorderLine.localPosition = new Vector3(emptyXpos + (barSize * rate), 0, 0);

			Transform parent = bossHealthBorderLine.gameObject.transform.parent;
			foreach (Transform obj in parent)
			{
				if (0 <= obj.gameObject.name.LastIndexOf("Clone"))
				{
					Destroy(obj.gameObject);
				}
			}
			for (int i = 1; i < GameManager.bossInfo.PhasePerHP.Length; i++)
			{
				rate = (float)(GameManager.bossInfo.PhasePerHP[i] * GameManager.bossInfo.HitsPerHP * 2) / GameManager.bossInfo.maxHp;

				RectTransform bd = (RectTransform)Instantiate(bossHealthBorderLine).transform;
				bd.SetParent(parent, false);
				bd.localPosition = new Vector3(emptyXpos + (barSize * rate), 0, 0);
			}
		}
		else
		{
			bossHealthBorderLine.gameObject.SetActive(false);
		}
	}

	float alphaSet(bool show)
	{
		if (show)
		{
			return 1f;
		}
		else
		{
			return 0f;
		}
	}

	// public void GameOver()
	// {
	// 	GameOverAnimNo = 1;
	// 	return;
	// }

	IEnumerator LivesMinus()
	{
		yield return new WaitForSeconds(1f);

		byte totalMinusLives = 0;

		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < allPlayers.Length; i++)
		{
			if (allPlayers[i].GetComponent<PlayerInfo>() != null)
			{
				PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
				if (player.HP <= 0)
				{
					totalMinusLives++;
				}
			}
		}

		for (int i = 0; i < totalMinusLives && GameManager.Lives > 0; i++)
		{
			livesDecreasedGroup.GetComponent<Animator>().Play("LivesDecreasedEnd", 0, 0f);
			livesDecreasedGroup.GetComponent<AudioSource>().Play();
			livesMinusAmount++;
			yield return new WaitForSeconds(0.5f);
		}

		yield return new WaitForSeconds(0.75f);

		GameManager.Lives -= livesMinusAmount;
		livesMinusAmount = 0;
		GameManager.restartTrigger = true;
	}
	IEnumerator LifeLostPenalty()
	{
		yield return new WaitForSeconds(1f);

		GameManager.restartTrigger = true;
	}

	IEnumerator IncreaseScoreFromMaze(uint score, int playerNo)
	{
		int combo = 3; //Fantastic
		if (score >= 25000)  {combo++;} //Excellent
		if (score >= 100000) {combo++;} //Incredible
		if (score >= 250000) {combo++;} //Wonderful

		yield return new WaitForSeconds(0.5f);

		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < allPlayers.Length; i++)
		{
			if (allPlayers[i].GetComponent<PlayerInfo>() != null)
			{
				PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
				if (player.playerNumber == playerNo && player.primary)
				{
					player.scorePopUp((int)score, false, allPlayers[i].transform.position, combo);
				}
			}
		}
	}
}
