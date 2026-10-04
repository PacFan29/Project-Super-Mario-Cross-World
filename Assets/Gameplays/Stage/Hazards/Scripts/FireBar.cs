using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBar : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public byte fireLength = 3;
    public bool bothSide = false;
    [Header("オブジェクト")]
    public RectTransform group;
    [Header("エフェクト")]
    public GameObject brickBreak;
    [Header("コイン")]
    public GameObject coinSpawner;

    private bool breaked = false;
    
    void Start() {
        GameObject fireball = group.GetChild(0).gameObject;

        if (bothSide) {
            Instantiate(fireball, this.transform.position - transform.right * 3.5f, this.transform.rotation, group);
        }
        for (byte i = 2; i <= fireLength; i++) {
            Instantiate(fireball, this.transform.position + transform.right * 3.5f * i, this.transform.rotation, group);
            if (bothSide) {
                Instantiate(fireball, this.transform.position - transform.right * 3.5f * i, this.transform.rotation, group);
            }
        }
    }
    void FixedUpdate()
    {
        group.Rotate(0f, rotationSpeed / 5f, 0f, Space.Self);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<PlayerInfo>() != null)
        {
            if (col.gameObject.GetComponent<PlayerInfo>().giant && !breaked)
            {
                this.GetComponent<AudioSource>().Play();

                this.GetComponent<BoxCollider>().isTrigger = true;
                this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                this.gameObject.tag = "Other";

                int coins = 5 * col.gameObject.GetComponent<PlayerInfo>().rate;
                Instantiate(brickBreak, this.transform.position, Quaternion.identity);
                CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                c.SetUp(coins, col.gameObject.GetComponent<PlayerInfo>());

                col.gameObject.GetComponent<PlayerInfo>().scorePopUp(1000, false, this.transform.position);

                breaked = true;

                foreach (Transform child in this.transform)
                {
                    Destroy(child.gameObject);
                }

                StartCoroutine("DestroyThis");
            }
        }
    }

    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(1.1f);
        Destroy(gameObject);
    }
}
