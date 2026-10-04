using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("ＢＧＭ")]
    public AudioClip music;
	public float loopBegin = 0f;
	public float loopEnd = 999999f;

    [Header("コインの切り替え")]
    public GameData data;
    private Character previous;
    public Character collectables;

    private MusicManager manager;
    private Transform playerTrans;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindAnyObjectByType<MusicManager>();
        previous = data.character;
        playerTrans = null;

        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    void Update()
    {
        // float minX = this.transform.position.x - this.transform.localScale.x * 0.5f - 2f;
        // float maxX = this.transform.position.x + this.transform.localScale.x * 0.5f + 2f;
        // float minY = this.transform.position.y - this.transform.localScale.y * 0.5f - 2f;
        // float maxY = this.transform.position.y + this.transform.localScale.y * 0.5f + 2f;
        // float minZ = this.transform.position.z - this.transform.localScale.z * 0.5f - 2f;
        // float maxZ = this.transform.position.z + this.transform.localScale.z * 0.5f + 2f;

        if (playerTrans != null)
        {
            // if (
            //     playerTrans.position.x < minX || 
            //     playerTrans.position.x > maxX || 
            //     playerTrans.position.y < minY || 
            //     playerTrans.position.y > maxY || 
            //     playerTrans.position.z < minZ || 
            //     playerTrans.position.z > maxZ
            // )
            if (!playerTrans.gameObject.activeInHierarchy)
            {
                Debug.Log("Out");
                PlayerInfo player = playerTrans.gameObject.GetComponent<PlayerInfo>();
                playerTrans = null;

                if (player.playerNumber == 0 && player.primary) {
                    if (music != null)
                    {
                        manager.ReturnMusic();
                    }
                    data.character = previous;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();
            playerTrans = player.gameObject.transform;

            if (player.playerNumber == 0 && player.primary) {
                if (music != null)
                {
                    manager.ChangeMusic(music, loopBegin, loopEnd);
                }
                if (!GameManager.sonicStage)
                {
                    data.character = collectables;
                }
            }
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();
            playerTrans = null;

            if (player.playerNumber == 0 && player.primary) {
                if (music != null)
                {
                    manager.ReturnMusic();
                }
                data.character = previous;
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<WarpPipe>() != null) {
            other.gameObject.GetComponent<WarpPipe>().changeMusic = true;
        }
    }
}
