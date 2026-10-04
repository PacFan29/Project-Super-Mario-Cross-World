using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _17MrI : MonoBehaviour
{
    private GameObject targetPlayer;
    private TriggerDamage dam;
    private uint Score = 1500;

    [Header("角度")]
    private Vector3 direction = Vector3.zero;
    public float rotationSpeed;
    public float totalRotationY = 0f;
    private float lastRotationY;

    [Header("エフェクト・コイン")]
    public GameObject vanishEffect;
    public GameObject coinSpawner;

    protected bool weaknessExploited = false;
    private byte combo = 0;
    
    protected bool active = true;
    private bool attackedByPlayer;
    private byte NormalAttack = 0;

    protected PlayerInfo player;
    
    [Header("弱点")]
    public WeaponTypes[] weaknesses;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastRotationY = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetPlayer();

        this.GetComponent<TriggerDamage>().enabled = active;

        if (targetPlayer != null)
        {
            Vector3 currentDir = this.transform.forward;
            float currentRotationY = transform.eulerAngles.y;
            direction = Vector3.Lerp(this.transform.forward, (targetPlayer.transform.position - this.transform.position).normalized, 0.3f);

            rotationSpeed = Mathf.DeltaAngle(lastRotationY, currentRotationY);

            this.transform.forward = direction;

            totalRotationY += rotationSpeed;
            lastRotationY = currentRotationY;

            if (Mathf.Abs(totalRotationY) >= 360f)
            {
                //一周回転
                TakeDamage(true, targetPlayer.GetComponent<PlayerInfo>(), 0, false, null);
                totalRotationY = 0f;
            }

            if (Mathf.Abs(rotationSpeed) <= 0.5f)
            {
                lastRotationY = currentRotationY;
                totalRotationY = 0f; 
            }
        }
    }

    public void SetPlayer() {
        float minDistance = Mathf.Infinity;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player.GetComponent<PlayerInfo>() != null && Vector3.Distance(player.transform.position, this.transform.position) < minDistance) {
                minDistance = Vector3.Distance(player.transform.position, this.transform.position);
                targetPlayer = player;
            }
        }
    }

    void OnCollisionStay(Collision col){
        if (col.gameObject.tag == "Player" && active){
            player = col.gameObject.GetComponent<PlayerInfo>();
            if (player.invincible) {
                TakeDamage(true, player, 0, false, null);
            }
        }
    }

    public void TakeDamage(bool atkByPl, PlayerInfo playerObj, byte normalAtk, bool stomped, ComboManager comboM){
        if (active) {
            attackedByPlayer = atkByPl;
            
            if (attackedByPlayer) {
                player = playerObj;
                NormalAttack = normalAtk;

                if (comboM != null) {
                    combo = comboM.ComboIncrease();
                } else {
                    combo = player.ComboIncrease();
                }
            }
            StartCoroutine(DefeatedAnimation(attackedByPlayer, normalAtk));
            active = false;
        }
    }

    IEnumerator DefeatedAnimation(bool attackedByPlayer, byte normalAtk)
    {
        yield return null;

        Instantiate(vanishEffect, this.transform.position, Quaternion.identity);
        
        if (attackedByPlayer && combo > 0){
            uint pts;
            switch (NormalAttack) {
                case 1:
                //属性攻撃
                pts = Score * (uint)Math.Max(1, Math.Min(5, (int)combo));
                break;

                case 2:
                //スペシャル攻撃
                pts = Score * 2 * (uint)Math.Pow(4, Math.Min(2, Math.Max(0, Math.Min(2, (int)combo - 1))));
                break;

                default:
                //通常攻撃、無敵
                pts = Score * (uint)Math.Pow(2, Math.Max(0, Math.Min(4, (int)combo - 1)));
                break;
            }
            //弱点を突けたら得点2倍
            player.scorePopUp((int)pts, weaknessExploited, this.transform.position, combo - 1);

            if (player.rateTime > 0) player.rateTime += 0.5f;
        }

        int coins = 5;
        coins *= player.rate;

        CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
        c.SetUp(
            (int)Math.Pow(2, Math.Max(0, Math.Min(2, combo - 1))) + (int)coins - 1 + (player.coinUp ? 2 : 0),
            player);
        
        Destroy(gameObject);
    }
}
