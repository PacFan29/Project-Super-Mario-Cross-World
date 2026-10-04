using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardBrickManager : MonoBehaviour
{
    [Header("効果音")]
    public AudioClip breakedSound;
    [HideInInspector] public bool breaked = false;

    [Header("破片")]
    public GameObject skin;
    public GameObject brickBreak;

    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }
    void Update()
    {
        skin.SetActive(!breaked);
    }
    public void BlockHit(PlayerInfo player, WeaponTypes weapon = WeaponTypes.None)
    {
        if (weapon == WeaponTypes.Bomb)
        {
            StartCoroutine(DestroyBlock(player));
        }
    }
    public IEnumerator DestroyBlock(PlayerInfo player)
    {
        source.PlayOneShot(breakedSound);
        player.scoreIncrease(100, this.transform.position);

        this.GetComponent<BoxCollider>().isTrigger = true;
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        this.gameObject.tag = "Other";

        breaked = true;
        Instantiate(brickBreak, this.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.1f);

        Destroy(gameObject);
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<PlayerInfo>() != null)
        {
            if (col.gameObject.GetComponent<PlayerInfo>().giant)
            {
                StartCoroutine(DestroyBlock(col.gameObject.GetComponent<PlayerInfo>()));
            }
        }
    }
}
