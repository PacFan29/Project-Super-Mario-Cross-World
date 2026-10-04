using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _06HammerBros : EnemyMovements
{
    public enum BrosType
    {
        Hammer,
        Fire,
        Ice,
        Thunder,
        Boomerang,
        Bubble,
        Wind,
        SuperBall
    }
    [Header("ブロスの属性")]
    public BrosType brosType;
    private List<Vector3> spotList = new List<Vector3>();
    private int hammerAmounts = 2;
    [Header("スーパーボールブロス限定")]
    public AudioClip[] landStompedSound;
    public AudioClip[] landDamageSound;
    public AudioClip[] landDefeatedSound;
    public GameObject landVanishEffect;

    void Awake()
    {
        spotList.Add(new Vector3(this.transform.position.x, 0f, this.transform.position.z));

        foreach (Transform spot in this.transform)
        {
            if (0 <= spot.gameObject.name.LastIndexOf("Spot"))
            {
                spotList.Add(new Vector3(spot.position.x, 0f, spot.position.z));
            }
        }

        EnemyManager m = GetComponent<EnemyManager>();

        if (brosType == BrosType.SuperBall)
        {
            landStompedSound.CopyTo(m.stompedSound, 0);
            landDamageSound.CopyTo(m.damageSound, 0);
            landDefeatedSound.CopyTo(m.defeatedSound, 0);

            m.vanishEffect = landVanishEffect;
        }

        //属性による弱点の変化
        switch (brosType)
        {
            case BrosType.Hammer:
            //ハンマー
            m.TurtleEnemy();
            break;
            
            case BrosType.Fire:
            //ファイア
            m.FireType();
            break;
            
            case BrosType.Ice:
            //アイス
            m.IceType();
            break;
            
            case BrosType.Thunder:
            //サンダー
            m.ThunderType();
            break;

            case BrosType.Boomerang:
            //ブーメラン
            m.BoomerangType();
            break;
            
            case BrosType.Bubble:
            //アワ
            m.BubbleType();
            break;
            
            case BrosType.Wind:
            //ウィンド
            m.WindType();
            break;
            
            case BrosType.SuperBall:
            //スーパーボール
            m.InsulatorType();
            break;
        }
    }
    public override void Movements()
    {
        manager.blowAfterStomped = true;

        switch (action)
        {
            case 0:
                if (manager.Grounded)
                {
                    time -= Time.deltaTime;
                    if (time <= 0f)
                    {
                        time = 0.1f;
                        action = 1;
                        manager.actionId = 1;
                        hammerAmounts = 2;
                    }
                }
                break;

            case 1:
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    time = 0.6f;
                    action = 2;
                    manager.velocity.y = 45f;
                    manager.Grounded = false;
                }
                break;

            case 2:
                if (manager.Grounded)
                {
                    time -= Time.deltaTime;
                    if (time <= 0.2f && hammerAmounts > 0)
                    {
                        manager.actionId = 2;
                    }
                    else
                    {
                        manager.actionId = 0;
                    }
                    if (time <= 0f)
                    {
                        manager.actionId = 0;
                        time = 1f;
                        if (hammerAmounts > 0)
                        {
                            hammerAmounts--;
                        }
                        else
                        {
                            action = 0;
                        }
                    }
                }
                break;
        }
    }
}
