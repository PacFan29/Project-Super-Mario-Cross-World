using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabableObject : MonoBehaviour
{
    public enum GrabMode
    {
        Forward,
        Upward
    }
    private Vector3 velocity;
    private Rigidbody rb;
    private float gravity = 1.875f;
    public GrabMode grabMode;
    [HideInInspector] public bool enableRigidbody = true;

    public int grabableTrigger = 0; //0＝持ち上げ可能、1＝持ち上げ中、2＝持ち上げ不可能、3＝置く、4＝上に投げる
    public bool grabable = true;
    public PlayerInfo player;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grabableTrigger == 1)
        {
            if (grabMode == GrabMode.Upward)
            {
                this.transform.position = player.gameObject.transform.position + player.gameObject.transform.up * 4f;
                rb.position = this.transform.position;
            }
            else
            {
                this.transform.position = player.gameObject.transform.position + player.skin.forward * 3.5f;
                rb.position = this.transform.position;
            }
            this.transform.rotation = player.skin.rotation;
            rb.rotation = this.transform.rotation;

            if (!player.Buttons["X"] || !player.grabbing)
            {
                player.grabbing = false;
                if (player.Crouching)
                {
                    grabableTrigger = 3;
                    Placed();
                    return;
                }
                if (player.Buttons["LB"])
                {
                    grabableTrigger = 4;
                    KickedUpward();
                    return;
                }
                grabableTrigger = 2;
                Thrown();
            }
        }

        OverrideUpdate();
    }
    public virtual void OverrideUpdate()
    {
        ;
    }

    void FixedUpdate()
    {
        if (enableRigidbody && grabableTrigger != 1)
        {
            rb.AddForce(-Vector3.up * gravity, ForceMode.VelocityChange);
        }
    }

    void OnCollisionStay(Collision col){
        if (col.gameObject.tag == "Player")
        {
            player = col.gameObject.GetComponent<PlayerInfo>();

            if (!player.Crouching && !player.rolling && !player.attacking && grabableTrigger == 0 && player.Buttons["X"] && !player.grabbing && grabable)
            {
                //持ち上げ
                grabableTrigger = 1;
                player.grabbing = true;
            }
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            if (player.grabbing)
            {
                player.grabbing = false;
            }
        }
    }

    public void Placed()
    {
        ResetTrigger();

        //置く
        this.transform.position = player.gameObject.transform.position + player.skin.forward * 4f;
        rb.position = this.transform.position;
        rb.linearVelocity = new Vector3(player.skin.forward.x * 5f, 0f, player.skin.forward.z * 5f);
    }
    public void Thrown()
    {
        ResetTrigger();

        //投げる
        rb.linearVelocity = new Vector3(player.skin.forward.x * 45f, 0f, player.skin.forward.z * 45f);
    }
    public void KickedUpward()
    {
        ResetTrigger();

        //上に投げる
        float forwardSpeed = 0f;
        if (player != null)
        {
            forwardSpeed = player.XZmag / 4f;
        }
        rb.linearVelocity = new Vector3(player.skin.forward.x * forwardSpeed, 70f, player.skin.forward.z * forwardSpeed);
    }

    public virtual void ResetTrigger()
    {
        ;
    }
}
