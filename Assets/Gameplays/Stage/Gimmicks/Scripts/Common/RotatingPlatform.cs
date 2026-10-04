using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MovingPlatformParent
{
    public float rotationSpeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.Rotate(0f, rotationSpeed / 5f, 0f, Space.Self);
    }
}
