using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoringController : MonoBehaviour
{
    [Header("値・レート")]
    public int points;
    public int rate = 1;
    public bool noScore = false;
    [Header("コンボによる変色")]
    public int combo;
    public Gradient[] comboColors = new Gradient[8];
    private float comboGradientTime = 0f;
    [Header("その他")]
    public GameObject textParent;
    public TextMesh mainColor;
    private int previousPts = 0;
    public bool active;
    public bool isTime;
    public bool isRing;
    public float scale = 1f;
    public Transform ringIcon;
    
    void Awake() {
        if (textParent == null)
        {
            return;
        }
        foreach(Transform ob in textParent.GetComponentInChildren<Transform>()) {
            if (ob.gameObject.GetComponent<TextMesh>() != null) {
                ob.gameObject.GetComponent<TextMesh>().text = "";
            }
        }
    }
    void Update(){
        if (active) {
            if (rate < 1) rate = 1;
            
            float size = 0f;
            if (isRing) {
                size = Math.Min(5.5f, (Math.Abs(points) / 20f) + 0.5f);
            } else {
                if (isTime) {
                    size = Math.Min(5.5f, (Math.Abs(points) / 200f) + 0.5f);
                } else {
                    size = Math.Min(5.5f, (Math.Abs(points * rate) / 10000f) + 0.5f);
                    if (Math.Abs(points * rate) > 10000) {
                        size = Math.Min(2.5f, (Math.Abs(points * rate) / 90000f) + 1.5f);
                    }
                }
            }
            this.transform.localScale = new Vector3 (size, size, 1f) * scale;

            if (Math.Abs(points) == 0){
                destroyObject();
            } else {
                if (Math.Abs(points) > 0 && Math.Abs(previousPts) <= 0) {
                    previousPts = points;
                    if (textParent != null)
                    {
                        GetChildren(textParent);
                    }
                }
            }
        } else
        {
            this.transform.localScale = Vector3.one * scale;
        }
        //スケール 0.51~1.50

        if (GameManager.isPaused)
        {
            this.GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
        }
        else
        {
            this.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
            if (comboGradientTime < 1f) {
                comboGradientTime += Time.unscaledDeltaTime;
                if (comboGradientTime > 1f) {
                    comboGradientTime = 1f;
                }
            }
        }
        if (mainColor != null)
        {
            // if (combo >= 7)
            // {
            //     mainColor.color = comboColors[7].Evaluate(comboGradientTime);
            // }
            // else if (combo > -1)
            if (combo > 7)
            {
                mainColor.color = comboColors[6].Evaluate(comboGradientTime);
            }
            else if (combo > -1)
            {
                mainColor.color = comboColors[combo].Evaluate(comboGradientTime);
            }
            else
            {
                mainColor.color = comboColors[0].Evaluate(comboGradientTime);
            }
        }
        if (points < 0)
        {
            mainColor.color = Color.red;
        }

        if (ringIcon != null) {
            ringIcon.localPosition = new Vector3(-0.9765f - ((points.ToString().Length - 1) * 0.234f), 0.248f, 0f);
        }
    }
    public void destroyObject() {
        Destroy(gameObject);
    }
    void GetChildren(GameObject obj) {
        Transform children = obj.GetComponentInChildren<Transform>();
        //子要素がいなければ終了
        if (children.childCount == 0) {
            return;
        }
        foreach(Transform ob in children) {
            //ここに何かしらの処理
            //例　ボーンについてる武器を取得する
            string ptsStr = points.ToString();
            if (isRing) {
                ptsStr = "×" + ptsStr;
            } else {
                if (isTime) {
                    if (points >= 0) {
                        ptsStr = "+" + ptsStr + "s";
                    } else {
                        ptsStr = ptsStr + "s";
                    }
                } else {
                    if (rate > 1) {
                        ptsStr += "×";
                        ptsStr += rate.ToString();
                    }
                }
            }
            if (noScore)
            {
                if (points < 0)
                {
                    ptsStr = "";
                } else {
                    switch (combo)
                    {
                        case 0:
                            ptsStr = "Good";
                            break;

                        case 1:
                            ptsStr = "Great";
                            break;

                        case 2:
                            ptsStr = "Super";
                            break;

                        case 3:
                            ptsStr = "Fantastic";
                            break;

                        case 4:
                            ptsStr = "Excellent";
                            break;

                        case 5:
                            ptsStr = "Incredible";
                            break;

                        case 6:
                            ptsStr = "Wonderful";
                            break;

                        default:
                            ptsStr = "";
                            break;
                    }
                }
                // ptsStr = "";
            }
            if (ob.gameObject.GetComponent<TextMesh>() != null) {
                ob.gameObject.GetComponent<TextMesh>().text = ptsStr;
            }
            GetChildren(ob.gameObject);
        }
    }
}
