using UnityEngine;

public class BDoing : MonoBehaviour
{
    public enum BDoingType
    {
        Normal,
        Super,
        Arrow
    }
    private AudioSource audioS;

    public BDoingType bDoingType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioS = GetComponent<AudioSource>();
    }
    
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && col.gameObject.GetComponent<PlayerInfo>() != null){
            Debug.Log(col.gameObject.GetComponent<PlayerInfo>().groundAttack);
            if (col.gameObject.GetComponent<PlayerInfo>().groundAttack)
            {
                switch (bDoingType)
                {
                    case BDoingType.Normal:
                        col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(80f, transform.up);
                        break;
                    
                    case BDoingType.Super:
                        col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(120f, transform.up);
                        break;
                    
                    case BDoingType.Arrow:
                        col.gameObject.GetComponent<PlayerInfo>().JumpFromSprings(80f, Quaternion.Euler(new Vector3(-30f, 0f, 0f)) * this.transform.parent.forward, true);
                        break;
                }
                audioS.Play();
            }
        }
    }
}
