using UnityEngine;

public class ParticleWeapon : ComboManager
{
    [Header("属性")]
    public WeaponTypes weaponType;
    [Header("攻撃力")]
    public int attackPower = 2;

    private void OnParticleCollision(GameObject col) {
        if (attackPower > 0) {
            // if (LayerMask.LayerToName(col.layer) == "Enemy")
            if (col.GetComponent<EnemyManager>() != null)
            {
                EnemyManager enemy = col.GetComponent<EnemyManager>();
                enemy.TakeDamage(true, player, attackPower, 1, false, this, weaponType, true);
            }
            // else if (LayerMask.LayerToName(col.layer) == "Boss")
            else if (col.GetComponent<BossManager>() != null)
            {
                Debug.Log("Boss");
                BossManager boss = col.GetComponent<BossManager>();
                boss.Damage(player, 2, false, weaponType, true);
            }
            if (col.GetComponent<MonitorManager>() != null)
            {
                if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
                {
                    MonitorManager monitor = col.GetComponent<MonitorManager>();
                    monitor.player = player;
                    monitor.destroyed = true;
                }
            }
            else if (col.GetComponent<ItemCapsule>() != null)
            {
                if (!col.GetComponent<ItemCapsule>().destroyed)
                {
                    ItemCapsule capsule = col.GetComponent<ItemCapsule>();
                    capsule.player = player;
                    capsule.destroyed = true;
                }
            }
            else if (col.GetComponent<QuestionBlockManager>() != null)
            {
                if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
                {
                    col.GetComponent<QuestionBlockManager>().BlockHit(player, false, false, weaponType);
                }
            }
            else if (col.GetComponent<HardBrickManager>() != null)
            {
                if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
                {
                    col.GetComponent<HardBrickManager>().BlockHit(player, weaponType);
                }
            }
            else if (col.GetComponent<Box>() != null)
            {
                if (LayerMask.LayerToName(col.layer) != "Ignore Raycast")
                {
                    StartCoroutine(col.GetComponent<Box>().DestroyBox(player));
                }
            }
            else if (col.GetComponent<EnemyIce>() != null)
            {
                col.GetComponent<EnemyIce>().TakeDamage(weaponType, true);
            }
        }
	}
}
