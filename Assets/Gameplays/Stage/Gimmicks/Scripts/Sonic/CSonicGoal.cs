using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSonicGoal : GoalManager
{
    [Header("当たり判定")]
    // public BoxCollider beforeGoal;
    // public CapsuleCollider bonusCollider;
    private int step = 0;
    private uint bonus = 1000;
    private GameObject playerObj;
    [Header("効果音")]
    public AudioClip goalSound;
    public AudioClip bonusSound;
    public AudioClip landSound;

    [Header("リジッドボディ")]
    public LayerMask GroundLayer;
    private Vector3 velocity;
    [Header("本体")]
    public Transform goalPost;
    private float rotationSpeed = 4f;
    [Header("エフェクト")]
    public ParticleSystem p1;
    public ParticleSystem p2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    // void Update()
    // {
    //     beforeGoal.enabled = (step == 0);
    //     bonusCollider.enabled = (step == 1);
    // }
    void FixedUpdate() {
        if (step == 1) {
            velocity.y -= 0.25f;

            RaycastHit hit;
            Physics.Raycast(transform.position, -Vector3.up, out hit, 3f, GroundLayer);

            if (hit.distance > 0 && velocity.y < 0) {
                step = 2;
                velocity = Vector3.zero;
                this.transform.position += Vector3.up * (2.83f - hit.distance);
                SoundPlay(landSound);

                var p1e = p1.emission;
                var p2e = p2.emission;
                p1e.rateOverTime = 0;
                p2e.rateOverTime = 0;

                cleared(playerObj, 3);
            }

            this.GetComponent<Rigidbody>().linearVelocity = velocity;
        }

        if (step > 0 && step < 3)
        {
            if ((goalPost.eulerAngles.y % 360) - rotationSpeed <= 0f && rotationSpeed <= 4f)
            {
                goalPost.rotation = Quaternion.Euler(0f, 0f, 0f);
                step = 3;
                return;
            }
            goalPost.Rotate(0f, -rotationSpeed, 0f);

            if (step == 2 && rotationSpeed > 4f)
            {
                rotationSpeed -= 0.5f;
                if (rotationSpeed < 4f)
                {
                    rotationSpeed = 4f;
                }
            }
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            GameManager.timeIncrease = false;
            
            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            DestroyAllEnemies(player);
            playerObj = col.gameObject;
            switch (step) {
                case 0:
                SoundPlay(goalSound);
                if (player.XZmag >= 20f) {
                    step = 1;
                    velocity.y = player.XZmag * 0.3f;
                    p1.gameObject.SetActive(true);
                    rotationSpeed = player.XZmag / 2.5f;
                } else {
                    step = 2;
                    cleared(playerObj, 3);
                }
                break;

                case 1:
                if (!player.Grounded && player.finalVelocity.y > 0) {
                    player.scorePopUp((int)bonus, false, this.transform.position);
                    if (bonus > 0) bonus -= 100;
                    SoundPlay(bonusSound);
                    velocity.y = 10f;
                }
                break;
            }
        }
    }
}
