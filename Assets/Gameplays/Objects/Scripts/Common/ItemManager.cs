using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    RestoreHP,
    RestoreWP,
    Invincible,
    FireFlower,
    IceFlower,
    ThunderShroom,
    BoomerangFlower,
    BubbleFlower,
    WindFlower,
    SuperBallFlower,
    PoisonShroom,
    Extend,
    MegaMushroom,
    PowerPellet
}

[ExecuteInEditMode]
public class ItemManager : DimensionManager
{
    public GameData data;
    public Options options;
    private Vector3 velocity;
    private bool Grounded = false;
    private Vector3 objNormalVector = Vector3.zero;
    private Vector3 afterReflectVelo = Vector3.zero;

    public ItemType itemType;
    public GameObject[] restoreHP = new GameObject[4];
    public GameObject restoreWP;
    public GameObject invincible;
    // public GameObject[] invincible = new GameObject[2];
    public GameObject[] powerUps = new GameObject[7];
    public GameObject poison;
    public GameObject[] extend = new GameObject[4];
    public GameObject megaMushroom;
    public GameObject powerPellet;
    public GameObject yashichi;
    private int skinIndex;
    [HideInInspector] public bool floating = false;

    [HideInInspector] public bool stock = false;

    private bool activateYashichi = false;

    [Header("エフェクト")]
    public GameObject healEffect;
    public GameObject effect;
    public GameObject pacGotEffect;
    public GameObject megaGotEffect;
    public GameObject amiboHpEffect;
    public GameObject amiboWpEffect;
    public GameObject eCanEffect;
    public GameObject starEffect;
    public GameObject vanishEffect;
    public GameObject yashichiEffect;

    [Header("コイン")]
    public GameObject coinSpawner;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        afterReflectVelo = velocity;

        Skin();
    }

    public void Split(bool downWard = false)
    {
        this.GetComponent<AudioSource>().Play();

        velocity = this.transform.forward * 3;
        velocity.y = downWard ? 0f : 8f;
        afterReflectVelo = velocity;
    }

    // Update is called once per frame
    // void Update()
    void Skin()
    {
        List<GameObject> meshes = new List<GameObject>();
        meshes = addItem(meshes, restoreHP);
        meshes = addItem(meshes, new GameObject[1] { restoreWP });
        meshes = addItem(meshes, new GameObject[1] { invincible });
        meshes = addItem(meshes, powerUps);
        meshes = addItem(meshes, new GameObject[1] { poison });
        meshes = addItem(meshes, extend);
        meshes = addItem(meshes, new GameObject[1] { megaMushroom });
        meshes = addItem(meshes, new GameObject[1] { powerPellet });
        meshes = addItem(meshes, new GameObject[1] { yashichi });

        foreach (var mesh in meshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(false);
            }
        }

        int index = 0;
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (allPlayers.Length < 2)
        {
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i].GetComponent<PlayerInfo>() != null)
                {
                    if (allPlayers[i].GetComponent<PlayerInfo>().playerNumber == 0)
                    {
                        index = allPlayers[i].GetComponent<PlayerInfo>().playerType;
                    }
                }
            }
        }

        skinIndex = 0;
        switch (itemType)
        {
            case ItemType.RestoreHP:
                //スーパーキノコ（HP 8回復）
                restoreHP[index].SetActive(true);
                skinIndex = index;
                break;

            case ItemType.RestoreWP:
                //アミーボ 武器（武器エネルギー 8回復）
                restoreWP.SetActive(true);
                skinIndex = 2;
                break;

            case ItemType.Invincible:
                //スーパースター（無敵）
                invincible.SetActive(true);
                // if ((int)data.character < 2) {
                //     invincible[(int)data.character].SetActive(true);
                // }
                // skinIndex = (int)data.character;

                // if (skinIndex == 1) {
                //     floating = true;
                // }
                break;

            case ItemType.FireFlower:
                //ファイアフラワー
                powerUps[0].SetActive(true);
                break;

            case ItemType.IceFlower:
                //アイスフラワー
                powerUps[1].SetActive(true);
                break;

            case ItemType.ThunderShroom:
                //サンダーキノコ
                powerUps[2].SetActive(true);
                break;

            case ItemType.BoomerangFlower:
                //ブーメランフラワー
                powerUps[3].SetActive(true);
                break;

            case ItemType.BubbleFlower:
                //アワフラワー
                powerUps[4].SetActive(true);
                break;

            case ItemType.WindFlower:
                //ウィンドフラワー
                powerUps[5].SetActive(true);
                break;

            case ItemType.SuperBallFlower:
                //スーパーボールフラワー
                powerUps[6].SetActive(true);
                break;

            case ItemType.PoisonShroom:
                //毒キノコ（8ダメージ）
                poison.SetActive(true);
                break;

            case ItemType.Extend:
                //1UPキノコ
                activateYashichi = !options.lives;
                if (activateYashichi)
                {
                    yashichi.SetActive(true);
                } else {
                    extend[index].SetActive(true);
                    skinIndex = index;
                    if (skinIndex == 1 || skinIndex == 1)
                    {
                        floating = true;
                    }
                }
                break;

            case ItemType.MegaMushroom:
                //巨大キノコ
                megaMushroom.SetActive(true);
                break;

            case ItemType.PowerPellet:
                //パワークッキー
                powerPellet.SetActive(true);
                floating = true;
                break;
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            PlayerInfo player = col.GetComponent<PlayerInfo>();
            CoinSpawner c;

            if (!player.primary) {
                return;
            }

            switch (itemType)
            {
                case ItemType.RestoreHP:
                    //スーパーキノコ（HP 8回復）
                    if (skinIndex == 0)
                    {
                        if (player.HP == player.maxHP)
                        {
                            Instantiate(effect, this.transform.position, Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(healEffect, this.transform.position, Quaternion.identity);
                        }
                    }
                    else if (skinIndex == 1)
                    {
                        MakeEffect();
                    }
                    else if (skinIndex == 2)
                    {
                        Instantiate(amiboHpEffect, this.transform.position, Quaternion.identity);
                    }
                    else if (skinIndex == 3)
                    {
                        ;
                    }
                    if (col.GetComponent<_16MSonic>() != null)
                    {
                        col.GetComponent<_16MSonic>().BoostIncrease(50, false);
                    }
                    else
                    {
                        if (player.GotShroom(this.transform.position))
                        {
                            c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                            c.SetUp(5, player);
                        }
                        player.HPRestore(8);
                    }
                    break;

                case ItemType.RestoreWP:
                    //アミーボ 武器（武器エネルギー 10回復）
                    if (player.GotRestoreWP(10, this.transform.position))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    Instantiate(amiboWpEffect, this.transform.position, Quaternion.identity);
                    break;

                case ItemType.Invincible:
                    //スーパースター（無敵）
                    player.Invincible(10, 0);
                    MakeEffect();
                    // if (skinIndex == 1)
                    // {
                    //     player.Invincible(7.5f, 1);
                    // }
                    // else
                    // {
                    //     player.Invincible(10, 0);
                    //     MakeEffect();
                    // }
                    break;

                case ItemType.FireFlower:
                    //ファイアフラワー
                    if (player.PowerUpAnim(1))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.IceFlower:
                    //アイスフラワー
                    if (player.PowerUpAnim(2))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.ThunderShroom:
                    //サンダーキノコ
                    if (player.PowerUpAnim(3))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.BoomerangFlower:
                    //ブーメランフラワー
                    if (player.PowerUpAnim(4))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.BubbleFlower:
                    //アワフラワー
                    if (player.PowerUpAnim(5))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.WindFlower:
                    //ウィンドフラワー
                    if (player.PowerUpAnim(6))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.SuperBallFlower:
                    //スーパーボールフラワー
                    if (player.PowerUpAnim(7))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    }
                    MakeEffect();
                    break;

                case ItemType.PoisonShroom:
                    //毒キノコ（8ダメージ）
                    if (player.poisonProof || player.invincible)
                    {
                        player.scorePopUp(1000, false, this.transform.position);
                        Instantiate(vanishEffect, this.transform.position, Quaternion.identity);
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(5, player);
                    } else
                    {
                        player.TakeDamage(8, this.transform.position, 6);
                    }
                    break;

                case ItemType.Extend:
                    //1UPキノコ
                    if (activateYashichi)
                    {
                        if (player.GotYashichi(this.transform.position))
                        {
                            c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                            c.SetUp(10, player);
                        }
                        Instantiate(yashichiEffect, this.transform.position, Quaternion.identity);
                    } else
                    {
                        player.OneUp();
                        MakeEffect();
                    }
                    break;

                case ItemType.MegaMushroom:
                    //巨大キノコ（巨大）
                    // if (skinIndex == 1) {
                    //     player.Invincible(7.5f, 1);
                    // } else {
                    //     player.Invincible(10, 0);
                    //     MakeEffect();
                    // }
                    player.Giant(20f, 0);
                    break;

                case ItemType.PowerPellet:
                    //パワークッキー（巨大）
                    // player.Invincible(7.5f, 1);
                    player.Giant(7.5f, 1);
                    break;
            }

            if (itemType != ItemType.PoisonShroom && !stock)
            {
                // player.scoreIncrease(1000);
                // if (itemType == ItemType.RestoreHP || (itemType == ItemType.RestoreWP && col.GetComponent<MegaManActions>() != null) || itemType == ItemType.Extend)
                // {
                //     player.scoreIncrease(100, this.transform.position);
                // }
                // else
                if (!(itemType == ItemType.RestoreHP || (itemType == ItemType.RestoreWP && col.GetComponent<MegaManActions>() != null) || itemType == ItemType.Extend))
                {
                    player.scorePopUp(1000, false, this.transform.position);
                }
            }

            Destroy(gameObject);
        }
    }

    void MakeEffect()
    {
        switch (skinIndex)
        {
            case 0:
                //マリオ
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;

            case 1:
                //パックマン
                Instantiate(pacGotEffect, this.transform.position, Quaternion.identity);
                break;

            case 2:
                //ロックマン
                Instantiate(megaGotEffect, this.transform.position, Quaternion.identity);
                break;

            case 3:
                //ソニック
                //Instantiate(ef, this.transform.position, Quaternion.identity);
                break;
        }
    }

    void FixedUpdate()
    {
        if (!floating)
        {
            switch (dimension)
            {
                case DimensionType.XWay2D:
                    //2D(X方向、Z固定)
                    velocity.z = 0f;
                    break;

                case DimensionType.ZWay2D:
                    //2D(Z方向、X固定)
                    velocity.x = 0f;
                    break;
            }
            if (Grounded)
            {
                if (velocity.y > 0)
                {
                    Grounded = false;
                }
            }
            else
            {
                velocity.y -= 0.25f;
                if (velocity.y < -20f)
                {
                    velocity.y = -20f;
                }
            }
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
    public void OnTriggerStay(Collider col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default" && !floating)
        {
            //跳ね返る
            RaycastHit Ghit;
            RaycastHit Whit;
            Vector3 XZvel = new Vector3(velocity.x, 0f, velocity.z);

            bool groundHit = Physics.Raycast(transform.position + Vector3.up * 2.7f, -Vector3.up, out Ghit, Math.Abs(velocity.y) * 5.4f);
            bool wallHit = Physics.Raycast(transform.position - XZvel.normalized * 5f, XZvel.normalized, out Whit, 10f);

            if (groundHit && velocity.y <= 0)
            {
                transform.position += Vector3.up * ((5.4f - Ghit.distance) - 1.35f);
                Grounded = true;
                velocity = Vector3.zero;
            }
            if (wallHit)
            {
                objNormalVector = Whit.normal;
                Vector3 reflectVec = Vector3.Reflect(afterReflectVelo, objNormalVector);
                velocity.x = reflectVec.x;
                velocity.z = reflectVec.z;
                // 計算した反射ベクトルを保存
                afterReflectVelo = velocity;
                afterReflectVelo.y = 0;
            }
        }
    }

    List<GameObject> addItem(List<GameObject> list, GameObject[] meshes)
    {
        foreach (var item in meshes)
        {
            if (item != null) list.Add(item);
        }
        return list;
    }

    public bool IsVisible()
    {
        if (this.GetComponent<MeshRenderer>() != null)
        {
            return this.GetComponent<MeshRenderer>().isVisible;
        }
        return false;
    }
    public bool DestroyWhenPlayerGoaled(PlayerInfo ply, int combo)
    {
        if (IsVisible())
        {
            int pts = 1000 * (int)Math.Pow(2, Math.Max(0, Math.Min(4, combo - 1)));
            ply.scorePopUp(pts, false, this.transform.position, combo - 1);

            CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
            c.SetUp(5, ply);
            Destroy(gameObject);
        }
        return IsVisible();
    }
}
