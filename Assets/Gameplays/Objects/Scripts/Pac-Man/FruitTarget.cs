using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitTarget : MonoBehaviour
{
    public GameObject sprite;
    public GameObject effect;
    [Header("インデックス")]
    public byte index = 0;
    [Header("得点")]
    public uint score = 100;
    private bool earned = false;

    void FixedUpdate() {
        sprite.transform.Rotate(0f, -3f, 0f, Space.Self);
    }
    
    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && col.gameObject.GetComponent<PlayerInfo>() != null && !earned){
            int combo = 0; //Good
            if (index >= 2) {combo++;} //Great
            if (index >= 4) {combo++;} //Super
            if (index >= 6) {combo++;} //Fantastic
            if (index >= 8) {combo++;} //Excellent

            col.gameObject.GetComponent<PlayerInfo>().scorePopUp((int)score, false, this.transform.position, combo);
            this.GetComponent<AudioSource>().Play();
            StartCoroutine("GotIt");

            GameManager.fruits[index]++;
            earned = true;
        }
    }

    IEnumerator GotIt() {
        sprite.SetActive(false);
        effect.SetActive(true);

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }
}
