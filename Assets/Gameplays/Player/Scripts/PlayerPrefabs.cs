using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPrefabs", menuName = "ScriptableObjects/Player Prefabs")]
public class PlayerPrefabs : ScriptableObject
{
    [Serializable]
    public class PlayerData {
        [Header("解放済みか")]
        public bool unlocked;
        [Header("アイコン")]
        public Sprite livesIcon;
        [Header("アート")]
        public Sprite artwork;
        public float artworkScale = 1.0f;
        [Header("色")]
        public Color color;
        [Header("名前")]
        public string name;
        [Header("プレハブ")]
        public GameObject prefab;
    }
    public PlayerData[] playerData = new PlayerData[17];

    #if UNITY_EDITOR
    [ContextMenu("初期状態に戻す")]
    void InitPlayers() {
        Initialize();
    }
    #endif

    public void Initialize() {
        foreach (PlayerData p in playerData) {
            p.unlocked = false;
        }

        //初期はマリオのみ選択可能
        playerData[0].unlocked = true;
    }
}
