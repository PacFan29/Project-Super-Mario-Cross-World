using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManagement : MonoBehaviour
{
    public ObjectRecordSaver saver;
    private List<GameObject> objList = new List<GameObject>();
    private List<int> idList = new List<int>();

    private List<int> deletedIds = new List<int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int objId = 0;
        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            // if (obj.GetComponent<PlayerInfo>() == null || obj.GetComponent<BossManager>() == null)
            if (
                (
                    obj.GetComponent<EnemyManager>() != null || 
                    obj.GetComponent<CoinManager>() != null || 
                    obj.GetComponent<ItemManager>() != null || 
                    obj.GetComponent<QuestionBlockManager>() != null || 
                    obj.GetComponent<MonitorManager>() != null || 
                    obj.GetComponent<ItemCapsule>() != null || 
                    obj.GetComponent<FruitTarget>() != null
                ) && obj != null 
            )
            {
                objList.Add(obj);
                // idList.Add(obj.GetInstanceID());
                idList.Add(objId);
                objId++;
            }
        }

        if (saver.deleteAll)
        {
            List<int> dList = saver.LoadDeletedObjects();
            for (int i = 0; i < objList.Count; i++)
            {
                foreach (int d in dList)
                {
                    if (idList[i] == d)
                    {
                        // Destroy(objList[i]);
                        objList.Remove(objList[i]);
                        deletedIds.Add(idList[i]);
                        idList.Remove(idList[i]);
                    }
                }
            }
        }

        deletedIds = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i] == null)
            {
                objList.Remove(objList[i]);
                deletedIds.Add(idList[i]);
                idList.Remove(idList[i]);
            }
        }
    }

    public void Save()
    {
        saver.SaveDeletedObjects(deletedIds);
    }
    public void Reset()
    {
        saver.ResetObjects();
    }
}
