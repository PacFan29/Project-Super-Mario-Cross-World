using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _01Metall : EnemyMovements
{
    [Header("弾")]
    public GameObject bullet;

    private bool playerFar = true;
    private bool shot = true;

    // Update is called once per frame
    public override void Movements()
    {

        if (PlayerFoundMet() && playerFar)
        {
            playerFar = false;
            shot = false;
            time = 1.4f;
            manager.actionId = 1;
        }
        if (!playerFar && time <= 0f)
        {
            if (!PlayerFoundMet())
            {
                playerFar = true;
            }
        }

        if (time > 0)
        {
            time -= Time.deltaTime;

            if (time <= 1.1f && !shot)
            {
                shot = true;
                Instantiate(bullet, this.transform.position, manager.skin.transform.rotation);
                Instantiate(bullet, this.transform.position, manager.skin.transform.rotation * Quaternion.Euler(0f, -45f, 0f));
                Instantiate(bullet, this.transform.position, manager.skin.transform.rotation * Quaternion.Euler(0f, 45f, 0f));
            }
        } else
        {
            SetPlayer();
            direction = targetPlayer.transform.position - this.transform.position;
            direction.y = 0;
            manager.actionId = 0;
        }
        manager.defended = time <= 0f;
    }

    bool PlayerFoundMet() {
        if (targetPlayer != null) {
            PlayerInfo plInfo = targetPlayer.GetComponent<PlayerInfo>();
            return GetDistance() >= 2 && GetDistance() <= 20 && SeenPlayer() && plInfo.HP > 0 && !plInfo.tookDamage;
        } else {
            return false;
        }
    }
}
