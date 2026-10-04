using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPadManager : MonoBehaviour
{
    public float speed = 10f;
    public bool controlLock = true;
    public bool isDashRing = false;
    public bool isRainbowRing = false;
    private bool point = true;
    private bool actionChainTrigger = true;

    [Header("アニメーター")]
    public Animator anim;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") {
            AudioSource sound = GetComponent<AudioSource>();
            sound.Play();
            if (actionChainTrigger) {
                GameManager.IncreaseActionChain();
                actionChainTrigger = false;
            }

            if (anim != null)
            {
                anim.Play("DashRingThrough", 0, 0);
            }

            other.transform.position = this.transform.position;

            PlayerInfo player = other.GetComponent<PlayerInfo>();
            if (controlLock) player.controlLockTimer = 0.1f;

            if (isDashRing && controlLock) {
                player.lockUntilGrounded = true;
            }

            player.VelocitySetUp(this.transform.forward * speed);

            if (isRainbowRing) {
                if (point) {
                    player.scorePopUp(1000, false, this.transform.position, 6);
                    point = false;
                }

                if (other.gameObject.GetComponent<_16MSonic>() != null) {
                    other.gameObject.GetComponent<_16MSonic>().trick = true;
                    other.gameObject.GetComponent<_16MSonic>().trickManager.trickPattern = 1;
                }
            }
        }
    }
}
