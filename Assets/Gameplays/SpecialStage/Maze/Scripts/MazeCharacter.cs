using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCharacter : MonoBehaviour
{
    protected Rigidbody rb;
    protected const float tilePixel = 6f;
    protected Vector2 direction = Vector2.left;
    protected Vector3 velocity;
    protected float speed = 22.5f;
    protected Vector2[] dirs = {Vector2.up, Vector2.left, Vector2.down, Vector2.right};

    protected MazeManager manager;

    protected Vector3 inititalPosition;
    protected Vector2 initialDir;
    protected float keepY;
    protected Vector3 keepYPos;

    protected float jumpVel = 0f;
    protected bool Grounded = true;

    protected float yVelRate = 1f;

    [Header("スキン")]
    public Transform skin;
    [Header("アニメーション")]
    public Animator playerAnim;

    [HideInInspector] public bool stop = false;
    [HideInInspector] public bool hide = false;

    protected int actionId = 0;

    protected bool stay = false;
    protected bool rolling = false;
    // Start is called before the first frame update
    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        manager = FindAnyObjectByType<MazeManager>();
        keepY = this.transform.position.y;
        jumpVel = 0f;

        inititalPosition = this.transform.position;
        initialDir = direction;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (SpecialStageManager.ready && !SpecialStageManager.cleared && !stay) {
            velocity = new Vector3(direction.x, 0f, direction.y) * speed;
            velocity.y = jumpVel;

            if (!Grounded && actionId != -2)
            {
                jumpVel -= 1.875f * yVelRate;
                if (this.transform.position.y <= keepY && jumpVel < 0f)
                {
                    this.transform.position = new Vector3(this.transform.position.x, keepY, this.transform.position.z);
                    rb.position = this.transform.position;
                    jumpVel = 0f;
                    Grounded = true;

                    if (this.GetComponent<MazePlayer>() != null)
                    {
                        this.GetComponent<MazePlayer>().SoundPlay(this.GetComponent<MazePlayer>().landSound);
                    }
                }
            }
            if (Grounded && this.transform.position.y < keepY)
            {
                this.transform.position = new Vector3(this.transform.position.x, keepY, this.transform.position.z);
                rb.position = this.transform.position;
            }
        } else {
            velocity = Vector3.zero;
        }
        keepYPos = new Vector3(this.transform.position.x, keepY, this.transform.position.z);
        if (velocity != Vector3.zero) {
            skin.forward = new Vector3(velocity.x, 0f, velocity.z).normalized;
            RaycastHit hitInfo;
            Physics.Raycast(keepYPos, skin.forward, out hitInfo, tilePixel, getLayer());

            if ((hitInfo.distance - 3f) <= 0f && hitInfo.distance > 0f && this.GetComponent<CapsuleCollider>().enabled) {
                rb.position += skin.forward * (hitInfo.distance - 3);
                velocity = Vector3.zero;
                velocity.y = jumpVel;
            }
        }
        if (stop)
        {
            rb.linearVelocity = Vector3.zero;
        } else
        {
            rb.linearVelocity = velocity;
        }

        if (playerAnim != null) {
            playerAnim.SetFloat("Speed", velocity.magnitude);
            playerAnim.SetBool("Crouching", false);
            playerAnim.SetBool("Rolling", rolling);
            playerAnim.SetBool("Skidding", false);

            playerAnim.SetBool("Grounded", Grounded);
            playerAnim.SetFloat("Y velocity", jumpVel);

            playerAnim.SetInteger("Actions", actionId);
        }

        skin.gameObject.SetActive(!hide);
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(keepYPos + new Vector3(1, 0, 0) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(keepYPos + new Vector3(-1, 0, 0) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(keepYPos + new Vector3(0, 0, 1) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(keepYPos + new Vector3(0, 0, -1) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
    }
    public bool canRotate(Vector2 direction)
    {
        RaycastHit hitInfo;
        Vector3 dir = new Vector3(direction.x, 0f, direction.y);
        Physics.BoxCast(keepYPos, new Vector3(tilePixel * 0.375f, 1f, tilePixel * 0.375f), dir, out hitInfo, Quaternion.identity, tilePixel * 0.75f, getLayer());
        // Debug.Log(hitInfo.collider != null);
        return hitInfo.collider == null;
    }
    public void gridPos() {
        Vector3 pos = this.transform.position;
        pos.x = (float)Math.Round(pos.x / tilePixel, 0);
        pos.z = (float)Math.Round(pos.z / tilePixel, 0);

        pos.x *= tilePixel;
        pos.z *= tilePixel;

        rb.position = pos;
    }

    int getLayer() {
        return 1 << 0;
    }

    public void Initialize()
    {
        actionId = 0;
        stop = false;
        this.transform.position = inititalPosition;
        rb.position = inititalPosition;
        direction = initialDir;
        jumpVel = 0f;
        Grounded = true;
        hide = false;
    }
}
