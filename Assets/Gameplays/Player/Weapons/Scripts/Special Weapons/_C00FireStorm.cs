using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _C00FireStorm : WeaponMovements
{
    [Header("炎")]
    public GameObject fire;

    private bool trigger = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        

        weaponType = WeaponTypes.Fire;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            this.transform.position = player.gameObject.transform.position;
        }

        if (player != null && !trigger)
        {
            StartCoroutine("LifeTime");

            WeaponMovements wp = Instantiate(fire, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation).GetComponent<WeaponMovements>();
            wp.player = player;

            trigger = true;
        }
    }
    void FixedUpdate()
    {
        this.transform.GetChild(0).Rotate(0f, 20f, 0f, Space.Self);
        this.transform.GetChild(0).Rotate(20f, 0f, 0f, Space.Self);
        this.transform.GetChild(0).Rotate(0f, 0f, 20f, Space.Self);
    }
    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
