using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringManager : MonoBehaviour
{
    private AudioSource audioS;
    public float force;
    public bool lockControl;
    public AudioClip springSound;
    private bool actionChainTrigger = true;
    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player"){
            if (actionChainTrigger) {
                GameManager.IncreaseActionChain();
                actionChainTrigger = false;
            }
            audioS.PlayOneShot(springSound);
            col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(force, transform.up, lockControl);
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player"){
            if (actionChainTrigger) {
                GameManager.IncreaseActionChain();
                actionChainTrigger = false;
            }
            audioS.PlayOneShot(springSound);
            col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(force, transform.up, lockControl);
        }
    }
}
