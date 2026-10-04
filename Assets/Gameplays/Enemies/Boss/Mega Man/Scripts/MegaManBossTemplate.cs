using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaManBossTemplate : MegaManBossManager
{
    [Header("弾")]
    public GameObject bullet;

    public override void OverrideFixedUpdate() {
        if (totalHp > 0)
        {
            SetPlayer();

            if (targetPlayer != null)
            {
                Vector3 direction = targetPlayer.transform.position - transform.position;
                direction.y = 0;

                Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                this.transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 0.5f);
            }
        }
    }

    public override IEnumerator Attack()
    {
        while (true){
            Transform b = Instantiate(bullet, this.transform.position, this.transform.rotation).transform;
            if (targetPlayer != null)
            {
                b.LookAt(targetPlayer.transform.position);
            }
            b.position += b.forward * 0.375f;
            yield return new WaitForSeconds(0.5f);
        }
    }
    public override IEnumerator PhaseChange() {
        yield return null;
    }
    // public override IEnumerator DefeatedAnimation(){
    //     sound.PlayOneShot(defeatedSound);
    //     GameManager.timeScale = 0.5f;

    //     yield return new WaitForSeconds(0.875f);

    //     sound.PlayOneShot(explosionSound);
    //     GameManager.timeScale = 1f;

    //     yield return new WaitForSeconds(2.5f);
        
    //     OnDestroy();
        
    //     yield return new WaitForSeconds(5f);

    //     Destroy(gameObject);
    // }
}
