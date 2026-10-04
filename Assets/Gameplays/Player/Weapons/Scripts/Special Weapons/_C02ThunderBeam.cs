using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _C02ThunderBeam : WeaponMovements
{
    [Header("ビーム")]
    public GameObject beam;

    private bool trigger = false;

    void Update()
    {
        if (player != null && !trigger)
        {
            this.transform.position = player.gameObject.transform.position;

            WeaponMovements wp = Instantiate(beam, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation).GetComponent<WeaponMovements>();
            wp.player = player;

            wp = Instantiate(beam, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation * Quaternion.Euler(-90f, 0f, 0f)).GetComponent<WeaponMovements>();
            wp.player = player;

            wp = Instantiate(beam, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation * Quaternion.Euler(90f, 0f, 0f)).GetComponent<WeaponMovements>();
            wp.player = player;

            wp = Instantiate(beam, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation * Quaternion.Euler(0f, -90f, 0f)).GetComponent<WeaponMovements>();
            wp.player = player;

            wp = Instantiate(beam, this.transform.position + this.transform.forward * 1.5f, this.transform.rotation * Quaternion.Euler(0f, 90f, 0f)).GetComponent<WeaponMovements>();
            wp.player = player;

            trigger = true;

            StartCoroutine("LifeTime");
        }
    }
    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
