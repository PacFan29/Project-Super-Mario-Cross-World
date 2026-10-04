using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonitorType {
    TenRings,
    RandomRings,
    SpeedUp,
    Invincible,
    FireFlower,
    IceFlower,
    ThunderShroom,
    BoomerangFlower,
    BubbleFlower,
    WindFlower,
    SuperBallFlower,
    Shield,
    FireShield,
    BubbleShield,
    ThunderShield,
    PoisonShield,
    ForceShield,
    ArmageddonShield,
    // WhirlwindShield,
    // ElementalShield,
    BlueRing,
    Eggman,
    Extend
}

[ExecuteInEditMode]
public class MonitorManager : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    public Options options;
    [Header("モニターのタイプ")]
    public MonitorType monitorType;
    [HideInInspector] public PlayerInfo player;

    [HideInInspector] public bool destroyed;
    private bool isDestroyed;
    private GameObject playerCol;

    [Header("エフェクト等")]
    public GameObject monitorSkin;
    public GameObject explosion;
    public GameObject yashichiEffect;
    [Header("効果音")]
    public AudioClip[] coinSounds = new AudioClip[5];
    [Header("画面")]
    public MeshRenderer screen;
    public Material[] screenMaterials;
    
    [Header("アイコン")]
    public SpriteRenderer icon;
    public Sprite[] iconSprites;
    public Sprite[] randomRingSprites;
    private int randomRingIndex;

    private bool activateYashichi = false;

    // Update is called once per frame
    void Update()
    {
        if (monitorType == MonitorType.Extend && options.difficulty == Options.Difficulty.Easy)
        {
            monitorType = MonitorType.RandomRings;
        }
        if (destroyed && !isDestroyed)
        {
            this.GetComponent<AudioSource>().Play();

            if (player != null)
            {
                player.scoreIncrease(1000, this.transform.position);
            }
            isDestroyed = true;

            monitorSkin.SetActive(false);
            explosion.SetActive(true);

            this.GetComponent<BoxCollider>().isTrigger = true;
            this.tag = "Untagged";
            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            StartCoroutine("MonitorEffect");
        }

        //画面
        Material[] mat = new Material[1];
        mat[0] = screenMaterials[(int)monitorType];
        activateYashichi = !options.lives;
        if (activateYashichi && monitorType == MonitorType.Extend)
        {
            mat[0] = screenMaterials[21];
        }

        screen.materials = mat;

        //アイコン
        if (activateYashichi && monitorType == MonitorType.Extend)
        {
            icon.sprite = iconSprites[21];
        } else if (monitorType == MonitorType.RandomRings)
        {
            icon.sprite = randomRingSprites[randomRingIndex];
        } else
        {
            icon.sprite = iconSprites[(int)monitorType];
        }
    }

    IEnumerator MonitorEffect()
    {
        // int soundIndex = 0;
        // float soundVolume = 1f;
        // AudioSource playerGotit;
        icon.gameObject.SetActive(true);
        randomRingIndex = UnityEngine.Random.Range(0, randomRingSprites.Length);

        yield return new WaitForSeconds(0.5f);

        switch (monitorType)
        {
            case MonitorType.TenRings:
                GotRings(10);
                // GameManager.Coins += 10;

                // switch (data.character)
                // {
                //     case Character.PacMan:
                //         //クッキー
                //         player.GotCookie(10);
                //         soundIndex = 1;
                //         soundVolume = 0.35f;
                //         break;

                //     case Character.RockMan:
                //         //ネジ
                //         soundIndex = 2;
                //         soundVolume = 1f;
                //         break;

                //     case Character.Sonic:
                //         //リング
                //         player.GotRing(10);
                //         soundIndex = 3;
                //         soundVolume = 0.7f;
                //         break;

                //     case Character.Other:
                //         //クリスタル
                //         soundIndex = 4;
                //         soundVolume = 0.7f;
                //         break;
                // }
                // player.scoreIncrease(500);
                // if (player.gameObject.GetComponent<_16MSonic>() != null) {
                //     player.gameObject.GetComponent<_16MSonic>().BoostIncrease(10, false);
                // }

                // playerGotit = player.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
                // playerGotit.clip = coinSounds[soundIndex];
                // playerGotit.volume = soundVolume;
                // playerGotit.Play();
                break;

            case MonitorType.RandomRings:
                byte[] randomAmounts = { 1, 5, 10, 20, 30, 40, 50 };
                // int am = randomAmounts[UnityEngine.Random.Range(0, randomAmounts.Length)];
                int am = randomAmounts[randomRingIndex];
                GotRings(am);
                // GameManager.Coins += (uint)am;

                // switch (data.character)
                // {
                //     case Character.PacMan:
                //         //クッキー
                //         player.GotCookie(10);
                //         soundIndex = 1;
                //         soundVolume = 0.35f;
                //         break;

                //     case Character.RockMan:
                //         //ネジ
                //         soundIndex = 2;
                //         soundVolume = 1f;
                //         break;

                //     case Character.Sonic:
                //         //リング
                //         player.GotRing(10);
                //         soundIndex = 3;
                //         soundVolume = 0.7f;
                //         break;

                //     case Character.Other:
                //         //クリスタル
                //         soundIndex = 4;
                //         soundVolume = 0.7f;
                //         break;
                // }
                // player.scoreIncrease(am * 50);
                // if (player.gameObject.GetComponent<_16MSonic>() != null) {
                //     player.gameObject.GetComponent<_16MSonic>().BoostIncrease(am, false);
                // }

                // playerGotit = player.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
                // playerGotit.clip = coinSounds[soundIndex];
                // playerGotit.volume = soundVolume;
                // playerGotit.Play();
                break;

            case MonitorType.SpeedUp:
                player.SpeedUp();
                break;

            case MonitorType.Invincible:
                player.Invincible(20, 2);
                break;
            
            case MonitorType.FireFlower:
                if (player.PowerUpAnim(1))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.IceFlower:
                if (player.PowerUpAnim(2))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.ThunderShroom:
                if (player.PowerUpAnim(3))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.BoomerangFlower:
                if (player.PowerUpAnim(4))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.BubbleFlower:
                if (player.PowerUpAnim(5))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.WindFlower:
                if (player.PowerUpAnim(6))
                {
                    GotRings(5);
                }
                break;
            
            case MonitorType.SuperBallFlower:
                if (player.PowerUpAnim(7))
                {
                    GotRings(5);
                }
                break;

            case MonitorType.Shield:
                player.GotShield(1);
                break;

            case MonitorType.FireShield:
                player.GotShield(2);
                break;

            case MonitorType.BubbleShield:
                player.GotShield(3);
                break;

            case MonitorType.ThunderShield:
                player.GotShield(4);
                break;

            case MonitorType.PoisonShield:
                player.GotShield(5);
                break;

            case MonitorType.ForceShield:
                player.GotShield(6);
                break;

            case MonitorType.ArmageddonShield:
                player.GotShield(7);
                break;

            // case MonitorType.WhirlwindShield:
            //     player.GotShield(8);
            //     break;

            // case MonitorType.ElementalShield:
            //     player.GotShield(9);
            //     break;

            case MonitorType.BlueRing:
                if (player.gameObject.GetComponent<_16MSonic>() != null)
                {
                    player.gameObject.GetComponent<_16MSonic>().ActiveBlueRing();
                } else
                {
                    if (GameManager.courseMode == CourseMode.RingAttack) {
                        player.gameObject.GetComponent<CommonActions>().ActiveBlueRing();
                    } else {
                        GotRings(10);
                    }
                }
                break;

            case MonitorType.Eggman:
                player.TakeDamage(8, this.transform.position);
                break;

            case MonitorType.Extend:
                if (activateYashichi)
                {
                    if (player.GotYashichi(player.gameObject.transform.position))
                    {
                        GotRings(10);
                    }
                    Instantiate(yashichiEffect, player.gameObject.transform.position, Quaternion.identity);
                } else
                {
                    player.OneUp();
                }
                break;
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void GotRings(int amounts)
    {
        int soundIndex = 0;
        float soundVolume = 1f;
        AudioSource playerGotit;

        GameManager.Coins += (uint)amounts;

        switch (data.character)
        {
            case Character.PacMan:
                //クッキー
                player.GotCookie((byte)amounts);
                soundIndex = 1;
                soundVolume = 0.35f;
                break;

            case Character.RockMan:
                //ネジ
                soundIndex = 2;
                soundVolume = 1f;
                break;

            case Character.Sonic:
                //リング
                player.GotRing((byte)amounts);
                soundIndex = 3;
                soundVolume = 0.7f;
                break;

            case Character.Other:
                //クリスタル
                soundIndex = 4;
                soundVolume = 0.7f;
                break;
        }
        player.scoreIncrease(amounts * 50, this.transform.position);
        if (player.gameObject.GetComponent<_16MSonic>() != null) {
            player.gameObject.GetComponent<_16MSonic>().BoostIncrease(amounts, false);
        }

        playerGotit = player.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        playerGotit.clip = coinSounds[soundIndex];
        playerGotit.volume = soundVolume;
        playerGotit.Play();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && !destroyed)
        {
            player = col.gameObject.GetComponent<PlayerInfo>();
            playerCol = col.gameObject;
            if (!player.primary) {
                return;
            }

            if (
                (player.rolling && player.Grounded) ||
                player.attacking
            )
            {
                if (player.attacking) player.Stomp();
                destroyed = true;
            }
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<InstaShield>() != null && !destroyed)
        {
            player = col.GetComponent<InstaShield>().player;
            if (!player.primary) {
                return;
            }

            player.Stomp();
            destroyed = true;
        }
    }
}
