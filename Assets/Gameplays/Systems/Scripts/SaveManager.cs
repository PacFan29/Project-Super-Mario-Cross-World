using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public class OptionsData
    {
        public float musicVolume;
        public float soundVolume;
        public float voiceVolume;
        
        public int difficulty;

        public int hudOptions;
        public bool bossHealthBar;

        public OptionsData(
            float musicVolume, float soundVolume, float voiceVolume,
            int difficulty, int hudOptions, bool bossHealthBar
        )
        {
            this.musicVolume = musicVolume;
            this.soundVolume = soundVolume;
            this.voiceVolume = voiceVolume;

            this.difficulty = difficulty;
            this.hudOptions = hudOptions;
            this.bossHealthBar = bossHealthBar;
        }
    }
    public class GlobalData
    {
        // public bool beginnerMode;
        // public bool lives;
        // public bool hardMode;
        // public int difficulty;

        public sbyte playerID;
        public uint playerHP;
        public byte playerLives;
        public byte[] playerPowerUps;
        public sbyte[] playerShields;
        public int coins;
        public float[] stockItems;

        public GlobalData(
            // bool beginnerMode, bool lives, bool hardMode,
            // int difficulty,
            sbyte playerID, uint playerHP, byte playerLives, byte[] playerPowerUps, sbyte[] playerShields, int coins, float[] stockItems
        ){

            // this.beginnerMode = beginnerMode;
            // this.lives = lives;
            // this.hardMode = hardMode;
            // this.difficulty = difficulty;
            
            this.playerID = playerID;
            this.playerHP = playerHP;
            this.playerLives = playerLives;
            this.playerPowerUps = playerPowerUps;
            this.playerShields = playerShields;
            this.coins = coins;
            this.stockItems = stockItems;
        }
    }
    public class StageData
    {
        public bool cleared;
        public bool timeCleared;
        public bool ringCleared;
        public bool unlocked;
        public int score; //0～999,999,999まで
        public int time;  //0～5,999,999まで（float型なので、小数点以下を切り捨てにする）
        public int coins; //0～999,999まで
        public int timeAttackBestTime;  //0～5,999,999まで（float型なので、小数点以下を切り捨てにする）
        public int ringAttackBestRings; //0～999,999まで
        public bool[] greenStars; //例）[false, false, true, false, true]
        public int bestRank;  //0(F)～6(S)まで
        public int timeBestRank;  //0(F)～6(S)まで
        public int ringBestRank;  //0(F)～6(S)まで

        public StageData(
            bool cleared, bool timeCleared, bool ringCleared,
            bool unlocked, 
            int score, int time, int coins, 
            int timeAttackBestTime, int ringAttackBestRings, 
            bool[] greenStars, 
            int bestRank, int timeBestRank, int ringBestRank
        ){
            this.cleared = cleared;
            this.timeCleared = timeCleared;
            this.ringCleared = ringCleared;
            this.unlocked = unlocked;
            this.score = score;
            this.time = time;
            this.coins = coins;
            this.timeAttackBestTime = timeAttackBestTime;
            this.ringAttackBestRings = ringAttackBestRings;
            this.greenStars = greenStars;
            this.bestRank = bestRank;
            this.timeBestRank = timeBestRank;
            this.ringBestRank = ringBestRank;
        }
    }

    public GameData gameData;
    public Options options;
    public StageInfos stageInfos;

    public void SaveOptions()
    {
        OptionsData opt = new OptionsData(
            options.musicVolume,
            options.soundVolume,
            options.voiceVolume,
            (int)options.difficulty,
            (int)options.hudOptions,
            options.bossHealthBar
        );
        saveProcess(Application.persistentDataPath + "/options.json", opt);
    }
    public void SaveGlobal()
    {
        float[] stockItems = new float[gameData.stockItems.Length];
        for (int i = 0; i < gameData.stockItems.Length; i++) {
            stockItems[i] = gameData.stockItems[i].amounts;
        }

        GlobalData global = new GlobalData(
            // options.beginnerMode,
            // options.lives,
            // options.hardMode,
            // (int)options.difficulty,
            gameData.characterIds[0],
            gameData.HPs[0],
            // gameData.lives[0],
            gameData.lives,
            gameData.powerUps,
            gameData.shields,
            (int)gameData.coins,
            stockItems
        );
        saveProcess(globalPathSet((int)gameData.fileNo), global);
    }
    public void SaveStageInfo(int world, int stage)
    {
        StageInfos.Stage info = stageInfos.worlds[world].stages[stage];

        StageData save = new StageData(
            /* ステージのデータ */
            info.cleared,
            info.timeCleared,
            info.ringCleared,
            info.unlocked,
            (int)info.highScore, 
            (int)info.bestTime, 
            (int)info.bestCoins,
            (int)info.timeAttackBestTime, 
            (int)info.ringAttackBestRings, 
            info.greenStars,
            info.bestRank, 
            info.timeBestRank, 
            info.ringBestRank
        );
        saveProcess(stagePathSet((int)gameData.fileNo, world, stage), save);
    }
    public void saveProcess<T>(string filePath, T save) {
        string json = JsonUtility.ToJson(save);
 
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
        Debug.Log(json);
    }

    public void LoadOptions() {
        string filePath = Application.persistentDataPath + "/options.json";

        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();

            OptionsData save = JsonUtility.FromJson<OptionsData>(data);

            options.musicVolume = save.musicVolume;
            options.soundVolume = save.soundVolume;
            options.voiceVolume = save.voiceVolume;

            options.difficulty = (Options.Difficulty)save.difficulty;

            options.hudOptions = (Options.HUDOptions)save.hudOptions;
            options.bossHealthBar = save.bossHealthBar;
        }
    }
    public void LoadGlobal()
    {
        string filePath = globalPathSet((int)gameData.fileNo);

        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
 
            GlobalData save = JsonUtility.FromJson<GlobalData>(data);

            // options.beginnerMode = save.beginnerMode;
            // options.lives = save.lives;
            // options.hardMode = save.hardMode;
            // options.difficulty = (Options.Difficulty)save.difficulty;

            gameData.characterIds[0] = save.playerID;
            gameData.HPs[0] = save.playerHP;
            // gameData.lives[0] = save.playerLives;
            gameData.lives = save.playerLives;
            gameData.powerUps = save.playerPowerUps;
            gameData.shields = save.playerShields;

            gameData.coins = (uint)save.coins;
            float[] stockItems = save.stockItems;

            for (int i = 0; i < gameData.stockItems.Length; i++) {
                gameData.stockItems[i].amounts = (byte)stockItems[i];
            }
        }
    }
    // public void LoadDifficulty()
    // {
    //     string filePath = globalPathSet(0);

    //     if (File.Exists(filePath))
    //     {
    //         StreamReader streamReader;
    //         streamReader = new StreamReader(filePath);
    //         string data = streamReader.ReadToEnd();
    //         streamReader.Close();
 
    //         GlobalData save = JsonUtility.FromJson<GlobalData>(data);

    //         // options.beginnerMode = save.beginnerMode;
    //         // options.lives = save.lives;
    //         // options.hardMode = save.hardMode;
    //         options.difficulty = (Options.Difficulty)save.difficulty;
    //     }
    // }
    public void LoadEntireStages()
    {
        for (int w = 0; w < stageInfos.worlds.Length; w++) {
            for (int s = 0; s < stageInfos.worlds[w].stages.Length; s++) {
                LoadSingleStage(w, s);
            }
        }
    }
    public void LoadSingleStage(int world, int stage) {
        StageInfos.Stage info = stageInfos.worlds[world].stages[stage];
        string filePath = stagePathSet((int)gameData.fileNo, world, stage);

        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
 
            StageData save = JsonUtility.FromJson<StageData>(data);

            info.cleared = save.cleared;
            info.timeCleared = save.timeCleared;
            info.ringCleared = save.ringCleared;
            info.unlocked = save.unlocked;
            info.highScore = (uint)save.score;
            info.bestTime = (uint)save.time;
            info.bestCoins = (uint)save.coins;
            info.timeAttackBestTime = (uint)save.timeAttackBestTime;
            info.ringAttackBestRings = (uint)save.ringAttackBestRings;
            info.greenStars = save.greenStars;
            info.bestRank = save.bestRank;
            info.timeBestRank = save.timeBestRank;
            info.ringBestRank = save.ringBestRank;
        }
    }

    public GlobalData LoadAndGetGlobal(int fileNo) {
        string filePath = globalPathSet(fileNo);

        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
 
            GlobalData save = JsonUtility.FromJson<GlobalData>(data);

            return save;
        } else {
            return null;
        }
    }
    public List<StageData> LoadAndGetEntireStages(int fileNo) {
        List<StageData> saveList = new List<StageData>();

        for (int w = 0; w < stageInfos.worlds.Length; w++) {
            for (int s = 0; s < stageInfos.worlds[w].stages.Length; s++) {
                StageInfos.Stage info = stageInfos.worlds[w].stages[s];
                string filePath = stagePathSet(fileNo, w, s);

                if (File.Exists(filePath))
                {
                    StreamReader streamReader;
                    streamReader = new StreamReader(filePath);
                    string data = streamReader.ReadToEnd();
                    streamReader.Close();
        
                    StageData save = JsonUtility.FromJson<StageData>(data);

                    saveList.Add(save);
                }
            }
        }

        return saveList;
    }

    public string globalPathSet(int fileNo){
        if (!Directory.Exists(Application.persistentDataPath + "/file" + fileNo)) {
            Directory.CreateDirectory(Application.persistentDataPath + "/file" + fileNo);
        }
        return Application.persistentDataPath + "/file" + fileNo + "/global.json";
    }
    public string stagePathSet(int fileNo, int worldNo, int stageNo){
        if (!Directory.Exists(Application.persistentDataPath + "/file" + fileNo)) {
            Directory.CreateDirectory(Application.persistentDataPath + "/file" + fileNo);
        }
        return Application.persistentDataPath + "/file" + fileNo + "/world" + (worldNo + 1) + "-" + (stageNo + 1) + ".json";
    }
    public void DeleteThisFile(int fileNo){
        if (Directory.Exists(Application.persistentDataPath + "/file" + fileNo)) {
            //データの削除
            Directory.Delete(Application.persistentDataPath + "/file" + fileNo);
        }
    }
}
