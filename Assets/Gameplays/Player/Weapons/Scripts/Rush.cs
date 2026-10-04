using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rush : MonoBehaviour
{
    private Vector3 coilCollider = new Vector3(2f, 2f, 3f);
    private Vector3 jetCollider = new Vector3(2f, 1f, 4f);

    public enum RushMode
    {
        Coil,
        Jet
    }

    [Header("持ち主")]
    public MegaManActions owner;
    [Header("状態")]
    public RushMode mode;
    [Header("スキン")]
    public GameObject coilSkin;
    public GameObject jetSkin;

    private bool activate = true;
    private BoxCollider box;
    private Vector3 velocity;
    private bool jetMoving = false;

    GameObject ridingPlayer;

    [Header("効果音")]
    public AudioSource soundSource;
    public AudioClip fallSound;
    public AudioClip appearSound;
    public AudioClip jumpSound;
    public AudioClip jetStartSound;
    public AudioClip returnSound;
    public AudioSource jetLoop;
    [Header("ボイス")]
    public AudioSource voiceSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        box = this.GetComponent<BoxCollider>();
        jetMoving = false;

        velocity = Vector3.zero;

        soundSource.PlayOneShot(appearSound);
    }

    // Update is called once per frame
    void Update()
    {
        coilSkin.SetActive(mode == RushMode.Coil);
        jetSkin.SetActive(mode == RushMode.Jet);

        if (mode == RushMode.Coil)
        {
            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        } else
        {
            this.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        if (mode == RushMode.Coil)
        {
            box.size = coilCollider;
        } else if (mode == RushMode.Jet)
        {
            box.size = jetCollider;
        }

        if (jetMoving)
        {
            velocity = this.transform.forward * 12.5f;

            if (owner.RanOutOfRushEnergy())
            {
                Leave();
            }
            if (ridingPlayer != null)
            {
                velocity += ridingPlayer.GetComponent<PlayerInfo>().rushJetInput * 4.5f;
                owner.ConsumeRushJetEnergy();
            }
        } else
        {
            velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        this.transform.position += velocity * 0.05f;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<PlayerInfo>() != null && activate)
        {
            //他のプレイヤーでも可能。ただし、エネルギー消費は呼び出した本人が負担する。
            //例：1Pがロックマン、2Pがマリオ
            //　マリオがラッシュコイルを使って高く飛んだ
            //　ロックマンの残り武器エネルギーが減る
            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            if (mode == RushMode.Coil)
            {
                soundSource.PlayOneShot(jumpSound);
                player.JumpFromSprings(80f, Vector3.up);
                owner.ConsumeRushCoilEnergy();
                activate = false;
                StartCoroutine("RushCoilEnd");
            }
            if (mode == RushMode.Jet)
            {
                soundSource.PlayOneShot(jetStartSound);
                jetLoop.Play();

                col.gameObject.transform.parent = transform;
                ridingPlayer = col.gameObject;
                player.rushJetObj = this.transform;
                col.gameObject.transform.localPosition = new Vector3(0f, 2.25f, 0f);
                col.gameObject.GetComponent<Rigidbody>().position = col.gameObject.transform.position;
                jetMoving = true;
            }
        }
    }
    void OnCollisionExit(Collision col) {
        if (col.gameObject.GetComponent<PlayerInfo>() != null) {
            col.gameObject.transform.parent = null;
            col.gameObject.GetComponent<PlayerInfo>().rushJetObj = null;
            ridingPlayer = null;
        }
    }

    void Leave()
    {
        jetLoop.Stop();
        soundSource.PlayOneShot(returnSound);

        jetMoving = false;
        if (ridingPlayer != null)
        {
            if (ridingPlayer.transform.parent == this.transform)
            {
                ridingPlayer.GetComponent<PlayerInfo>().rushJetObj = null;
                ridingPlayer.transform.parent = null;
            }
        }
        Destroy(gameObject);
    }
    IEnumerator RushCoilEnd()
    {
        yield return new WaitForSeconds(0.5f);

        if (!activate)
        {
            Leave();
        }
    }

    public void Cancel()
    {
        Leave();
    }

    public void ModeChange(RushMode m, Vector3 pos)
    {
        if (
            (mode == RushMode.Coil && !activate) || 
            (mode == RushMode.Jet && jetMoving)
            )
        {
            this.transform.position = pos;
            activate = true;
            jetMoving = false;
        }
        mode = m;
    }
}
