using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType {
	Normal,
	Boss,
	Sonic,
	SonicBoss,
    Shooting,
	Wario,
	BossRush,
	Trail
}
public enum Character {
	Mario,
	PacMan,
	RockMan,
	Sonic,
    Other
}
// public enum HUDStyle {
//     Default,
//     Mario3DWorld,
// 	PacManWorld,
// 	RockMan11,
// 	SonicMania,
// 	SonicWorldAdventure,
// 	SonicColors,
// 	SonicGenerations,
// 	SonicForces
// }
public enum CourseMode {
    ScoreAttack,
    NoScoreAndRankS,
    TimeAttack,
    RingAttack
}

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/Game Data")]
public class GameData : ScriptableObject
{
    [Serializable]
    public class Item {
        public float amounts;
        public bool megaManOnly;
        public bool exceptSonic;
        public bool special;
        public byte limit;
        [Header("アイコン")]
        public Sprite[] sprites;
    }

    [Header("ファイル番号")]
    public uint fileNo = 0;
    [Header("コイン総計")]
    public uint coins = 0;
    [Header("ステージの形式")]
    public GameType HUDType = GameType.Normal;
    public Character character = Character.Mario;
    public Character keyItem = Character.Mario;
    // public HUDStyle HUDStyle = HUDStyle.Default;
    public CourseMode courseMode;
    public bool openWorldAsDefault = false;
    // public bool noScoreAndRankS = false;
    // public bool timeAttack = false;
    [Header("プレイヤー")]
    public bool partnerSelectEnabled;
    public sbyte[] characterIds = {0, 5, 8, 12};
    public sbyte[] characterIdsSub = {1, 6, 9, 13};//ソニックマニアプラス　アンコールモード式
    public uint[] HPs = {1000, 1000, 1000, 1000};
    public byte lives = 5;
    // public byte[] lives = { 2, 2, 2, 2 };
    public byte[] powerUps = {0, 0, 0, 0};
    public sbyte[] shields = {0, 0, 0, 0};
    
    [Serializable]
    public class LocalBadges
    {
        public int[] badges = new int[3]{-1, -1, -1};
    }
    public BadgeList badgeList;
    public LocalBadges[] allBadges = new LocalBadges[4];
    // public Dictionary<string, string> stageInfo = new Dictionary<string, string>() {
    //     {"worldNo", "0"},
    //     {"stageNo", "0"},
    //     {"targetScore", "50000"},
    //     {"targetTime", "600"},
    //     {"subtitle", "基本的なステージ"}
    // };
    public Dictionary<string, string> stageInfo = new Dictionary<string, string>() {
        {"worldNo", "0"},
        {"stageNo", "0"},
        {"aRankLine", "45000"},
        {"sRankLine", "50000"},
        {"targetTime", "600"},
        {"subtitle", "基本的なステージ"}
    };

    [Header("アイテムストック")]
    public Item[] stockItems = new Item[7];
    [Header("グリーンスター")]
    public bool[] greenStars = new bool[3];
    [Header("チェックポイント")]
    public uint checkpointScore = 0;
    public float totalTime = 0f;
    public uint checkpointCoins = 0;

    public bool isCpPassed = false;
    public Vector3 CpPosition;
    
    public void ResetAll() {
        ResetCheckPoint();

        if (HUDType == GameType.Sonic) {
            HUDType = GameType.Normal;
        }
        openWorldAsDefault = false;

        lives = 5;
        HPs = new uint[4]{1000, 1000, 1000, 1000};
        powerUps = new byte[4]{0, 0, 0, 0};
        shields = new sbyte[4]{0, 0, 0, 0};

        foreach (Item item in stockItems) {
            item.amounts = 0;
        }
        
        totalTime = 0f;
    }
    // public void ResetExceptTime() {
    //     Lives();
    //     GameManager.TotalTime = totalTime;
    // }
    public void LevelComplete() {
        Lives();
        totalTime = 0f;
    }
    public void Lives()
    {
        // byte index = 0;
        // foreach (PlayerStatus player in GameManager.players)
        // {
        //     lives[index] = player.Lives;
        //     index++;
        // }
        lives = GameManager.Lives;
    }

    public void LostLife(byte playerNumber) {
        lives--;
        // Lives();
        // lives[playerNumber]--;
    }

    public void ResetCheckPoint() {
        checkpointScore = 0;
        totalTime = 0f;
        checkpointCoins = 0;
        isCpPassed = false;
        CpPosition = Vector3.zero;
    }

    #if UNITY_EDITOR
    [ContextMenu("記録を全て消去する")]
    void DeleteAllRecords() {
        // Debug.Log(Application.persistentDataPath);
        Initialize();
    }
    #endif

    public void Initialize() {
        coins = 0;
        HUDType = GameType.Normal;
        character = Character.Mario;
        // HUDStyle = HUDStyle.Default;
        openWorldAsDefault = false;

        characterIds = new sbyte[4]{0, -1, -1, -1};
        HPs = new uint[4]{1000, 1000, 1000, 1000};
        // lives = new byte[4]{2, 2, 2, 2};
        lives = 5;
        powerUps = new byte[4]{0, 0, 0, 0};
        shields = new sbyte[4]{0, 0, 0, 0};

        foreach (Item item in stockItems) {
            item.amounts = 0;
        }

        ResetCheckPoint();
    }

    public BadgeEffect GetBadgeEffect(int index)
    {
        if (index == -1)
        {
            return BadgeEffect.None;
        }
        return badgeList.badges[index].effect;
    }
    public float GetBadgeValue(int index)
    {
        if (index == -1)
        {
            return 0f;
        }
        return badgeList.badges[index].effectValue;
    }
}
