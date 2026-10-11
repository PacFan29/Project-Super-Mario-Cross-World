using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Disclaimer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("StartScene");
    }

    IEnumerator StartScene()
    {
        yield return new WaitForSeconds(5f);

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene("InspiredBy");
    }
}
