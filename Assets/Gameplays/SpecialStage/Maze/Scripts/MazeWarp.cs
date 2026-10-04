using UnityEngine;

public class MazeWarp : MonoBehaviour
{
    public int entranceIndex;
    public int exitIndex;

    private MazeWarp exitWarp;
    public MazeCharacter target;
    public bool keep = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (MazeWarp w in FindObjectsByType<MazeWarp>(FindObjectsSortMode.None))
        {
            if (w.exitIndex == entranceIndex)
            {
                exitWarp = w;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (MazeCharacter c in FindObjectsByType<MazeCharacter>(FindObjectsSortMode.None))
        {
            Vector3 pos = new Vector3(
                this.transform.position.x,
                c.gameObject.transform.position.y,
                this.transform.position.z
            );
            if (Vector3.Distance(c.gameObject.transform.position, pos) <= 2f && !keep)
            {
                c.gameObject.transform.position = new Vector3(
                    exitWarp.gameObject.transform.position.x, 
                    c.gameObject.transform.position.y, 
                    exitWarp.gameObject.transform.position.z
                );
                c.gameObject.GetComponent<Rigidbody>().position = c.gameObject.transform.position;
                
                exitWarp.target = c;
                exitWarp.keep = true;
            }
        }

        if (keep && target != null)
        {
            Vector3 ePos = new Vector3(
                this.transform.position.x,
                target.gameObject.transform.position.y,
                this.transform.position.z
            );
            if (Vector3.Distance(target.gameObject.transform.position, ePos) > 2f)
            {
                target = null;
                keep = false;
            }
        }
    }
}
