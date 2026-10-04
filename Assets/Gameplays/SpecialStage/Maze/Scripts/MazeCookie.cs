using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCookie : MonoBehaviour
{
    public GameObject skin;
    public bool enable = true;
    public bool power = false;
    
    void OnTriggerEnter(Collider other) {
        if (enable && other.gameObject.GetComponent<MazePlayer>() != null) {
            if (power) {
                other.gameObject.GetComponent<MazePlayer>().scorePopUp(1000, this.transform.position);
                other.gameObject.GetComponent<MazePlayer>().PowerUp();
            } else
            {
                SpecialStageManager.Score += 50;
                SpecialStageManager.Coins++;
            }
            FindAnyObjectByType<MazeManager>().clearedPlayerNo = (byte)other.gameObject.GetComponent<MazePlayer>().playerNumber;

            enable = false;
            skin.SetActive(false);
            this.GetComponent<AudioSource>().Play();
            StartCoroutine("GotIt");
        }
    }

    IEnumerator GotIt() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
