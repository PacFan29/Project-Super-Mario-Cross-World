using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArea : EventArea
{
    [Header("ボス")]
    public GameObject[] bosses;
    private List<BossManager> bossList = new List<BossManager>();
    [Header("ＢＧＭ切り替え")]
    public AudioClip music;
	public float loopBegin = 0f;
	public float loopEnd = 999999f;
    [Header("ボスを倒した後")]
    public GameObject activeAfterWin;
    public AudioClip musicAfterWin;
	public float loopBeginAfterWin = 0f;
	public float loopEndAfterWin = 999999f;

    public void Awake()
    {
        foreach (GameObject boss in bosses)
        {
            boss.SetActive(false);
        }
        if (activeAfterWin != null)
        {
            activeAfterWin.SetActive(false);
        }
    }
    public override void OverrideUpdate()
    {
        bool stopMusic = true;
        bool noBossObject = true;
        foreach (BossManager boss in bossList)
        {
            if (boss != null)
            {
                noBossObject = false;
            }
            if (boss.HP > 0)
            {
                stopMusic = false;
            }
        }
        if (stopMusic && musicManager.gameObject.GetComponent<AudioSource>().isPlaying && MusicManager.musicIndex != 8)
        {
            musicManager.gameObject.GetComponent<AudioSource>().Stop();
        }

        if (noBossObject)
        {
            EventEnd();
        }
    }
    public override void EventStart()
    {
        foreach (GameObject boss in bosses)
        {
            if (boss.GetComponent<BossManager>() != null)
            {
                bossList.Add(boss.GetComponent<BossManager>());
                boss.SetActive(true);
            }
        }
        if (music != null)
        {
            musicManager.ChangeMusic(music, loopBegin, loopEnd);
        }
    }

    public override void EventEnd()
    {
        if (music != null && musicAfterWin == null)
        {
            musicManager.ReturnMusic();
        }
        if (musicAfterWin != null)
        {
            musicManager.ChangeMusic(musicAfterWin, loopBeginAfterWin, loopEndAfterWin);
        }
        if (activeAfterWin != null)
        {
            activeAfterWin.SetActive(true);
        }
        DestroyThisObject();
    }
}
