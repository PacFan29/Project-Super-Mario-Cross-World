using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellManager : ComboManager
{
    public Transform skin;
    public bool isMoving = false;
    private Vector3 velocity;
    private Rigidbody rb;
    float colliderRadius;
    private bool damageActive;
    private float gravity = 1.875f;

    //ボールが当たった物体の法線ベクトル
    private Vector3 objNomalVector = Vector3.zero;
    // 跳ね返った後のvelocity
    [HideInInspector] public Vector3 afterReflectVero = Vector3.zero;

    public LayerMask GroundLayer;
    [HideInInspector] public bool Grounded = true;
    private RaycastHit hit;
    private int stompCombo = 0;
    private int fireballCombo = 0;

    // private int reflectStep = 0;
    private float wallHitDistance = 0f;
    // private bool wallHit;
    private Vector3 reflectForward;

    [Header("エフェクト")]
    public GameObject hitEffect;
    [Header("1UPキノコ")]
    public GameObject item;
    
    private GrabableObject grabable;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliderRadius = GetComponent<SphereCollider>().radius;

        if (this.GetComponent<GrabableObject>() == null)
        {
            grabable = this.gameObject.AddComponent<GrabableObject>();
            grabable.grabMode = GrabableObject.GrabMode.Forward;
        }
        else
        {
            grabable = this.GetComponent<GrabableObject>();
        }
        grabable.enableRigidbody = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (grabable.grabableTrigger == 1)
        {
            return;
        }
        else if (grabable.grabableTrigger == 2)
        {
            isMoving = true;
        } else if (grabable.grabableTrigger == 3)
        {
            velocity = new Vector3(this.transform.forward.x * 5f, 0f, this.transform.forward.z * 5f);
            grabable.grabableTrigger = 0;
        } else if (grabable.grabableTrigger == 4)
        {
            float forwardSpeed = 0f;
            if (grabable.player != null)
            {
                forwardSpeed = player.XZmag / 4f;
            }
            velocity = new Vector3(0f, 70f, 0f);
            grabable.grabableTrigger = 0;
        }

        if (isMoving)
        {
            Vector3 speed = 45 * this.transform.forward;
            velocity.x = speed.x;
            velocity.z = speed.z;
            skin.Rotate(0f, 18.0f, 0f);
            grabable.grabableTrigger = 2;

            // if (wallHit)
            // {
            //     this.transform.forward = reflectForward;
            //     wallHit = false;
            // }
        }
        else
        {
            if (Grounded)
            {
                velocity.x = 0f;
                velocity.z = 0f;
            }
            combo = 0;
            damageActive = false;
            if (grabable.grabableTrigger == 2)
            {
                grabable.grabableTrigger = 0;
            }
        }

        Physics.BoxCast(transform.position, Vector3.one * 0.75f, -transform.up, out hit, Quaternion.identity, 1f, GroundLayer);
        if (Grounded) {
            if (hit.distance <= 0f || velocity.y > 0) {
                Grounded = false;
            } else {
                transform.position += Vector3.up * (0.6f - hit.distance);
            }
        } else {
            velocity.y -= gravity;

            if (hit.distance > 0 && velocity.y < 0) {
                fireballCombo = 0;
                Grounded = true;
                gravity = 1.875f;
                velocity.y = 0f;
            }
        }

        rb.linearVelocity = velocity;
        rb.position = transform.position;

        if (player != null && player.GetCombo() <= 0 && player.Grounded) {
            stompCombo = 0;
        }

        RaycastHit wHit;

        if (isMoving)
        {
            // if (Physics.SphereCast(this.transform.position, 1f, transform.forward, out wHit, 45f))
            // Debug.DrawRay(this.transform.position, this.transform.position + this.transform.forward * 45f, Color.green);
            if (Physics.Raycast(this.transform.position, this.transform.forward, out wHit, 5f, GroundLayer))
            {
                wallHitDistance = wHit.distance;
                objNomalVector = wHit.normal;

                if (objNomalVector.y < 0.1f)
                {
                    // wallHit = true;
                    Vector3 reflectVec = Vector3.Reflect(this.transform.forward, objNomalVector);
                    reflectForward = reflectVec.normalized;
                    afterReflectVero = this.transform.forward;

                    this.transform.forward = reflectForward;

                    Instantiate(hitEffect, wHit.point, Quaternion.identity);

                    if (wHit.collider.gameObject.GetComponent<QuestionBlockManager>() != null)
                    {
                        wHit.collider.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);
                    }
                }
            }
        }

        // if (reflectStep > 0) {
            //     reflectStep--;
            // }

            if (this.transform.position.y <= -100f)
            {
                Destroy(gameObject);
            }
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.tag == "Player" || col.gameObject.name == "InstaShield"){
            if (col.gameObject.name == "InstaShield") {
                //ダブル回転アタックの攻撃が当たった場合
                player = col.gameObject.GetComponent<InstaShield>().player;
            } else {
                player = col.gameObject.GetComponent<PlayerInfo>();
            }

            if (col.gameObject.name != "InstaShield" && player.Buttons["X"]) {
                return;
            }

            if (!isMoving)
            {
                if (Stomped(col))
                {
                    player.Stomp();
                    StompComboIncrease();
                }
                else
                {
                    if (player.rolling)
                    {
                        if (player.playerType != 0) player.Stomp();
                    }
                    else if (player.attacking)
                    {
                        player.Stomp();
                        StompComboIncrease();
                    }
                    Vector3 distance = player.transform.position - this.transform.position;
                    distance.y = 0;

                    this.transform.forward = -distance.normalized;
                }
                isMoving = true;
                afterReflectVero = this.transform.forward;
            }
            else if (isMoving && (Stomped(col) || player.attacking || (player.rolling && player.playerType != 0)))
            {
                player.Stomp();
                StompComboIncrease();
                grabable.grabableTrigger = 0;
                isMoving = false;
            }
            ComboReset();
        } else if (LayerMask.LayerToName(col.gameObject.layer) == "Enemy" && isMoving) {
            EnemyManager enemy = col.gameObject.GetComponent<EnemyManager>();
            enemy.TakeDamage(true, player, 3, 0, false, this);
        // } else if (LayerMask.LayerToName(col.gameObject.layer) == "Default") {
        //     //跳ね返る
        //     // 当たった物体の法線ベクトルを取得
        //     objNomalVector = col.contacts[0].normal;
        //     Debug.Log(objNomalVector);
        //     if (objNomalVector.y < 0.1f) {
        //         Vector3 reflectVec = Vector3.Reflect (afterReflectVero, objNomalVector);
        //         reflectVec.y = 0;
        //         this.transform.forward = reflectVec.normalized;
        //         // 計算した反射ベクトルを保存
        //         afterReflectVero = this.transform.forward;

        //         if (col.gameObject.GetComponent<QuestionBlockManager>() != null) {
        //             col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);
        //         }
        //     }
        // }
        }
    }
    void OnCollisionExit(Collision col) {
        if (col.gameObject.tag == "Player") {
            damageActive = true;
        }
    }
    void OnCollisionStay(Collision col){
        if (col.gameObject.tag == "Player" && (isMoving && !Stomped(col)) && damageActive) {
            player = col.gameObject.GetComponent<PlayerInfo>();
            if (!player.rolling && !player.attacking && !Stomped(col)) {
                player.TakeDamage(6, this.transform.position);
            }
        }
        // if (LayerMask.LayerToName(col.gameObject.layer) == "Default") {
        //     //跳ね返る
        //     // 当たった物体の法線ベクトルを取得
        //     objNomalVector = col.contacts[0].normal;
        //     Debug.Log(objNomalVector);

        //     RaycastHit hit;
        //     Vector3 XZvel = new Vector3(velocity.x, 0f, velocity.z);
        //     bool wallHit = Physics.Raycast(transform.position - XZvel.normalized * 3f, XZvel.normalized, out hit, 6f, 1 << 0);

        //     if (objNomalVector.y < 0.1f && reflectStep <= 0 && wallHit) {
        //         Vector3 reflectVec = Vector3.Reflect (afterReflectVero, objNomalVector);
        //         reflectVec.y = 0;
        //         this.transform.forward = reflectVec.normalized;
        //         // 計算した反射ベクトルを保存
        //         afterReflectVero = reflectVec.normalized;

        //         reflectStep = 2;

        //         if (col.gameObject.GetComponent<QuestionBlockManager>() != null) {
        //             col.gameObject.GetComponent<QuestionBlockManager>().BlockHit(player, false);
        //         }
        //     }
        // }
    }

    bool Stomped(Collision col) {
        float playerY = col.transform.position.y - 2f /*+ (colliderRadius * this.transform.localScale.y)*/;
        float thisY = this.transform.position.y;

        return player.Stompable && playerY >= thisY;
    }

    void StompComboIncrease() {
        if (stompCombo < 8) stompCombo++;
        if (stompCombo >= 8) player.OneUp();
    }

    public void Shot(PlayerInfo player) {
        fireballCombo++;
        grabable.grabableTrigger = 0;
        isMoving = false;

        velocity.y = 40f;
        gravity = 2.5f;

        if (fireballCombo >= 8) {
            GameObject itemObj = Instantiate(item, this.transform.position + (Vector3.up * 1.375f), this.transform.rotation);
            itemObj.GetComponent<ItemManager>().itemType = ItemType.Extend;
            itemObj.transform.Rotate(0, 180, 0);

            itemObj.GetComponent<ItemManager>().Split();

            player.scorePopUp(500, false, this.transform.position);
            Destroy(gameObject);
        }
    }
}
