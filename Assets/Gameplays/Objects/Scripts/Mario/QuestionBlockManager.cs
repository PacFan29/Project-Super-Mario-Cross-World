using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    None,
    Coin,
    TenCoins,
    RestoreHP,
    RestoreWP,
    Invincible,
    FireFlower,
    IceFlower,
    ThunderShroom,
    BoomerangShroom,
    BubbleFlower,
    New,
    SuperBallFlower,
    PoisonShroom,
    Extend,
    MegaMushroom,
    PowerPellet,
    Shield,
    SpeedUp,
    FireShield,
    BubbleShield,
    ThunderShield,
    PoisonShield,
    ForceShield,
    ArmageddonShield
    // WhirlwindShield,
    // ElementalShield,
}

public class QuestionBlockManager : MonoBehaviour
{
    [Header("ブロックの状態")]
    public bool isEmpty;
    public bool isHidden;
    public bool isLong;
    [HideInInspector] public bool breaked = false;
    public Transform skinGroup;
    public GameObject fullBlock;
    public GameObject emptyBlock;
    public GameObject brickBreak;
    [Header("ブロックのタイプ")]
    public BlockType blockType;

    private int leftCoins = 10;
    [Header("コイン・アイテム")]
    public GameObject coin;
    public GameObject tencoin;
    public GameObject item;
    public GameObject shield;
    [Header("効果音")]
    public AudioClip upperPunched;
    public AudioClip headHit;
    public AudioClip breakedSound;
    private AudioSource source;

    private float whackedTime = 0f;
    private int isDownward = 1;
    private float time = 1f;

    protected PlayerInfo currentPlayer;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }
    void Awake()
    {
        if (isHidden)
        {
            this.GetComponent<BoxCollider>().isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        fullBlock.SetActive(!isEmpty && !isHidden && !breaked);
        emptyBlock.SetActive(isEmpty && !breaked);

        // brickBreak.SetActive(breaked);

        if (whackedTime > 0f)
        {
            whackedTime -= Time.deltaTime;
            if (whackedTime < 0f) whackedTime = 0f;
        }
        float movement = (float)Math.Sin(((double)whackedTime / 0.2) * 180 * Math.PI / 180.0);
        skinGroup.localPosition = Vector3.up * 0.4f * movement * isDownward;
        skinGroup.localScale = Vector3.one * (1f + 0.2f * movement);

        if (leftCoins < 10 && leftCoins > 1 && blockType == BlockType.TenCoins)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                leftCoins--;
                time = 1f;
            }
        }
    }

    public void BlockHit(PlayerInfo player, bool downWard, bool destroyed = false, WeaponTypes weapon = WeaponTypes.None)
    {
        currentPlayer = player;

        if (weapon == WeaponTypes.Laser)
        {
            destroyed = true;
            StartCoroutine(DestroyBlock(player));
        }

        RaycastHit enemyHit;
        int enemyLayer = LayerMask.GetMask("Enemy");
        if (Physics.BoxCast(this.transform.position, Vector3.one * 2.75f / 2f, Vector3.up, out enemyHit, this.transform.rotation, 2.75f, enemyLayer) && !downWard)
        {
            if (enemyHit.collider.gameObject.GetComponent<EnemyManager>() != null) {
                EnemyManager enemy = enemyHit.collider.gameObject.GetComponent<EnemyManager>();
                enemy.TakeDamage(true, player, 3, 0, false, null);
            }
        }

        if (!isEmpty)
        {
            GameManager.ChainTimeRecovery();

            if (!destroyed)
            {
                if (blockType != BlockType.None && this.GetComponent<BrickManager>() != null || this.GetComponent<BrickManager>() == null)
                {
                    source.PlayOneShot(upperPunched);
                }
                time = 1f;
                whackedTime = 0.2f;

                isDownward = downWard ? -1 : 1;
            }

            this.GetComponent<BoxCollider>().isTrigger = false;
            isHidden = false;
            GameObject itemObj;
            GameObject itemCap;
            switch (blockType)
            {
                case BlockType.Coin:
                    SplitCoin(player, Vector3.zero, downWard);
                    break;

                case BlockType.TenCoins:
                    if (destroyed)
                    {
                        Split10Coins(player, Vector3.zero, downWard);
                    }
                    else
                    {
                        SplitCoin(player, Vector3.zero, downWard);
                        leftCoins--;
                    }
                    break;

                case BlockType.None:
                    if (
                        weapon == WeaponTypes.None ||
                        weapon == WeaponTypes.Thunder ||
                        weapon == WeaponTypes.Laser ||
                        weapon == WeaponTypes.Bomb ||
                        weapon == WeaponTypes.Dash ||
                        weapon == WeaponTypes.Boomerang
                    )
                    {
                        DestroyBrick(player);
                    }
                    else
                    {
                        return;
                    }
                    break;

                // case BlockType.MegaMushroom:
                //     itemObj = Instantiate(item, this.transform.position + (Vector3.up * 1.375f), this.transform.rotation);
                //     itemObj.GetComponent<ItemManager>().itemType = ItemType.MegaMushroom;
                //     itemObj.transform.Rotate(0, 180, 0);

                //     itemObj.GetComponent<ItemManager>().Split();
                //     break;

                // case BlockType.PowerPellet:
                //     itemObj = Instantiate(item, this.transform.position + (Vector3.up * 1.375f), this.transform.rotation);
                //     itemObj.GetComponent<ItemManager>().itemType = ItemType.PowerPellet;
                //     itemObj.transform.Rotate(0, 180, 0);

                //     itemObj.GetComponent<ItemManager>().Split();
                //     break;

                default:
                    int index = (int)blockType - 3;
                    if (index >= Enum.GetNames(typeof(ItemType)).Length)
                    {
                        itemCap = Instantiate(shield, player.gameObject.transform.position + Vector3.up * 5f * isDownward, Quaternion.identity);
                        switch (blockType)
                        {
                            case BlockType.Shield:
                                itemCap.GetComponent<ItemCapsule>().monitorType = MonitorType.Shield;
                                break;

                            case BlockType.SpeedUp:
                                itemCap.GetComponent<ItemCapsule>().monitorType = MonitorType.SpeedUp;
                                break;

                            default:
                                itemCap.GetComponent<ItemCapsule>().monitorType = (MonitorType)(index - 4);
                                break;
                        }
                        itemCap.transform.Rotate(0, 180, 0);

                        itemCap.GetComponent<ItemCapsule>().Split(downWard);
                    }
                    else
                    {
                        itemObj = Instantiate(item, this.transform.position + (Vector3.up * 1.375f * isDownward), this.transform.rotation);
                        itemObj.GetComponent<ItemManager>().itemType = (ItemType)index;
                        itemObj.transform.Rotate(0, 180, 0);

                        itemObj.GetComponent<ItemManager>().Split(downWard);
                    }
                    break;
            }

            if (blockType != BlockType.TenCoins || (blockType == BlockType.TenCoins && leftCoins <= 0))
            {
                isEmpty = true;
            }

            if (isLong)
            {
                for (int i = 0; i <= 1; i++)
                {
                    Vector3 offset = this.transform.right * 2.75f * (i * 2 - 1);
                    if (blockType == BlockType.TenCoins && destroyed)
                    {
                        Split10Coins(player, offset, downWard);
                    }
                    else
                    {
                        SplitCoin(player, offset, downWard);
                    }
                }
            }
        }
    }

    void SplitCoin(PlayerInfo player, Vector3 offset, bool downWard)
    {
        CoinManager coinObj = Instantiate(coin, this.transform.position + offset, Quaternion.identity).GetComponent<CoinManager>();
        coinObj.targetPlayer = player.gameObject;

        StartCoroutine(coinObj.SplitCoin(downWard));
    }
    void Split10Coins(PlayerInfo player, Vector3 offset, bool downWard)
    {
        CoinManager coinObj = Instantiate(tencoin, this.transform.position + offset, Quaternion.identity).GetComponent<CoinManager>();
        coinObj.targetPlayer = player.gameObject;

        StartCoroutine(coinObj.SplitCoin(downWard));
    }

    public virtual void DestroyBrick(PlayerInfo player)
    {
        ;
    }

    public IEnumerator DestroyBlock(PlayerInfo player)
    {
        source.PlayOneShot(breakedSound);
        player.scoreIncrease(10, this.transform.position);

        this.GetComponent<BoxCollider>().isTrigger = true;
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        this.gameObject.tag = "Other";

        breaked = true;
        Instantiate(brickBreak, this.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.1f);

        Destroy(gameObject);
    }

    public IEnumerator RepeatDownward(PlayerInfo player)
    {
        BlockHit(player, true);

        while (player.groundAttack)
        {
            yield return new WaitForSeconds(0.25f);

            if (player.groundAttack && !breaked)
            {
                BlockHit(player, true);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<PlayerInfo>() != null)
        {
            if (col.gameObject.GetComponent<PlayerInfo>().giant)
            {
                BlockHit(col.gameObject.GetComponent<PlayerInfo>(), false, true);
                StartCoroutine(DestroyBlock(col.gameObject.GetComponent<PlayerInfo>()));
            }
        }
    }
}
