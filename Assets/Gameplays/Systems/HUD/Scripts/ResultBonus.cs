using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

public class ResultBonus : MonoBehaviour
{
    public enum BonusType
    {
        Time,
        DamageTaken,
        MaxCombo,
        ItemCollected,
        Rings,
        AverageSpeed,
        BossTime,
        NoDamage,
        NoMiss,
        WeaknessRatio,
        BossRings,
        MaxChain,
        HitRatio
    }
    [Serializable]
    public class BonusItem
    {
        public Sprite icon;
        public BonusType bonusType;
        public bool hideZeroValue;
        public string JPLocalization;
        public string ENLocalization;
    }

    [Header("ボーナス項目")]
    public BonusItem info;
    [Header("アイコン")]
    public GameObject timeIcon;
    public Image icon;
    public RectTransform min;
    public RectTransform sec;
    [Header("項目")]
    public Text jpTxt;
    public Text enTxt;
    [Header("タイム専用")]
    public int targetTime;
    public int maxTimeBonus;
    [Header("被ダメージ専用")]
    public int maxDamageCount;
    [Header("値")]
    public float valueAmm;
    private int formatIndex;
    /*
    0 = 非表示
    1 = タイム
    2 = 整数
    3 = パーセント
    4 = 小数第1位
    */
    public Text valueTxt;
    [Header("ボーナス")]
    public Text bonusTxt;
    private int bonus;
    public int bonusDisp;
    private float progress;
    [Header("リングアタック有無")]
    public bool isRingAttack;
    [Header("多言語対応")]
    public LocalizedStringTable JPHeaderTable;
    public LocalizedStringTable ENHeaderTable;
	UnityEngine.Localization.Tables.StringTable JPHeadPairs;
	UnityEngine.Localization.Tables.StringTable ENHeadPairs;
    [Header("アニメーションカーブ")]
    public AnimationCurve increaseAnim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //アイコン
        timeIcon.gameObject.SetActive(info.bonusType == BonusType.Time ||info.bonusType == BonusType.BossTime);
        icon.gameObject.SetActive(!(info.bonusType == BonusType.Time ||info.bonusType == BonusType.BossTime));
        icon.sprite = info.icon;
        
		//時計の針の傾き
		sec.transform.rotation = Quaternion.Euler(0, 0, ((GameManager.TotalTime / -1000f) * 6f) % 360f);
		min.transform.rotation = Quaternion.Euler(0, 0, ((GameManager.TotalTime / -60000f) * 6f) % 360f);

        //項目名
        try
        {
            JPHeadPairs = JPHeaderTable.GetTable();
            ENHeadPairs = ENHeaderTable.GetTable();

            jpTxt.text = JPHeadPairs[info.JPLocalization].Value;
            enTxt.text = ENHeadPairs[info.ENLocalization].Value;
        } catch
        {
            jpTxt.text = "";
            enTxt.text = "";
        }
        
        //値・ボーナス
        if (isRingAttack) {
            GetBonusRate();
        } else {
            GetBonus();
        }
        bonus = Mathf.Clamp(bonus, -999999, 999999);

        //値
        switch (formatIndex)
        {
            case 1:
                //タイム
                valueTxt.text = getTimeFormat((uint)valueAmm);
                break;
            case 2:
                //整数
                valueTxt.text = ((int)valueAmm).ToString("###,##0");
                break;
            case 3:
                //パーセント
                valueTxt.text = valueAmm.ToString("##0.00") + "%";
                break;
            case 4:
                //小数第1位
                valueTxt.text = valueAmm.ToString("###,##0.0");
                break;
            default:
                //非表示
                valueTxt.text = "";
                break;
        }

        //ボーナス
        if (progress < 1f)
        {
            progress += Time.deltaTime;
        }
        progress = Mathf.Clamp(progress, 0f, 1f);
        bonusDisp = (int)Math.Round((float)bonus * Mathf.Lerp(0f, 1f, increaseAnim.Evaluate(progress)));
        if (bonus >= 0)
        {
            bonusTxt.text = bonusDisp.ToString("+###,##0");
        } else
        {
            bonusTxt.text = bonusDisp.ToString("###,##0");
        }
        if (isRingAttack) {
            bonusTxt.text = bonusTxt.text + "%";
        }

        if (this.GetComponent<AudioSource>().isPlaying && (progress >= 1f || bonus == 0))
        {
            this.GetComponent<AudioSource>().Stop();
        }
    }

    public string getTimeFormat(uint totalTime){
        double minutes = totalTime / 60000;
        double seconds = (totalTime / 1000) % 60;
        double milli = (totalTime % 1000) / 10;
        return minutes.ToString("00") 
            + "\'" + seconds.ToString("00") 
            + "\"" + milli.ToString("00");
    }

    public int GetBonus() {
        formatIndex = 0;
        switch (info.bonusType)
        {
            case BonusType.Time:
                //タイム
                valueAmm = GameManager.TotalTime;
                formatIndex = 1;
                bonus = (int)Math.Round(Math.Max(0f, ((float)targetTime * 1000f - GameManager.TotalTime) / ((float)targetTime * 1000f)) * (float)maxTimeBonus);
                break;
            case BonusType.DamageTaken:
                //被ダメージ
                valueAmm = (float)GameManager.damageTaken;
                formatIndex = 2;
                bonus = Math.Max(0, maxDamageCount - GameManager.damageTaken) * 1000;
                break;
            case BonusType.MaxCombo:
                //最大コンボ
                valueAmm = (float)GameManager.maxCombo;
                formatIndex = 2;
                if (GameManager.maxCombo >= 12)
                {
                    bonus = (int)GameManager.maxCombo * 1000;
                } else
                {
                    bonus = (int)GameManager.maxCombo * ((int)GameManager.maxCombo - 1) * 100;
                }
                break;
            case BonusType.ItemCollected:
                //アイテム回収率
                valueAmm = GameManager.itemCollectedRate;
                formatIndex = 3;
                bonus = (int)(GameManager.itemCollectedRate * 200);
                break;
            case BonusType.Rings:
                //リング
                valueAmm = (float)GameManager.Coins;
                formatIndex = 2;
                bonus = (int)GameManager.Coins * 100;
                break;
            case BonusType.AverageSpeed:
                //平均スピード
                valueAmm = GameManager.averageSpeed;
                formatIndex = 4;
                bonus = (int)(GameManager.averageSpeed * 10);
                break;
            case BonusType.BossTime:
                //タイム（ボスステージ）
                valueAmm = GameManager.TotalTime;
                formatIndex = 1;
                if (GameManager.TotalTime > targetTime * 1000f)
                {
                    bonus = (int)Math.Round(40000f * Math.Max(0f, (30000f - (GameManager.TotalTime - (float)targetTime * 1000f)) / 30000f));
                } else
                {
                    bonus = 40000 + (int)Math.Round(10000f * ((float)targetTime * 1000f - GameManager.TotalTime) / ((float)targetTime * 1000f));
                }
                break;
            case BonusType.NoDamage:
                //ノーダメージ
                bonus = (GameManager.damageTaken == 0) ? 10000 : 0;
                break;
            case BonusType.NoMiss:
                //ノーミス
                bonus = (GameManager.damageTaken > 0 && GameManager.noMiss) ? 3000 : 0;
                break;
            case BonusType.WeaknessRatio:
                //弱点比率
                if (GameManager.bossCount == 0)
                {
                    GameManager.bossCount = 1;
                }
                valueAmm = (float)GameManager.weaknessRate / (float)GameManager.bossCount;
                bonus = (int)Math.Round(valueAmm * 50f);
                break;
            case BonusType.BossRings:
                //リング（ボスステージ）
                valueAmm = (float)GameManager.Coins;
                formatIndex = 2;
                bonus = (int)Math.Min(100, GameManager.Coins) * 100;
                break;
            case BonusType.MaxChain:
                //最大チェーン
                valueAmm = (float)GameManager.maxChain;
                formatIndex = 2;
                bonus = (int)GameManager.maxChain * 50;
                break;
            case BonusType.HitRatio:
                //命中率
                if (GameManager.shootCount == 0)
                {
                    valueAmm = 0f;
                } else
                {
                    valueAmm = (float)GameManager.hitCount * 100f / (float)GameManager.shootCount;
                }
                formatIndex = 3;
                bonus = (int)Math.Round(valueAmm * 200f);
                break;
        }
        return bonus;
    }
    public int GetBonusRate() {
        formatIndex = 0;
        switch (info.bonusType)
        {
            case BonusType.Time:
                //タイム
                valueAmm = GameManager.TotalTime;
                formatIndex = 1;
                bonus = Math.Max(0, Math.Min(100, (int)Math.Round(((GameManager.TotalTime - 60000f) / (Math.Min(600, targetTime) * 1000f - 60000f - (Math.Min(600, targetTime) * 250f))) * 100)));
                break;
            case BonusType.DamageTaken:
                //被ダメージ
                valueAmm = (float)GameManager.damageTaken;
                formatIndex = 2;
                bonus = (int)Math.Round(((float)Math.Max(0, maxDamageCount - GameManager.damageTaken) / (float)maxDamageCount) * 50);
                break;
            case BonusType.MaxCombo:
                //最大コンボ
                valueAmm = (float)GameManager.maxCombo;
                formatIndex = 2;
                if (GameManager.maxCombo >= 11)
                {
                    bonus = 50;
                } else
                {
                    bonus = Math.Max(0, ((int)GameManager.maxCombo - 1) * 5);
                }
                break;
            case BonusType.AverageSpeed:
                //平均スピード
                valueAmm = GameManager.averageSpeed;
                formatIndex = 4;
                bonus = Math.Min(50, (int)Math.Round((GameManager.averageSpeed / 1000f) * 50));
                break;
        }
        return bonus;
    }
}
