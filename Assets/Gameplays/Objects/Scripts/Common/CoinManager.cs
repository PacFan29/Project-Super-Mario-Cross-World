using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [Header("枚数")]
    public int amounts = 1;
    [Header("データ")]
    public GameData data;
    [Header("その他")]
    public GameObject[] shapes = new GameObject[5];
    public GameObject superRingAmountDisp;
    public GameObject multiCrystalAmountDisp;
    public AudioClip[] sounds = new AudioClip[5];
    public GameObject coinEffect;
    public GameObject cookieEffect;
    public GameObject boltEffect;
    public GameObject ringEffect;
    public GameObject crystalEffect;

    private Character character;
    [HideInInspector] public bool magnetised = false;
    [HideInInspector] public bool split;
    [HideInInspector] public bool drop;
    private Rigidbody rb;
    private Vector3 velocity;
    private float angle;
    private float speed;
    private float gravity = 3.125f;

    private float spawnTime = 0f;
    [HideInInspector] public GameObject targetPlayer;

    
    //ボールが当たった物体の法線ベクトル
    private Vector3 objNormalVector = Vector3.zero;
    // 跳ね返った後のvelocity
    private Vector3 afterReflectVelo = Vector3.zero;

    private float radius;
    private float dropTime = 10f;
    [Header("クリスタル")]
    public MeshRenderer crystalMesh;
    private float crystalColor;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        radius = this.GetComponent<SphereCollider>().radius;

        crystalColor = UnityEngine.Random.Range(0f, 1f);
        if (crystalMesh != null)
        {
            Material cm = crystalMesh.material;
            cm.SetFloat("_Color", crystalColor);
        }
    }
    public void DropCoin(float ang, float spd, float yVel)
    {
        drop = true;
        angle = ang;
        speed = spd;

        float cosRad = (float)Math.Cos(angle * Mathf.Deg2Rad); //X・Z方向に使用
        float sinRad = (float)Math.Sin(angle * Mathf.Deg2Rad); //Y方向に使用

        switch(GameManager.dimension) {
            case DimensionType.Normal3D:
                //3D
                velocity.x = sinRad * speed;
                velocity.y = yVel;
                velocity.z = cosRad * speed;
                break;

            case DimensionType.XWay2D:
                //X方向2D
                velocity.x = sinRad * speed;
                if (Math.Abs(yVel) > 0)
                {
                    velocity.y = yVel;
                }
                else
                {
                    velocity.y = cosRad * speed;
                }
                break;
            
            case DimensionType.ZWay2D:
                //Z方向2D
                if (Math.Abs(yVel) > 0)
                {
                    velocity.y = yVel;
                }
                else
                {
                    velocity.y = cosRad * speed;
                }
                velocity.z = sinRad * speed;
                break;
        }

        afterReflectVelo = velocity;
        afterReflectVelo.y = 0;
    }
    void FixedUpdate()
    {
        if (split) {
            // this.transform.Rotate(0f, -30f, 0f, Space.Self);
            this.transform.GetChild(0).Rotate(0f, -30f, 0f, Space.Self);
            // superRingAmountDisp.transform.Rotate(0f, 30f, 0f, Space.Self);
            // multiCrystalAmountDisp.transform.Rotate(0f, 30f, 0f, Space.Self);
        } else {
            // this.transform.Rotate(0f, -3f, 0f, Space.Self);
            this.transform.GetChild(0).Rotate(0f, -3f, 0f, Space.Self);
            // superRingAmountDisp.transform.Rotate(0f, 3f, 0f, Space.Self);
            // multiCrystalAmountDisp.transform.Rotate(0f, 3f, 0f, Space.Self);
        }

        if (magnetised && !split)
        {
            velocity = (targetPlayer.transform.position - this.transform.position).normalized * 100f;
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players)
            {
                if (
                    Vector3.Distance(player.transform.position, transform.position) <= 16 &&
                    (
                        (player.GetComponent<PlayerInfo>().elecProof ||
                        (player.GetComponent<_16MSonic>() != null && player.GetComponent<_16MSonic>().boost)) &&
                        !split && amounts < 2
                    )
                )
                {
                    targetPlayer = player;
                    magnetised = true;
                }
            }

            if (drop)
            {
                dropTime -= Time.deltaTime;
                if (dropTime <= 0) Destroy(gameObject);
            }

            if (split || drop) velocity.y -= gravity;
        }

        rb.linearVelocity = velocity;
    }

    // Update is called once per frame
    void Update()
    {
        character = data.character;

        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i].SetActive(false);
        }
        switch (character)
        {
            case Character.Mario:
                //コイン
                shapes[0].SetActive(true);
                break;

            case Character.PacMan:
                //クッキー
                shapes[1].SetActive(true);
                break;

            case Character.RockMan:
                //ネジ
                shapes[2].SetActive(true);
                break;

            case Character.Sonic:
                //リング
                shapes[3].SetActive(true);
                // if (superRingAmountDisp != null)
                // {
                //     if (superRingAmountDisp.transform.parent != null)
                //     {
                //         superRingAmountDisp.transform.parent = null;
                //     }
                // }
                break;

            case Character.Other:
                //クリスタル
                shapes[4].SetActive(true);
                // if (multiCrystalAmountDisp != null)
                // {
                //     if (multiCrystalAmountDisp.transform.parent != null)
                //     {
                //         multiCrystalAmountDisp.transform.parent = null;
                //     }
                // }
                break;
        }
        if (superRingAmountDisp != null)
        {
            superRingAmountDisp.SetActive(character == Character.Sonic);
        }
        if (multiCrystalAmountDisp != null)
        {
            multiCrystalAmountDisp.SetActive(character == Character.Other);
        }

        if (spawnTime > 0f)
        {
            spawnTime -= Time.deltaTime;
            if (spawnTime <= 0f)
            {
                GameManager.Coins += (uint)amounts;
                targetPlayer.GetComponent<PlayerInfo>().scoreIncrease(50 * amounts, this.transform.position);
                GameManager.ringRushProgress += 0.01f * amounts;
                DestroyCoin();
            }
        }
    }
    float getCoinAcc (bool signFit) {
        if (signFit) {
            return 0.9375f;
        } else {
            return 3.75f;
        }
    }

    void OnTriggerStay(Collider col) {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Default" && drop) {
            //跳ね返る
            RaycastHit hit;
            Vector3 XZvel = new Vector3(velocity.x, 0f, velocity.z);

            bool groundHit = Physics.Raycast(transform.position, -Vector3.up, out hit, Math.Abs(velocity.y) * radius * 20f, 1 << 0);
            bool wallHit = Physics.Raycast(transform.position, XZvel, out hit, radius * 5f, 1 << 0);

            if (groundHit && velocity.y <= 0)
            {
                velocity.x = 0f;
                velocity.y = 0f;
                velocity.z = 0f;
                gravity = 0f;
            }
            if (wallHit) {
                objNormalVector = hit.normal;
                Vector3 reflectVec = Vector3.Reflect (afterReflectVelo, objNormalVector);
                velocity.x = reflectVec.x;
                velocity.z = reflectVec.z;
                // 計算した反射ベクトルを保存
                afterReflectVelo = velocity;
                afterReflectVelo.y = 0;
            }
        }
    }
    public void CoinGet(GameObject player)
    {
        if (!split)
        {
            GameManager.ChainTimeRecovery();
            CoinSound(player);
            DestroyCoin();
        }
    }
    void OnTriggerEnter(Collider col){
        // if (col.gameObject.tag == "Player" && !split){
        //     GameManager.ChainTimeRecovery();
        //     CoinSound(col.gameObject);
        //     DestroyCoin();
        // }
        if (col.gameObject.tag == "Player")
        {
            CoinGet(col.gameObject);
        }
    }

    void CoinSound(GameObject col, bool sp = false) {
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
        if (!sp)
        {
            GameManager.Coins += (uint)amounts;
            player.scoreIncrease(50 * amounts, this.transform.position);
            GameManager.ringRushProgress += 0.01f * amounts;
        }

        if (col.GetComponent<_16MSonic>() != null) {
            col.GetComponent<_16MSonic>().BoostIncrease(amounts, false);
        }

        AudioSource playerGotit = col.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        playerGotit.clip = sounds[soundIndex];
        playerGotit.volume = soundVolume;
        if (data.character == Character.Other)
        {
            playerGotit.PlayOneShot(sounds[soundIndex]);
        } else
        {
            playerGotit.Play();
        }
    }

    void DestroyCoin() {
        switch (character) {
            case Character.Mario:
            //コイン
            Instantiate(coinEffect, this.transform.position, Quaternion.identity);
            break;

            case Character.PacMan:
            //クッキー
            Instantiate(cookieEffect, this.transform.position, Quaternion.identity);
            break;

            case Character.RockMan:
            //ネジ
            Instantiate(boltEffect, this.transform.position, Quaternion.identity);
            break;
                
            case Character.Sonic:
            //リング
            Instantiate(ringEffect, this.transform.position, Quaternion.identity);
            break;

            case Character.Other:
            //クリスタル
            Instantiate(crystalEffect, this.transform.position, Quaternion.identity);
            break;
        }
        
        // Destroy(superRingAmountDisp);
        // Destroy(multiCrystalAmountDisp);
        Destroy(gameObject);
    }

    public IEnumerator SplitCoin(bool downWard)
    {
        split = true;
        if (downWard)
        {
            velocity.y = 0f;
            gravity = 1f;
        }
        else
        {
            velocity.y = 50f;
        }
        CoinSound(targetPlayer, true);

        spawnTime = 0.5f;
        yield return null;
    }
}
