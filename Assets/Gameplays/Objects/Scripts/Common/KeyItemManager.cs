using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItemManager : MonoBehaviour
{
    public GameData data;
    protected bool gotIt;
    protected Animator animator;

    [Header("グループ")]
    public Transform group;

    [Header("エフェクト")]
    public ParticleSystem effect;
    public GameObject gotEffect;
    [Header("効果音")]
    public AudioClip[] gotSounds = new AudioClip[4];
    public AudioClip alreadyGotSound;
    protected AudioSource source;
    public int index;
    protected int skinIndex;
    protected float volume;

    protected bool isInvisible = false;
    protected bool invisibleTrigger = false;

    [Header("獲得済み用マテリアル")]
    public Material alreadyGotIt;

    [Header("コイン")]
    public GameObject coinSpawner;
    
    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();

        source.clip = gotSounds[0];
        source.volume = 1f;
    }
    void FixedUpdate()
    {
        float rotateSpeed = gotIt ? 30f : 3f;
        group.Rotate(0f, -rotateSpeed, 0f, Space.Self);

        SkinManager();

        animator.SetBool("GotIt", gotIt);
    }

    public virtual void SkinManager()
    {
        ;
    }

    void DestroyObj(){
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && !gotIt) {
            GameManager.ringRushProgress += 0.1f;
            
            if (isInvisible) {
                //獲得済み
                source.clip = alreadyGotSound;
                source.volume = 0.5f;
                source.Play();
                source.pitch = 1f;

                gotEffect.transform.parent = null;
            } else {
                //通常
                // source.clip = gotSounds[skinIndex];
                // source.volume = volume;
                // source.pitch = GameManager.greenStarPitch;
                UpdateElement();
                source.Play();

                StartCoroutine("TimeStop");

                var emi = effect.emission;
                emi.rateOverTime = 0f;

                Transform efChild = effect.gameObject.transform.GetChild(0);
                ParticleSystem ef2 = efChild.gameObject.GetComponent<ParticleSystem>();
                var emi2 = ef2.emission;
                emi2.rateOverTime = 0f;
                
                // GameManager.keyItems[index] = true;

                // if (skinIndex == 0)
                // {
                //     GameManager.greenStarPitch += 0.05f;
                // }
            }

            PlayerInfo player = other.GetComponent<PlayerInfo>();
            gotIt = true;
            if (isInvisible) {
                //Great
                player.scorePopUp(3000, false, this.transform.position, 1);
                TenCoins(player);
            } else {
                //Excellent
                player.scorePopUp(3000, false, this.transform.position, 4);
                if (GameManager.courseMode == CourseMode.RingAttack) {
                    TenCoins(player);
                }
            }
            player.StartCoroutine(player.Excellent());

            gotEffect.SetActive(true);
        }
    }

    public virtual void UpdateElement()
    {
        ;
    }

    public IEnumerator TimeStop()
    {
        GameManager.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.1f);
        GameManager.timeScale = 1f;
    }

    void TenCoins(PlayerInfo player)
    {
        CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
        c.SetUp(10, player);
    }
}
