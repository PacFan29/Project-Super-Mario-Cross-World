using UnityEngine;

public class SMRChild : MonoBehaviour
{
    public GameObject gotEffect;
    public GameObject vanishEffect;
    void FixedUpdate()
    {
        this.transform.Rotate(0f, -3f, 0f, Space.Self);

        SilverMoonRing p = this.transform.parent.gameObject.GetComponent<SilverMoonRing>();
        if (p != null)
        {
            if (p.ringCount < 5)
            {
                if (p.timeLimit <= 3f)
                {
                    this.transform.GetChild(0).gameObject.SetActive(p.timeLimit % 0.1f >= 0.05f);
                } else
                {
                    this.transform.GetChild(0).gameObject.SetActive(p.timeLimit % 0.2f >= 0.1f);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerInfo player = other.GetComponent<PlayerInfo>();
            player.scorePopUp(3000, false, this.transform.position, 4);

            SilverMoonRing p = this.transform.parent.gameObject.GetComponent<SilverMoonRing>();
            if (p.ringCount == 1)
            {
                p.StartCoroutine("Perfect");
            }
            else
            {
                p.GotIt();
            }

            Instantiate(gotEffect, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void DestroyObject()
    {
        Instantiate(vanishEffect, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
