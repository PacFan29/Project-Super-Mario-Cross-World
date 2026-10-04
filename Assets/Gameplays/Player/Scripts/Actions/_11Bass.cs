using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _11Bass : MegaManActions
{
    [Header("バスター")]
    public GameObject buster;

    // private bool sliding;
    private int doubleJumpTrigger = 0;

    private bool shooting;    
    private bool homing;
    private GameObject hTargetObj;

    [Header("効果音")]
    public AudioClip busterSound;
    // public AudioClip dashSound;
    // public AudioClip fireSound;
    // public AudioClip iceSound;
    // public AudioClip thunderSound;


    void Update()
    {
        enableRush = false;

        //共通アクションの実行
        actions();

        /*
        フォルテでしかできない技
        ・バスター連射
        ・ダッシュ
        ・ダブルジャンプ
        */
        if (info != null)
        {
            SetWeaponList(3);
            //プレイヤーIDを11に設定
            info.setPlayerId(11);
        }

        // if (info.ButtonsDown["X"] && (weaponId == 0 || weaponId >= 9)){
        if (!info.grabbing && !info.giant)
        {
            if (info.ButtonsDown["X"] && weaponId == 0)
            {
                info.ComboReset();
                info.Crouching = true;
                StartCoroutine("ForteShoot");
                // } else if (info.ButtonsUp["X"] && (weaponId == 0 || weaponId >= 9)) {
            }
            else if (info.ButtonsUp["X"] && weaponId == 0)
            {
                info.Crouching = false;
                shooting = false;
                info.onlyTurningDirection = false;
                StopCoroutine("ForteShoot");
            }
        }

        if (doubleJumpTrigger == 1)
        {
            if (info.ButtonsDown["A"] && canJump)
            {
                info.constantSetUp();
                info.Crouching = false;
                inputA = true;
                doubleJumpTrigger = 2;
                info.Jump();
            }
        }
        else if (doubleJumpTrigger == 0)
        {
            if (!info.Grounded && !inputA)
            {
                doubleJumpTrigger = 1;
            }
        }
        if (info.Grounded && doubleJumpTrigger != 0)
        {
            doubleJumpTrigger = 0;
        }

        canJump = !info.GetCrouchButton("RB");

        if (!canJump && info.ButtonsDown["A"] && !sliding && info.Grounded && !info.giant)
        {
            //info.axisInput = false;
            // info.SoundPlay(dashSound);
            info.SoundPlay(slidingSound);
            StartCoroutine("Dash");
        }
        if (sliding)
        {
            info.dustTrailEffect();

            Vector3 XZvel = new Vector3(info.finalVelocity.x, 0, info.finalVelocity.z);
            info.skin.transform.forward = XZvel.normalized;

            if (Vector3.Angle(XZvel, info.input) >= 130)
            {
                Debug.Log("Canceled");
                info.ForwardSetUp(-XZvel.normalized, 5f);
                info.onlyTurningDirection = false;
                sliding = false;
                info.constantSetUp();
                info.ForwardSetUp(Vector3.zero, 0);
                info.axisInput = true;

                StopCoroutine("Dash");
            }
            if (!info.Grounded)
            {
                sliding = false;
            }
        }
        if (slidingTrigger && !sliding)
        {
            slidingTrigger = false;
            info.axisInput = true;
            info.Crouching = false;
            info.onlyTurningDirection = false;
            sliding = false;
            StopCoroutine("Dash");
            info.constantSetUp();
        }

        AutoAimEnemy();
    }

    IEnumerator ForteShoot()
    {
        info.currentXZmag = 0f;
        shooting = true;
        // while (info.Buttons["X"] && (weaponId == 0 || weaponId >= 9)) {
        while (info.Buttons["X"] && !info.grabbing && !info.giant && weaponId == 0)
        {
            info.onlyTurningDirection = true;
            AutoAimEnemy();
            info.SoundPlay(busterSound);

            GameObject solarbrit = Instantiate(buster, transform.position, info.skin.rotation);
            MegaBuster bust = solarbrit.GetComponent<MegaBuster>();
            if (homing)
            {
                solarbrit.transform.rotation = Quaternion.LookRotation(hTargetObj.transform.position - this.transform.position);
                bust.forteShoot = true;
            }
            bust.player = GetComponent<PlayerInfo>();
            bust.level = 0;
            bust.power = 0.5f;
            bust.powerUp = info.powerUpActive;

            yield return new WaitForSeconds(0.075f);
        }
        // info.Crouching = false;
        info.onlyTurningDirection = false;
        shooting = false;
    }

    IEnumerator Dash()
    {
        slidingTrigger = true;
        info.Crouching = false;
        info.onlyTurningDirection = true;
        sliding = true;
        info.ForwardSetUp(Vector3.zero, 50f);
        info.constantChange("frc", 0);
        info.constantChange("top", 50f);

        yield return new WaitForSeconds(0.4342f);

        info.onlyTurningDirection = false;
        sliding = false;
        info.constantSetUp();
        info.ForwardSetUp(Vector3.zero, 0);
        info.axisInput = true;
        slidingTrigger = false;
    }
    
    void AutoAimEnemy() {
        bool looking;
        GameObject[] homingTargets = GameObject.FindGameObjectsWithTag("HomingTarget");
        float enDistance;
        float MaxHDistance = 48;
        
        if (homing){
            if (hTargetObj == null) {
                looking = false;
            } else {
                looking = Vector3.Angle(info.skin.forward, (hTargetObj.transform.position - this.transform.position)) <= 90;
            }

            if (hTargetObj == null || !shooting || !looking){
                homing = false;
            } else {
                if (Vector3.Distance(hTargetObj.transform.position, transform.position) > MaxHDistance){
                    homing = false;
                }
                info.target.transform.position = hTargetObj.transform.position;
            }
        } else {
            float minDis = Mathf.Infinity;
            for (int i = 0; i < homingTargets.Length; i++){
                //ターゲットの対象が空でないかつ、正常に動いていれば認識できる。
                looking = Vector3.Angle(info.skin.forward, (homingTargets[i].transform.position - this.transform.position)) <= 90;
                
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
                    if (enDistance <= MaxHDistance && enDistance < minDis && shooting){
                        //最短距離でターゲットを認識する
                        minDis = enDistance;
                        hTargetObj = homingTargets[i];
                        homing = true;
                    }
                }
            }
        }
        info.target.SetActive(homing);
    }
}
