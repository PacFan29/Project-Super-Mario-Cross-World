using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("速度設定")]
    public float speed;
    public float gravity;
    public bool xBounce;
    public float yBounceVel;
    [Header("ダメージ属性")]
    public DamageType type;
    [Header("貫通判定")]
    public bool canPenetrate;
    [Header("エフェクト")]
    public GameObject destroyEffect;

    private float lifeTime = 10f;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lifeTime = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = this.transform.forward * speed;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            DestroyObject();
        }
    }

    void DestroyObject()
    {
        if (destroyEffect != null)
        {
            destroyEffect.SetActive(true);
            destroyEffect.transform.parent = null;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && col.gameObject.GetComponent<PlayerInfo>().shieldActive == 0)
        {
            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            player.TakeDamage(4, this.transform.position, (int)type);
            if (!canPenetrate)
            {
                DestroyObject();
            }
        }
    }
}
