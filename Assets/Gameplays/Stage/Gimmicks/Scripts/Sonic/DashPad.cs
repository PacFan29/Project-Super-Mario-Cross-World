using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPad : MonoBehaviour
{
    private bool actionChainTrigger = true;
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && col.gameObject.GetComponent<PlayerInfo>().Grounded){
            this.GetComponent<AudioSource>().Play();

            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            player.ForwardSetUp(this.transform.forward, 100f, false, true);
            if (actionChainTrigger) {
                GameManager.IncreaseActionChain();
                actionChainTrigger = false;
            }
            //player.controlLockTimer = 0.5f;
        }
    }
}
