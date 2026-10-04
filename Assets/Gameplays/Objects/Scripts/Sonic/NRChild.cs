using UnityEngine;

public class NRChild : MonoBehaviour
{
    [Header("インデックス")]
    public int index;
    [Header("エフェクト")]
    public GameObject gotEffect;
    public GameObject vanishEffect;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerInfo player = other.GetComponent<PlayerInfo>();

            NumberRing p = this.transform.parent.gameObject.GetComponent<NumberRing>();

            if (p.GotIt(index))
            {
                player.scorePopUp(3000, false, this.transform.position, 4);
                Instantiate(gotEffect, this.transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    public void DestroyObject()
    {
        Instantiate(vanishEffect, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
