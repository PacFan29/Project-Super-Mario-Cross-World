using UnityEngine;

public class Conveyor : MovingPlatformParent
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject p in playerList)
        {
            if (p.GetComponent<PlayerInfo>() != null)
            {
                p.GetComponent<PlayerInfo>().conveyorVelocity = transform.forward * speed;
            }
        }
    }

    public override void OnCollisionExit(Collision other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null) {
            other.gameObject.transform.parent = null;
            other.gameObject.GetComponent<PlayerInfo>().conveyorVelocity = Vector3.zero;
            playerList.Remove(other.gameObject);
        }
    }
}
