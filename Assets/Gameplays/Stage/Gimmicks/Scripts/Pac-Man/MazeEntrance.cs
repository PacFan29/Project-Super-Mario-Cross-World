using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeEntrance : MonoBehaviour
{
    [Header("データ")]
    public GameData data;
    public ObjectRecordSaver saver;
    [Header("メイズＩＤ")]
    public int mazeIndex = 0;
    [Header("エフェクト")]
    public GameObject effect;

    private bool passed = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (data.CpPosition == this.transform.position && !passed) {
            passed = true;
            StartCoroutine("IncreaseScoreFromMaze");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && !passed)
        {
            FindAnyObjectByType<ObjectManagement>().Save();

            data.totalTime = GameManager.TotalTime;
            data.isCpPassed = true;
            data.CpPosition = this.transform.position;

            saver.previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
            saver.deleteAll = true;

            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i].GetComponent<PlayerInfo>() != null)
                {
                    PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
                    saver.savedScores[player.playerNumber] = (uint)player.localScore;
                    player.SavePlayerInfo();
                }
            }
            saver.savedCoins = GameManager.Coins;
            Array.Copy(GameManager.fruits, saver.savedFruits, GameManager.fruits.Length);

            Instantiate(effect, this.transform.position, Quaternion.identity);
            Destroy(this.transform.GetChild(0).gameObject);
            StartCoroutine("MazeEnter");
        }
    }
    IEnumerator IncreaseScoreFromMaze()
	{
		yield return new WaitForSeconds(0.5f);

		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < allPlayers.Length; i++)
		{
			if (allPlayers[i].GetComponent<PlayerInfo>() != null)
			{
				PlayerInfo player = allPlayers[i].GetComponent<PlayerInfo>();
				if (player.playerNumber == saver.clearedPlayerNo)
				{
                    Instantiate(effect, allPlayers[i].transform.position, Quaternion.identity);
				}
			}
		}
        Destroy(gameObject);
	}

    IEnumerator MazeEnter()
    {
        yield return new WaitForSeconds(1.5f);

        WipeManager.WipeSetUp(true, 1.5f);
        MusicManager.musicFade = true;

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(10 + mazeIndex);
    }
}