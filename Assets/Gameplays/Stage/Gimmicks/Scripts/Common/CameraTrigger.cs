using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    private CameraManager cam;
    public Quaternion forceRotation = Quaternion.Euler(20f, 0f, 0f);
    public Vector3 forceOffset = new Vector3(0, 20f, -50f);
    // Start is called before the first frame update
    void Start()
    {
        cam = FindAnyObjectByType<CameraManager>();
        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();

            if (player.playerNumber == 0 && player.primary) {
                cam.currentForceRotation = forceRotation;
                cam.currentForceOffset = forceOffset;
                cam.enableForceRotation = true;
            }
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();

            if (player.playerNumber == 0 && player.primary) {
                cam.enableForceRotation = false;
            }
        }
    }
}
