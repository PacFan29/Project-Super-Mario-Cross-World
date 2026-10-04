using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponTypes
{
    Fire, Ice, Thunder,
    Laser, Cutter, Water,
    Needle, Bomb, Stone, Dash,
    Boomerang, Wind, Insulator,
    None
}
public enum Availability
{
    Default,
    EarnFromBoss,
    PowerUp
}
public enum EnableCharacter
{
    MegaMan,
    Roll,
    ProtoMan,
    Bass
}
public enum ConsumptionType
{
    Constant,
    Time
}
public enum WeaponDamageType {
    Effective,
    Weak,
    NoEffective,
    Heal
}
[Serializable]
public class SpecialWeapon
{
    [Header("習得済か")]
    public bool Enable = false; //使用可能か
    [Header("情報")]
    public Sprite weaponIcon;
    public string Name = "武器名"; //武器名
    public Color WeaponColor = Color.red; //武器のカラー
    public WeaponTypes Type; //属性
    [Header("使用可能キャラ")]
    public EnableCharacter[] enableCharacters; //リストが空＝全員使える
    [Header("エネルギーデータ")]
    public float[] EnergyRemains = new float[4];
    [Header("消費エネルギー")]
    // public float Energy = 28; //残りエネルギー
    public float Consumption = 1; //消費エネルギー
    public ConsumptionType consumptionType;
    [Header("プレハブ")]
    public GameObject prefab;
    // [Header("単体攻撃 or 複数攻撃")]
    // public bool isSingle = false;
    [Header("リロード時間")]
    public float reloadTime = 0f;
    [Header("個数制限（0＝無制限）")]
    public byte limitCount = 0;

    public void RestoreAll()
    {
        for (int i = 0; i < EnergyRemains.Length; i++)
        {
            EnergyRemains[i] = 28;
        }
    }

    // public bool isConsumable() {
    //     return Energy >= Consumption;
    // }
    // public void Restore(byte amount) {
    //     Energy += amount;
    //     if (Energy > 28) Energy = 28;
    // }
    // public void Consume() {
    //     Energy -= Consumption;
    //     if (Energy < 0) Energy = 0;
    // }
}
[CreateAssetMenu(fileName = "SpecialWeaponData", menuName = "ScriptableObjects/Special Weapon Data")]
public class SpecialWeaponData : ScriptableObject
{
    /*
    特殊武器0：ワイドレーザー
    特殊武器1：スライスキャッチャー
    特殊武器2：ウォータージェット
    特殊武器3：スピア
    特殊武器4：スプレッドダイナマイト
    特殊武器5：ボルダーウォール
    特殊武器6：フレアバースト
    特殊武器7：フラッシュダッシュ 
    */
    public SpecialWeapon rush; //ラッシュコイル・ラッシュジェット（ロックマン）
    public SpecialWeapon rollRush; //ラッシュコイル・ラッシュジェット（ロールのみ）
    public SpecialWeapon vaccumArm; //バキュームアーム（ロールのみ）
    public SpecialWeapon blues; //ブルースコイル・ブルースジェット（ブルースのみ）
    public SpecialWeapon gospel; //ゴスペルブースト（フォルテのみ）
    public SpecialWeapon[] powerUps = new SpecialWeapon[5]; //マリオのパワーアップ
    public SpecialWeapon[] weapons = new SpecialWeapon[8];

#if UNITY_EDITOR
    [ContextMenu("エネルギーを全快にする")]
    void Initialize()
    {
        RestoreAllEnergies();
    }
#endif

    public void RestoreAllEnergies()
    {
        rush.RestoreAll();
        rollRush.RestoreAll();
        vaccumArm.RestoreAll();
        blues.RestoreAll();
        gospel.RestoreAll();
        foreach (SpecialWeapon powerUp in powerUps)
        {
            powerUp.RestoreAll();
        }
        foreach (SpecialWeapon weapon in weapons)
        {
            weapon.RestoreAll();
        }
    }
}
