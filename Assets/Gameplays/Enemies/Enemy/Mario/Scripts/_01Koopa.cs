using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _01Koopa : EnemyMovements
{
    [Header("アニメーション")]
    public Animator nakedAnim;

    [Header("スキン")]
    public GameObject normalSkin;
    public GameObject nakedSkin;

    [Header("甲羅")]
    public GameObject shell;
    private GameObject targetShell;
    
    void FixedUpdate()
    {
        bool hasShell = this.GetComponent<EnemyManager>().hasShell;
        normalSkin.SetActive(hasShell);
        nakedSkin.SetActive(!hasShell);
    }

    public override void Movements()
    {
        /*
        状態：通常（クリボーと同じ動き）
        状態：甲羅なし（甲羅を焦点にする） 追いかける制限時間：10秒
        状態：追い詰める（プレイヤーを焦点にする）
        */
        Vector3 runVel = Vector3.zero;

        time -= Time.deltaTime;

        switch (action) {
            case 0:
            //待つ、向きをランダムにする
            runVel = Vector3.zero;
            if (time <= 0f) {
                if (LookingAnim(0.15f)) {
                    action = 1;
                    time = 2f;
                }
            } else {
                directionRate = 0f;
                startTime = Time.time;
                startDir = direction;
                endDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            }
            break;

            case 1:
            //歩く
            runVel = 3 * direction;
                
            if (time <= 0f) {
                action = 0;
                time = 1f;
            }
            break;

            case 2:
            //プレイヤーを目掛けて追いかける
            direction = targetPlayer.transform.position - this.transform.position;
            direction.y = 0;

            runVel = 10 * direction.normalized;
            break;

            case 5:
            runVel = Vector3.zero;
            if (time <= 0f) {
                if (LookingAnim(0.15f)) {
                    action = 6;
                    time = 2f;
                }
            } else {
                directionRate = 0f;
                startTime = Time.time;
                startDir = direction;
                endDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            }
            break;
            
            case 6:
            //歩く
            runVel = 3 * direction;
                
            if (time <= 0f) {
                action = 5;
                time = 1f;
            }
            break;
            
            case 7:
            //地震の甲羅を目掛けて追いかける
            direction = targetShell.transform.position - this.transform.position;
            direction.y = 0;

            runVel = 10 * direction.normalized;
            break;

            case 10:
            //甲羅から脱却
            runVel = 20 * direction.normalized;
            if (time <= 0f) {
                action = 11;
                time = 0.4f;
            }
            break;

            case 11:
            //甲羅から脱却
            if (time <= 0f) {
                action = 5;
                time = 1f;
            }
            break;

            default:
            runVel = Vector3.zero;
            break;
        }

        if (action < 2) {
            SetPlayer();
        }

        if (action < 5) {
            if (PlayerFound()) {
                if (action < 2) {
                    directionRate = 0f;
                    startTime = Time.time;
                    startDir = direction;
                    Vector3 d = targetPlayer.transform.position - this.transform.position;
                    d.y = 0;
                    endDir = d.normalized;
                    action = 3;
                }
                if (action == 3) {
                    if (LookingAnim(0.15f)) {
                        action = 4;
                        StartCoroutine("Attack");
                    }
                }
                time = 0.2f;
            } else {
                if (action > 1) action = 0;
            }
        }

        if (action >= 5 && action < 10) {
            if (ShellFound()) {
                if (action < 7) {
                    directionRate = 0f;
                    startTime = Time.time;
                    startDir = direction;
                    Vector3 d = targetShell.transform.position - this.transform.position;
                    d.y = 0;
                    endDir = d.normalized;
                    action = 8;
                }
                if (action == 8) {
                    if (LookingAnim(0.15f)) {
                        action = 9;
                        StartCoroutine("ShellFoundMovement");
                    }
                }
                time = 0.2f;
            } else {
                if (action > 1) action = 0;
            }
        }

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
    
    IEnumerator ShellFoundMovement() {
        manager.actionId = 1;
        yield return new WaitForSeconds(0.75f);

        if (manager.isActive() && manager.isNear()) {
            manager.actionId = 0;
            action = 7;
        }
    }

    public override bool DamageDefended() {
        targetShell = Instantiate(shell, this.transform.position, this.GetComponent<EnemyManager>().skin.transform.rotation);
        this.transform.position += this.GetComponent<EnemyManager>().skin.transform.forward * 2f;
        this.GetComponent<EnemyManager>().hasShell = false;

        action = 10;
        time = 0.4f;

        return true;
    }

    void OnCollisionStay(Collision col){
        if (col.gameObject == targetShell && !targetShell.GetComponent<ShellManager>().isMoving && action == 7){
            this.GetComponent<EnemyManager>().hasShell = true;
            action = 0;
            Destroy(targetShell);
        }
    }

    float GetShellDistance() {
        if (targetShell != null) {
            return Vector3.Distance(targetShell.transform.position, this.transform.position);
        } else {
            return Mathf.Infinity;
        }
    }

    bool SeenShell() {
        if (targetShell != null) {
            return Math.Abs(targetShell.transform.position.y - this.transform.position.y) < 10f;
        } else {
            return false;
        }
    }

    bool ShellFound() {
        if (targetShell != null) {
            return GetShellDistance() >= 2 && GetShellDistance() <= 30 && SeenShell();
        } else {
            return false;
        }
    }
}
