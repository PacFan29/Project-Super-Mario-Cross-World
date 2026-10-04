using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRushArea : EventArea
{
    [Header("ウェーブ")]
    public Transform waveParent;
    public GameObject[] waves;
    public int waveNo = -1;

    private List<GameObject> enemies = new List<GameObject>();

    [Header("ＢＧＭ切り替え")]
    public AudioClip music;
	public float loopBegin = 0f;
	public float loopEnd = 999999f;
    [Header("敵をすべて倒した後")]
    public GameObject activeAfterWin;

    public override void Start()
    {
        musicManager = GameObject.FindAnyObjectByType<MusicManager>();
        Destroy(this.transform.GetChild(0).gameObject);

        waves = new GameObject[waveParent.childCount];
        var childIndex = 0;

        // 子オブジェクトを順番に配列に格納
        foreach (Transform wave in waveParent)
        {
            waves[childIndex++] = wave.gameObject;
            wave.gameObject.SetActive(false);
        }

        waveNo = -1;
    }
    public override void OverrideUpdate()
    {
        if (waveNo >= 0 && waveNo < waves.Length)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null)
                {
                    enemies.Remove(enemy);
                }
            }
            if (enemies.Count <= 0) {
                waveNo++;
                if (waveNo == waves.Length)
                {
                    EventEnd();
                } else
                {
                    EnemySetUp();
                }
            }
        }
    }
    public override void EventStart()
    {
        waveNo = 0;

        if (music != null)
        {
            musicManager.ChangeMusic(music, loopBegin, loopEnd);
        }

        EnemySetUp();
    }

    public override void EventEnd()
    {
        if (music != null)
        {
            musicManager.ReturnMusic();
        }
        if (activeAfterWin != null)
        {
            activeAfterWin.SetActive(true);
        }
        DestroyThisObject();
    }

    void EnemySetUp()
    {
        waves[waveNo].SetActive(true);

        enemies = new List<GameObject>();
        while (waves[waveNo].transform.childCount > 0)
        {
            foreach (Transform enemy in waves[waveNo].transform)
            {
                if (enemy.gameObject.GetComponent<EnemyManager>() != null || enemy.gameObject.GetComponent<TriggerDamage>() != null)
                {
                    enemies.Add(enemy.gameObject);
                }
                enemy.gameObject.transform.parent = null;
            }
        }
    }
}
