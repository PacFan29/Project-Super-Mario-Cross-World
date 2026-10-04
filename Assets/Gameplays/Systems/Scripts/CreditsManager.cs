using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    private AudioSource source;
    private int switchIndex = 0;
    private bool switched;
    [Header("データ")]
    public GameData data;
    public CreditsLines lines;
    [Header("２曲目")]
    public AudioClip[] secondTracks = new AudioClip[16];
    private int seriesIndex = -1;
    [Header("３曲目")]
    public AudioClip[] thirdTracks = new AudioClip[4];
    [Header("行")]
    public GameObject creditsLine;

    private float yPos = 0;
    private List<Transform> lineList = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        switchIndex = 0;
        switched = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (seriesIndex == -1)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in allPlayers)
            {
                if (p.GetComponent<PlayerInfo>() != null)
                {
                    if (p.GetComponent<PlayerInfo>().playerNumber == 0 && p.GetComponent<PlayerInfo>().primary)
                    {
                        if (p.GetComponent<MarioActions>() != null)
                        {
                            seriesIndex = 0;
                        }
                        else if (p.GetComponent<PacManActions>() != null)
                        {
                            seriesIndex = 1;
                        }
                        else if (p.GetComponent<MegaManActions>() != null)
                        {
                            seriesIndex = 2;
                        }
                        else if (p.GetComponent<SonicActions>() != null)
                        {
                            seriesIndex = 3;
                        }
                    }
                }
            }

            if (seriesIndex != -1)
            {
                StartCoroutine("Prepare");
            }
        }

        if (!source.isPlaying && !switched)
        {
            StartCoroutine("SwitchClip");
            switched = true;
        }

        foreach (Transform l in lineList)
        {
            if (l != null)
            {
                l.position += Vector3.up * 3.2f * Time.deltaTime;
            }
        }
        yPos += 3.2f * Time.deltaTime;
    }

    IEnumerator SwitchClip()
    {
        yield return new WaitForSeconds(3f);
        switchIndex++;

        if (switchIndex == 2)
        {
            source.clip = thirdTracks[seriesIndex];
        } else
        {
            source.clip = secondTracks[data.characterIds[0]];
        }
        source.Play();
        
        if (switchIndex < 2)
        {
            switched = false;
        }
    }

    IEnumerator Prepare()
    {
        yPos = -6.4f;
        foreach (CreditsLines.CreditsItem item in lines.items)
        {
            CreditsLine l = Instantiate(creditsLine, new Vector3(0, yPos, 0), Quaternion.identity).GetComponent<CreditsLine>();
            l.textDetail = item.header;
            l.seriesIndex = seriesIndex;
            l.isHeader = true;
            l.SetUp();
            lineList.Add(l.gameObject.transform);

            yPos -= 2.75f * 3f;
            yield return new WaitForSeconds(1.5f);

            string[] ls = item.detail.Split('\n');
            foreach (string ll in ls)
            {
                l = Instantiate(creditsLine, new Vector3(0, yPos, 0), Quaternion.identity).GetComponent<CreditsLine>();
                l.textDetail = ll;
                l.seriesIndex = seriesIndex;
                l.isHeader = false;
                l.SetUp();
                lineList.Add(l.gameObject.transform);
                yPos -= 2.75f * 3f;
                yield return new WaitForSeconds(1.5f);
            }

            yPos -= 2.75f * 3f;

            // yield return new WaitForSeconds(1.5f * (ls.Length + 1));
        }
    }
}
