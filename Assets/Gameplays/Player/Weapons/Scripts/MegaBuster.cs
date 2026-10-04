using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBuster : ComboManager
{
    public byte level = 0;
    public float power = 1f;
    public bool stronger;
    private Rigidbody rb;
    private float sizeRate = 1f;
    public GameObject hitEffect;
    [HideInInspector] public byte powerUp = 0;
    [HideInInspector] public bool forteShoot;

    [Header("スキン")]
    public GameObject[] normalBullets = new GameObject[4];
    public GameObject[] fireBullets = new GameObject[4];
    public GameObject[] iceBullets = new GameObject[4];
    public GameObject[] thunderBullets = new GameObject[4];

    private WeaponTypes weaponType;

    private bool activeCollision = true;
    private bool getHit = false;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.shootCount < 999999999)
        {
            GameManager.shootCount++;
        }
        rb = GetComponent<Rigidbody>();
        StartCoroutine("LifeTime");
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = this.transform.forward * 50 * (activeCollision ? 1 : 2);

        //スキン、サイズ
        foreach (GameObject obj in normalBullets) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
        foreach (GameObject obj in fireBullets) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
        foreach (GameObject obj in iceBullets) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
        foreach (GameObject obj in thunderBullets) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
        
        switch (level) {
            case 1:
            // this.transform.localScale = Vector3.one * 1.5f;
            sizeRate = 1.5f;
            break;

            case 2:
            //チャージショット(攻撃力 3)
            power = 3f;
            // this.transform.localScale = Vector3.one * 2.5f;
            sizeRate = 2.5f;
            break;

            case 3:
            //ブルースストライク(攻撃力 5)
            power = 5f;
            // this.transform.localScale = Vector3.one * 3.2f;
            sizeRate = 3.2f;
            break;

            default:
            // this.transform.localScale = Vector3.one;
            sizeRate = 1f;
            break;
        }
        switch (powerUp) {
            case 0:
            normalBullets[level].SetActive(true);
            weaponType = WeaponTypes.None;
            break;
                
            case 1:
            fireBullets[level].SetActive(true);
            weaponType = WeaponTypes.Fire;
            break;

            case 2:
            iceBullets[level].SetActive(true);
            weaponType = WeaponTypes.Ice;
            break;

            case 3:
            thunderBullets[level].SetActive(true);
            weaponType = WeaponTypes.Thunder;
            break;
        }
        this.GetComponent<CapsuleCollider>().height = 2.35f * sizeRate;
        this.GetComponent<CapsuleCollider>().radius = 0.5f * sizeRate;

        //追従
        bool looking;
        GameObject hTargetObj = null;
        GameObject[] homingTargets = GameObject.FindGameObjectsWithTag("HomingTarget");
        float enDistance;
        float MaxHDistance = 20;
        bool homing = false;
        
        float minDis = Mathf.Infinity;
        for (int i = 0; i < homingTargets.Length; i++){
            //ターゲットの対象が空でないかつ、正常に動いていれば認識できる。
            looking = Vector3.Angle(this.transform.forward, (homingTargets[i].transform.position - this.transform.position)) <= 15;

            if (
                homingTargets[i] != null && 
                (
                    (homingTargets[i].GetComponent<EnemyManager>() != null && homingTargets[i].GetComponent<EnemyManager>().isActive()) ||
                    (homingTargets[i].GetComponent<BossManager>() != null && homingTargets[i].GetComponent<BossManager>().HP > 0) || 
                    (homingTargets[i].GetComponent<EnemyManager>() == null && homingTargets[i].GetComponent<BossManager>() == null)
                )
                && looking
            ){
                //プレイヤーから敵までの距離
                enDistance = Vector3.Distance(homingTargets[i].transform.position, transform.position);
                if (enDistance <= MaxHDistance && enDistance < minDis){
                    //最短距離でターゲットを認識する
                    minDis = enDistance;
                    hTargetObj = homingTargets[i];
                    homing = true;
                }
            }
        }

        if (homing) {
            Vector3 distance = hTargetObj.transform.position - this.transform.position;
            if (!forteShoot)
            {
                distance.y = 0;
            }

            this.transform.forward = Vector3.Lerp(this.transform.forward, distance.normalized, 0.25f);
        }

        //壁に触れたら破壊
        RaycastHit hit;
        if (Physics.SphereCast(this.transform.position - this.transform.forward * 0.7f, 0.5f, this.transform.forward, out hit, 1.6f, 1 << 0))
        {
            if (
                hit.collider.gameObject.GetComponent<QuestionBlockManager>() == null && 
                hit.collider.gameObject.GetComponent<MonitorManager>() == null && 
                hit.collider.gameObject.GetComponent<Box>() == null && 
                hit.collider.gameObject.GetComponent<EnemyIce>() == null
                // hit.collider.gameObject.GetComponent<EnemyManager>() == null &&
                // hit.collider.gameObject.GetComponent<BossManager>() == null &&
                // hit.collider.gameObject.GetComponent<ShellManager>() == null
            ) {
                // Debug.Log(hit.collider.gameObject.name);
                Destroy(gameObject);
            }
        }
    }
    // void OnTriggerEnter(Collider col){
    void OnTriggerStay(Collider col){
        if (!activeCollision)
        {
            return;
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy"){
            
            if (col.GetComponent<MazeGhost>() != null)
            {
                HitManager();
                col.GetComponent<MazeGhost>().BusterHit();
                Destroy(gameObject);
            }
            if (col.GetComponent<EnemyManager>() != null) {
                HitManager();
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                // if (power <= 1 && enemy.HP > 0) {
                //     Destroy(gameObject);
                // }
                var enDam = enemy.TakeDamage(true, player, power, 0, false, this, weaponType, false, false, false, this.transform.position);
                Debug.Log(enDam.beated);
                if (enDam.reflected)
                {
                    Reflected();
                } else
                {
                    if (power <= 1 || (power > 1 && !enDam.beated))
                    {
                        Destroy(gameObject);
                    }
                }
                // if (enemy.HP > 0 && level > 0) {
                //     Destroy(gameObject);
                // }
            } else if (col.GetComponent<ShellManager>() != null) {
                HitManager();
                col.GetComponent<ShellManager>().Shot(player);
                if (power <= 1) {
                    Destroy(gameObject);
                }
            }
        } else if (LayerMask.LayerToName(col.gameObject.layer) == "Boss"){
            HitManager();
            BossManager boss = col.GetComponent<BossManager>();
            if (power <= 1 && boss.HP > 0) {
                Destroy(gameObject);
            }
            boss.Damage(player, (int)(power * 2), false);
            if (boss.HP > 0 && level > 0) {
                Destroy(gameObject);
            }
        } else if (col.gameObject.GetComponent<MonitorManager>() != null) {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast") {
                HitManager();
                MonitorManager monitor = col.gameObject.GetComponent<MonitorManager>();
                monitor.player = player;
                monitor.destroyed = true;

                if (level <= 1) {
                    Destroy(gameObject);
                }
            }
        } else if (col.gameObject.GetComponent<ItemCapsule>() != null) {
            if (!col.gameObject.GetComponent<ItemCapsule>().destroyed) {
                HitManager();
                ItemCapsule capsule = col.gameObject.GetComponent<ItemCapsule>();
                capsule.player = player;
                capsule.destroyed = true;

                if (level <= 1) {
                    Destroy(gameObject);
                }
            }
        } else if (col.gameObject.GetComponent<QuestionBlockManager>() != null) {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast") {
                HitManager();
                col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);

                if (level <= 1) {
                    Destroy(gameObject);
                }
            }
        } else if (col.gameObject.GetComponent<Box>() != null) {
            if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast") {
                HitManager();
                StartCoroutine(col.gameObject.GetComponent<Box>().DestroyBox(player));

                if (level <= 1) {
                    Destroy(gameObject);
                }
            }
        }
        else if (col.GetComponent<EnemyIce>() != null)
        {
            HitManager();
            col.GetComponent<EnemyIce>().TakeDamage(WeaponTypes.None, false, power < 1);
            if (level <= 1) {
                Destroy(gameObject);
            }
        }
        // else if (LayerMask.LayerToName(col.gameObject.layer) == "Default")
        // {
        //     Destroy(gameObject);
        //     // if (level <= 1)
        //     // {
        //     //     Destroy(gameObject);
        //     // }
        //     // RaycastHit hit;
        //     // Physics.SphereCast(transform.position - transform.forward * 0.5f * this.transform.localScale.z, 0.9f * (1f / sizeRate), transform.forward, out hit, Mathf.Infinity, 1 << 0);
        //     // if (hit.distance > 0)
        //     // {
        //     //     Destroy(gameObject);
        //     // }
        // }
        
    }
    IEnumerator LifeTime(){
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void OnDestroy() {
        Instantiate(hitEffect, this.transform.position, this.transform.rotation);
    }

    void Reflected()
    {
        activeCollision = false;
        if (power > 1)
        {
            Destroy(gameObject);
        } else
        {
            Vector3 reflectedForward = this.transform.forward * -10f;
            reflectedForward.y = 0f;
            reflectedForward = reflectedForward.normalized;
            reflectedForward.y = 1f;
            this.transform.forward = reflectedForward.normalized;
            this.transform.Rotate(0f, UnityEngine.Random.Range(-10f, 10f), 0f);

            StopCoroutine("LifeTime");
            StartCoroutine("ReflectedLifeTime");
        }
    }
    IEnumerator ReflectedLifeTime(){
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void HitManager()
    {
        if (!getHit)
        {
            if (GameManager.hitCount < 999999999)
            {
                GameManager.hitCount++;
            }
            getHit = true;
        }
    }
}
