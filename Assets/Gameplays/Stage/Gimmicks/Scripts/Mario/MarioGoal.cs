using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioGoal : GoalManager
{
    [Header("旗")]
    public Transform villainFlag;
    public Transform playerFlag;
    [HideInInspector] public float bottomPos;
    [HideInInspector] public float upperPos;
    [HideInInspector] public float rate = 0.0f;
    [HideInInspector] public int step = 0;

    [Header("効果音")]
    public AudioClip touchedSound;
    public AudioClip touchedTopSound;
    public AudioClip downSound;
    public AudioClip flagChangeSound;
    public AudioClip goldAddSound;

    [Header("エフェクト")]
    public GameObject topEffect;
    public GameObject goldFlagEffect;

    [Header("コインスポナー")]
    public GameObject coinSpawner;
    // Start is called before the first frame update
    void Start()
    {
        bottomPos = playerFlag.position.y;
        upperPos = villainFlag.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float diff = upperPos - bottomPos;
        float yPos = diff * rate + bottomPos;
        playerFlag.transform.position = new Vector3(this.transform.position.x, yPos, this.transform.position.z);

        villainFlag.gameObject.SetActive(step == 0);
        playerFlag.gameObject.SetActive(step == 2);
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && step == 0){
            GameManager.timeIncrease = false;
            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            if (!player.primary)
            {
                return;
            }
            DestroyAllEnemies(player);

            CoinSpawner c;

            if (player.giant)
            {
                //プレイヤーが巨大の場合、ゴールポールを破壊してしまう。
                if (GameManager.courseMode == CourseMode.RingAttack) {
                    c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                    c.SetUp(10, player);
                } else {
                    player.scorePopUp(12800, false, this.transform.position, 6);
                }
                cleared(player.gameObject);
            }
            else
            {
                float yPos = col.gameObject.transform.position.y - (this.transform.position.y + 2.55125f);
                int height = (int)Math.Round((yPos / 20) * 7);
                height = Math.Min(height, 7);
                int score = (int)Math.Pow(2, height) * 100;
                bool perfect = height == 7;

                score = (int)((float)score * GameManager.GetEqualityRate()[player.playerNumber]);

                if (GameManager.courseMode == CourseMode.RingAttack) {
                    height = (int)Math.Round((yPos / 15.55) * 9);
                    height = Math.Min(height, 9);
                    score = (int)height + 1;
                    perfect = height == 9;
                }

                if (perfect) {
                    SoundPlay(touchedTopSound);
                    step = 1;
                    topEffect.SetActive(true);
                } else {
                    SoundPlay(touchedSound);
                    step = 2;
                }

                Vector3 position = this.transform.position;
                position.y = col.gameObject.transform.position.y;
                if (GameManager.courseMode == CourseMode.RingAttack) {
                    c = Instantiate(coinSpawner, position, Quaternion.identity).GetComponent<CoinSpawner>();
                    c.SetUp(score, player);
                } else {
                    player.scorePopUp(score, false, position, Math.Max(0, height - 1));
                }

                StartCoroutine(player.GoalPole(this, perfect));
            }
        }
    }

    public void PlayDownSound()
    {
        this.GetComponent<AudioSource>().clip = downSound;
        this.GetComponent<AudioSource>().Play();
    }
    public void PlayFlagChangeSound(bool isTop)
    {
        this.GetComponent<AudioSource>().clip = flagChangeSound;
        this.GetComponent<AudioSource>().Play();

        if (isTop)
        {
            this.GetComponent<AudioSource>().PlayOneShot(goldAddSound);
            goldFlagEffect.SetActive(true);
        }
    }
}
