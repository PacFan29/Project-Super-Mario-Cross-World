using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGimmick : MovingPlatformParent
{
    protected PlayerInfo player;
    protected bool gotOn = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gotOn && !player.Grounded) {
            player = null;
            gotOn = false;
        }
    }

    void OnCollisionStay(Collision other) {
        if (
            other.gameObject.GetComponent<PlayerInfo>() != null && 
            !gotOn && 
            other.gameObject.transform.position.y > this.transform.position.y
        ) {
            player = other.gameObject.GetComponent<PlayerInfo>();
            if (player.Grounded) {
                gotOn = true;
            }
        }
    }

    // void OnCollisionExit(Collision other) {
    //     if (other.gameObject.GetComponent<PlayerInfo>() != null && player != null) {
    //         player = null;
    //         gotOn = false;
    //     }
    // }
    public override void OnCollisionExit(Collision other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null && player != null) {
            player = null;
            gotOn = false;
            other.gameObject.transform.parent = null;
            playerList.Remove(other.gameObject);
        }
    }
}
