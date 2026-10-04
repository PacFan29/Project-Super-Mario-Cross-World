using UnityEngine;

public class Bumper : MonoBehaviour
{
    private int hitCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitCount = 0;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            Vector3 angle = (col.gameObject.transform.position - this.transform.position).normalized;

            PlayerInfo player = col.gameObject.GetComponent<PlayerInfo>();
            player.VelocitySetUp(35f * angle);

            this.GetComponent<AudioSource>().Play();

            if (hitCount < 10) {
                hitCount++;
                player.scorePopUp(100, false, this.transform.position);
            }
        }
    }
}
