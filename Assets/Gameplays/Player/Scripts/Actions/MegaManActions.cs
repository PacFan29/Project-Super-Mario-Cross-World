using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MegaManActions : CommonActions
{
    public SpecialWeaponData weapons;
    public List<SpecialWeapon> weaponList = new List<SpecialWeapon>();
    public List<float> weaponRemainsList = new List<float>();
    public List<int> weaponPowerUpList = new List<int>();
    public byte weaponId = 0;

    private int rushIndex = 0;
    // protected PlayerInfo info;

    protected bool sliding;
    protected bool slidingTrigger;
    protected int powerUpBeginIndex;
    protected bool enableRush;

    protected bool inputA;
    protected bool canJump = true;
    [Header("エフェクト")]
    public GameObject deathEffect;
    [Header("スライディング効果音")]
    public AudioClip slidingSound;
    [Header("武器切り替え効果音")]
    public AudioClip weaponChangeSound;
    [Header("ラッシュ")]
    public AudioClip[] summonCoilVoices;
    public AudioClip[] summonJetVoices;

    public GameObject rush;
    private GameObject currentRush;

    // protected PlayerVoiceManager voices;

    private float beforeAxis = 0f;

    // Start is called before the first frame update
    void Start()
    {
        info = GetComponent<PlayerInfo>();
        info.Stompable = false;
        initialStompable = false;
        info.playerType = 2;
        info.desacelerar = true;
        enabledToSwim = false;

        voices = GetComponent<PlayerVoiceManager>();
    }

    // Update is called once per frame
    public void actions()
    {
        enabledDash = true;
        enabledGroundPound = true;
        enabledFireball = false;
        gravityControl = true;
        dashButton = 2;
        commonActions();

        if (GameManager.players.Count > info.playerNumber)
        {
            //プレイヤークラスの値を設定
            if (info.primary)
            {
                if (weaponList.Count <= 0 || weaponRemainsList.Count <= 0)
                {
                    // GameManager.players[info.playerNumber].setStatus(info.playerId, info.maxHP, info.HP, info.lives, info.localScore);
                    GameManager.players[info.playerNumber].setStatus(
                        info.playerId, (ushort)(info.maxHP * (GameManager.extra ? 0.6f : 1f)), info.HP,
                        (uint)info.localScore);
                }
                else
                {
                    // GameManager.players[info.playerNumber].setStatus(info.playerId, info.maxHP, info.HP, info.lives, info.localScore, true, weapons.weapons[weaponId - 1].Energy);
                    GameManager.players[info.playerNumber].setStatus(
                        info.playerId, (ushort)(info.maxHP * (GameManager.extra ? 0.6f : 1f)), info.HP,
                        (uint)info.localScore,
                        true,
                        weaponList[weaponId].weaponIcon, weaponList[weaponId].WeaponColor,
                        weaponRemainsList[weaponId], weaponList[weaponId].Consumption);
                }
            }
        }

        //パワーアップ差し替え
        if (weaponList.Count > 0)
        {
            if (info.powerUpActive > 0)
            {
                info.SoundPlay(weaponChangeSound);
                weaponRemainsList[weaponPowerUpList[info.powerUpActive - 1]] = 28;
                weaponId = (byte)weaponPowerUpList[info.powerUpActive - 1];
                info.powerUpActive = 0;
            }
        }

        float view_axis = 0f;
        bool judge = false;
        if (info.primary)
        {
            if (info.playerNumber > 0)
            {
                (view_axis, judge) = AxisOnce("Alt Horizontal " + (info.playerNumber + 1));
            } else
            {
                (view_axis, judge) = AxisOnce("Alt Horizontal");
            }
        }

        if (view_axis > 0 && judge)
        {
            SwitchWeaponR();
        }
        else if (view_axis < 0 && judge)
        {
            SwitchWeaponL();
        }
        
        //しゃがむ
        if (info.GetCrouchButton("RB") && info.Grounded && !info.Crouching && !info.rolling) {
            info.Crouching = true;
        } else if (!info.GetCrouchButton("RB") && info.Grounded && info.Crouching && !info.rolling && !info.isNarrow() && !sliding) {
            info.Crouching = false;
        }

        if (info.ButtonsDown["A"] && canJump && info.Grounded)
        {
            info.constantSetUp();
            // if (this.GetComponent<_08MegaMan>() != null && info.XZmag > 30f) {
            //     info.ForwardSetUp(info.skin.forward, 0f);
            // }
            sliding = false;
            info.Crouching = false;
            inputA = true;
            info.Jump();
        }
        if (info.Crouching)
        {
            this.GetComponent<CapsuleCollider>().height = 2.4f;
        }
        else
        {
            // this.GetComponent<CapsuleCollider>().height = 3.5f;
            this.GetComponent<CapsuleCollider>().height = info.InitialColliderHeight;
        }

        if (inputA && !info.gravityLock)
        {
            //ジャンプの勢い
            //Aを押し続けるとより高く跳べる。
            if (info.finalVelocity.y > 0 && info.ButtonsUp["A"])
            {
                //途中でAを離したとき
                // info.YvelSetUp(0);
                inputA = false;
            }
            if (info.finalVelocity.y <= -0.1)
            {
                //高く跳びきったとき
                inputA = false;
            }
        }

        if (enableRush)
        {
            if (info.ButtonsDown["Y"])
            {
                //ラッシュコイル
                SummonRushCoil();
            }
            if (info.ButtonsDown["B"])
            {
                //ラッシュジェット
                SummonRushJet();
            }
        }
    }

    public IEnumerator DamageAnimation(Vector3 direction)
    {
        info.canInput = false;
        float time = 0.3f;
        Vector3 damageVelocity = direction.normalized * 30f;
        damageVelocity.y = 0;

        while (time > 0)
        {
            info.VelocitySetUp(damageVelocity);
            info.skin.transform.forward = -damageVelocity.normalized;
            time -= Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        info.VelocitySetUp(Vector3.zero);
        yield return new WaitForSeconds(0.7f);

        info.canInput = true;
        info.tookDamage = false;
        yield return null;
    }
    public IEnumerator DeathAnimation()
    {
        info.activePhysics = false;
        GameManager.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.7f);

        GameManager.timeScale = 1;
        info.StopAllSounds();
        info.skinGroup.SetActive(false);
        GetComponent<CapsuleCollider>().isTrigger = true;
        GameObject d_effect = Instantiate(deathEffect, transform.position, Quaternion.identity);

        // if (GameManager.is3D()) {
        //     //3Dの場合、X軸に90°回転する。
        //     d_effect.transform.rotation = Quaternion.Euler(90, 0, 0);
        // } else if (GameManager.dimension == DimensionType.ZWay2D) {
        //     d_effect.transform.rotation = Quaternion.Euler(0, 90, 0);
        // }
        if (GameManager.dimension == DimensionType.ZWay2D)
        {
            d_effect.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        voices.Death();
    }

    (float a, bool b) AxisOnce(string axis)
    {
        bool judge = false;
        float view_axis = Input.GetAxis(axis);

        //RTが押されたら視点を変える：前フレームの入力値が0の場合のみ実施
        judge = Math.Abs(view_axis) > 0 && beforeAxis == 0.0f;
        beforeAxis = view_axis;

        return (view_axis, judge);
    }

    public void SwitchWeaponL()
    {
        info.SoundPlay(weaponChangeSound);
        if (weaponId == 0)
        {
            weaponId = (byte)(weaponList.Count - 1);
        }
        else
        {
            weaponId--;
        }
        while (weaponRemainsList[weaponId] < 0)
        {
            if (weaponId == 0)
            {
                weaponId = (byte)(weaponList.Count - 1);
            }
            else
            {
                weaponId--;
            }
        }
    }
    public void SwitchWeaponR()
    {
        info.SoundPlay(weaponChangeSound);
        weaponId++;
        if (weaponId >= weaponList.Count)
        {
            weaponId = 0;
        }
        while (weaponRemainsList[weaponId] < 0)
        {
            weaponId++;
            if (weaponId >= weaponList.Count)
            {
                weaponId = 0;
            }
        }
    }
    //特殊武器
    public void SetWeaponList(int characterNo)
    {
        if (weaponList.Count > 0)
        {
            return;
        }

        //ラッシュコイル・ラッシュジェット
        if (
            Array.IndexOf(weapons.rush.enableCharacters, (EnableCharacter)characterNo) >= 0
            )
        {
            weaponList.Add(weapons.rush);
            weaponRemainsList.Add(weapons.rush.EnergyRemains[info.playerNumber]);
            rushIndex = weaponList.Count - 1;
        }
        if (
            Array.IndexOf(weapons.rollRush.enableCharacters, (EnableCharacter)characterNo) >= 0
            )
        {
            weaponList.Add(weapons.rollRush);
            weaponRemainsList.Add(weapons.rollRush.EnergyRemains[info.playerNumber]);
            rushIndex = weaponList.Count - 1;
        }
        //バキュームアーム（ロールのみ）
        if (
            Array.IndexOf(weapons.vaccumArm.enableCharacters, (EnableCharacter)characterNo) >= 0
            )
        {
            weaponList.Add(weapons.vaccumArm);
            weaponRemainsList.Add(weapons.vaccumArm.EnergyRemains[info.playerNumber]);
        }
        //ブルースコイル・ブルースジェット
        if (
            Array.IndexOf(weapons.blues.enableCharacters, (EnableCharacter)characterNo) >= 0
            )
        {
            weaponList.Add(weapons.blues);
            weaponRemainsList.Add(weapons.blues.EnergyRemains[info.playerNumber]);
            rushIndex = weaponList.Count - 1;
        }
        //ゴスペルブースト
        if (
            Array.IndexOf(weapons.gospel.enableCharacters, (EnableCharacter)characterNo) >= 0
            )
        {
            weaponList.Add(weapons.gospel);
            weaponRemainsList.Add(weapons.gospel.EnergyRemains[info.playerNumber]);
            rushIndex = weaponList.Count - 1;
        }
        //パワーアップ
        powerUpBeginIndex = weaponList.Count;
        for (int w = 0; w < weapons.powerUps.Length; w++)
        {
            weaponPowerUpList.Add(weaponList.Count);
            weaponList.Add(weapons.powerUps[w]);
            weaponRemainsList.Add(weapons.powerUps[w].EnergyRemains[info.playerNumber]);
        }
        //特殊武器
        foreach (SpecialWeapon weapon in weapons.weapons)
        {
            if (weapon.Enable)
            {
                weaponList.Add(weapon);
                weaponRemainsList.Add(weapon.EnergyRemains[info.playerNumber]);
            }
        }
    }
    public bool isWeaponConsumable()
    {
        if (weaponList[weaponId].consumptionType == ConsumptionType.Time)
        {
            return weaponRemainsList[weaponId] >= 0;
        } else
        {
            return weaponRemainsList[weaponId] >= weaponList[weaponId].Consumption;
        }
    }
    public void WeaponRestore(byte amount)
    {
        if (weaponRemainsList[weaponId] >= 0)
        {
            weaponRemainsList[weaponId] += amount;
            if (weaponRemainsList[weaponId] > 28) weaponRemainsList[weaponId] = 28;
        }
    }
    public void RestoreAllWeapons(byte amount)
    {
        for (int i = 0; i < weaponRemainsList.Count; i++)
        {
            if (weaponRemainsList[i] >= 0)
            {
                weaponRemainsList[i] += amount;
                if (weaponRemainsList[i] > 28) weaponRemainsList[i] = 28;
            }
        }
    }
    public void ConsumeWeaponEnergy()
    {
        if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0 || info.awakenerChip)
        {
            return;
        }
        if (weaponList[weaponId].consumptionType == ConsumptionType.Time)
        {
            weaponRemainsList[weaponId] -= weaponList[weaponId].Consumption * Time.deltaTime;
        } else
        {
            weaponRemainsList[weaponId] -= weaponList[weaponId].Consumption;
        }
        if (weaponRemainsList[weaponId] < 0) weaponRemainsList[weaponId] = 0;
    }
    public void ConsumeRushCoilEnergy()
    {
        if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0 || info.awakenerChip)
        {
            return;
        }
        weaponRemainsList[rushIndex] -= 2;
        if (weaponRemainsList[rushIndex] < 0) weaponRemainsList[rushIndex] = 0;
    }
    public bool ConsumeRushJetEnergy()
    {
        if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0 || info.awakenerChip)
        {
            return false;
        }
        // weaponRemainsList[rushIndex] -= 2 * Time.deltaTime;
        weaponRemainsList[rushIndex] -= Time.deltaTime;
        if (weaponRemainsList[rushIndex] < 0) weaponRemainsList[rushIndex] = 0;
        
        return weaponRemainsList[rushIndex] <= 0f;
    }

    public void Reset()
    {
        info.activePhysics = true;
        //actionId = 0;
        GetComponent<CapsuleCollider>().isTrigger = false;
        info.skinGroup.SetActive(true);
    }

    public bool WPRestore(byte value, Vector3 itemPos, bool isMini)
    {
        if (weaponRemainsList[weaponId] >= 28)
        {
            if (
                (info.data.stockItems[1].amounts >= 1 && !isMini) ||
                (info.data.stockItems[1].amounts >= 1.75f && isMini)
            )
            {
                info.scorePopUp(1000, false, itemPos);
                return true;
            }
            else
            {
                StartCoroutine(info.ItemStock(1, 1, isMini ? 2 : 1));
            }
        }
        else
        {
            WeaponRestore(value);
        }
        return false;
    }

    public bool[] PowerUpStatus()
    {
        bool[] powerUps = new bool[weapons.powerUps.Length];

        for (int i = 0; i < weapons.powerUps.Length; i++)
        {
            powerUps[i] = weaponRemainsList[powerUpBeginIndex + i] > -1;
        }

        return powerUps;
    }
    public bool LeastOnePowerUp()
    {
        bool[] powerUps = PowerUpStatus();
        bool leastOne = false;

        foreach (bool p in powerUps)
        {
            leastOne = leastOne || p;
        }

        return leastOne;
    }

    public void SummonRushCoil()
    {
        weaponId = (byte)rushIndex;
        if (weaponRemainsList[rushIndex] < 2)
        {
            if (currentRush != null)
            {
                //キャンセル
                currentRush.GetComponent<Rush>().Cancel();
            }
            return;
        }
        if (currentRush != null)
        {
            if (currentRush.GetComponent<Rush>().mode == Rush.RushMode.Coil)
            {
                //キャンセル
                currentRush.GetComponent<Rush>().Cancel();
            } else
            {
                //コイルに切り替え
                voices.VoicePlay(summonCoilVoices, true);
                // currentRush.GetComponent<Rush>().mode = Rush.RushMode.Coil;
                currentRush.GetComponent<Rush>().ModeChange(Rush.RushMode.Coil, this.transform.position + info.skin.forward * 4f - Vector3.up * -0.7f);
            }
        } else
        {
            voices.VoicePlay(summonCoilVoices, true);
            currentRush = Instantiate(rush, this.transform.position + info.skin.forward * 4f - Vector3.up * -0.7f, info.skin.rotation);
            Rush r = currentRush.GetComponent<Rush>();
            r.owner = this;
            r.mode = Rush.RushMode.Coil;
        }
    }
    public void SummonRushJet()
    {
        weaponId = (byte)rushIndex;
        if (weaponRemainsList[rushIndex] <= 0f)
        {
            if (currentRush != null)
            {
                //キャンセル
                currentRush.GetComponent<Rush>().Cancel();
            }
            return;
        }
        if (currentRush != null)
        {
            if (currentRush.GetComponent<Rush>().mode == Rush.RushMode.Jet)
            {
                //キャンセル
                currentRush.GetComponent<Rush>().Cancel();
            } else
            {
                //ジェットに切り替え
                voices.VoicePlay(summonJetVoices, true);
                // currentRush.GetComponent<Rush>().mode = Rush.RushMode.Jet;
                currentRush.GetComponent<Rush>().ModeChange(Rush.RushMode.Jet, this.transform.position + info.skin.forward * 4f - Vector3.up * -0.7f);
            }
        } else
        {
            voices.VoicePlay(summonJetVoices, true);
            currentRush = Instantiate(rush, this.transform.position + info.skin.forward * 4f - Vector3.up * -0.7f, info.skin.rotation);
            Rush r = currentRush.GetComponent<Rush>();
            r.owner = this;
            r.mode = Rush.RushMode.Jet;
        }
    }

    public bool RanOutOfRushEnergy()
    {
        return weaponRemainsList[rushIndex] <= 0f;
    }
}
