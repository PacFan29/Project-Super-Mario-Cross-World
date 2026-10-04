using System;
using UnityEngine;
using UnityEngine.UI;

public enum BadgeEffect
{
    FloatingHighJump,
    SpeedUp,
    AttackPlus,
    DefensePlus,
    AllPowerUp,
    AutoShield,
    AllShield,
    ScoreUp,
    CoinUp,
    SafetyBounce,
    Sensor,
    Weakness,
    SpikeBoots,
    AutoRestore,
    Invisible,
    DashSpeedUp,
    SuperBoost,
    SpringFeet,
    ReduceBoost,
    AutoRings,
    EnergyBalancer,
    EnergyBalancerNeo,
    AutoChargeChip,
    AwakenerChip,
    None
}
public enum BadgeTarget
{
    All,
    OnlyMarioSide,
    OnlyTeamMegaMan,
    OnlySonicSide
}
public enum BadgeClassification
{
    Action,
    PowerUp,
    Master
}

[CreateAssetMenu(fileName = "BadgeList", menuName = "ScriptableObjects/BadgeList")]
public class BadgeList : ScriptableObject
{
    [Serializable]
    public class Badge
    {
        [Header("アイコン")]
        public Sprite icon;
        [Header("名称")]
        public string name;
        [Header("効果")]
        public BadgeEffect effect;
        public float effectValue;
        [Header("分類")]
        public BadgeClassification classification;
        [Header("対象")]
        public BadgeTarget target;
        [Header("価格")]
        public uint price;
    }

    public Badge[] badges;
}
