using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingPlatformParent : MonoBehaviour
{
    protected List<GameObject> playerList = new List<GameObject>();

    public virtual void OnCollisionEnter(Collision other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            other.gameObject.transform.parent = transform;
            playerList.Add(other.gameObject);
        }
    }
    public virtual void OnCollisionExit(Collision other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            other.gameObject.transform.parent = null;
            playerList.Remove(other.gameObject);
        }
    }
}
