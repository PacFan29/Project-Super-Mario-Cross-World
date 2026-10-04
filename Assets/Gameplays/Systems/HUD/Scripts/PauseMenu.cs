using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameData data;
    [Header("ポーズテキスト")]
    public GameObject[] pauseTexts = new GameObject[4];
    [Header("ボタン")]
    public Sprite[] marioButtonSprites = new Sprite[2];
    public Sprite[] sonicButtonSprites = new Sprite[2];
    public CommandButton[] buttons = new CommandButton[3];
    public CommandButton[] buttons2 = new CommandButton[2];

    private int select = 0;
    private int select2 = 0;
    private float beforeAxis = 0f;
    private bool active = true;
    private bool areYouSure = false;
    
    [Header("メッセージ")]
    public Text message;
    public MessageBox nextStep;
    
    private AudioSource source;
    
    [Header("効果音")]
    public AudioClip acceptSound;
    public AudioClip cancelSound;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        Animator[] animators = this.GetComponentsInChildren<Animator>();

        for (int i = 0; i < animators.Length; i++){
			animators[i].updateMode = AnimatorUpdateMode.UnscaledTime;
		}
    }
    void OnEnable() {
        select = 0;
    }

    // Update is called once per frame
    void Update()
    {
        pauseTexts[0].SetActive(data.HUDType != GameType.Sonic && data.HUDType != GameType.SonicBoss);
		pauseTexts[1].SetActive(data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss);
        // pauseTexts[0].SetActive(data.character == Character.Mario);
		// pauseTexts[1].SetActive(data.character == Character.PacMan);
		// pauseTexts[2].SetActive(data.character == Character.RockMan);
		// pauseTexts[3].SetActive(data.character == Character.Sonic);

        foreach (CommandButton b in buttons)
        {
            if (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss)
            {
                Array.Copy(sonicButtonSprites, b.sprites, sonicButtonSprites.Length);
            } else
            {
                Array.Copy(marioButtonSprites, b.sprites, marioButtonSprites.Length);
            }
        }

        if (active) {
            if (areYouSure) {
                AreYouSure();
            } else {
                SelectMenu();
            }
        }
    }
    void SelectMenu()
    {
        float view_axis;
        bool judge;
        (view_axis, judge) = AxisOnce("Vertical");

        if (view_axis > 0 && judge && select > 0){
            select--;
        } else if (view_axis < 0 && judge && select < (buttons.Length-1)){
            select++;
        }

        for (int i = 0; i < buttons.Length; i++){
            buttons[i].selected = (i == select);
        }

        if (Input.GetButtonDown("A") || Input.GetButtonDown("Start")){
        // if (Input.GetButtonDown("A")){
            buttons[select].accepted = true;
            source.Play();

            switch (select) {
                case 0:
                StartCoroutine("Resume");
                break;

                case 1:
                StartCoroutine("SwitchStep");
                break;

                case 2:
                StartCoroutine("SwitchStep");
                break;
            }
        }
    }
    void AreYouSure() {
        float view_axis;
        bool judge;
        (view_axis, judge) = AxisOnce("Horizontal");

        if (view_axis > 0 && judge && select2 < (buttons2.Length-1)){
            select2++;
        } else if (view_axis < 0 && judge && select2 > 0){
            select2--;
        }

        for (int i = 0; i < buttons2.Length; i++){
            buttons2[i].selected = (i == select2);
        }

        if (Input.GetButtonDown("A") || Input.GetButtonDown("Start")){
            buttons2[select2].accepted = true;
            
            switch (select2) {
                case 0:
                //いいえ
                source.PlayOneShot(cancelSound);
                StartCoroutine("SwitchStep");
                break;

                case 1:
                //はい
                source.PlayOneShot(acceptSound);
                StartCoroutine(Fade(select));
                break;
            }
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

    IEnumerator Fade(int select) {
        active = false;
        yield return new WaitForSecondsRealtime(0.5f);

        FadeManager.FadeSetUp(true, 1f);
        yield return new WaitForSecondsRealtime(2f);

        switch (select) {
            case 1:
            //やりなおす
            data.ResetCheckPoint();
            // data.ResetExceptTime();
            GameManager.isPaused = false;
            GameManager.ResetEntire();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            break;

            case 2:
            //コースを出る
            data.ResetCheckPoint();
            // data.ResetExceptTime();
            GameManager.isPaused = false;
            GameManager.ResetEntire();
            SceneManager.LoadScene("WorldMap");
            break;
        }
    }
    
    IEnumerator SwitchStep() {
        active = false;
        yield return new WaitForSecondsRealtime(0.5f);

        areYouSure = !areYouSure;
        select2 = 0;

        if (areYouSure) {
            for (int i = 0; i < buttons2.Length; i++){
                buttons2[i].accepted = false;
            }
        } else {
            for (int i = 0; i < buttons.Length; i++){
                buttons[i].accepted = false;
            }
        }
        nextStep.display = areYouSure ? 1 : -1;

        active = true;
    }
    IEnumerator Resume()
    {
        active = false;

        yield return new WaitForSecondsRealtime(0.5f);
        
        for (int i = 0; i < buttons.Length; i++){
            buttons[i].accepted = false;
        }
        active = true;
        GameManager.isPaused = false;
    }
}
