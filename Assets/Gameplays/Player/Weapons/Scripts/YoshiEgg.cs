using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YoshiEgg : ComboManager
{
    private Rigidbody rb;
    private Vector3 velocity;
    private Vector3 objNormalVector = Vector3.zero;
    private Vector3 afterReflectVelo = Vector3.zero;

    private int state = 0; //0 = ヨッシーの後ろについてくる状態、1 = 抱えられた、2 = 投げられた

    [Header("スキン")]
    public MeshRenderer skin;
    public Material[] colors = new Material[3];
    [Header("ヒット数")]
    public int hits = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        skin.material = colors[Math.Max(0, hits - 1)];
    }

    void FixedUpdate()
    {
        rb.linearVelocity = velocity;
    }
    
    void OnTriggerEnter(Collider col) {
        if (IsThrown())
        {
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
                hits--;
                if (hits <= 0)
                {
                    Destroy(gameObject);
                } else
                {
                    RaycastHit hit;
                    Physics.Raycast(transform.position - velocity.normalized * 3f, velocity.normalized, out hit, 6f, 1 << 0);

                    objNormalVector = hit.normal;
                    Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
                    velocity = reflectVec;
                    // 計算した反射ベクトルを保存
                    afterReflectVelo = velocity;
                }
            }

            if (col.GetComponent<CoinManager>() != null){
                CoinManager coin = col.GetComponent<CoinManager>();
                coin.CoinGet(player.gameObject);
            }
        }
    }

    bool IsThrown()
    {
        return state == 2;
    }
}
