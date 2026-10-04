using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : PlayerPhysics
{
    private GameObject cam;
    [Header("操作可能")]
    public bool canInput = true;
    public bool axisInput = true;
    
    private bool inputLockByExternal = false;

    [HideInInspector] public bool lockUntilGrounded = false;
    [HideInInspector] public Transform rushJetObj = null;
    [HideInInspector] public Vector3 rushJetInput;
    [Header("ステータス")]
    public byte playerNumber = 0;
    //ソニックマニアプラス　アンコールモード式
    public bool primary = true;
    [Header("相方")]
    public PlayerInfo partner;

    public Dictionary<string, float> Axises = new Dictionary<string, float>() {
        {"Horizontal", 0f},
        {"Vertical", 0f},
        {"Alt Horizontal", 0f},
        {"Alt Vertical", 0f},
        {"LT/RT", 0f}
    };
    public Dictionary<string, bool> Buttons = new Dictionary<string, bool>() {
        {"A", false},
        {"B", false},
        {"X", false},
        {"Y", false},
        {"Start", false},
        {"Back", false},
        {"LB", false},
        {"RB", false}
    };
    public Dictionary<string, bool> ButtonsDown = new Dictionary<string, bool>() {
        {"A", false},
        {"B", false},
        {"X", false},
        {"Y", false},
        {"Start", false},
        {"Back", false},
        {"LB", false},
        {"RB", false}
    };
    public Dictionary<string, bool> ButtonsUp = new Dictionary<string, bool>() {
        {"A", false},
        {"B", false},
        {"X", false},
        {"Y", false},
        {"Start", false},
        {"Back", false},
        {"LB", false},
        {"RB", false}
    };

    //セッター
    void SetAxises(bool lockThis = false){
        //キーだけ集めたリストを作る。
        List<string> keyArray = new List<string>();
        foreach(KeyValuePair<string, float> axis in Axises){
            keyArray.Add(axis.Key);
        }
        //辞書を更新する
        foreach(string key in keyArray){
            Axises[key] = (controlLockTimer <= 0 && canInput && !inputLockByExternal && !lockUntilGrounded && GameManager.ready && Time.timeScale > 0 && !lockThis) ? Input.GetAxis(playerJudge(key)) : 0f;
        }
    }
    void SetButtons(bool lockThis = false){
        bool inputReady = controlLockTimer <= 0 && canInput && !inputLockByExternal && !lockUntilGrounded && GameManager.ready && Time.timeScale > 0 && !lockThis;

        //キーだけ集めたリストを作る。
        List<string> keyArray = new List<string>();
        foreach(KeyValuePair<string, bool> button in Buttons){
            keyArray.Add(button.Key);
        }
        //辞書を更新する
        foreach(string key in keyArray){
            //ボタン長押しの状態
            Buttons[key] = Input.GetButton(playerJudge(key)) && inputReady;
        }
        foreach(string key in keyArray){
            //ボタンが押された状態
            ButtonsDown[key] = Input.GetButtonDown(playerJudge(key)) && inputReady;
        }
        foreach(string key in keyArray){
            //ボタンを放した状態
            ButtonsUp[key] = Input.GetButtonUp(playerJudge(key)) && inputReady;
        }
    }
    //ゲッター
    public Dictionary<string, float> GetAxises(){ return Axises; }
    public Dictionary<string, bool> GetButtons(){ return Buttons; }
    //プレイヤーに応じて
    public string playerJudge(string input){
        if (playerNumber > 0){ input += String.Format(" {0}", (playerNumber+1)); }
        return input;
    }

    public void Controller(){
        SetAxises(!primary);
        SetButtons(!primary);

        if (!primary)
        {
            bool inputReady = controlLockTimer <= 0 && canInput && !inputLockByExternal && !lockUntilGrounded && GameManager.ready && Time.timeScale > 0;

            Vector3 partnerPoint = partner.gameObject.transform.position - this.transform.position;
            float partnerDistance = Vector3.Distance(partner.gameObject.transform.position, this.transform.position);
            if (partnerDistance > 4f && inputReady)
            {
                float inputRate = 1f;
                if (playerType == 3)
                {
                    inputRate = Math.Min(1f, partnerDistance * 0.1875f);
                    Axises["LT/RT"] = 0f;
                } else
                {
                    if (partnerDistance > 6f)
                    {
                        Axises["LT/RT"] = 1f;
                    } else
                    {
                        Axises["LT/RT"] = 0f;
                    }
                }
                input = new Vector3(partnerPoint.x, 0f, partnerPoint.z).normalized * inputRate;
                Buttons["A"] = partnerPoint.y >= 10f;
            } else
            {
                input = Vector3.zero;
            }
            if (inputReady)
            {
                ButtonsDown["A"] = partnerPoint.y >= 3.75f && Grounded;
                Buttons["A"] = partnerPoint.y >= 2.00f && !Grounded;
            }
            return;
        }

        if (GameManager.StageClear())
        {
            canInput = false;
        }

        inputLockByExternal = false;
        if (FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length > 0)
		{
			if (!FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None)[0].canPlayerInput)
			{
                ForwardSetUp(Vector3.zero, 0f);
				inputLockByExternal = true;
			}
		}

        cam = Camera.main.gameObject;
        //カメラの前方向
        Vector3 lookDir = cam.transform.forward;
        lookDir.y = 0;
        lookDir = lookDir.normalized;
        //カメラの右方向
        Vector3 right = cam.transform.right;
        right.y = 0;
        right = right.normalized;

        if (controlLockTimer > 0) {
            controlLockTimer -= Time.deltaTime;
            if (controlLockTimer <= 0) {
                constantChange("grv", Gravity);
                gravityLock = false;
            }
        }

        //左スティック
        if (axisInput && controlLockTimer <= 0 && rushJetObj == null) {
            if (!is3D()) {
                //2D
                float inputWay = 0;
                if (Math.Abs(Axises["Horizontal"]) > 0.2){
                    inputWay = (Axises["Horizontal"] > 0) ? Math.Max(0f, Axises["Horizontal"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Horizontal"] + 0.2f) / 0.8f;
                }
                if (dimension == DimensionType.ZWay2D){
                    input.x = 0;
                    input.z = inputWay;
                } else {
                    input.x = inputWay;
                    input.z = 0;
                }
            } else {
                //3D
                float h = (Axises["Horizontal"] > 0) ? Math.Max(0f, Axises["Horizontal"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Horizontal"] + 0.2f) / 0.8f;
                float v = (Axises["Vertical"] > 0) ? Math.Max(0f, Axises["Vertical"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Vertical"] + 0.2f) / 0.8f;
                input.x = (h * right.x) + (v * lookDir.x);
                input.z = (h * right.z) + (v * lookDir.z);
            }
        } else {
            input.x = 0f;
            input.z = 0f;
        }

        if (rushJetObj != null)
        {
            if (!is3D()) {
                //2D
                float inputWay = 0;
                float inputWayY = 0;
                if (Math.Abs(Axises["Horizontal"]) > 0.2){
                    inputWay = (Axises["Horizontal"] > 0) ? Math.Max(0f, Axises["Horizontal"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Horizontal"] + 0.2f) / 0.8f;
                }
                if (Math.Abs(Axises["Vertical"]) > 0.2){
                    inputWayY = (Axises["Vertical"] > 0) ? Math.Max(0f, Axises["Vertical"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Vertical"] + 0.2f) / 0.8f;
                }
                if (dimension == DimensionType.ZWay2D){
                    rushJetInput.x = 0;
                    rushJetInput.z = inputWay;
                } else {
                    rushJetInput.x = inputWay;
                    rushJetInput.z = 0;
                }
                rushJetInput.y = inputWayY;
            } else {
                //3D
                float h = (Axises["Horizontal"] > 0) ? Math.Max(0f, Axises["Horizontal"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Horizontal"] + 0.2f) / 0.8f;
                float v = (Axises["Vertical"] > 0) ? Math.Max(0f, Axises["Vertical"] - 0.2f) / 0.8f : Math.Min(0f, Axises["Vertical"] + 0.2f) / 0.8f;
                rushJetInput.x = (h * right.x) + (v * lookDir.x);
                rushJetInput.y = 0f;
                rushJetInput.z = (h * right.z) + (v * lookDir.z);
            }
        } else
        {
            rushJetInput = Vector3.zero;
        }
    }

    public bool GetCrouchButton(string buttonName) {
        if (is3D()){
            return Buttons[buttonName];
        } else {
            return Buttons[buttonName] || (Axises["Vertical"] < -0.5);
        }
    }
    public bool GetCrouchButtonDown(string buttonName) {
        if (is3D()){
            return ButtonsDown[buttonName];
        } else {
            return ButtonsDown[buttonName] || (Axises["Vertical"] < -0.5);
        }
    }
}
