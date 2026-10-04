using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineManager : GrabableObject
{
    [Header("効果音")]
    private AudioSource audioS;
    public AudioClip bounceSound;
    public AudioClip bounceHighSound;
    [Header("アニメーション")]
    public Animator anim;
    // Start is called before the first frame update
    void Awake()
    {
        audioS = GetComponent<AudioSource>();
    }

    public override void OverrideUpdate()
    {
        anim.SetBool("Grounded", true);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && col.contacts[0].normal == Vector3.down){
            float force = 40f;
            if (Input.GetButton("A")) {
                force = 80f;
                audioS.PlayOneShot(bounceHighSound);
                anim.Play("ReactionHigh", 0, 0);
            } else {
                audioS.PlayOneShot(bounceSound);
                anim.Play("Reaction", 0, 0);
            }
            col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(force, transform.up);
        }
    }

    public override void ResetTrigger()
    {
        grabableTrigger = 0;
    }
}
