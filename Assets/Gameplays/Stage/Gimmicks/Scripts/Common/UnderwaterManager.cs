using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterManager : MonoBehaviour
{
    public GameObject splashEffect;
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            PlayerInfo player = other.GetComponent<PlayerInfo>();
            player.underwaterButLava = false;
            player.underwaterTrigger = true;
        }
        if (other.GetComponent<CameraManager>() != null) {
            other.GetComponent<CameraManager>().underwater = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerInfo player = other.GetComponent<PlayerInfo>();
            player.underwaterTrigger = false;
        }
        if (other.GetComponent<CameraManager>() != null) {
            other.GetComponent<CameraManager>().underwater = false;
        }
    }
}
