using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTemplate : MonoBehaviour
{
    [Header("バー")]
    public Image healthFill;
    public float minFill = 0.0f;
    public float maxFill = 1.0f;
    [Header("残機")]
    public Text livesText;
    public bool cross = false;
    public SpriteNumber livesSprite;

    [HideInInspector] public int playerNo = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HealthDisp();
        LivesDisp();
    }

    public virtual void HealthDisp() {
        if (healthFill != null) {
            int max_health = (int)GameManager.players[playerNo].MaxHP;
            int health = (int)GameManager.players[playerNo].HP;
            float health_percent = (float)health / max_health;
            healthFill.fillAmount = (health_percent * (maxFill - minFill)) + minFill;
        }
    }
    public virtual void LivesDisp() {
        if (livesText != null) {
            livesText.text = cross ? "×" : "";
            // livesText.text += GameManager.players[playerNo].Lives.ToString();
        } else if (livesSprite != null) {
            // livesSprite.value = GameManager.players[playerNo].Lives;
        }
    }
}
