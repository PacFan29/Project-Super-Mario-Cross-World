using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenDemon : MonoBehaviour
{
    public GameObject skin;
    private Transform targetPlayer;
    private int step = 0;
    private float time = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        skin.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (skin == null) {
            Destroy(gameObject);
        }

        skin.GetComponent<ItemManager>().floating = true;

        if (step == 1) {
            if (time > 0) {
                time -= Time.deltaTime;
            } else {
                skin.GetComponent<SphereCollider>().enabled = true;

                Vector3 direction = targetPlayer.transform.position - this.transform.position;
                direction = direction.normalized;

                this.transform.position += direction * 25f * Time.deltaTime;

                Vector3 skinDir = direction;
                skinDir.y = 0;
                skin.transform.forward = skinDir;
            }
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && step == 0){
            targetPlayer = col.transform;
            step = 1;
            skin.SetActive(true);
        }
    }
}
