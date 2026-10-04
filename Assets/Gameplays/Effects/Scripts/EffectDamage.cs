using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDamage : ComboManager
{
    public float lifeTime = 1f;
    private bool isTrigger = true;

    [Header("属性")]
    public WeaponTypes weaponType = WeaponTypes.None;
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine("LifeTime");
    }

    void OnTriggerEnter(Collider col) {
        if (isTrigger)
        {
            if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy")
            {
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                enemy.TakeDamage(true, player, 3, 1, false, this, weaponType, true);
            }
            else if (LayerMask.LayerToName(col.gameObject.layer) == "Boss")
            {
                BossManager boss = col.GetComponent<BossManager>();
                boss.Damage(player, 1, false, weaponType, true);
            }
            if (col.gameObject.GetComponent<MonitorManager>() != null)
            {
                if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
                {
                    MonitorManager monitor = col.gameObject.GetComponent<MonitorManager>();
                    monitor.player = player;
                    monitor.destroyed = true;
                }
            }
            else if (col.gameObject.GetComponent<ItemCapsule>() != null)
            {
                if (!col.gameObject.GetComponent<ItemCapsule>().destroyed)
                {
                    ItemCapsule capsule = col.gameObject.GetComponent<ItemCapsule>();
                    capsule.player = player;
                    capsule.destroyed = true;
                }
            }
            else if (col.gameObject.GetComponent<QuestionBlockManager>() != null)
            {
                if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
                {
                    col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);
                }
            }
            else if (col.gameObject.GetComponent<Box>() != null)
            {
                if (LayerMask.LayerToName(col.gameObject.layer) != "Ignore Raycast")
                {
                    StartCoroutine(col.gameObject.GetComponent<Box>().DestroyBox(player));
                }
            }
        }
    }

    IEnumerator LifeTime() {
        yield return new WaitForSeconds(lifeTime);

        isTrigger = false;
    }
}
