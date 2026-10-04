using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _13Bobomb : EnemyMovements
{
    public GameObject spark;
    private float explodeTimeLimit = 3f;
    private bool explodeMode = false;
    // Update is called once per frame
    public override void Movements()
    {
        Vector3 runVel = Vector3.zero;

        time -= Time.deltaTime;
        if (explodeMode)
        {
            explodeTimeLimit -= Time.deltaTime;
        }

        spark.SetActive(explodeTimeLimit <= 2.5f);
        if (explodeTimeLimit <= 0f)
        {
            Instantiate(manager.vanishEffect, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        switch (action)
        {
            case 0:
                runVel = Vector3.zero;
                if (time <= 0f)
                {
                    if (LookingAnim(0.15f))
                    {
                        action = 1;
                        time = 2f;
                    }
                }
                else
                {
                    directionRate = 0f;
                    startTime = Time.time;
                    startDir = direction;
                    endDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
                }
                break;

            case 1:
                runVel = 3 * direction;

                if (time <= 0f)
                {
                    action = 0;
                    time = 1f;
                }
                break;

            case 2:
                direction = targetPlayer.transform.position - this.transform.position;
                direction.y = 0;

                runVel = 10 * direction.normalized;
                break;

            default:
                runVel = Vector3.zero;
                break;
        }

        if (action < 2)
        {
            SetPlayer();
        }

        if (PlayerFound())
        {
            if (action < 2)
            {
                directionRate = 0f;
                startTime = Time.time;
                startDir = direction;
                Vector3 d = targetPlayer.transform.position - this.transform.position;
                d.y = 0;
                endDir = d.normalized;
                action = 3;
            }
            if (action == 3)
            {
                if (LookingAnim(0.15f))
                {
                    explodeMode = true;
                    action = 4;
                    StartCoroutine("Attack");
                }
            }
            time = 0.2f;
        }
        // else
        // {
        //     if (action > 1) action = 0;
        // }

        manager.velocity.x = runVel.x;
        manager.velocity.z = runVel.z;
    }

    IEnumerator Attack() {
        manager.actionId = 1;
        yield return new WaitForSeconds(0.75f);

        if (manager.isActive() && manager.isNear()) {
            manager.actionId = 0;
            action = 2;
        }
    }
}
