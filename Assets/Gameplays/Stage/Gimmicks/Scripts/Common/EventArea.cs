using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventArea : MonoBehaviour
{
    private bool eventTrigger = false;
    protected MusicManager musicManager;

    public virtual void Start()
    {
        musicManager = GameObject.FindAnyObjectByType<MusicManager>();
        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Update()
    {
        if (eventTrigger)
        {
            OverrideUpdate();
        }
    }

    public virtual void OverrideUpdate()
    {
        ;
    }

    //イベント開始トリガー
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerInfo>() != null && !eventTrigger)
        {
            eventTrigger = true;
            EventStart();
        }
    }

    //イベント開始
    public virtual void EventStart()
    {
        ;
    }

    //イベント終了
    public virtual void EventEnd()
    {
        DestroyThisObject();
    }

    public void DestroyThisObject()
    {
        Destroy(gameObject);
    }
}
