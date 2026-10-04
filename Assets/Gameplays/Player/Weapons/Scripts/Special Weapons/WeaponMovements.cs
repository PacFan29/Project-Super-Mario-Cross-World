using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponMovements : ComboManager
{
    [Header("重力")]
    public float gravity = 0f;
    [Header("各状態")]
    public bool isAttacking; //攻撃状態か
    public bool canPenetrate = false; //敵にあたっても貫通するか
    public int penetrateWall = 0; //壁を貫通するか（0＝貫通しない、1＝貫通する、2＝コルーチン）
    private bool wallCoroutine = false;
    public bool reflectable = false; //跳ね返るか
    protected Rigidbody rb;
    protected Vector3 velocity;
    private Vector3 objNormalVector = Vector3.zero;
    protected Vector3 afterReflectVelo = Vector3.zero;

    protected WeaponTypes weaponType = WeaponTypes.None;

    protected bool hitTrigger = false;

    protected bool activeVelocity = true;
    [Header("攻撃力")]
    public int attackPower = 2;
    public int bossAttackPower = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        afterReflectVelo = velocity;
    }

    void FixedUpdate()
    {
        velocity.y -= gravity;

        if (activeVelocity)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.AddForce(-Vector3.up * gravity * 20f, ForceMode.Acceleration);
        }
    }

    void OnCollisionStay(Collision col)
    {
        CollisionActive(col.gameObject);
    }

    void OnTriggerStay(Collider col)
    {
        CollisionActive(col.gameObject);
    }

    void CollisionActive(GameObject col)
    {
        bool beated = false;
        bool reflected = false;
        if (LayerMask.LayerToName(col.layer) == "Enemy")
        {
            if (isAttacking)
            {
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                hitTrigger = enemy.damageTime <= 0f;
                (beated, reflected) = enemy.TakeDamage(true, player, attackPower, 1, false, this, weaponType, canPenetrate, false, false, this.transform.position);
                hitTrigger = hitTrigger && !beated;
            }
            if (!canPenetrate) { Destroy(gameObject); }
        }
        else if (LayerMask.LayerToName(col.layer) == "Boss")
        {
            if (isAttacking)
            {
                BossManager boss = col.GetComponent<BossManager>();
                if (this.GetComponent<RollBloom>() != null)
                {
                    hitTrigger = boss.damageInterval <= 0f;
                    boss.Damage(player, boss.singleDamage, false, weaponType, canPenetrate);
                } else
                {
                    hitTrigger = boss.damageInterval <= 0f;
                    boss.Damage(player, bossAttackPower * 2, false, weaponType, canPenetrate);
                }
            }
            if (!canPenetrate) { Destroy(gameObject); }
        }
        else if (LayerMask.LayerToName(col.layer) == "Default")
        {
            //跳ね返る
            if (reflectable)
            {
                RaycastHit hit;
                Physics.Raycast(transform.position - velocity.normalized * 3f, velocity.normalized, out hit, 6f);

                objNormalVector = hit.normal;
                Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
                velocity = reflectVec;
                // 計算した反射ベクトルを保存
                afterReflectVelo = velocity;
            }
            else if (penetrateWall == 2 && !wallCoroutine)
            {
                wallCoroutine = true;
                StartCoroutine("AfterWallCollision");
            }
            else if (penetrateWall == 0)
            {
                Destroy(gameObject);
            }
        }
        if (col.GetComponent<MonitorManager>() != null)
        {
            if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
            {
                MonitorManager monitor = col.GetComponent<MonitorManager>();
                monitor.player = player;
                monitor.destroyed = true;

                if (!canPenetrate)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<ItemCapsule>() != null)
        {
            if (!col.GetComponent<ItemCapsule>().destroyed)
            {
                ItemCapsule capsule = col.GetComponent<ItemCapsule>();
                capsule.player = player;
                capsule.destroyed = true;

                if (!canPenetrate)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<QuestionBlockManager>() != null)
        {
            if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
            {
                col.GetComponent<QuestionBlockManager>().BlockHit(player, false, false, weaponType);

                if (!canPenetrate)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<HardBrickManager>() != null)
        {
            if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
            {
                col.GetComponent<HardBrickManager>().BlockHit(player, weaponType);

                if (!canPenetrate)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<Box>() != null)
        {
            if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
            {
                StartCoroutine(col.GetComponent<Box>().DestroyBox(player));

                if (!canPenetrate)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<EnemyIce>() != null)
        {
            col.GetComponent<EnemyIce>().TakeDamage(weaponType, canPenetrate);
            if (!canPenetrate)
            {
                Destroy(gameObject);
            }
        }
    }

    public void VelocityChange(int index, float speed)
    {
        switch (index)
        {
            case 0:
                velocity.x = speed;
                break;

            case 1:
                velocity.y = speed;
                break;

            case 2:
                velocity.z = speed;
                break;
        }
    }

    public virtual IEnumerator AfterWallCollision()
    {
        yield return null;
    }
}
