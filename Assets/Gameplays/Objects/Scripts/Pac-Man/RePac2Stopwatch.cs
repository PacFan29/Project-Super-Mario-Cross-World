using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RePac2Stopwatch : MonoBehaviour
{
    [Header("短縮秒数（インデックス：0～4）")]
    public int reduceTimeIndex = 0;
    [Header("エフェクト")]
    public GameObject skinGroup;
    public GameObject[] skins;
    public GameObject effect;

    private bool earned = false;

    void OnEnable()
    {
        if (GameManager.courseMode != CourseMode.TimeAttack) {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject s in skins) {
            s.SetActive(false);
        }
        skins[reduceTimeIndex].SetActive(true);
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && !earned){
            GameManager.stopwatches.Add(reduceTimeIndex);
            GameManager.stopwatches.Sort();

            StartCoroutine("GotIt");
            this.GetComponent<AudioSource>().Play();
            earned = true;
        }
    }

    IEnumerator GotIt() {
        skinGroup.SetActive(false);
        effect.SetActive(true);

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
