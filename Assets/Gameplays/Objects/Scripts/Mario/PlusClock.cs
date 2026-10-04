using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlusClock : MonoBehaviour
{
    [Header("データ")]
    public Options options;
    [Header("延長秒数")]
    public int extendTime = 10;
    [Header("エフェクト")]
    public GameObject skin;
    public GameObject effect;
    public GameObject popUp;

    private bool earned = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        skin.transform.Rotate(0f, -3f, 0f, Space.Self);
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player" && !earned){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            // if (options.hardMode) {
            // if (options.difficulty == Options.Difficulty.Hard) {
            bool hud = FindAnyObjectByType<HUDManager>() != null;
            bool timeDecrease = false;
            if (hud) {
                timeDecrease = FindAnyObjectByType<HUDManager>().TimeType;
            }
            if (hud && timeDecrease) {
                GameManager.TotalTime += extendTime * 1000;
            } else {
                GameManager.TotalTime -= extendTime * 1000;
            }
            GameObject p = Instantiate(popUp, this.transform.position, Quaternion.identity);
            // p.GetComponent<ScoringController>().points = extendTime * (options.hardMode ? 1 : -1);
            p.GetComponent<ScoringController>().points = extendTime * (timeDecrease ? 1 : -1);

            StartCoroutine("GotIt");
            this.GetComponent<AudioSource>().Play();
            earned = true;
        }
    }

    IEnumerator GotIt() {
        skin.SetActive(false);
        effect.SetActive(true);

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
