using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaknessDamage
{
    [Header("弱点")]
    public WeaponTypes weakness;
    [Header("ダメージ")]
    public WeaponDamageType damageType;

    public WeaknessDamage (WeaponTypes w, WeaponDamageType t)
    {
        this.weakness = w;
        this.damageType = t;
    }
}
public enum NormalAttack
{
    Normal,
    NoEffective, //テレサの場合、透ける。ほかの敵は反射。ただし、弱点武器だけ有効。
    Reflect //すべて無効
}

public abstract class EnemyManager : DimensionManager
{
    [Header("ステータス")]
    public float HP = 1;
    public ushort attackPower = 5;
    public uint Score = 300;
    private bool attackedByPlayer;
    private int combo = 0;
    private byte NormalAttack = 0;
    protected bool active = true;
    public GameObject skin;
    public LayerMask GroundLayer;
    public bool defended = false;
    public bool hasShell = false;
    public bool spiked = false;
    public bool metal = false;
    private bool metalInit = false;
    public float forwardReflectAngle = 0f; //正面 xxx°の範囲で反射（0～180）

    [Header("ステータス")]
    public byte coins = 1;
    private int coinRate = 1;
    public GameObject coinSpawner;
    // public GameObject coinObj;

    [Header("弱点")]
    public NormalAttack normalAttack;
    public WeaknessDamage[] weaknesses;

    [Header("エフェクト")]
    public GameObject vanishEffect;
    public GameObject stompEffect;
    public GameObject hitEffect;
    public GameObject reflectEffect;
    protected PlayerInfo player;

    private AudioSource sound;
    float colliderRadius;
    //private Rigidbody rb;
    private RaycastHit hit;
    public Vector3 velocity;
    public bool Grounded = true;
    public bool Flying = false;
    [Header("凍結")]
    public GameObject frozen;
    public bool isFrozen = false;
    private Vector3 frozenLocalPos;
    private Quaternion frozenLocalRot;

    [Header("効果音")]
    public AudioClip[] stompedSound;
    public AudioClip[] damageSound;
    public AudioClip[] defeatedSound;
    public AudioClip[] reflectSound;
    public AudioClip[] guardBreakSound;
    public AudioClip metalReflectSound;

    [Header("アニメーション")]
    public Animator anim;
    [HideInInspector] public byte defeatedCase = 0;
    [HideInInspector] public byte actionId = 0;

    protected Vector3 skinInitial;
    protected Vector3 skinRate = Vector3.one;
    protected Vector3 skinCurrentRate = Vector3.one;

    [HideInInspector] public float damageTime = 0f;
    private float disablePlayerDamageTime = 0f;

    private float damageOpac = 0f;

    [HideInInspector] public bool blowAfterStomped = false;
    [HideInInspector] public bool stompedButNeverStretched = false;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (HP <= 0) HP = 1;
        sound = GetComponent<AudioSource>();

        colliderRadius = GetComponent<CapsuleCollider>().radius;

        rb = GetComponent<Rigidbody>();

        skinInitial = skin.transform.localScale;

        coinRate = 1;

        if (metal && !metalInit)
        {
            metalInit = true;
            Score += Math.Max(100, (uint)Math.Floor((float)Score / 150f) * 50);
            if (coins > 0)
            {
                coins += 1;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<InstaShield>() != null && active)
        {
            Debug.Log("Kicked");
            player = col.gameObject.GetComponent<InstaShield>().player;
            DamageManager(player, true);
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && HP > 0 && active)
        {
            player = col.gameObject.GetComponent<PlayerInfo>();
            if (PlayerAttacking(col.gameObject))
            {
                DamageManager(player);
            }
        }
    }
    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.tag == "Player" && HP > 0 && active && disablePlayerDamageTime <= 0f)
        {
            player = col.gameObject.GetComponent<PlayerInfo>();
            if (!PlayerAttacking(col.gameObject))
            {
                player.TakeDamage(attackPower, this.transform.position);
            }
        }
    }

    void DamageManager(PlayerInfo player, bool instaShield = false)
    {
        if (player.invincible || player.giant)
        {
            if (player.playerType == 3) player.Stomp();
            // TakeDamage(true, player, 99, 0, false, null);
            TakeDamage(true, player, 99, 0, false, null, WeaponTypes.None, false, false, true);
        }
        else if (player.rolling)
        {
            if (Stomped(player.gameObject, false) && spiked && !instaShield)
            {
                player.TakeDamage(6, this.transform.position, 4);
            }
            else
            {
                if (player.playerType != 0) player.Stomp();
                TakeDamage(true, player, 3, 0, Stomped(player.gameObject, false), null);
            }
        }
        else if (Stomped(player.gameObject, true))
        {
            if (spiked && !instaShield)
            {
                player.TakeDamage(6, this.transform.position, 4);
            }
            else
            {
                bool notBeated = TakeDamage(true, player, 3, 0, true, null).beated;
                player.Stomp(!notBeated);
            }
        }
        else if (player.attacking || instaShield)
        {
            TakeDamage(true, player, 3, 0, Stomped(player.gameObject, true) && !instaShield, null);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FreezePos(dimension);

        if (damageTime > 0)
        {
            damageTime -= Time.fixedDeltaTime;
        }
        if (disablePlayerDamageTime > 0)
        {
            disablePlayerDamageTime -= Time.fixedDeltaTime;
        }

        if (HP <= 0)
        {
            active = false;
        }

        if (!Flying || !active)
        {
            Physics.BoxCast(transform.position, Vector3.one * 0.75f, -transform.up, out hit, Quaternion.identity, 2f, GroundLayer);
            if (Grounded)
            {
                if (hit.distance <= 0f)
                {
                    Grounded = false;
                }
                else
                {
                    transform.position += Vector3.up * (0.6f - hit.distance);
                }
            }
            else
            {
                velocity.y -= 1.875f;

                if (hit.distance > 0 && velocity.y < 0)
                {
                    Grounded = true;
                    velocity.y = 0f;
                }
            }
        }
        // if (!isNear())
        // {
        //     velocity.x = 0f;
        //     velocity.z = 0f;
        // }
        if (isFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            this.transform.localPosition = frozenLocalPos;
            this.transform.localRotation = frozenLocalRot;
        }
        else
        {
            rb.linearVelocity = velocity;
        }

        if (this.transform.position.y <= -100f && active)
        {
            HP = 0;
            StartCoroutine("DestroyThisObject");
        }

        if (anim != null) AnimatorManager();

        SkinManager();

        if (damageOpac > 0f)
        {
            damageOpac -= 3 * Time.fixedDeltaTime;
            if (damageOpac <= 0f)
            {
                damageOpac = 0f;
            }
        }

        // PlayerInfo[] players = FindObjectsOfType<PlayerInfo>();
        // PlayerInfo nearistPlayer = null;
        // float minDistance = Mathf.Infinity;

        // foreach (PlayerInfo player in players) {
        //     float distance = Vector3.Distance(player.gameObject.transform.position, transform.position);
        //     if (minDistance > distance) {
        //         nearistPlayer = player;
        //         minDistance = distance;
        //     }
        // }
        // if (nearistPlayer != null) {
        //     if (!PlayerAttacking(nearistPlayer.gameObject) && nearistPlayer.XZmag >= 45 && minDistance < 30) {
        //         Time.timeScale = (minDistance / 30f);
        //     } else {
        //         if (Time.timeScale < 1f) {
        //             Time.timeScale = 1f;
        //         }
        //     }
        // }

        skinCurrentRate = Vector3.Lerp(skinCurrentRate, skinRate, 0.6f);

        Vector3 skinScale = skinInitial;
        skinScale.x *= skinCurrentRate.x;
        skinScale.y *= skinCurrentRate.y;
        skinScale.z *= skinCurrentRate.z;
        skin.transform.localScale = skinScale;

        if (skinRate != Vector3.one && defeatedCase != 1)
        {
            skinRate = Vector3.one;
        }
    }
    public void AnimatorManager()
    {
        Vector3 speed = new Vector3(velocity.x, 0, velocity.z);

        anim.SetInteger("actionId", actionId);
        anim.SetFloat("Speed", speed.magnitude);
        anim.SetBool("Grounded", true);
        anim.SetInteger("DefeatedCase", defeatedCase);
    }

    public void SkinManager()
    {
        ChangeSkin(skin.transform);
    }
    void ChangeSkin(Transform p)
    {
        Material[] currentMat;
        foreach (Transform child in p)
        {
            if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
            {
                currentMat = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;

                currentMat[0].SetFloat("MetalTrigger", metal ? 1f : 0f);
                currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

                child.gameObject.GetComponent<SkinnedMeshRenderer>().materials = currentMat;
            }

            if (child.gameObject.GetComponent<MeshRenderer>() != null)
            {
                currentMat = child.gameObject.GetComponent<MeshRenderer>().materials;

                currentMat[0].SetFloat("MetalTrigger", metal ? 1f : 0f);
                currentMat[0].SetFloat("DamageAlpha", damageOpac * 0.8f);

                child.gameObject.GetComponent<MeshRenderer>().materials = currentMat;
            }

            ChangeSkin(child);
        }
    }

    public bool isActive()
    {
        return active;
    }
    public bool isNear()
    {
        // return Vector3.Distance(Camera.main.transform.position, this.transform.position) <= 128;
        return true;
    }

    //戻り値詳細：1つ目のbool…倒した判定、2つ目のbool…跳ね返された判定
    public (bool beated, bool reflected) TakeDamage(
        bool atkByPl, PlayerInfo playerObj, float damage, byte normalAtk, bool stomped, 
        ComboManager comboM, WeaponTypes weapon = WeaponTypes.None, bool canPenetrate = false, 
        bool forceDoubleRate = false, bool forceDamage = false, Vector3? pointPos = null)
    {
        float totalDamage = 0f;
        bool beated = false;
        if (damageTime > 0f || isFrozen)
        {
            return (false, false);
        }

        Vector3 posDiff = this.transform.forward;
        if (pointPos != null)
        {
            posDiff = ((Vector3)pointPos - this.transform.position).normalized;
        }
        float pointAngle = Vector3.Angle(this.transform.forward, posDiff);
        if (pointPos == null)
        {
            pointAngle = 360f;
        }
        // Debug.Log("Pos diff : " + posDiff);
        // Debug.Log("Pos angle : " + pointAngle);

        GameManager.actionChainTime = 2f;

        if (metal && ((atkByPl && weapon == WeaponTypes.None && !playerObj.metal) || (weapon != WeaponTypes.None)))
        {
            if (damageTime <= 0f)
            {
                SoundPlay(new AudioClip[1]{metalReflectSound});
            }
            if (canPenetrate && damageTime <= 0f)
            {
                damageTime = 0.1f;
            }
            return (false, true);
        }

        bool doubleRate = forceDoubleRate;
        bool weakDamage = false;
        //効果なし
        if (
            (normalAttack != global::NormalAttack.Normal && !(forceDamage && normalAttack != global::NormalAttack.Reflect)) || 
            (!forceDamage && pointAngle <= forwardReflectAngle))
        {
            damage = 0;
        }
        float wDamage = damage;
        foreach (WeaknessDamage weakness in weaknesses)
        {
            if (weakness.weakness == weapon && weapon != WeaponTypes.None)
            {
                switch (weakness.damageType)
                {
                    case WeaponDamageType.Effective:
                        //効果は抜群だ！
                        wDamage = 6;
                        doubleRate = true;
                        break;

                    case WeaponDamageType.Weak:
                        //効果はいま一つのようだ・・・
                        weakDamage = true;
                        wDamage = 1;
                        break;

                    case WeaponDamageType.NoEffective:
                        //効果がないみたいだ・・・
                        wDamage = 0;
                        break;

                    case WeaponDamageType.Heal:
                        wDamage = -6;
                        break;
                }
                break;
            }
        }
        if (wDamage == 6)
        {
            if (normalAttack == global::NormalAttack.Reflect)
            {
                wDamage = 0;
            } else
            {
                if (wDamage < damage) wDamage = damage * 2;
            }
        } else
        {
            if (pointAngle <= forwardReflectAngle)
            {
                wDamage = 0;
            }
        }

        if (canPenetrate && damageTime <= 0f)
        {
            damageTime = 0.1f;
        }
        disablePlayerDamageTime = 0.3f;

        float attackRate = playerObj.attackUp ? 1.5f : 1f;
        totalDamage = (weapon != WeaponTypes.None ? wDamage : damage) * attackRate;

        if (active)
        {
            if (weapon == WeaponTypes.Ice)
            {
                if (!isFrozen)
                {
                    //凍結
                    player = playerObj;
                    isFrozen = true;
                    GetComponent<CapsuleCollider>().isTrigger = true;
                    GameObject ice = Instantiate(frozen, this.transform.position + Vector3.up * skin.transform.localPosition.y, this.transform.rotation);
                    Vector3 iceScale = new Vector3(colliderRadius * 0.5f, (1 + (Math.Max(0f, GetComponent<CapsuleCollider>().height - colliderRadius * 2) / (colliderRadius * 2f))) * colliderRadius * 0.5f, colliderRadius * 0.5f);
                    ice.transform.localScale = iceScale;
                    this.transform.parent = ice.transform;
                    frozenLocalPos = this.transform.localPosition;
                    frozenLocalRot = this.transform.localRotation;

                    EnemyIce enIce = ice.GetComponent<EnemyIce>();
                    enIce.enemy = this;
                    enIce.setHP();

                    if (doubleRate)
                    {
                        enIce.weaknessEffective = true;
                    }
                }
            }
            else
            {
                if (defended && !forceDamage)
                {
                    bool defendedDamage = this.GetComponent<EnemyMovements>().DamageDefended();
                    if (stomped)
                    {
                        if (defendedDamage)
                        {
                            SoundPlay(stompedSound);
                            Instantiate(hitEffect, playerObj.gameObject.transform.position - Vector3.up * 1.65f, Quaternion.identity);
                        } else
                        {
                            SoundPlay(reflectSound);
                            Instantiate(reflectEffect, playerObj.gameObject.transform.position - Vector3.up * 1.65f, Quaternion.identity);
                        }
                    }
                    else
                    {
                        if (defendedDamage)
                        {
                            SoundPlay(damageSound);
                            Instantiate(hitEffect, this.transform.position, Quaternion.identity);
                        } else
                        {
                            SoundPlay(reflectSound);
                            Instantiate(reflectEffect, this.transform.position, Quaternion.identity);
                        }
                    }
                    // this.GetComponent<EnemyMovements>().DamageDefended();
                    return (false, !defendedDamage);
                }
                if (hasShell && stomped)
                {
                    SoundPlay(stompedSound);
                    Instantiate(hitEffect, this.transform.position + Vector3.up * GetComponent<CapsuleCollider>().height, Quaternion.identity);
                    this.GetComponent<EnemyMovements>().DamageDefended();
                    return (false, false);
                }

                attackedByPlayer = atkByPl;
                HP -= totalDamage;
                if (stomped)
                {
                    if (HP <= 0)
                    {
                        damageOpac = 0f;
                    } else
                    {
                        if (totalDamage > 0)
                        {
                            damageOpac = 1f;
                        }
                    }
                    // Instantiate(hitEffect, this.transform.position + Vector3.up * GetComponent<CapsuleCollider>().height, Quaternion.identity);
                    if (totalDamage > 0)
                    {
                        SoundPlay(stompedSound);
                        Instantiate(hitEffect, playerObj.gameObject.transform.position - Vector3.up * 1.65f, Quaternion.identity);
                    } else
                    {
                        SoundPlay(reflectSound);
                        Instantiate(reflectEffect, playerObj.gameObject.transform.position - Vector3.up * 1.65f, Quaternion.identity);
                    }
                }
                else
                {
                    if (HP <= 0)
                    {
                        SoundPlay(defeatedSound);
                        Instantiate(hitEffect, this.transform.position, Quaternion.identity);
                        damageOpac = 0f;
                    }
                    else
                    {
                        if (totalDamage > 0)
                        {
                            SoundPlay(damageSound);
                            Instantiate(hitEffect, this.transform.position, Quaternion.identity);
                            damageOpac = 1f;
                        } else
                        {
                            SoundPlay(reflectSound);
                            Instantiate(reflectEffect, this.transform.position, Quaternion.identity);
                        }
                    }
                }

                if (attackedByPlayer)
                {
                    player = playerObj;
                    NormalAttack = normalAtk;

                    if (HP <= 0)
                    {
                        beated = true;
                        if (Score > 0)
                        {
                            GameManager.IncreaseActionChain(true);
                            GameManager.ChainIncrease(true);
                            if (comboM != null)
                            {
                                combo = comboM.ComboIncrease();
                            }
                            else
                            {
                                combo = player.ComboIncrease();
                            }
                        }
                        else
                        {
                            GameManager.IncreaseActionChain();
                            GameManager.ChainTimeRecovery();
                            if (comboM != null)
                            {
                                combo = comboM.GetCombo();
                            }
                            else
                            {
                                combo = player.GetCombo();
                            }
                        }

                        if (stomped && !stompedButNeverStretched)
                        {
                            skinRate = new Vector3(1.5f, 0.4f, 1.5f);
                        }
                    }
                    else
                    {
                        GameManager.ChainTimeRecovery();

                        if (comboM != null)
                        {
                            combo = comboM.GetCombo();
                        }
                        else
                        {
                            combo = player.GetCombo();
                        }
                    }
                }

                if (attackedByPlayer && combo > 0)
                {
                    uint pts;
                    if ((doubleRate || !weakDamage) && NormalAttack == 1)
                    {
                        NormalAttack = 0;
                    }

                    switch (NormalAttack)
                    {
                        case 1:
                            //属性攻撃
                            pts = Score * (uint)Math.Max(1, Math.Min(16, combo));
                            break;

                        case 2:
                            //スペシャル攻撃
                            pts = Score * 2 * (uint)Math.Pow(4, Math.Min(2, Math.Max(0, Math.Min(2, combo - 1))));
                            break;

                        default:
                            //通常攻撃、無敵
                            pts = Score * (uint)Math.Pow(2, Math.Max(0, Math.Min(4, combo - 1)));
                            break;
                    }
                    //弱点を突けたら得点2倍
                    if (totalDamage > 0 || HP <= 0)
                    {
                        player.scorePopUp((int)pts, doubleRate, this.transform.position, combo - 1);
                    }

                    // if (player.rateTime > 0) player.rateTime += 0.5f;
                }
                if (HP <= 0)
                {
                    if (player != null)
                    {
                        coinRate *= player.rate;
                    }
                    if (doubleRate)
                    {
                        coinRate *= 2;
                    }
                    if (GameManager.ringRush) {
                        coinRate *= 2;
                    }
                    // if (player.coinUp) {
                    //     coins += 2;
                    // }
                    StartCoroutine("DefeatedTimeStop");
                    StartCoroutine(DefeatedAnimation(stomped));
                }
            }
        }
        return (beated || HP <= 0, totalDamage <= 0);
    }

    bool Stomped(GameObject col, bool stompable)
    {
        float playerY = col.transform.position.y - 2f + (colliderRadius * this.transform.localScale.y);
        float thisY = this.transform.position.y;

        if (stompable)
        {
            return player.Stompable && playerY >= thisY;
        }
        return playerY >= thisY;
    }

    public void SoundPlay(AudioClip[] soundClips)
    {
        if (soundClips != null)
        {
            int soundIndex = UnityEngine.Random.Range(0, soundClips.Length);

            //効果音の再生
            sound.PlayOneShot(soundClips[soundIndex]);
        }
    }
    public IEnumerator DefeatedTimeStop()
    {
        GameManager.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.1f);
        GameManager.timeScale = 1f;
    }
    public virtual IEnumerator DefeatedAnimation(bool stomped)
    {
        yield return null;
    }

    public IEnumerator DestroyThisObject()
    {
        GameManager.ringRushProgress += 0.1f;

        active = false;
        GetComponent<CapsuleCollider>().isTrigger = true;

        if (player != null && player.GetComponent<_16MSonic>() != null && Score > 0)
        {
            player.GetComponent<_16MSonic>().BoostIncrease(5, false);
        }

        GameObject ve = Instantiate(vanishEffect, this.transform.position, Quaternion.identity);
        // vanishEffect.SetActive(true);
        // vanishEffect.transform.SetParent(null);

        Vector3 thisPos = this.transform.position;

        skin.SetActive(false);

        if (player != null && HP <= 0 && coins > 0)
        {
            CoinSpawner c = Instantiate(coinSpawner, thisPos, Quaternion.identity).GetComponent<CoinSpawner>();
            int totalCoins = (int)(
                (coins + (GameManager.courseMode == CourseMode.RingAttack ? (Math.Pow(2, Math.Max(0, Math.Min(2, combo - 1))) - 1) : 0))
                 * coinRate + (player.coinUp ? 2 : 0));
            c.SetUp(totalCoins, player);
            // c.SetUp(
            //     (int)Math.Pow(2, Math.Max(0, Math.Min(2, combo - 1))) + (int)coins - 1 + (player.coinUp ? 2 : 0),
            //     player);
            // for (int i = 0; i < coins; i++)
            // {
            //     CoinManager c = Instantiate(coinObj, thisPos, Quaternion.identity).GetComponent<CoinManager>();
            //     c.targetPlayer = player.gameObject;
            //     StartCoroutine(c.SplitCoin(false));

            //     yield return new WaitForSeconds(0.2f);
            // }
        }

        // while (ve != null)
        // {
        //     yield return null;
        // }
        yield return new WaitForSecondsRealtime(0.15f);
        // yield return null;
        Destroy(gameObject);
    }

    bool PlayerAttacking(GameObject playerObj)
    {
        player = playerObj.GetComponent<PlayerInfo>();
        return (player.invincible || player.giant || player.rolling || player.attacking || Stomped(playerObj, true) && !player.tookDamage);
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
            int pts = (int)(Score * Math.Pow(2, Math.Max(0, Math.Min(4, combo - 1))));
            ply.scorePopUp(pts, false, this.transform.position, combo - 1);

            vanishEffect.SetActive(true);
            vanishEffect.transform.SetParent(null);

            CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
            c.SetUp(
                (int)Math.Pow(2, Math.Max(0, Math.Min(2, combo - 1))) + (int)coins - 1 + (ply.coinUp ? 2 : 0),
                ply);
        }
        Destroy(gameObject);
        return IsVisible();
    }
    public bool ReplaceTo50Coins(PlayerInfo ply, int combo)
    {
        if (IsVisible())
        {
            int pts = (int)(Score * Math.Pow(2, Math.Max(0, Math.Min(4, combo - 1))));
            ply.scorePopUp(pts, false, this.transform.position, combo - 1);

            vanishEffect.SetActive(true);
            vanishEffect.transform.SetParent(null);

            CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
            c.SetUp(50, ply, true);

            Destroy(gameObject);
        }
        return IsVisible();
    }
    public void FreeFromFrozen(ComboManager ice, bool attacked = false, bool weaknessEffective = false)
    {
        isFrozen = false;
        GetComponent<CapsuleCollider>().isTrigger = false;

        if (attacked)
        {
            TakeDamage(true, player, 99, 0, false, ice, WeaponTypes.None, false, false, true);
        }
        if (weaknessEffective)
        {
            TakeDamage(true, player, 99, 0, false, ice, WeaponTypes.None, false, true, true);
        }
    }

    [ContextMenu("なんでも平均的ダメージ")]
    public void AllAverage() {
        weaknesses = new WeaknessDamage[0];
    }
    [ContextMenu("特殊武器なら何でも有効")]
    public void AllEffective() {
        int len = Enum.GetValues(typeof(WeaponTypes)).Length;
        int count = 0;
        weaknesses = new WeaknessDamage[Enum.GetValues(typeof(WeaponTypes)).Length - 1];

        for (int i = 0; i < len; i++)
        {
            //iに対応するNumberの要素を取得
            if ((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), i) != WeaponTypes.None)
            {
                weaknesses[count] = new WeaknessDamage((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), i), WeaponDamageType.Effective);
                count++;
            }
        }
    }
    [ContextMenu("全部無効")]
    public void AllNonEffective() {
        int len = Enum.GetValues(typeof(WeaponTypes)).Length;
        int count = 0;
        weaknesses = new WeaknessDamage[Enum.GetValues(typeof(WeaponTypes)).Length - 1];

        for (int i = 0; i < len; i++)
        {
            //iに対応するNumberの要素を取得
            if ((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), i) != WeaponTypes.None)
            {
                weaknesses[count] = new WeaknessDamage((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), i), WeaponDamageType.NoEffective);
                count++;
            }
        }
    }
    [ContextMenu("カメの敵")]
    public void TurtleEnemy() {
        weaknesses = new WeaknessDamage[6];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Bomb, WeaponDamageType.Effective);
        //耐性
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Weak);
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Stone, WeaponDamageType.Weak);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Dash, WeaponDamageType.Weak);
        //無効
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Cutter, WeaponDamageType.NoEffective);
    }
    [ContextMenu("属性：ファイア")]
    public void FireType() {
        weaknesses = new WeaknessDamage[6];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Wind, WeaponDamageType.Effective);
        //耐性
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Weak);
        //無効
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.NoEffective);
        //回復
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Heal);
    }
    [ContextMenu("属性：アイス")]
    public void IceType() {
        weaknesses = new WeaknessDamage[8];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Bomb, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Effective);
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Stone, WeaponDamageType.Effective);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Effective);
        //耐性
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.Weak);
        weaknesses[6] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.Weak);
        //回復
        weaknesses[7] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Heal);
    }
    [ContextMenu("属性：サンダー")]
    public void ThunderType() {
        weaknesses = new WeaknessDamage[7];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Stone, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Cutter, WeaponDamageType.Effective);
        //耐性
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Weak);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Weak);
        //回復
        weaknesses[6] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Heal);
    }
    [ContextMenu("属性：レーザー")]
    public void LaserType() {
        weaknesses = new WeaknessDamage[6];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Stone, WeaponDamageType.Effective);
        //耐性
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Weak);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Weak);
    }
    [ContextMenu("属性：ウォーター")]
    public void WaterType() {
        weaknesses = new WeaknessDamage[5];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Dash, WeaponDamageType.Effective);
        //耐性
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Weak);
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        //無効
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.NoEffective);
    }
    [ContextMenu("属性：ブーメラン")]
    public void BoomerangType() {
        weaknesses = new WeaknessDamage[5];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Dash, WeaponDamageType.Effective);
        //耐性
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Weak);
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        //無効
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.NoEffective);
    }
    [ContextMenu("属性：バブル")]
    public void BubbleType() {
        weaknesses = new WeaknessDamage[7];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Cutter, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Needle, WeaponDamageType.Effective);
        //耐性
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Weak);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        //無効
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.NoEffective);
        weaknesses[6] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.NoEffective);
    }
    [ContextMenu("属性：ウィンド")]
    public void WindType() {
        weaknesses = new WeaknessDamage[6];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Stone, WeaponDamageType.Effective);
        //無効
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Cutter, WeaponDamageType.NoEffective);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Needle, WeaponDamageType.NoEffective);
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Boomerang, WeaponDamageType.NoEffective);
    }
    [ContextMenu("属性：絶縁体")]
    public void InsulatorType() {
        weaknesses = new WeaknessDamage[10];

        //有効
        weaknesses[0] = new WeaknessDamage(WeaponTypes.Laser, WeaponDamageType.Effective);
        weaknesses[1] = new WeaknessDamage(WeaponTypes.Cutter, WeaponDamageType.Effective);
        weaknesses[2] = new WeaknessDamage(WeaponTypes.Bomb, WeaponDamageType.Effective);
        //耐性
        weaknesses[3] = new WeaknessDamage(WeaponTypes.Ice, WeaponDamageType.Weak);
        weaknesses[4] = new WeaknessDamage(WeaponTypes.Fire, WeaponDamageType.Weak);
        weaknesses[5] = new WeaknessDamage(WeaponTypes.Water, WeaponDamageType.Weak);
        weaknesses[6] = new WeaknessDamage(WeaponTypes.Dash, WeaponDamageType.Weak);
        weaknesses[7] = new WeaknessDamage(WeaponTypes.Insulator, WeaponDamageType.Weak);
        //無効
        weaknesses[8] = new WeaknessDamage(WeaponTypes.Thunder, WeaponDamageType.NoEffective);
        weaknesses[9] = new WeaknessDamage(WeaponTypes.Wind, WeaponDamageType.NoEffective);
    }
}
