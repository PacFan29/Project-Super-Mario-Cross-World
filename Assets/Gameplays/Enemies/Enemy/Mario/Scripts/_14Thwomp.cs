using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _14Thwomp : MonoBehaviour
{
    public GameObject[] skin1 = new GameObject[3];
    public GameObject[] skin2 = new GameObject[4];
    public GameObject[] skin3 = new GameObject[4];

    public LayerMask GroundLayer;

    protected GameObject targetPlayer;
    private Vector3 iniPos;
    private bool waiting = true;
    private bool falled = false;
    private float yVel = 0f;

    protected bool active = true;
    private bool attackedByPlayer;
    private byte NormalAttack = 0;

    protected PlayerInfo player;
    private uint Score = 1500;
    private byte combo = 0;
    
    [Header("弱点")]
    public WeaponTypes[] weaknesses;
    // Start is called before the first frame update
    void Start()
    {
        ShowSkin(0);
        iniPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (waiting) {
            Vector3 thisPos = this.transform.position;
            thisPos.y = 0;

            float minDistance = Mathf.Infinity;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players) {
                if (player.GetComponent<PlayerInfo>() != null && Vector3.Distance(player.transform.position, this.transform.position) < minDistance) {
                    minDistance = Vector3.Distance(player.transform.position, this.transform.position);
                    targetPlayer = player;
                }
            }

            Vector3 playerPos = targetPlayer.transform.position;
            playerPos.y = 0;

            if (Vector3.Distance(thisPos, playerPos) <= 15f) {
                waiting = false;
                StartCoroutine(Fall());
            }
        } else {
            if (yVel < 0) {
                if (yVel > -100) {
                    yVel -= 2.5f;
                }

                RaycastHit hit;
                Physics.Raycast(transform.position, -transform.up, out hit, 17f, GroundLayer);

                // Debug.Log(hit.distance);

                if (hit.distance <= 4f && hit.distance > 0) {
                    falled = true;
                    yVel = 0f;
                }
            }
        }

        Vector3 velocity = Vector3.up * yVel;
        this.GetComponent<Rigidbody>().linearVelocity = velocity;

        if (yVel > 0 && this.transform.position.y > iniPos.y) {
            this.transform.position = iniPos;
            yVel = 0f;

            waiting = true;
        }
    }

    void ShowSkin(int index) {
        for (int i = 0; i < 4; i++) {
            if (i < skin1.Length) {
                skin1[i].SetActive(index == 0);
            }
            if (i < skin2.Length) {
                skin2[i].SetActive(index == 1);
            }
            if (i < skin3.Length) {
                skin3[i].SetActive(index == 2);
            }
        }
    }

    IEnumerator Fall() {
        ShowSkin(1);
        yield return new WaitForSeconds(0.1f);
        ShowSkin(2);
        yield return new WaitForSeconds(0.1f);

        yVel = -2.5f;

        while (!falled) {
            yield return new WaitForSeconds(0f);
        }
        falled = false;

        yield return new WaitForSeconds(1.5f);

        ShowSkin(0);
        yVel = 7.5f;
    }

    void OnCollisionStay(Collision col){
        if (col.gameObject.tag == "Player" && active){
            player = col.gameObject.GetComponent<PlayerInfo>();
            if (player.invincible || player.giant) {
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
            StartCoroutine(DefeatedAnimation());
            active = false;

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
                player.scorePopUp((int)pts, false, this.transform.position, combo - 1);

                if (player.rateTime > 0) player.rateTime += 0.5f;
            }
        }
    }

    public IEnumerator DefeatedAnimation() {
        yield return null;
        Destroy(gameObject);
    }
}
