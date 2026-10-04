using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _05BoulderShield : WeaponMovements
{
    [Header("回転グループ")]
    public Transform group;
    [Header("残っている岩")]
    public GameObject[] stoneObjs = new GameObject[8];
    public GameObject breakEffect;

    private int remainedStones = 8;
    private bool holding = true;

    private float lifeTime = 1.25f;

    private void Awake() {
        weaponType = WeaponTypes.Stone;
        remainedStones = 8;
        holding = true;
        lifeTime = 1.25f;
    }

    void Update()
    {
        group.Rotate(0f, 600f * Time.deltaTime, 0f, Space.Self);
        group.Rotate(800f * Time.deltaTime, 0f, 0f, Space.Self);

        if (holding)
        {
            comboLimit = 1;
            if (player != null)
            {
                this.transform.position = player.gameObject.transform.position;
                if (player.ButtonsDown["X"])
                {
                    lifeTime = 1.25f;
                    holding = false;
                }
            }
            velocity = Vector3.zero;
            this.transform.forward = player.skin.forward;
        } else
        {
            comboLimit = 99;
            velocity = this.transform.forward * 80f;

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                Destroy(gameObject);
            }
        }

        if (hitTrigger)
        {
            remainedStones--;
            Instantiate(breakEffect, stoneObjs[remainedStones].transform.position, Quaternion.Euler(-90f, 0f, 0f));
            hitTrigger = false;
        }

        if (remainedStones < 1)
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < stoneObjs.Length; i++)
        {
            stoneObjs[i].SetActive(i < remainedStones);
        }
    }
}
