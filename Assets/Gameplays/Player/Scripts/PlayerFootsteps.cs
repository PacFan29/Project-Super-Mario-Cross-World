using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    // public PlayerInfo playerInfo;
    public Transform parentTrans;
    [Header("エフェクト")]
    public GameObject effect;
    [Header("効果音")]
    public AudioClip[] footstepPlane;
    public AudioClip[] footstepGrass;
    public AudioClip[] footstepMetal;
    public AudioClip[] footstepSand;
    public AudioClip[] footstepSnow;
    public AudioClip[] footstepIce;
    public AudioClip[] footstepCloud;
    public AudioClip[] footstepGlass;
    public AudioClip[] footstepSoil;
    public AudioClip[] footstepStone;
    public AudioClip[] footstepWoodThick;
    public AudioClip[] footstepWoodThin;
    // public AudioClip footstepDuringMetal;
    public AudioClip[] footstepsDuringMetal;

    public void Footstep() {
        bool enabled = true;
        Renderer[] childrenRenderer = this.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < childrenRenderer.Length; i++){
			enabled = enabled ? childrenRenderer[i].enabled : false;
		}
        if (!enabled) return;

        Instantiate(effect, this.transform.position, Quaternion.identity);

        AudioClip[] clips = new AudioClip[1];

        RaycastHit hit;
        // Transform parentTrans = playerInfo.gameObject.transform;
        Physics.Raycast(transform.position + parentTrans.up * 1f, -parentTrans.up, out hit, 20f, 1 << 0);

        bool metal = false;
        if (parentTrans.gameObject.GetComponent<PlayerInfo>() != null)
        {
            metal = parentTrans.gameObject.GetComponent<PlayerInfo>().metal;
        }

        if (hit.collider != null) {
            if (metal) {
            // if (playerInfo.metal) {
                this.GetComponent<AudioSource>().PlayOneShot(footstepsDuringMetal[UnityEngine.Random.Range(0, footstepsDuringMetal.Length)]);
            } else {
                switch (hit.collider.gameObject.tag) {
                    case "FloorGrass":
                    clips = footstepGrass;
                    break;
                    
                    case "FloorMetal":
                    clips = footstepMetal;
                    break;
                    
                    case "FloorSand":
                    clips = footstepSand;
                    break;

                    case "FloorSnow":
                    clips = footstepSnow;
                    break;

                    case "IceFloor":
                    clips = footstepIce;
                    break;

                    case "FloorCloud":
                    clips = footstepCloud;
                    break;

                    case "FloorGlass":
                    clips = footstepGlass;
                    break;

                    case "FloorSoil":
                    clips = footstepSoil;
                    break;

                    case "FloorStone":
                    clips = footstepStone;
                    break;

                    case "FloorWoodThick":
                    clips = footstepWoodThick;
                    break;

                    case "FloorWoodThin":
                    clips = footstepWoodThin;
                    break;

                    default:
                    clips = footstepPlane;
                    break;
                }

                int index = UnityEngine.Random.Range(0, clips.Length);
                this.GetComponent<AudioSource>().PlayOneShot(clips[index]);
            }
        } else {
            // if (playerInfo.metal) {
            if (metal) {
                this.GetComponent<AudioSource>().PlayOneShot(footstepsDuringMetal[UnityEngine.Random.Range(0, footstepsDuringMetal.Length)]);
            } else {
                int index = UnityEngine.Random.Range(0, footstepPlane.Length);
                this.GetComponent<AudioSource>().PlayOneShot(footstepPlane[index]);
            }
        }
    }
}
