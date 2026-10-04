using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SonicTrickManager : MonoBehaviour
{
    //トリックパターン１
    public int trickCombo;
    private float beforeHAxis = 1f;
    private float beforeVAxis = 1f;
    private byte axisIndex = 0;
    private byte latestAxis = 0;
    private byte axisMulti = 0;
    //トリックパターン２
    public byte[] buttonCounts;
    public float[] timeLimits;

    public byte trickPattern;
    private byte buttonStep = 0;
    private bool trickSuccess;
    private string[] trickButtons;
    private float trickTime = 2f;
    public PlayerInfo info;
    private _16MSonic sonic;
    [Header("エフェクト")]
    public GameObject trickEffect;
    private GameObject tEf;

    [Header("効果音")]
    public AudioClip trickFinishSound;
    public AudioClip buttonSound;
    public AudioClip successSound;
    public AudioClip failedSound;
    private AudioSource source;

    [Header("UI")]
    public GameObject[] UIs = new GameObject[2];
    public Text comboCount;
    public RectTransform buttonGroup;
    public InputImageManager commandButton;
    public Image timeBar;
    // Start is called before the first frame update
    void Start()
    {
        sonic = info.gameObject.GetComponent<_16MSonic>();
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //アクション
        switch (trickPattern) {
            case 1:
            if (!UIs[0].activeInHierarchy) UIs[0].SetActive(true);
            TrickCombo();
            break;

            case 2:
            if (trickTime > 0) {
                bool buttonPressed = false;
                bool sameButton = false;
                if (buttonStep < trickButtons.Length) {
                    (buttonPressed, sameButton) = CheckSameButton(trickButtons[buttonStep]);
                }

                if (buttonPressed) {
                    if (sameButton) {
                        SoundPlay(buttonSound);
                        buttonStep++;
                    } else {
                        trickTime = 0f;
                    }
                    if (buttonStep >= trickButtons.Length) {
                        //ボーナス：3,000 + (残りタイム * 1,000)点
                        trickSuccess = true;
                        SoundPlay(successSound);
                        int score = 3000 + (int)(trickTime * 1000);
                        info.scoreIncrease(score, this.transform.position);
                        trickTime = 0f;
                    }
                }
            }
            break;

            default:
            if (trickTime > 0) {
                trickTime = 0f;
            }
            break;
        }

        //エフェクト
        if (tEf != null) tEf.transform.position = info.gameObject.transform.position;

        //UI
        // for (int i = 0; i < UIs.Length; i++) {
        //     UIs[i].SetActive(trickPattern == (i+1));
        // }
        comboCount.text = trickCombo.ToString();
    }

    void TrickCombo() {
        if (trickTime > 0) {
            trickTime -= Time.deltaTime;
        }

        if (trickCombo >= 1 && Input.GetButton("LB") && Input.GetButton("RB")) {
            if (tEf != null) Destroy(tEf);
            tEf = Instantiate(trickEffect, info.gameObject.transform.position, Quaternion.identity);
            tEf.GetComponent<AudioSource>().pitch = 1.11f;
            info.scorePopUp(trickCombo * 100, false, sonic.transform.position, (int)Math.Min(6, trickCombo - 1));
            sonic.BoostIncrease(10 * trickCombo, true);
            sonic.trick = false;

            trickPattern = 0;

            info.SoundPlay(trickFinishSound);
        }
        if (!sonic.trick || info.Grounded) {
            beforeHAxis = 1f;
            beforeVAxis = 1f;

            trickCombo = 0;
            info.axisInput = true;
            axisIndex = 0;
            latestAxis = 0;
            axisMulti = 0;
            sonic.trick = false;

            trickPattern = 0;

            UIs[0].SetActive(false);
            return;
        }
        float H_axis, V_axis;
        bool H_judge, V_judge;
        (H_axis, H_judge) = AxisOnce("Horizontal");
        (V_axis, V_judge) = AxisOnce("Vertical");

        if (
            (
                (H_axis > 0 && H_judge) ||
                (H_axis < 0 && H_judge) ||
                (V_axis > 0 && V_judge) ||
                (V_axis < 0 && V_judge)
            ) && trickTime <= 0
        ){
            if (tEf != null) Destroy(tEf);
            tEf = Instantiate(trickEffect, info.gameObject.transform.position, Quaternion.identity);
            float efPitch = 1f;

            if (axisIndex > 0 && axisIndex == latestAxis) {
                if (axisMulti < 8) axisMulti *= 2;
            } else {
                axisMulti = 1;
            }
            latestAxis = axisIndex;
            
            switch (axisMulti) {
                case 1:
                efPitch = 0.9f;
                info.scorePopUp(120, false, sonic.transform.position, 3);
                break;
                
                case 2:
                efPitch = 0.8f;
                info.scorePopUp(60, false, sonic.transform.position, 2);
                break;

                case 4:
                efPitch = 0.72f;
                info.scorePopUp(30, false, sonic.transform.position, 1);
                break;

                case 8:
                efPitch = 0.64f;
                info.scorePopUp(15, false, sonic.transform.position);
                break;
            }
            tEf.GetComponent<AudioSource>().pitch = efPitch;

            info.SoundPlay(trickFinishSound);

            sonic.BoostIncrease((int)(10 / axisMulti), true);
            trickCombo++;

            trickTime = 0.3f;
        }
    }

    public IEnumerator Command() {
        sonic.trick = true;

        trickSuccess = false;
        string[] buttonNames = {"A", "B", "X", "Y", "LB", "RB"};
        float maxTime = 2f;
        buttonStep = 0;

        while (GameManager.timeScale > 0.05f) {
            GameManager.timeScale -= 0.025f;
            yield return new WaitForSeconds(0f);
        }

        UIs[1].SetActive(true);
        trickPattern = 2;

        for (int i = 0; i < buttonCounts.Length; i++) {
            trickButtons = new string[buttonCounts[i]];

            buttonGroup.anchoredPosition = new Vector3(-35f * (buttonCounts[i] - 1), 17f, 0);
            foreach (Transform obj in buttonGroup.transform) {
                if ( 0 <= obj.gameObject.name.LastIndexOf("Clone") ) {
                    Destroy(obj.gameObject);
                }
            }
            RectTransform[] otherButtons = new RectTransform[buttonCounts[i] - 1];

            for (int b = 0; b < buttonCounts[i]; b++) {
                trickButtons[b] = buttonNames[UnityEngine.Random.Range(0, buttonNames.Length)];

                if (b == 0) {
                    commandButton.access = trickButtons[0];
                } else {
                    RectTransform btn = (RectTransform)Instantiate(commandButton.gameObject).transform;
                    btn.SetParent(buttonGroup.transform , false);
                    btn.localPosition = new Vector2(
                        btn.localPosition.x + 70 * b,
                        btn.localPosition.y);

                    btn.gameObject.GetComponent<InputImageManager>().access = trickButtons[b];
                    
                    otherButtons[b - 1] = btn;
                }
            }
            buttonStep = 0;

            trickSuccess = false;
            trickTime = timeLimits[i];
            maxTime = trickTime;

            while (trickTime > 0) {
                timeBar.fillAmount = trickTime / maxTime;
                trickTime -= (Time.deltaTime / Time.fixedDeltaTime);

                yield return new WaitForSeconds(0f);
            }
            if (!trickSuccess && trickTime <= 0f) {
                break;
            }
        }

        if (trickSuccess) {
            info.YvelSetUp(Math.Max(80, (info.finalVelocity.y + 40)));
            sonic.TrickSuccessVoice();
        } else {
            SoundPlay(failedSound);
            sonic.TrickFailedVoice();
        }
        
        UIs[1].SetActive(false);
        GameManager.timeScale = 1f;

        while (!info.Grounded) {
            yield return new WaitForSeconds(0f);
        }
        info.canInput = true;
        info.axisInput = true;
        sonic.trick = false;
        trickPattern = 0;
    }

    (float a, bool b) AxisOnce(string axis){
        bool judge = false;
        float view_axis = Input.GetAxis(axis);

        //RTが押されたら視点を変える：前フレームの入力値が0の場合のみ実施
        if (axis == "Horizontal") {
            judge = Math.Abs(view_axis) > 0 && beforeHAxis == 0.0f;
            beforeHAxis = view_axis;

            if (view_axis < 0) {
                axisIndex = 1;
            } else if (view_axis > 0) {
                axisIndex = 2;
            }
        } else {
            judge = Math.Abs(view_axis) > 0 && beforeVAxis == 0.0f;
            beforeVAxis = view_axis;

            if (view_axis < 0) {
                axisIndex = 3;
            } else if (view_axis > 0) {
                axisIndex = 4;
            }
        }

        return (view_axis, judge);
    }

    (bool buttonPressed, bool sameButton) CheckSameButton(string button) {
        bool buttonPressed = false;
        bool sameButton = false;

        Dictionary<string, bool> ButtonsDown = new Dictionary<string, bool>() {
            {"A", false},
            {"B", false},
            {"X", false},
            {"Y", false},
            {"LB", false},
            {"RB", false}
        };

        //キーだけ集めたリストを作る。
        List<string> keyArray = new List<string>();
        foreach(KeyValuePair<string, bool> buttonDown in ButtonsDown){
            keyArray.Add(buttonDown.Key);
        }

        //辞書を更新する
        foreach(string key in keyArray){
            //ボタンが押された状態
            ButtonsDown[key] = Input.GetButtonDown(key);
            if (!buttonPressed) buttonPressed = ButtonsDown[key];
        }

        sameButton = (ButtonsDown[button]);

        return (buttonPressed, sameButton);
    }

    void SoundPlay(AudioClip sound) {
        source.clip = sound;
        source.Play();
    }
}
