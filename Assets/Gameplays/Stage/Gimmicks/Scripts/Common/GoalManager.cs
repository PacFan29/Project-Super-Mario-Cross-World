using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GoalManager : MonoBehaviour
{
    [Header("データ")]
    public GameData data;

    protected void GoToResult()
    {
        data.ResetCheckPoint();
        SceneManager.LoadScene("ResultScreen");
    }

    public void cleared(GameObject player, int index = 0)
    {
        MusicManager.musicFade = true;
        GameManager.timeIncrease = false;
        StartCoroutine(player.GetComponent<PlayerInfo>().CourseCleared(index));
    }

    protected void SoundPlay(AudioClip clip)
    {
        if (clip != null)
        {
            this.GetComponent<AudioSource>().PlayOneShot(clip);
        }
    }

    protected void DestroyAllEnemies(PlayerInfo player)
    {
        byte combo = 1;

        foreach (EnemyManager enemy in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
        {
            if (enemy.DestroyWhenPlayerGoaled(player, combo))
            {
                if ((combo >= 8 && !GameManager.extra) || (combo == 8 && GameManager.extra))
                {
                    player.OneUp();
                }
                if (GameManager.maxCombo < combo) GameManager.maxCombo = combo;
                combo++;
            }
        }
        foreach (ItemManager item in FindObjectsByType<ItemManager>(FindObjectsSortMode.None))
        {
            if (item.DestroyWhenPlayerGoaled(player, combo))
            {
                if ((combo >= 8 && !GameManager.extra) || (combo == 8 && GameManager.extra))
                {
                    player.OneUp();
                }
                if (GameManager.maxCombo < combo) GameManager.maxCombo = combo;
                combo++;
            }
        }
    }
}
