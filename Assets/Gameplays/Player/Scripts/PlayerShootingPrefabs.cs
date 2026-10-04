using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerShootingPrefabs", menuName = "ScriptableObjects/Player Shooting Prefabs")]
public class PlayerShootingPrefabs : ScriptableObject
{
    [Serializable]
    public class PlayerData {
        public Sprite icon;
        public Color color;
        public string name;
        public GameObject prefab;
    }
    public PlayerData[] playerData = new PlayerData[4];
}
