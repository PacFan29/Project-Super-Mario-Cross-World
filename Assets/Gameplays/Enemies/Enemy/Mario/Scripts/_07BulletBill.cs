using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _07BulletBill : EnemyMovements
{
    public GameObject fire;
    void FixedUpdate()
    {
        manager.stompedButNeverStretched = true;

        if (manager.isActive())
        {
            manager.velocity = this.transform.forward * 16f;
        }
        fire.SetActive(manager.isActive());
    }

    void OnCollisionEnter (Collision col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default") {
            manager.StartCoroutine("DestroyThisObject");
        } else if (col.gameObject.GetComponent<_07BulletBill>() != null) {
            col.gameObject.GetComponent<EnemyManager>().StartCoroutine("DestroyThisObject");
            manager.StartCoroutine("DestroyThisObject");
        }
    }
}
