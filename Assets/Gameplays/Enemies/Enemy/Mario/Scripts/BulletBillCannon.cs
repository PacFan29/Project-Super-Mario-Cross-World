using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBillCannon : MonoBehaviour
{
    public GameObject bill;
    public GameObject shootEffect;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Loop");
    }

    // Update is called once per frame
    void Update()
    {
        ;
    }

    IEnumerator Loop() {
        while (true) {
            yield return new WaitForSeconds(4f);

            Vector3 pos = this.transform.position + (this.transform.forward * 4f) + (transform.up * 1.75f);
            shootEffect.SetActive(true);
            Instantiate(bill, pos, this.transform.rotation);
        }
    }
}
