using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueCoin : MonoBehaviour
{
    [HideInInspector] public GameObject targetPlayer;
    [HideInInspector] public bool magnetised = false;
    public GameObject[] skins = new GameObject[2];

    public GameObject coinEffect;
    public AudioClip coinSound;
    public AudioClip ringSound;

    void Awake()
    {
        Update();
    }

    void Update()
    {
        skins[0].SetActive(!(GameManager.sonicStage || GameManager.courseMode == CourseMode.RingAttack));
        skins[1].SetActive(GameManager.sonicStage || GameManager.courseMode == CourseMode.RingAttack);
    }

    void FixedUpdate()
    {
        this.transform.Rotate(0f, -3f, 0f, Space.Self);

        if (magnetised)
        {
            GetComponent<Rigidbody>().linearVelocity = (targetPlayer.transform.position - this.transform.position).normalized * 100f;
        }
        else
        {
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players)
            {
                if (
                    Vector3.Distance(player.transform.position, transform.position) <= 16 &&
                    (
                        (player.GetComponent<PlayerInfo>().elecProof ||
                        (player.GetComponent<_16MSonic>() != null && player.GetComponent<_16MSonic>().boost))
                    )
                )
                {
                    targetPlayer = player;
                    magnetised = true;
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            GameManager.Coins += 2;
            GameManager.ringRushProgress += 0.02f;
            player.scoreIncrease(250, this.transform.position);

            bool isRing = skins[1].activeSelf;

            AudioSource playerGotit = col.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
            if (isRing)
            {
                playerGotit.clip = ringSound;
                playerGotit.volume = 0.7f;
            } else
            {
                playerGotit.clip = coinSound;
                playerGotit.volume = 1f;
            }
            playerGotit.Play();

            if (col.GetComponent<_16MSonic>() != null) {
                col.GetComponent<_16MSonic>().BoostIncrease(1, false);
            }

            Instantiate(coinEffect, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
