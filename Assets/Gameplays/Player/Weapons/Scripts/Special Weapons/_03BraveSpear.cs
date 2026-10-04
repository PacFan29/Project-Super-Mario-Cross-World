using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _03BraveSpear : WeaponMovements
{
    [Header("効果音")]
    public AudioClip hitWallSound;
    public AudioClip bounceSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        velocity = this.transform.forward * 60f;
        weaponType = WeaponTypes.Needle;

        StartCoroutine("LifeTime");
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && !isAttacking && col.gameObject.transform.position.y > this.transform.position.y)
        {
            this.GetComponent<AudioSource>().PlayOneShot(bounceSound);
            col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(50f, transform.up, false);
        }
    }

    public override IEnumerator AfterWallCollision()
    {
        this.GetComponent<AudioSource>().PlayOneShot(hitWallSound);
        velocity = Vector3.zero;
        isAttacking = false;
        yield return new WaitForSeconds(3.5f);


        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(2f);

        if (isAttacking)
        {
            Destroy(gameObject);
        }
    }
}
