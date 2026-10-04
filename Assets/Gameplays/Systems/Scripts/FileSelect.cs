using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSelect : MonoBehaviour
{
    public int select;
    private float beforeAxis = 0f;
    public Transform[] files = new Transform[3];

    // Update is called once per frame
    void Update()
    {
        float view_axis;
        bool judge;
        (view_axis, judge) = AxisOnce("Horizontal");

        if (view_axis < 0 && judge && select > 0){
            select--;
        } else if (view_axis > 0 && judge && select < (files.Length-1)){
            select++;
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < files.Length; i++){
            files[i].localScale = Vector3.Lerp(files[i].localScale, i == select ? Vector3.one * 1.1f : Vector3.one, 0.5f);
        }
    }

    (float a, bool b) AxisOnce(string axis){
        bool judge = false;
        float view_axis = Input.GetAxis(axis);

        //RTが押されたら視点を変える：前フレームの入力値が0の場合のみ実施
        judge = Math.Abs(view_axis) > 0 && beforeAxis == 0.0f;
        beforeAxis = view_axis;

        return (view_axis, judge);
    }
}
