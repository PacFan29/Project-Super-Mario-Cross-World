using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedMeasure : MonoBehaviour
{
    [Header("グループ")]
    public RectTransform group;
    [Header("値")]
    public Text speedValue;
    [Header("効果音")]
    public GameObject measureSound;
    public AudioClip speedShow;
    public AudioClip speedResult;

    private int step = 0;
    private bool toggle = false;

    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        group.localPosition = Vector3.right * 250f;
        source = this.GetComponent<AudioSource>();

        measureSound.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        string randFormat = "0000";
        if (step < 4) {
            if (step == 1)
            {
                randFormat = "000";
            } else if (step == 2)
            {
                randFormat = "00";
            } else if (step == 3)
            {
                randFormat = "0";
            }
            speedValue.text = (UnityEngine.Random.Range((int)Math.Pow(10, (3 - step)), (int)Math.Pow(10, (4 - step)))).ToString(randFormat);
        } else {
            speedValue.text = "";
        }

        string spdFormat = "###0";
        if (step > 0) {
            if (GameManager.currentSpeed >= (int)Math.Pow(10, step) && step < 4)
            {
                spdFormat = "";
                for (int i = 0; i < step; i++)
                {
                    spdFormat += "0";
                }
            }
            speedValue.text += (Math.Round(GameManager.currentSpeed) % (int)Math.Pow(10, step)).ToString(spdFormat);
        }
    }
    void Update()
    {
        if (toggle) {
            group.localPosition = Vector3.Lerp(group.localPosition, Vector3.zero, 0.2f);
        } else {
            group.localPosition = Vector3.Lerp(group.localPosition, Vector3.right * 250f, 0.2f);
        }
    }

    public IEnumerator Begin() {
        toggle = true;
        step = 0;
        measureSound.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        measureSound.SetActive(false);

        while (step < 3) {
            step++;
            source.PlayOneShot(speedShow);
            yield return new WaitForSeconds(0.3f);
        }
        step = 4;
        source.PlayOneShot(speedResult);
        yield return new WaitForSeconds(1.0f);
        toggle = false;
    }
}
