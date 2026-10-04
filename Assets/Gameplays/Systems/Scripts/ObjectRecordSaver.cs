using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectRecordSaver", menuName = "ScriptableObjects/ObjectRecordSaver")]
public class ObjectRecordSaver : ScriptableObject
{
    [Header("削除されたオブジェクト")]
    public bool deleteAll;
    public List<int> deletedObjects = new List<int>();
    [Header("メイズの記録")]
    public int previousSceneIndex = 0;
    public int mazeState; //0＝なし、1＝失敗、2＝クリア
    public int clearedPlayerNo;
    public uint[] savedScores = new uint[4];
    public uint savedCoins;
    public byte[] savedFruits = new byte[10];
    public uint mazeScore;
    public uint mazeCookies;
    public byte[] mazeFruits = new byte[10];

    public void SaveDeletedObjects(List<int> list)
    {
        deletedObjects = new List<int>(list);
    }
    public List<int> LoadDeletedObjects()
    {
        return deletedObjects;
    }

    public void ResetObjects()
    {
        deleteAll = false;
        deletedObjects = new List<int>();
    }
}
