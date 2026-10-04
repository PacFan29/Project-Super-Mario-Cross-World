using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public GameData data;
    public Options options;
    public SaveManager saveManager;
    public FileSelect fileSelect;

    private int step = 0;
    // Start is called before the first frame update
    void Start()
    {
        // options.beginnerMode = true;
        // options.lives = false;
        // options.hardMode = false;
        options.difficulty = Options.Difficulty.Easy;

        saveManager.LoadOptions();

        data.Initialize();
        saveManager.stageInfos.Initialize();

        step = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (step == 1) {
            data.fileNo = (uint)fileSelect.select;
            if (Input.GetButtonDown("A"))
            {
                GameStart();
            }
            else if (Input.GetButtonDown("B"))
            {
                step = 0;
            }
        }
        
        if (
            (
                Input.GetButtonDown("A") || Input.GetButtonDown("B") ||
                Input.GetButtonDown("X") || Input.GetButtonDown("Y") ||
                Input.GetButtonDown("Start")
            ) && step == 0
        )
        {
            step = 1;
        }

        fileSelect.gameObject.SetActive(step == 1);
    }

    public void PlayerChange(int no) {
        data.characterIds[0] = (sbyte)no;
    }
    public void DifficultyChange(int no) {
        // switch (no) {
        //     case 0:
        //     options.beginnerMode = true;
        //     options.lives = false;
        //     options.hardMode = false;
        //     break;
            
        //     case 1:
        //     options.beginnerMode = false;
        //     options.lives = true;
        //     options.hardMode = false;
        //     break;

        //     case 2:
        //     options.beginnerMode = false;
        //     options.lives = true;
        //     options.hardMode = true;
        //     break;
        // }
        options.difficulty = (Options.Difficulty)no;
    }

    public void GameStart()
    {
        saveManager.SaveOptions();
        saveManager.LoadGlobal();
        saveManager.SaveGlobal();
        SceneManager.LoadScene("WorldMap");
    }
}
