using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    // public GameObject[] UIPrefabs = new GameObject[9];
    [Header("ＨＵＤ")]
    public GameObject UIPrefab;
    [Header("キャラクター生成")]
    public GameObject disactive;
    public PlayerPrefabs playerPrefs;
    public PlayerShootingPrefabs playerShootingPrefs;
    [Header("データ")]
    public GameData data;
    [Header("タイムアタック置き換えオブジェクト")]
    public GameObject stopwatchObj;
    // Start is called before the first frame update
    void Awake()
    {
        // if (UIPrefabs != null) {
        //     Instantiate(UIPrefabs[(int)data.HUDStyle], this.transform.position, Quaternion.identity);
        // }
        if (UIPrefab != null) {
            Instantiate(UIPrefab, this.transform.position, Quaternion.identity);
        }
        disactive.SetActive(false);

        int playerId = 0;
        int playerLength = 0;
        foreach (int id in data.characterIds) {
            if (id < 0) {
                break;
            } else {
                playerLength++;
            }
        }

        bool sonicStage = false;
        if (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss) {
            playerId = 16;
            playerLength = 1;
            sonicStage = true;
        }

        Vector3 spawnPos;
        if (data.isCpPassed) {
            spawnPos = data.CpPosition;
        } else {
            spawnPos = this.transform.position;
        }
        spawnPos += Vector3.left * (2f * (playerLength - 1));
        
        byte len = (byte)(sonicStage ? 1 : playerLength);
        for (byte i = 0; i < len; i++) {
            if (!sonicStage) {
                playerId = data.characterIds[i];
                //playerId = Mathf.Clamp(playerId, -1, 15);
            }

            if (playerId >= 0) {
                if (data.HUDType == GameType.Shooting) {
                    playerId = Mathf.Clamp(playerId, 8, 11);

                    PlayerInfo pl = Instantiate(playerShootingPrefs.playerData[playerId - 8].prefab, spawnPos, Quaternion.identity).GetComponent<PlayerInfo>();
                    pl.playerNumber = i;
                } else {
                    PlayerInfo pl = Instantiate(playerPrefs.playerData[playerId].prefab, spawnPos, Quaternion.identity).GetComponent<PlayerInfo>();
                    pl.playerNumber = i;
                    pl.primary = true;

                    if (
                        data.characterIdsSub[i] >= 0 && 
                        !sonicStage && FindAnyObjectByType<CreditsManager>() == null && 
                        ((data.courseMode == CourseMode.ScoreAttack || data.courseMode == CourseMode.NoScoreAndRankS) || FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0))
                    {
                        if (data.lives > 0)
                        {
                            PlayerInfo pl2 = Instantiate(playerPrefs.playerData[data.characterIdsSub[i]].prefab, spawnPos - Vector3.forward * 4f, Quaternion.identity).GetComponent<PlayerInfo>();
                            pl2.playerNumber = i;
                            pl2.primary = false;
                            pl2.partner = pl;
                            pl.partner = pl2;
                        } else
                        {
                            pl.partnerRevival = true;
                        }
                    }
                }
                // PlayerInfo pl = Instantiate(playerPrefs.playerData[playerId].prefab, spawnPos, Quaternion.identity).GetComponent<PlayerInfo>();
                // pl.playerNumber = i;
                spawnPos += Vector3.right * 4f;
            }
        }

        //タイムアタック用
        if (GameManager.courseMode == CourseMode.TimeAttack) {
            //フルーツターゲット
            foreach (FruitTarget obj in FindObjectsByType<FruitTarget>(FindObjectsSortMode.None)) {
                RePac2Stopwatch sw = Instantiate(stopwatchObj, obj.gameObject.transform.position, Quaternion.identity).GetComponent<RePac2Stopwatch>();
                sw.reduceTimeIndex = obj.index / 2;
                Destroy(obj.gameObject);
            }

            //グリーンスター、ハンコを「5秒短縮」に置き換え
            foreach (KeyItemManager obj in FindObjectsByType<KeyItemManager>(FindObjectsSortMode.None)) {
                RePac2Stopwatch sw = Instantiate(stopwatchObj, obj.gameObject.transform.position, Quaternion.identity).GetComponent<RePac2Stopwatch>();
                sw.reduceTimeIndex = 4;
                Destroy(obj.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
