using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceBall : ComboManager
{
    public bool downward = false;
    public Transform skin;
    private Rigidbody rb;
    private Vector3 velocity;
    private Vector3 objNormalVector = Vector3.zero;
    private Vector3 afterReflectVelo = Vector3.zero;

    public GameObject hitEffect;

    // Start is called before the first frame update
    public void OnStart()
    {
        if (downward)
        {
            velocity = transform.forward * 28.28f;
            velocity.y = -28.28f;
        } else {
            velocity = transform.forward * 40f;
        }

        afterReflectVelo = velocity;

        rb = GetComponent<Rigidbody>();
        StartCoroutine("LifeTime");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // velocity.y -= 2.5f;
        // if (velocity.y < -40f) {
        //     velocity.y = -40f;
        // }
        rb.linearVelocity = velocity;

        skin.forward = velocity.normalized;
    }

    IEnumerator LifeTime() {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }

    // void OnTriggerStay(Collider col) {
    void OnTriggerEnter(Collider col) {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy") {
            if (col.GetComponent<EnemyManager>() != null) {
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                if (enemy.HP > 0) {
                    Destroy(gameObject);
                }
                enemy.TakeDamage(true, player, 1, 1, false, this, WeaponTypes.Insulator);
            } else if (col.GetComponent<ShellManager>() != null) {
                col.GetComponent<ShellManager>().Shot(player);
                Destroy(gameObject);
            }
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Boss"){
            BossManager boss = col.GetComponent<BossManager>();
            boss.Damage(player, 2, false, WeaponTypes.Insulator);
            Destroy(gameObject);
        }
        if (col.gameObject.GetComponent<Box>() != null) {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast") {
                StartCoroutine(col.gameObject.GetComponent<Box>().DestroyBox(player));

                Destroy(gameObject);
            }
        }
        if (col.GetComponent<EnemyIce>() != null)
        {
            col.GetComponent<EnemyIce>().TakeDamage(WeaponTypes.Insulator);
            Destroy(gameObject);
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default")
        {
            //跳ね返る
            RaycastHit hit;
            Physics.Raycast(transform.position - velocity.normalized * 3f, velocity.normalized, out hit, 6f, 1 << 0);

            objNormalVector = hit.normal;
            Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
            velocity = reflectVec;
            // 計算した反射ベクトルを保存
            afterReflectVelo = velocity;
        }

        if (col.GetComponent<CoinManager>() != null){
            CoinManager coin = col.GetComponent<CoinManager>();
            coin.CoinGet(player.gameObject);
        }
    }

    void OnDestroy() {
        Instantiate(hitEffect, this.transform.position, this.transform.rotation);
    }
}
