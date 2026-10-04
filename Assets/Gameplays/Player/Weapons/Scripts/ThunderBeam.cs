using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderBeam : ComboManager
{
    private Rigidbody rb;
    private Vector3 velocity;
    private Vector3 objNormalVector = Vector3.zero;
    private Vector3 afterReflectVelo = Vector3.zero;

    public GameObject hitEffect;

    // Start is called before the first frame update
    void Start()
    {
        velocity = transform.forward * 40f;
        velocity.y = 0f;

        afterReflectVelo = velocity;

        rb = GetComponent<Rigidbody>();
        StartCoroutine("LifeTime");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = velocity;

        this.transform.forward = velocity.normalized;
    }
    
    IEnumerator LifeTime() {
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    void OnTriggerStay(Collider col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy")
        {
            if (col.GetComponent<EnemyManager>() != null)
            {
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                if (enemy.HP > 0)
                {
                    Destroy(gameObject);
                }
                enemy.TakeDamage(true, player, 1, 1, false, this, WeaponTypes.Thunder);
            }
            else if (col.GetComponent<ShellManager>() != null)
            {
                col.GetComponent<ShellManager>().Shot(player);
                Destroy(gameObject);
            }
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Boss")
        {
            BossManager boss = col.GetComponent<BossManager>();
            boss.Damage(player, 2, false, WeaponTypes.Thunder);
            Destroy(gameObject);
        }
        if (col.gameObject.GetComponent<Box>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                StartCoroutine(col.gameObject.GetComponent<Box>().DestroyBox(player));

                Destroy(gameObject);
            }
        }
        if (col.gameObject.GetComponent<QuestionBlockManager>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);

                Destroy(gameObject);
            }
        }
        if (col.gameObject.GetComponent<MonitorManager>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                MonitorManager monitor = col.gameObject.GetComponent<MonitorManager>();
                monitor.player = player;
                monitor.destroyed = true;

                Destroy(gameObject);
            }
        }
        if (col.gameObject.GetComponent<ItemCapsule>() != null)
        {
            if (!col.gameObject.GetComponent<ItemCapsule>().destroyed)
            {
                ItemCapsule capsule = col.gameObject.GetComponent<ItemCapsule>();
                capsule.player = player;
                capsule.destroyed = true;

                Destroy(gameObject);
            }
        }
        if (col.GetComponent<EnemyIce>() != null)
        {
            col.GetComponent<EnemyIce>().TakeDamage(WeaponTypes.Thunder);
            Destroy(gameObject);
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default")
        {
            //跳ね返る
            RaycastHit hit;
            Vector3 XZvel = new Vector3(velocity.x, 0f, velocity.z);

            bool wallHit = Physics.Raycast(transform.position - XZvel.normalized * 6f, XZvel.normalized, out hit, 6f, 1 << 0);

            if (wallHit)
            {
                objNormalVector = hit.normal;
                Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
                velocity.x = reflectVec.x;
                velocity.z = reflectVec.z;
                // 計算した反射ベクトルを保存
                afterReflectVelo = velocity;
                afterReflectVelo.y = 0;
                return;
            }
        }
    }

    void OnDestroy() {
        EffectDamage burst = Instantiate(hitEffect, this.transform.position, this.transform.rotation).GetComponent<EffectDamage>();
        burst.player = player;
        burst.weaponType = WeaponTypes.Thunder;
    }
}
