using UnityEngine;
using TMPro;

public class BrickManagerCredits : BrickManager
{
    [Header("クレジット限定テキスト")]
    public GameObject textGroup;
    public GameObject[] headers = new GameObject[4];
    public GameObject[] details = new GameObject[4];
    public string textDetail = "A";
    public bool isHeader;
    public int seriesIndex;

    void FixedUpdate()
    {
        textGroup.SetActive(!breaked);

        TextMeshPro txt = null;

        //ヘッダー
        foreach (GameObject header in headers)
        {
            header.SetActive(false);
        }
        if (isHeader)
        {
            headers[seriesIndex].SetActive(true);
            txt = headers[seriesIndex].GetComponent<TextMeshPro>();
        }

        //明細
        foreach (GameObject detail in details)
        {
            detail.SetActive(false);
        }
        if (!isHeader)
        {
            details[seriesIndex].SetActive(true);
            txt = details[seriesIndex].GetComponent<TextMeshPro>();
        }

        if (txt != null)
        {
            txt.text = textDetail;
        }

        if (isEmpty && currentPlayer != null && !breaked)
        {
            breaked = true;
            StartCoroutine("DestroyBlock", currentPlayer);
        }
    }
}
