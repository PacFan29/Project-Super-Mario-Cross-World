using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioEnemyManager : EnemyManager
{
    public override IEnumerator DefeatedAnimation(bool stomped) {
        GetComponent<CapsuleCollider>().isTrigger = true;
        active = false;

        bool blow = false;
        
        if (!stomped || (stomped && blowAfterStomped))
        {
            if (stomped && blowAfterStomped)
            {
                defeatedCase = 1;
                velocity = Vector3.zero;
                yield return new WaitForSeconds(0.2f);
                stomped = false;
                blow = true;
            }
            defeatedCase = 2;

            Vector3 distance;
            if (player != null && !blow)
            {
                distance = this.transform.position - player.transform.position;
            }
            else
            {
                distance = -transform.forward;
            }
            distance.y = 0;

            skin.transform.forward = -distance.normalized;
            Vector3 hitVelocity = 10 * distance.normalized;
            hitVelocity.y = 45;
            velocity = hitVelocity;

            Grounded = false;
        }
        else
        {
            defeatedCase = 1;

            if (GetComponent<_07BulletBill>() != null)
            {
                velocity = Vector3.up * 10f;
            }
            else
            {
                velocity = Vector3.zero;
            }
        }

        float wait = stomped ? 0.4f : 0.6f;
        yield return new WaitForSeconds(wait);

        StartCoroutine("DestroyThisObject");
    }
}
