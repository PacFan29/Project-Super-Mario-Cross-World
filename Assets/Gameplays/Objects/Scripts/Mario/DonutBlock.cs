using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonutBlock : GroundGimmick
{
    private Vector3 iniPosition;
    private float fallTime = 0.6f;
    private bool destroyed = false;

    public SkinnedMeshRenderer mesh;
    public Material normalMat;
    public Material stepMat;

    public AnimationCurve fallPosition;
    private float fallProgress = 0f;

    [Header("効果音")]
    public AudioClip step;
    public AudioClip fall;
    public AudioClip vanish;
    AudioSource source;

    private bool gotOnTrigger = false;
    [Header("エフェクト")]
    public GameObject effect;

    // Start is called before the first frame update
    void Start()
    {
        iniPosition = this.transform.position;
        source = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed) {
            Material[] mat = new Material[1];
            if (gotOn || fallTime <= 0) {
                if (!gotOnTrigger)
                {
                    source.PlayOneShot(step);
                    gotOnTrigger = true;
                }

                //プレイヤー着地～落下
                mat[0] = stepMat;

                if (fallTime <= 0) {
                    fallProgress += 0.667f * Time.deltaTime;
                    this.transform.position = iniPosition - Vector3.up * 5.5f * fallPosition.Evaluate(fallProgress);
                    if (fallProgress >= 1f)
                    {
                        source.PlayOneShot(vanish);
                        effect.SetActive(true);
                        fallTime = 2f;
                        destroyed = true;
                        player = null;
                        player.gameObject.transform.parent = null;
                        gotOn = false;
                    }
                } else
                {
                    fallTime -= Time.deltaTime;
                    if (fallTime <= 0)
                    {
                        source.PlayOneShot(fall);
                    }
                }
            } else {
                //プレイヤーから離れた
                mat[0] = normalMat;
                fallTime = 0.6f;
                gotOnTrigger = false;
            }
            mesh.materials = mat;
        } else {
            fallTime -= Time.deltaTime;

            if (fallTime <= 0) {
                //復活
                fallTime = 0.6f;
                destroyed = false;
                this.transform.position = iniPosition;
                fallProgress = 0f;
                effect.SetActive(false);
            }
        }

        this.GetComponent<BoxCollider>().enabled = !destroyed;
        mesh.gameObject.SetActive(!destroyed);
    }
}
