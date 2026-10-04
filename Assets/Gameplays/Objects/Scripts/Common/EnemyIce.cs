using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIce : ComboManager
{
    public bool weaknessEffective = false;
    public EnemyManager enemy;

    public float HP = 1;
    private float maxHP;
    private float damageTime;

    private bool attacking = false;

    private float timeLimit = 8f;
    private GrabableObject grabable;

    public GameObject breakEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (this.GetComponent<GrabableObject>() == null)
        {
            grabable = this.gameObject.AddComponent<GrabableObject>();
            grabable.grabMode = GrabableObject.GrabMode.Upward;
        }
        else
        {
            grabable = this.GetComponent<GrabableObject>();
        }
        grabable.enableRigidbody = true;
    }

    public void setHP()
    {
        Start();
        
        float xSize = transform.localScale.x / 0.625f;
        float ySize = transform.localScale.y / 0.625f;
        float zSize = transform.localScale.z / 0.625f;
        float boxArea = xSize * ySize * zSize;

        HP = Math.Max(1, (float)Math.Round(boxArea / 6f));
        maxHP = HP;

        grabable.grabable = HP < 6;
    }

    // Update is called once per frame
    void Update()
    {
        if (grabable.grabableTrigger == 0)
        {
            timeLimit -= Time.deltaTime;
        }
        else if (grabable.grabableTrigger == 2)
        {
            Vector3 velocity = 45 * this.transform.forward;
            velocity.y = GetComponent<Rigidbody>().linearVelocity.y;
            GetComponent<Rigidbody>().linearVelocity = velocity;
        }

        if (damageTime > 0)
        {
            damageTime -= Time.deltaTime;
        }

        if (timeLimit <= 0f)
        {
            if (enemy != null)
            {
                enemy.gameObject.transform.parent = null;
                enemy.FreeFromFrozen(this.GetComponent<ComboManager>(), attacking, weaknessEffective);
            }
            Instantiate(breakEffect, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        if (grabable.player != null)
        {
            player = grabable.player;
        }
    }

    public void Break()
    {
        if (enemy != null)
        {
            combo = 0;
            enemy.gameObject.transform.parent = null;
            enemy.FreeFromFrozen(this.GetComponent<ComboManager>(), true, weaknessEffective);
        }
        Instantiate(breakEffect, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnCollisionStay(Collision col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy" && grabable.grabableTrigger == 2)
        {
            EnemyManager enemy = col.gameObject.GetComponent<EnemyManager>();
            if (!enemy.TakeDamage(true, player, 3, 0, false, this).beated)
            {
                Break();
            }
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Boss" && grabable.grabableTrigger == 2)
        {
            BossManager boss = col.gameObject.GetComponent<BossManager>();
            boss.Damage(player, boss.singleDamage, false);
            Break();
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default" && grabable.grabableTrigger == 2)
        {
            //跳ね返る
            // 当たった物体の法線ベクトルを取得
            Vector3 objNomalVector = col.contacts[0].normal;

            RaycastHit hit;
            Vector3 XZvel = new Vector3(GetComponent<Rigidbody>().linearVelocity.x, 0f, GetComponent<Rigidbody>().linearVelocity.z);
            bool wallHit = Physics.Raycast(transform.position - XZvel.normalized * 3f, XZvel.normalized, out hit, 6f, 1 << 0);

            // if (objNomalVector.y < 0.1f && wallHit)
            if (wallHit)
            {
                Break();
            }
        }
    }

    public bool TakeDamage(WeaponTypes weapon = WeaponTypes.None, bool canPenetrate = false, bool halfDamage = false)
    {
        if (damageTime > 0f)
        {
            return false;
        }

        if (canPenetrate && damageTime <= 0f)
        {
            damageTime = 0.3f;
        }

        switch (weapon)
        {
            case WeaponTypes.Fire:
            case WeaponTypes.Bomb:
                //火属性・爆弾（効果抜群）
                HP -= 6;
                break;

            case WeaponTypes.Thunder:
                //雷属性（超効果抜群）
                HP -= 10;
                break;

            case WeaponTypes.Ice:
                //氷属性（回復）
                HP = maxHP;
                timeLimit = 8f;
                break;

            default:
                //その他
                if (halfDamage)
                {
                    HP -= 0.5f;
                }
                else
                {
                    HP -= 1;
                }
                break;
        }

        if (HP <= 0)
        {
            Break();
        }
        return HP <= 0;
    }
}
