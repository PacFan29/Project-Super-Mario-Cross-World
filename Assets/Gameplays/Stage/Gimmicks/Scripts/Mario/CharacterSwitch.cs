using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterSwitch : MarioSwitch
{
    // [Header("キャラクター")]
    // public int charIndex = 0;
    [Header("シリーズ")]
    public int seriesIndex = 0;
    [Header("マテリアル")]
    public SkinnedMeshRenderer baseMesh;
    public SkinnedMeshRenderer headMesh;
    public Material[] materials;

    void FixedUpdate()
    {
        Material[] mat = new Material[1];
        mat[0] = materials[seriesIndex];
        baseMesh.materials = mat;
        headMesh.materials = mat;
    }

    public override void OnCollisionEnter(Collision other) {
        if (other.gameObject.GetComponent<PlayerInfo>() != null && !pressed && other.contacts[0].normal == Vector3.down) {
            // if (charIndex == 0)
            // {
            //     // this.GetComponent<AudioSource>().Play();
            //     pressed = true;

            //     StartCoroutine(Gimmick());
            // }
            switch (seriesIndex)
            {
                case 0:
                    if (other.gameObject.GetComponent<MarioActions>() != null)
                    {
                        pressed = true;
                    }
                    break;
                    
                case 1:
                    if (other.gameObject.GetComponent<PacManActions>() != null)
                    {
                        pressed = true;
                    }
                    break;
                    
                case 2:
                    if (other.gameObject.GetComponent<MegaManActions>() != null)
                    {
                        pressed = true;
                    }
                    break;
                    
                case 3:
                    if (other.gameObject.GetComponent<SonicActions>() != null)
                    {
                        pressed = true;
                    }
                    break;
            }
            if (pressed)
            {
                // this.GetComponent<AudioSource>().Play();
                StartCoroutine(Gimmick());
            }
        }
    }

    public override IEnumerator Gimmick() {
        yield return null;
    }
}
