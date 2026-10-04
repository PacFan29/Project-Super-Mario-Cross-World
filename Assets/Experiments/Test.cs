using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AnimationCurve curve;

    public int chain;
    public int chainBonus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        chainBonus = (int)(curve.Evaluate((float)chain / 999f) * 2000) * 50;
    }
}
