using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPipe : MonoBehaviour
{
    private WarpPipe targetPipe;
    public bool changeMusic = false;
    public int enterIndex = -1;
    public int exitIndex = -1;

    private PlayerInfo player;
    // Start is called before the first frame update
    void Start()
    {
        WarpPipe[] allPipes = FindObjectsByType<WarpPipe>(FindObjectsSortMode.None);

        foreach (WarpPipe pipe in allPipes) {
            if (pipe.exitIndex == enterIndex) {
                targetPipe = pipe;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if (Physics.SphereCast(this.transform.position + Vector3.up * 1f, 1.25f, Vector3.up, out hit, 3f))
        {
            if (hit.collider.gameObject.GetComponent<PlayerInfo>() != null)
            {
                player = hit.collider.gameObject.GetComponent<PlayerInfo>();
            }
        } else
        {
            player = null;
        }

        if (player != null && hit.collider != null)
        {
            if (player.Crouching && player.activeCollision && enterIndex != -1 && player.Grounded) {
                if (targetPipe.changeMusic || (changeMusic && !targetPipe.changeMusic)) {
                    MusicManager.musicFade = true;
                }
                StartCoroutine(player.PipeEnter(this.transform.position, enterIndex));
            }
        }
    }

    // void OnCollisionStay(Collision col) {
    //     if (col.gameObject.tag == "Player") {
    //         PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();

    //         if (Input.GetButtonDown("RB") && player.activeCollision && enterIndex != -1) {
    //             if (targetPipe.changeMusic || (changeMusic && !targetPipe.changeMusic)) {
    //                 MusicManager.musicFade = true;
    //             }
    //             StartCoroutine(player.PipeEnter(this.transform.position, enterIndex));
    //         }
    //     }
    // }
}
