using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilverMoonRing : MonoBehaviour
{
    public float timeLimit = 10f;
    public int ringCount = 5;
    private bool started = false;

    [Header("効果音")]
    public AudioSource mainSource;
    public AudioClip gotSound;
    public AudioClip perfectSound;
    public AudioSource timeSource;
    public AudioClip timeUpSound;

    void Start()
    {
        ;
    }

    // Update is called once per frame
    void Update()
    {
        ringCount = this.transform.childCount;

        if (ringCount < 5 && ringCount > 0 && timeLimit > 0f)
        {
            timeLimit -= Time.deltaTime;

            if (timeLimit <= 0f) {
                StartCoroutine("TimeUp");
            }
        }
    }

    public void GotIt()
    {
        if (!started)
        {
            timeSource.Play();
            started = true;
        }
        mainSource.PlayOneShot(gotSound);
    }
    public IEnumerator Perfect()
    {
        timeSource.Stop();
        mainSource.PlayOneShot(perfectSound);
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
    public IEnumerator TimeUp()
    {
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.GetComponent<SMRChild>() != null)
            {
                child.gameObject.GetComponent<SMRChild>().DestroyObject();
            }
        }
        timeSource.Stop();
        mainSource.PlayOneShot(timeUpSound);
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
}
