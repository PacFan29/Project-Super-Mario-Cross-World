using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSkins : MonoBehaviour
{
    public PlayerInfo info;
    public Material[] powerUpMats = new Material[5];
    public byte index;

    // Update is called once per frame
    void Update()
    {
        Material[] currentMat = this.GetComponent<SkinnedMeshRenderer>().materials;

        currentMat[index] = powerUpMats[info.powerUpActive];
        try {
			currentMat[index].SetFloat("StarTrigger", (info.invincible ? 1f : 0f));
			currentMat[index].SetFloat("MetalTrigger", (info.metal ? 1f : 0f));
        } catch {}

        this.GetComponent<SkinnedMeshRenderer>().materials = currentMat;
    }
}
