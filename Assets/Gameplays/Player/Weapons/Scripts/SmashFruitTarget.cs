using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmashFruitTarget : ComboManager
{
    [Header("状態")]
    public bool preparing = true;
    [Header("フルーツスキン")]
    public int index;
    public SpriteRenderer skin;
    public Sprite[] fruitSkins = new Sprite[8];

    private int attackPower = 1;
    private int bossAttackPower = 1;
    private bool freeze = false;

    private bool hit = false;
    private bool alreadyHit = false;

    protected Rigidbody rb;
    protected Vector3 velocity;
    private Vector3 objNormalVector = Vector3.zero;
    // protected Vector3 afterReflectVelo = Vector3.zero;

    private float timeLimit = 1.5f;

    [Header("重力")]
    public float gravity = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // afterReflectVelo = velocity;
    }

    void FixedUpdate()
    {
        if (!preparing)
        {
            velocity.y -= gravity;
            rb.linearVelocity = velocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //攻撃力調整
        freeze = false;
        switch (index)
        {
            case 0:
            //チェリー
            case 1:
            //イチゴ
            attackPower = 1;
            bossAttackPower = 1;
            break;
            
            case 2:
            //オレンジ
            case 3:
            //リンゴ
            attackPower = 3;
            bossAttackPower = 1;
            break;

            case 4:
            //メロン
            attackPower = 5;
            bossAttackPower = 3;
            break;

            case 5:
            //ボス・ギャラクシアン
            attackPower = 3;
            bossAttackPower = 1;
            break;

            case 6:
            //ベル
            attackPower = 0;
            bossAttackPower = 0;
            freeze = true;
            break;

            case 7:
            //カギ
            attackPower = 5;
            bossAttackPower = 5;
            break;
        }

        if (hit)
        {
            // ScoreIncrease();
            if (index < 5)
            {
                //チェリー～メロンまでは命中した時、上に上がる。
                velocity = Vector3.up * 42.5f;
                gravity = 2f;
                if (!alreadyHit)
                {
                    timeLimit = 0.75f;
                    alreadyHit = true;
                }
            }
            hit = false;
        }
        if (velocity.y <= 0f && index == 6)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        if (timeLimit > 0f && !preparing)
        {
            timeLimit -= Time.deltaTime;
            if (timeLimit <= 0f)
            {
                Destroy(gameObject);
            }
        }

        skin.sprite = fruitSkins[index];
    }

    public void Thrown()
    {
        float forwardSpeed = 0f;
        float upSpeed = 0f;
        timeLimit = 1.5f;
        switch (index)
        {
            case 0:
            //チェリー
            forwardSpeed = 20f;
            upSpeed = 0f;
            gravity = 2f;
            break;
            
            case 1:
            //イチゴ
            forwardSpeed = 30f;
            upSpeed = 0f;
            gravity = 2f;
            break;
            
            case 2:
            //オレンジ
            forwardSpeed = 40f;
            upSpeed = 0f;
            gravity = 0f;
            break;

            case 3:
            //リンゴ
            forwardSpeed = 30f;
            upSpeed = -30f;
            gravity = 0f;
            break;

            case 4:
            //メロン
            forwardSpeed = 12.5f;
            upSpeed = 5f;
            gravity = 0.1f;
            timeLimit = 3f;
            break;

            case 5:
            //ボス・ギャラクシアン
            forwardSpeed = 30f;
            upSpeed = 0f;
            timeLimit = 3f;
            break;

            case 6:
            //ベル
            forwardSpeed = 20f;
            upSpeed = 60f;
            gravity = 2f;
            timeLimit = 2f;
            break;

            case 7:
            //カギ
            forwardSpeed = 100f;
            upSpeed = 0f;
            gravity = 0f;
            break;
        }

        velocity = this.transform.forward * forwardSpeed + Vector3.up * upSpeed;
        // afterReflectVelo = velocity;
        preparing = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (preparing)
        {
            return;
        }
        
        if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy")
        {
            EnemyManager enemy = col.gameObject.GetComponent<EnemyManager>();
            enemy.TakeDamage(true, player, attackPower, 1, false, this);
            hit = true;
        }
        else if (LayerMask.LayerToName(col.gameObject.layer) == "Boss")
        {
            BossManager boss = col.gameObject.GetComponent<BossManager>();
            if (bossAttackPower == 5)
            {
                boss.Damage(player, boss.singleDamage, false);
            } else
            {
                boss.Damage(player, bossAttackPower * 2, false);
            }
            hit = true;
        }
        else if (LayerMask.LayerToName(col.gameObject.layer) == "Default")
        {
            //跳ね返る
            RaycastHit hit;
            Physics.Raycast(transform.position - velocity.normalized * 3f, velocity.normalized, out hit, 6f);

            objNormalVector = hit.normal;
            // Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
            Vector3 reflectVec = Vector3.Reflect(velocity, objNormalVector);
            velocity = reflectVec;
            if (index < 2)
            {
                velocity.y *= 0.75f;
            }
            if (velocity.y > 40f)
            {
                velocity.y = 40f;
            }
            // 計算した反射ベクトルを保存
            // afterReflectVelo = velocity;

            if (index == 3)
            {
                gravity = 2.5f;
            }
        }
        if (col.gameObject.GetComponent<MonitorManager>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                MonitorManager monitor = col.gameObject.GetComponent<MonitorManager>();
                monitor.player = player;
                monitor.destroyed = true;
                hit = true;
            }
        }
        else if (col.gameObject.GetComponent<ItemCapsule>() != null)
        {
            if (!col.gameObject.GetComponent<ItemCapsule>().destroyed)
            {
                ItemCapsule capsule = col.gameObject.GetComponent<ItemCapsule>();
                capsule.player = player;
                capsule.destroyed = true;
                hit = true;
            }
        }
        else if (col.gameObject.GetComponent<QuestionBlockManager>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false, false);
                hit = true;
            }
        }
        else if (col.gameObject.GetComponent<Box>() != null)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
            {
                StartCoroutine(col.gameObject.GetComponent<Box>().DestroyBox(player));
                hit = true;
            }
        }
        else if (col.gameObject.GetComponent<EnemyIce>() != null)
        {
            col.gameObject.GetComponent<EnemyIce>().TakeDamage();
            hit = true;
        }
    }

    void ScoreIncrease()
    {
        switch (index)
        {
            case 0:
            //チェリー
            player.scorePopUp(100, false, this.transform.position);
            break;
            
            case 1:
            //イチゴ
            player.scorePopUp(300, false, this.transform.position);
            break;
            
            case 2:
            //オレンジ
            player.scorePopUp(500, false, this.transform.position);
            break;

            case 3:
            //リンゴ
            player.scorePopUp(700, false, this.transform.position);
            break;

            case 4:
            //メロン
            player.scorePopUp(1000, false, this.transform.position);
            break;

            case 5:
            //ボス・ギャラクシアン
            player.scorePopUp(2000, false, this.transform.position);
            break;

            case 6:
            //ベル
            player.scorePopUp(3000, false, this.transform.position);
            break;

            case 7:
            //カギ
            player.scorePopUp(5000, false, this.transform.position);
            break;
        }
    }
}
