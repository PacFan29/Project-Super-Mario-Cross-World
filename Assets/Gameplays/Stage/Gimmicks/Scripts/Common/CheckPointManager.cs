using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CheckPointManager : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    protected bool passed = false;
    private AudioSource source;
    protected int playerId;

    [Header("アニメーション")]
    public Animator anim;

    [Header("エフェクト")]
    public GameObject effect;
    // Start is called before the first frame update
    void Awake()
    {
        if (data.CpPosition == this.transform.position && data.isCpPassed) {
            passed = true;
        }

        source = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player" && !passed) {
            passed = true;

            StartCoroutine("Animation");

            // data.totalTime = GameManager.TotalTime;
            data.isCpPassed = true;
            data.CpPosition = this.transform.position;

            for (int g = 0; g < data.greenStars.Length; g++) {
                data.greenStars[g] = GameManager.keyItems[g];
            }

            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            playerId = player.playerId;

            player.scorePopUp(2000, false, this.transform.position);

            source.Play();

            GameManager.currentSpeed = (player.XZmag * 12.5f);

            GameManager.checkPointCount++;
            GameManager.totalSpeed += (player.XZmag * 12.5f);
        }
    }
    
    public virtual IEnumerator Animation() {
        if (anim != null) anim.Play("Get", 0, 0);
        if (effect != null) effect.SetActive(true);

        yield return null;
    }
}
