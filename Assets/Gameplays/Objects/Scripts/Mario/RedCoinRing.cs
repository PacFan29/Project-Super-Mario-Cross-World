using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCoinRing : RingManager
{
    public enum BonusPowerUp
    {
        Random,
        FireFlower,
        IceFlower,
        ThunderShroom,
        BoomerangFlower,
        BubbleFlower,
        WindFlower,
        SuperBallFlower
    }
    public enum BonusShield
    {
        Random,
        Shield,
        FireShield,
        BubbleShield,
        ThunderShield,
        PoisonShield,
        ForceShield,
        ArmageddonShield
    }
    private float time = 10f;

    [Header("赤コイン")]
    public GameObject redCoinsParent;
    [Header("ボーナス")]
    public BonusPowerUp bonusPowerUp;
    public BonusShield bonusShield;
    [Header("ボーナス（プレハブ）")]
    public GameObject powerUp;
    public GameObject shield;

    protected int redCoinCount = 0;
    /*
    ボーナスの定義

    ・プレイヤーのＨＰが25%以下の場合
    　→ＨＰ回復アイテム（例：スーパーキノコ）

    ・プレイヤーのＨＰが25%を超えている場合
    　→パワーアップ5種のランダム

    ・パワーアップした状態の場合
    　→バリア9種のランダム

    ・パワーアップした状態かつバリアが張っている状態の場合
    　→1UP
    */

    // Update is called once per frame
    void Update()
    {
        if (gotIt)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                Destroy(gameObject);
            }
        }
        redCoinsParent.SetActive(gotIt);
    }

    public int GotRedCoin(PlayerInfo pl)
    {
        redCoinCount++;
        player = pl;
        return redCoinCount;
    }

    public void Bonus()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        bool playerPinch = (float)player.HP / (float)player.maxHP <= 0.5f && player.gameObject.GetComponent<_16MSonic>() == null;
        bool playerHasPowerUp = player.powerUpActive > 0 || player.gameObject.GetComponent<_16MSonic>() != null;
        bool playerHasShield = player.shieldActive > 0;

        if (player.gameObject.GetComponent<MegaManActions>() != null)
        {
            playerHasPowerUp = player.gameObject.GetComponent<MegaManActions>().LeastOnePowerUp();
        }

        ItemManager item;
        ItemCapsule capsule;

        ItemManager MakeItem()
        {
            ItemManager i = Instantiate(powerUp, player.gameObject.transform.position + Vector3.up * 4f, Quaternion.identity).GetComponent<ItemManager>();
            i.Split();
            return i;
        }
        ItemCapsule MakeCapsule()
        {
            ItemCapsule c = Instantiate(shield, player.gameObject.transform.position + Vector3.up * 5f, Quaternion.identity).GetComponent<ItemCapsule>();
            c.Split();
            return c;
        }

        if (playerPinch)
        {
            //プレイヤーのＨＰが25%以下の場合
            //ＨＰ回復アイテム（例：スーパーキノコ）
            item = MakeItem();
            item.itemType = ItemType.RestoreHP;
        }
        else
        {
            if (playerHasPowerUp && playerHasShield)
            {
                //パワーアップした状態かつバリアが張っている状態の場合
                //1UP
                item = MakeItem();
                item.itemType = ItemType.Extend;
            }
            else if (playerHasPowerUp && !playerHasShield)
            {
                //パワーアップした状態の場合
                //バリア9種のうちいずれか
                capsule = MakeCapsule();
                switch (bonusShield)
                {
                    case BonusShield.Shield:
                        capsule.monitorType = MonitorType.Shield;
                        break;

                    case BonusShield.FireShield:
                        capsule.monitorType = MonitorType.FireShield;
                        break;

                    case BonusShield.BubbleShield:
                        capsule.monitorType = MonitorType.BubbleShield;
                        break;

                    case BonusShield.ThunderShield:
                        capsule.monitorType = MonitorType.ThunderShield;
                        break;

                    case BonusShield.PoisonShield:
                        capsule.monitorType = MonitorType.PoisonShield;
                        break;

                    case BonusShield.ForceShield:
                        capsule.monitorType = MonitorType.ForceShield;
                        break;

                    case BonusShield.ArmageddonShield:
                        capsule.monitorType = MonitorType.ArmageddonShield;
                        break;

                    // case BonusShield.WhirlwindShield:
                    //     capsule.monitorType = MonitorType.WhirlwindShield;
                    //     break;

                    // case BonusShield.ElementalShield:
                    //     capsule.monitorType = MonitorType.ElementalShield;
                    //     break;

                    default:
                        capsule.monitorType = (MonitorType)(11 + UnityEngine.Random.Range(0, Enum.GetValues(typeof(BonusShield)).Length - 1));
                        break;
                }
            }
            else
            {
                //プレイヤーのＨＰが25%を超えている場合
                //パワーアップ5種のうちいずれか
                item = MakeItem();
                switch (bonusPowerUp)
                {
                    case BonusPowerUp.FireFlower:
                        item.itemType = ItemType.FireFlower;
                        break;

                    case BonusPowerUp.IceFlower:
                        item.itemType = ItemType.IceFlower;
                        break;

                    case BonusPowerUp.ThunderShroom:
                        item.itemType = ItemType.ThunderShroom;
                        break;

                    case BonusPowerUp.BoomerangFlower:
                        item.itemType = ItemType.BoomerangFlower;
                        break;

                    case BonusPowerUp.BubbleFlower:
                        item.itemType = ItemType.BubbleFlower;
                        break;

                    case BonusPowerUp.WindFlower:
                        item.itemType = ItemType.WindFlower;
                        break;

                    case BonusPowerUp.SuperBallFlower:
                        item.itemType = ItemType.SuperBallFlower;
                        break;

                    default:
                        item.itemType = (ItemType)(3 + UnityEngine.Random.Range(0, Enum.GetValues(typeof(BonusPowerUp)).Length - 1));
                        break;
                }
            }
        }

        Destroy(gameObject);
    }
}
