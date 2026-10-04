using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHouse : MonoBehaviour
{
    private const float boxSize = 6f;

    [Serializable]
    public class GhostStay
    {
        public int remainedCookies;
        public MazeGhost ghost;
    }
    public List<GhostStay> ghostStays = new List<GhostStay>();

    public float GetLeft()
    {
        return boxSize * -2.5f;
    }
    public float GetRight()
    {
        return boxSize * 2.5f;
    }
    public float GetTop()
    {
        return boxSize * 1.5f;
    }
    public float GetBottom()
    {
        return boxSize * -1.5f;
    }

    public Vector3 GetBasePosition()
    {
        return this.transform.position + Vector3.forward * boxSize * 2f + Vector3.up * boxSize;
    }
    public Vector3 GetOutPosition()
    {
        return this.transform.position + Vector3.forward * boxSize * 0.5f + Vector3.up * boxSize;
    }

    void Update()
    {
        int cookieCount = 0;
        foreach (MazeCookie c in FindObjectsByType<MazeCookie>(FindObjectsSortMode.None))
        {
            if (c.enable)
            {
                cookieCount++;
            }
        }

        foreach (GhostStay ghost in ghostStays)
        {
            ghost.ghost.isReadyToOut = false;
            if (cookieCount <= ghost.remainedCookies)
            {
                ghostStays.Remove(ghost);
                ghost.ghost.isReadyToOut = true;
                break;
            }
        }
    }
}
