using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberRing : MonoBehaviour
{
    public int currentIndex = 0;
    [Header("効果音")]
    public AudioClip got5Sound;
    public AudioClip got4Sound;
    public AudioClip got3Sound;
    public AudioClip got2Sound;
    public AudioClip perfectSound;
    public AudioClip failSound;

    private AudioSource source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        source = this.GetComponent<AudioSource>();
    }

    public bool GotIt(int index)
    {
        if (currentIndex == index)
        {
            switch (currentIndex)
            {
                case 0:
                    //５獲得
                    source.PlayOneShot(got5Sound);
                    break;

                case 1:
                    //４獲得
                    source.PlayOneShot(got4Sound);
                    break;

                case 2:
                    //３獲得
                    source.PlayOneShot(got3Sound);
                    break;

                case 3:
                    //２獲得
                    source.PlayOneShot(got2Sound);
                    break;

                case 4:
                    //１獲得
                    source.PlayOneShot(perfectSound);
                    StartCoroutine("Perfect");
                    break;
            }
            currentIndex++;
            return true;
        }
        else
        {
            StartCoroutine("Fail");
            return false;
        }
    }
    public IEnumerator Perfect()
    {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
    public IEnumerator Fail()
    {
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.GetComponent<NRChild>() != null)
            {
                child.gameObject.GetComponent<NRChild>().DestroyObject();
            }
        }
        source.PlayOneShot(failSound);
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
}
