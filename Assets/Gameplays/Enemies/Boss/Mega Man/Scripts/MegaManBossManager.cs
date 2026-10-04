using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaManBossManager : BossManager
{
    [Header("ダメージ間隔")]
    public float intervalDamageTime = 1f;
    public bool doType = false;
    [Header("追加効果音")]
    public AudioClip explosionSound;
    [Header("追加エフェクト")]
    public GameObject explosion1;
    public GameObject explosion2;
    public GameObject thiunThiun;
    
    public override void SetHP()
    {
        // switch (options.difficulty)
        // {
        //     case Options.Difficulty.Easy:
        //         totalHp = (int)Math.Ceiling((float)HitsPerHP / 2f) * 2 * HP;
        //         break;

        //     case Options.Difficulty.Normal:
        //         totalHp = HitsPerHP * 2 * HP;
        //         break;

        //     case Options.Difficulty.Hard:
        //         if (HP < 4)
        //         {
        //             HP++;
        //         }
        //         else
        //         {
        //             HP = (int)((float)HP * 1.25f);

        //             for (int i = 0; i < PhasePerHP.Length; i++)
        //             {
        //                 PhasePerHP[i] = (int)((float)PhasePerHP[i] * 1.25f) + 1;
        //             }
        //         }
        //         totalHp = (int)Math.Round((float)HitsPerHP * 1.2f) * 2 * HP;
        //         break;

        // }
        totalHp = HitsPerHP * 2 * HP;

        maxHp = (int)totalHp;
        if (doType) {
            singleDamage = 8;
        } else {
            singleDamage = 6;
        }

        latestHP = HP;
    }

    public override void Damage(PlayerInfo playerObj, int damage, bool stomped, WeaponTypes weapon = WeaponTypes.None, bool canPenetrate = false)
    {
        player = playerObj;

        int wDamage = GetWeaponDamage(damage, weapon);
        if (damage == 1)
        {
            damage = 2;
        }

        if (!invincible)
        {
            if (HP > 0 && damageTime <= 0f)
            {
                // damageTime = 1f;
                damageTime = intervalDamageTime;
                SoundPlay(fireDamageSound);
                VoicePlay(damageVoices);

                int attackRate = playerObj.attackUp ? 2 : 1;

                float totalDamage = (weapon != WeaponTypes.None ? wDamage : damage) * attackRate;
                totalHp -= totalDamage;
                if (weaknessExploited)
                {
                    GameManager.weaknessRate += totalDamage * 100f / (float)maxHp;
                }
                HP = (int)(totalHp / 2f);
                // HP = (int)Math.Ceiling((float)totalHp / singleDamage);

                if (phase > 0)
                {
                    if (HP <= PhasePerHP[phase - 1])
                    {
                        phase--;
                        StartCoroutine("PhaseChange");
                    }
                }

                if (totalHp <= 0)
                {
                    if (stageClear) GameManager.timeIncrease = false;
                    if (enableBossBonus)
                    {
                        if (weaknessExploited)
                        {
                            player.scorePopUp((int)Score, true, this.transform.position, 0);
                        } else
                        {
                            player.scorePopUp((int)Score, false, this.transform.position, 0);
                        }
                    }

                    if (stopMusic)
                    {
                        MusicManager.musicIndex = 2;
                    }
                    StopCoroutine("Attack");
                    StartCoroutine("DefeatedAnimation");
                }
            }
        }

        if (showBossHP)
        {
            GameManager.bossInfo = this;
        }
    }
    
    public override int GetWeaponDamage(int damage, WeaponTypes weapon) {
        int wDamage = 4;
        if (doType) {wDamage = 6;}
        weaknessExploited = false;
        foreach (WeaknessDamage weakness in weaknesses) {
            if (weakness.weakness == weapon && weapon != WeaponTypes.None) {
                switch (weakness.damageType) {
                    case WeaponDamageType.Effective:
                        //効果は抜群だ！
                        if (doType) {
                            wDamage = 12;
                        } else {
                            wDamage = 8;
                        }
                        weaknessExploited = true;
                    break;

                    case WeaponDamageType.Weak:
                    //効果はいま一つのようだ・・・
                    wDamage = 2;
                    break;

                    case WeaponDamageType.NoEffective:
                    //効果がないみたいだ・・・
                    wDamage = 0;
                    break;

                    case WeaponDamageType.Heal:
                        if (doType) {
                            wDamage = -12;
                        } else {
                            wDamage = -8;
                        }
                    break;
                }
                break;
            }
        }
        if (wDamage == 2 && wDamage < damage) {
            wDamage = damage;
        }
        return wDamage;
    }
    
    public override IEnumerator DefeatedAnimation()
    {
        soundSource.PlayOneShot(defeatedSound);
        GameManager.timeScale = 0.5f;
        
        yield return new WaitForSeconds(0.875f);

        GameObject explode = Instantiate(explosion1, this.transform.position, Quaternion.identity);

        soundSource.PlayOneShot(explosionSound);
        GameManager.timeScale = 1f;

        yield return new WaitForSeconds(2.5f);

        animator.gameObject.SetActive(false);

        Destroy(explode);
        
        Instantiate(explosion2, this.transform.position, Quaternion.identity);
        OnDestroy();
        if (thiunThiun != null)
        {
            Instantiate(thiunThiun, this.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }

    [ContextMenu("クラシック")]
    public void ClassicType() {
        HP = 28;
        HitsPerHP = 1;
        intervalDamageTime = 0.3f;
        PhasePerHP = new int[0];
        doType = false;
    }
    [ContextMenu("４～７")]
    public void Classic4to7Type() {
        HP = 28;
        HitsPerHP = 1;
        intervalDamageTime = 1f;
        PhasePerHP = new int[0];
        doType = false;
    }
    [ContextMenu("８")]
    public void Classic8Type() {
        HP = 40;
        HitsPerHP = 1;
        intervalDamageTime = 1f;
        PhasePerHP = new int[0];
        doType = false;
    }
    [ContextMenu("１１")]
    public void ModernType() {
        HP = 28;
        HitsPerHP = 1;
        intervalDamageTime = 1f;
        PhasePerHP = new int[1]{14};
        doType = false;
    }
    [ContextMenu("デュアルオーバーライド")]
    public void DualOverrideType() {
        HP = 60;
        HitsPerHP = 1;
        intervalDamageTime = 0.2f;
        PhasePerHP = new int[1]{30};
        doType = true;
    }
}
