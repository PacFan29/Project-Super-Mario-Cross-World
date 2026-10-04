using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsLine : MonoBehaviour
{
    [Header("内容")]
    public int seriesIndex;
    public string textDetail = "Sample";
    public bool isHeader = false;
    [Header("レンガプレハブ")]
    public GameObject brick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetUp()
    {
        int len = textDetail.Length;
        float startXPos = (len - 1) * 0.5f * -2.75f;

        for (int i = 0; i < len; i++)
        {
            if (textDetail.Substring(i, 1) != " ")
            {
                BrickManagerCredits b = Instantiate(brick, Vector3.zero, Quaternion.identity, this.transform).GetComponent<BrickManagerCredits>();
                b.gameObject.transform.localPosition = new Vector3(startXPos + 2.75f * i, 0, 0);
                b.textDetail = textDetail.Substring(i, 1);
                b.isHeader = isHeader;
                b.seriesIndex = seriesIndex;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y > 85f)
        {
            Destroy(gameObject);
        }
    }
}
