using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    public GameObject[] shields = new GameObject[4];
    public Transform forceShieldGroup;
    public GameObject forceShieldWeak;
    private int shieldActive = 0; //0＝なし、1＝バリア、2＝フレイム、3＝アクア、4＝サンダー
    public AudioClip[] shieldSounds = new AudioClip[4];
    
    void Update()
    {
        shieldActive = transform.parent.gameObject.GetComponent<PlayerInfo>().shieldActive;
        Animator anim;

        for (int i = 0; i < shields.Length; i++)
        {
            //バリアの表示・非表示
            shields[i].SetActive(shieldActive == (i + 1));
            if (shields[i].GetComponent<Animator>() != null)
            {
                anim = shields[i].GetComponent<Animator>();
                if (transform.parent.gameObject.GetComponent<SonicActions>() != null)
                {
                    anim.SetBool("Active", transform.parent.gameObject.GetComponent<SonicActions>().bubbleShieldDrop);
                }
            }

            forceShieldWeak.SetActive(shieldActive == -1);
        }
    }

    void FixedUpdate()
    {
        forceShieldGroup.Rotate(0f, -3f, 0f, Space.Self);
    }
}
