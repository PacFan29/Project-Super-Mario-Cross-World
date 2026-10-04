using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageInfos", menuName = "ScriptableObjects/Stage Infos")]
public class StageInfos : ScriptableObject
{
    [Serializable]
    public class World {
        public Stage[] stages = new Stage[8];
    }
    [Serializable]
    public class Stage {
        [Header("目標スコア")]
        // public uint targetScore = 50000;
        public uint aRankLine = 40000;
        public uint sRankLine = 50000;
        [Header("目標タイム（秒単位）")]
        public uint targetTime = 600;
        public uint maxTimeBonus = 30000;
        [Header("許容ダメージ数")]
        public uint maxDamageCount = 10;
        [Header("タイムアタックの目標タイム（秒単位）")]
        public uint timeAttackTargetTime = 60;
        [Header("リングアタックの目標リング")]
        public uint ringsARankLine = 200;
        public uint ringsSRankLine = 250;
        [Header("ステージの形式")]
        public GameType stageType;
        public bool openWorldAsDefault = false;
        public bool greenStarsFromScratch = false;
        [Header("サブタイトル")]
        public string subtitle = "基本的なステージ";

        [Space(1)]

        [Header("記録")]
        public bool cleared = false;
        public bool timeCleared = false;
        public bool ringCleared = false;
        public bool unlocked = true;
        public uint highScore = 0;
        public uint bestTime = 5999999;
        public uint timeAttackBestTime = 5999999;
        public uint bestCoins = 0;
        public uint ringAttackBestRings = 0;
        public bool[] greenStars = new bool[3];
        public int bestRank = 0;
        public int timeBestRank = 0;
        public int ringBestRank = 0;

        public void Initialize() {
            cleared = false;
            timeCleared = false;
            ringCleared = false;
            unlocked = false;
            highScore = 0;
            bestTime = 5999999;
            timeAttackBestTime = 5999999;
            ringAttackBestRings = 0;
            bestCoins = 0;
            for (int i = 0; i < greenStars.Length; i++)
            {
                greenStars[i] = false;
            }
            bestRank = 0;
            timeBestRank = 0;
            ringBestRank = 0;
        }
    }

    public World[] worlds = new World[8];

    public int getAllGreenStars(int world) {
        //グリーンスター全体の数を取得
        int total = 0;
        foreach (Stage stage in worlds[world].stages) {
            total += stage.greenStars.Length;
        }
        return total;
    }
    public int getCurrentGreenStars(int world) {
        //現在獲得しているグリーンスターの数を取得
        int total = 0;
        foreach (Stage stage in worlds[world].stages) {
            foreach (bool single in stage.greenStars) {
                if (single) total++;
            }
        }
        return total;
    }
    public int getAllBestRankCount(int world) {
        //Ｓランク全体の数（全ステージ数）を取得
        return worlds[world].stages.Length;
    }
    public int getCurrentBestRankCount(int world) {
        //現在取っているＳランクの数を取得
        int total = 0;
        foreach (Stage stage in worlds[world].stages) {
            if (stage.bestRank == 6) total++;
        }
        return total;
    }
    public int getPercent(int world) {
        int max = 0;
        max += getAllGreenStars(world);
        max += getAllBestRankCount(world) * 2;

        int current = 0;
        foreach (Stage stage in worlds[world].stages) {
            if (stage.cleared) current++;
        }
        current += getCurrentGreenStars(world);
        current += getCurrentBestRankCount(world);

        return (int)Math.Floor(((float)current / (float)max) * 100f);
    }

    public int getGrandGreenStars() {
        //現在獲得しているグリーンスターの数を取得（全数）
        int total = 0;
        for (int i = 0; i < worlds.Length; i++) {
            total += getCurrentGreenStars(i);
        }
        return total;
    }
    public int getGrandBestRankCount() {
        //現在取っているＳランクの数を取得（全数）
        int total = 0;
        for (int i = 0; i < worlds.Length; i++) {
            total += getCurrentBestRankCount(i);
        }
        return total;
    }

    #if UNITY_EDITOR
    [ContextMenu("記録を全て消去する")]
    void DeleteAllRecords() {
        Initialize();
    }
    #endif

    public void Initialize() {
        foreach (World world in worlds) {
            foreach (Stage stage in world.stages) {
                stage.Initialize();
            }
        }

        worlds[0].stages[0].unlocked = true;
    }
}
