using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaManEnemyManager : EnemyManager
{
    [Header("アイテムドロップ")]
    public GameObject dropItem;

    public override IEnumerator DefeatedAnimation(bool stomped)
    {
        GetComponent<CapsuleCollider>().isTrigger = true;
        active = false;
        
        if (GameManager.courseMode != CourseMode.RingAttack) {
            coins = 0;
            Instantiate(dropItem, this.transform.position, Quaternion.identity);
        }

        StartCoroutine("DestroyThisObject");

        yield return null;
    }
}
