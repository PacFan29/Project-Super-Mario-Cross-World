using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTemplate : BossManager
{
    [Header("攻撃オブジェクト")]
    public GameObject bullet;
    [Header("エフェクト")]
    public GameObject vanishEffect;

    public Material[] materials = new Material[2];

    private int randomVal;
    
    public override IEnumerator Attack()
    {
        while (true){
            // if (phase == 0) {
            //     if (bullet != null) {
            //         GameObject blt = Instantiate(bullet, transform.position, Quaternion.identity);
            //     }
            //     yield return new WaitForSeconds(0.1f);
            // } else {
            //     for (int i = 0; i < 30; i++) {
            //         if (bullet != null) {
            //             GameObject blt = Instantiate(bullet, transform.position, Quaternion.identity);
            //         }
            //         yield return new WaitForSeconds(0.2f);
            //     }
            //     yield return new WaitForSeconds(4f);
            // }

            // defended = true;
            // for (int i = 0; i < 2; i++) {
            //     this.transform.forward = FacePlayer();
            //     velocity = this.transform.forward * 20f;
            //     yield return new WaitForSeconds(1f);
            // }
            // defended = false;
            // for (int i = 0; i < 2; i++) {
            //     this.transform.forward = FacePlayer();
            //     velocity = this.transform.forward * 20f;
            //     yield return new WaitForSeconds(1f);
            // }

            defended = true;
            for (int i = 0; i < 2; i++) {
                actionId = 0;
                yield return new WaitForSeconds(1.5f);
                actionId = 1;
                yield return new WaitForSeconds(0.25f);

                randomVal = UnityEngine.Random.Range(0, 3);

                if (phase == 0)
                {
                    switch (randomVal)
                    {
                        case 0:
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(0f, 15f * j, 0);
                        }
                        yield return new WaitForSeconds(0.3f);
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(15f * j, 0, 0);
                        }
                        yield return new WaitForSeconds(0.3f);
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(15f * j, -90f, -90f);
                        }
                        yield return new WaitForSeconds(0.6f);
                        for (int j = 0; j < 36; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(0f, 10f * j, 0);
                        }
                        for (int j = 0; j < 36; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(10f * j, 0, 0);
                        }
                        for (int j = 0; j < 36; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(15f * j, -90f, -90f);
                        }
                        yield return new WaitForSeconds(1f);
                        break;

                        case 1:
                        for (int j = 0; j < 30; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.LookAt(targetPlayer.transform.position);
                            yield return new WaitForSeconds(0.05f);
                        }
                        break;

                        case 2:
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(0f, 15f * j, 0);
                            yield return new WaitForSeconds(0.02f);
                        }
                        yield return new WaitForSeconds(0.3f);
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(0f, -15f * j, 0);
                            yield return new WaitForSeconds(0.02f);
                        }
                        yield return new WaitForSeconds(0.3f);
                        for (int j = 0; j < 24; j++)
                        {
                            Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                            blt.rotation = Quaternion.Euler(0f, 15f * j, 0);
                            yield return new WaitForSeconds(0.02f);
                        }
                        break;
                    }
                } else
                {
                    for (int j = 0; j < 24; j++)
                    {
                        Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                        blt.rotation = Quaternion.Euler(0f, 15f * j, 0);
                        yield return new WaitForSeconds(0.02f);
                    }
                }
            }
            defended = false;
            for (int i = 0; i < 2; i++) {
                actionId = 0;
                yield return new WaitForSeconds(1.5f);
                actionId = 1;
                yield return new WaitForSeconds(0.25f);
                for (int j = 0; j < 24; j++)
                {
                    Transform blt = Instantiate(bullet, transform.position, Quaternion.identity).transform;
                    blt.rotation = Quaternion.Euler(0f, 15f * j, 0);
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
    }

    public override void OverrideUpdate() {
        if (defended) {
            this.GetComponent<MeshRenderer>().materials = new Material[1]{materials[0]};
        } else {
            this.GetComponent<MeshRenderer>().materials = new Material[1]{materials[1]};
        }
    }

    public override void OverrideFixedUpdate()
    {
        if (actionId == 0)
        {
            this.transform.forward = FacePlayer();
            velocity += FacePlayer(false);
            if (velocity.magnitude > 17.5f)
            {
                velocity = Vector3.ClampMagnitude(velocity, 17.5f);
            }
        } else
        {
            velocity *= 0.8f;
        }
    }

    public override IEnumerator DamageAnimation(){
        defended = true;
        yield return null;
    }
    public override IEnumerator PhaseChange() {
        StopCoroutine("Attack");
        StartCoroutine("Attack");

        if (multiPhase)
        {
            SetHP();
        }
        yield return null;
    }
    public override IEnumerator DefeatedAnimation(){
        for (int i = 0; i < 30; i++){
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
            Instantiate(vanishEffect, transform.position + pos, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(gameObject);
    }
}
