using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _09Roll : MegaManActions
{
    [Header("ほうきアタック")]
    public GameObject bloomEffect1;
    public GameObject bloomEffect2;
    private float bloomTime = 0f;
    private bool bloomCombo;
    void Update()
    {
        enabledToSwim = true;
        enableRush = true;

        //共通アクションの実行
        actions();
        /*
        ロールでしかできない技
        ・ほうきで攻撃
        */
        if (info != null)
        {
            SetWeaponList(1);
            //プレイヤーIDを9に設定
            info.setPlayerId(9);
        }

        canJump = !info.GetCrouchButton("RB");
        
        if (!canJump && info.ButtonsDown["A"] && !sliding && info.Grounded && !info.giant)
        {
            //info.axisInput = false;
            info.SoundPlay(slidingSound);
            StartCoroutine("Sliding");
        }
        if (sliding)
        {
            info.dustTrailEffect();

            Vector3 XZvel = new Vector3(info.finalVelocity.x, 0, info.finalVelocity.z);
            info.skin.transform.forward = XZvel.normalized;

            if (Vector3.Angle(XZvel, info.input) >= 130)
            {
                if (info.isNarrow())
                {
                    info.ForwardSetUp(-XZvel.normalized, info.TopSpeed * 2);
                }
                else
                {
                    info.ForwardSetUp(-XZvel.normalized, 5f);
                    sliding = false;
                    info.Crouching = false;
                    info.constantSetUp();
                    info.ForwardSetUp(Vector3.zero, 0);
                    info.axisInput = true;

                    StopCoroutine("Sliding");
                }
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
            StopCoroutine("Sliding");
            info.constantSetUp();
            if (info.input != Vector3.zero) {
                info.ForwardSetUp(Vector3.zero, info.TopSpeed, true);
            } else
            {
                info.ForwardSetUp(Vector3.zero, 0f, true);
            }
        }

        if (bloomTime > 0f)
        {
            bloomTime -= Time.deltaTime;
        }

        if (info.ButtonsDown["X"] && weaponId == 0 && bloomTime <= 0.25f && !info.giant)
        {
            voices.Attack();

            if (bloomCombo && bloomTime > 0f)
            {
                bloomCombo = false;
                StartCoroutine(BloomAttack(true));
            } else
            {
                bloomCombo = true;
                StartCoroutine(BloomAttack(false));
            }
        }
    }

    IEnumerator Sliding() {
        slidingTrigger = true;
        sliding = true;
        info.Crouching = true;
        info.ForwardSetUp(Vector3.zero, 40f);
        info.constantChange("frc", 0);
        info.constantChange("top", 40f);

        yield return new WaitForSeconds(0.4342f);
        
        while (info.isNarrow()) {
            yield return new WaitForSeconds(0f);
        }

        sliding = false;
        info.Crouching = false;
        info.constantSetUp();
        if (info.finalVelocity.y <= 0f)
        {
            // info.ForwardSetUp(Vector3.zero, 0);
        }
        info.axisInput = true;
        slidingTrigger = false;
    }

    IEnumerator BloomAttack(bool combo)
    {
        if (combo)
        {
            bloomEffect2.SetActive(true);
            bloomTime = 0.65f;
            yield return new WaitForSeconds(0.35f);
            bloomEffect2.SetActive(false);
        } else
        {
            bloomEffect1.SetActive(true);
            bloomTime = 0.5f;
            yield return new WaitForSeconds(0.2f);
            bloomEffect1.SetActive(false);
        }
    }
}
