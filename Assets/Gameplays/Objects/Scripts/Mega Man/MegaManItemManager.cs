using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaManItemManager : MonoBehaviour
{
    /*
    ドロップ無し：20%
    ネジ(小)：25%
    武器エネルギー(小)：25%
    ライフエネルギー(小)：15%
    ネジ(大)：5%
    武器エネルギー(大)：5%
    ライフエネルギー(大)：4%
    1UP：1%
    */
    public enum MegaItemType
    {
        Coin,
        RestoreWPSmall,
        RestoreHPSmall,
        TenCoins,
        RestoreWP,
        RestoreHP,
        Extend,
        Yashichi
    }
    private Vector3 velocity;
    private float gravity = 3.125f;
    private Rigidbody rb;
    private float radius;

    private float timeLimit = 8f;

    private MegaItemType itemType;
    [Header("データ")]
    public GameData data;
    public Options options;

    [Header("スキン")]
    public GameObject[] coin = new GameObject[5];
    public GameObject[] restoreHPSmall = new GameObject[4];
    public GameObject restoreWPSmall;
    public GameObject[] tenCoins = new GameObject[5];
    public GameObject[] restoreHP = new GameObject[4];
    public GameObject restoreWP;
    public GameObject[] extend = new GameObject[4];
    public GameObject yashichi;
    private int skinIndex;

    [Header("効果音")]
    public AudioClip[] coinSounds = new AudioClip[5];
    public AudioClip[] tenCoinsSounds = new AudioClip[5];

    [Header("エフェクト")]
    public GameObject coinEffect;
    public GameObject tenCoinsEffect;
    public GameObject boltEffect;
    public GameObject ringEffect;
    public GameObject tenRingsEffect;
    public GameObject crystalEffect;
    public GameObject healEffect;
    public GameObject effect;
    public GameObject pacGotEffect;
    public GameObject megaGotEffect;
    public GameObject amiboHpEffect;
    public GameObject amiboWpEffect;
    public GameObject yashichiEffect;

    [Header("コイン")]
    public GameObject coinSpawner;

    private Character character;
    private int amounts = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        radius = this.GetComponent<SphereCollider>().radius;
    }

    void OnEnable()
    {
        character = data.character;
        int random = UnityEngine.Random.Range(1, 101);

        bool leastOneTypeMega = false;
        int index = 0;

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (allPlayers.Length < 2)
        {
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i].GetComponent<PlayerInfo>() != null)
                {
                    if (allPlayers[i].GetComponent<PlayerInfo>().playerType == 2) {
                        leastOneTypeMega = true;
                    }
                    if (allPlayers[i].GetComponent<PlayerInfo>().playerNumber == 0)
                    {
                        index = allPlayers[i].GetComponent<PlayerInfo>().playerType;
                    }
                }
            }
        }

        timeLimit = 8f;

        if (random > 99)
        {
            //1UP
            if (!options.lives)
            {
                itemType = MegaItemType.Yashichi;
            } else
            {
                itemType = MegaItemType.Extend;
            }
        }
        else if (random > 95)
        {
            //ライフエネルギー(大)
            itemType = MegaItemType.RestoreHP;
        }
        else if (random > 90)
        {
            //武器エネルギー(大)
            if (leastOneTypeMega)
            {
                itemType = MegaItemType.RestoreWP;
            }
            else
            {
                itemType = MegaItemType.TenCoins;
            }
        }
        else if (random > 85)
        {
            //ネジ(大)
            itemType = MegaItemType.TenCoins;
        }
        else if (random > 70)
        {
            //ライフエネルギー(小)
            itemType = MegaItemType.RestoreHPSmall;
        }
        else if (random > 45)
        {
            //武器エネルギー(小)
            if (leastOneTypeMega)
            {
                itemType = MegaItemType.RestoreWPSmall;
            }
            else
            {
                itemType = MegaItemType.Coin;
            }
        }
        else if (random > 20)
        {
            //ネジ(小)
            itemType = MegaItemType.Coin;
        }
        else
        {
            //ドロップなし
            Destroy(gameObject);
        }

        if (random > 20)
        {
            this.GetComponent<AudioSource>().Play();
        }

        List<GameObject> meshes = new List<GameObject>();
        meshes = addItem(meshes, coin);
        meshes = addItem(meshes, tenCoins);
        meshes = addItem(meshes, restoreHPSmall);
        meshes = addItem(meshes, new GameObject[1] { restoreWPSmall });
        meshes = addItem(meshes, restoreHP);
        meshes = addItem(meshes, new GameObject[1] { restoreWP });
        meshes = addItem(meshes, extend);
        meshes = addItem(meshes, new GameObject[1] { yashichi });

        foreach (var mesh in meshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(false);
            }
        }

        skinIndex = 0;
        switch (itemType)
        {
            case MegaItemType.Coin:
                switch (character)
                {
                    case Character.Mario:
                        //コイン
                        coin[0].SetActive(true);
                        break;

                    case Character.PacMan:
                        //クッキー
                        coin[1].SetActive(true);
                        break;

                    case Character.RockMan:
                        //ネジ
                        coin[2].SetActive(true);
                        break;

                    case Character.Sonic:
                        //リング
                        coin[3].SetActive(true);
                        break;

                    case Character.Other:
                        //クリスタル
                        coin[4].SetActive(true);
                        break;
                }
                break;

            case MegaItemType.TenCoins:
                switch (character)
                {
                    case Character.Mario:
                        //コイン
                        tenCoins[0].SetActive(true);
                        break;

                    case Character.PacMan:
                        //クッキー
                        tenCoins[1].SetActive(true);
                        break;

                    case Character.RockMan:
                        //ネジ
                        tenCoins[2].SetActive(true);
                        break;

                    case Character.Sonic:
                        //リング
                        tenCoins[3].SetActive(true);
                        break;

                    case Character.Other:
                        //クリスタル
                        tenCoins[4].SetActive(true);
                        break;
                }
                break;

            case MegaItemType.RestoreHPSmall:
                //スーパーキノコ（小、HP 2回復）
                restoreHPSmall[index].SetActive(true);
                skinIndex = index;
                break;

            case MegaItemType.RestoreHP:
                //スーパーキノコ（HP 8回復）
                restoreHP[index].SetActive(true);
                skinIndex = index;
                break;

            case MegaItemType.RestoreWPSmall:
                //アミーボ 武器（小、武器エネルギー 2回復）
                restoreWPSmall.SetActive(true);
                skinIndex = 2;
                break;

            case MegaItemType.RestoreWP:
                //アミーボ 武器（武器エネルギー 8回復）
                restoreWP.SetActive(true);
                skinIndex = 2;
                break;

            case MegaItemType.Extend:
                //1UPキノコ
                extend[index].SetActive(true);
                skinIndex = index;
                break;
            
            case MegaItemType.Yashichi:
                //弥七（1UPキノコの代理）
                yashichi.SetActive(true);
                break;
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

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity.y -= gravity;

        rb.linearVelocity = velocity;
    }
    void Update()
    {
        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default")
        {
            RaycastHit hit;
            Vector3 XZvel = new Vector3(velocity.x, 0f, velocity.z);

            bool groundHit = Physics.Raycast(transform.position, -Vector3.up, out hit, Math.Abs(velocity.y) * radius * 20f, 1 << 0);

            if (groundHit && velocity.y <= 0)
            {
                transform.position += Vector3.up * ((radius / 2) - hit.distance);
                velocity.x = 0f;
                velocity.y = 0f;
                velocity.z = 0f;
                gravity = 0f;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            PlayerInfo player = col.GetComponent<PlayerInfo>();
            CoinSpawner c;

            switch (itemType)
            {
                case MegaItemType.Coin:
                    amounts = 1;
                    CoinSound(col.gameObject);
                    MakeCoinEffect();
                    break;

                case MegaItemType.TenCoins:
                    amounts = 10;
                    CoinSound(col.gameObject, true);
                    MakeCoinEffect(true);
                    break;

                case MegaItemType.RestoreHPSmall:
                case MegaItemType.RestoreHP:
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
                        if (player.GotShroom(this.transform.position, itemType == MegaItemType.RestoreHPSmall))
                        {
                            c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                            c.SetUp(5, player);
                        }
                        if (itemType == MegaItemType.RestoreHPSmall)
                        {
                            player.HPRestore(2);
                        }
                        else
                        {
                            player.HPRestore(8);
                        }
                    }
                    break;

                case MegaItemType.RestoreWPSmall:
                case MegaItemType.RestoreWP:
                    //アミーボ 武器（武器エネルギー 8回復）
                    if (itemType == MegaItemType.RestoreWPSmall)
                    {
                        if (player.GotRestoreWP(2, this.transform.position, true))
                        {
                            c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                            c.SetUp(5, player);
                        }
                    }
                    else
                    {
                        if (player.GotRestoreWP(10, this.transform.position))
                        {
                            c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                            c.SetUp(5, player);
                        }
                    }
                    Instantiate(amiboWpEffect, this.transform.position, Quaternion.identity);
                    break;

                case MegaItemType.Extend:
                    //1UP
                    player.OneUp();
                    MakeEffect();
                    break;

                case MegaItemType.Yashichi:
                    //弥七
                    if (player.GotYashichi(this.transform.position))
                    {
                        c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                        c.SetUp(10, player);
                    }
                    Instantiate(yashichiEffect, this.transform.position, Quaternion.identity);
                    break;
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
    void MakeCoinEffect(bool big = false)
    {
        switch (character)
        {
            case Character.Mario:
                //コイン
                if (big)
                {
                    Instantiate(tenCoinsEffect, this.transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(coinEffect, this.transform.position, Quaternion.identity);
                }
                break;

            case Character.RockMan:
                //ネジ
                Instantiate(boltEffect, this.transform.position, Quaternion.identity);
                break;

            case Character.Sonic:
                //リング
                if (big)
                {
                    Instantiate(tenRingsEffect, this.transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(ringEffect, this.transform.position, Quaternion.identity);
                }
                break;

            case Character.Other:
                //クリスタル
                Instantiate(crystalEffect, this.transform.position, Quaternion.identity);
                break;
        }

        Destroy(gameObject);
    }

    void CoinSound(GameObject col, bool big = false) {
        int soundIndex = 0;
        float soundVolume = 1f;

        PlayerInfo player = col.GetComponent<PlayerInfo>();

        switch (data.character) {
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
        GameManager.Coins += (uint)amounts;
        player.scoreIncrease(50 * amounts, this.transform.position);

        if (col.GetComponent<_16MSonic>() != null) {
            col.GetComponent<_16MSonic>().BoostIncrease(amounts, false);
        }

        AudioSource playerGotit = col.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        if (big)
        {
            playerGotit.clip = tenCoinsSounds[soundIndex];
        }
        else
        {
            playerGotit.clip = coinSounds[soundIndex];
        }
        playerGotit.volume = soundVolume;
        playerGotit.Play();
    }
}
