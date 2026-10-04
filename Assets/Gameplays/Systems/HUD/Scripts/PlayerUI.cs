using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

public class PlayerUI : MonoBehaviour
{
	[Header("データ")]
	public PlayerPrefabs prefabs;
    public HealthBar health;
	[Header("プレイヤー")]
    public int playerNo;
    public Text playerNoTxt;
    public Image artwork;
    public Text playerName;

    private int characterNo = 0;
	private bool ready = false;

    [Header("プレイヤー名")]
    public LocalizedStringTable playerNameTable;
	UnityEngine.Localization.Tables.StringTable playerNamePairs;
    public Font jpFont;
    public int jpFontSize;
    public Font enFont;
    public int enFontSize;
    [Header("武器バー")]
    public GameObject weaponGroup;
    public Image weaponIcon;
    public Image weaponBar;
    public Text weaponRemain;
    private float weaponFill;
    private float weaponPercent;

    [Header("常に表示")]
    public bool alwaysShow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ready = GameManager.players.Count > playerNo;

		if (ready) {
			characterNo = GameManager.players[playerNo].ID;
		} else {
			//マリオをデフォルトとして
			characterNo = 0;
		}

        PlayerPrefabs.PlayerData player = prefabs.playerData[characterNo];
        PlayerStatus playerStatus = null;
        if (GameManager.players.Count > 0)
        {
            playerStatus = GameManager.players[playerNo];
        } else {
            return;
        }

        if (GameManager.players.Count > 1)
        {
            playerNoTxt.text = "P" + (playerNo + 1).ToString();
        }
        else
        {
            playerNoTxt.text = "";
        }

        if (ready)
        {
            health.maxValue = GameManager.players[playerNo].MaxHP;
            health.value = GameManager.players[playerNo].HP;

            artwork.sprite = player.artwork;
            artwork.gameObject.GetComponent<RectTransform>().localScale = Vector3.one * player.artworkScale;

            playerNamePairs = playerNameTable.GetTable();
            playerName.text = playerNamePairs[player.name].Value;
            if (Encoding.GetEncoding("Shift_JIS").GetByteCount(playerName.text) > playerName.text.Length)
            {
                playerName.font = jpFont;
                playerName.fontSize = jpFontSize;
            }
            else
            {
                playerName.font = enFont;
                playerName.fontSize = enFontSize;
            }

            health.lowHP = GameManager.extra;
            RectTransform barTrans = health.gameObject.GetComponent<RectTransform>();
            Vector2 barSize = new Vector2(120f, 20f);
            if (GameManager.extra)
            {
                barSize = new Vector2(72f, 20f);
            }
            barTrans.sizeDelta = barSize;

            weaponGroup.SetActive(GameManager.players[playerNo].skillShow);
            weaponIcon.sprite = GameManager.players[playerNo].skillIcon;
            weaponPercent = GameManager.players[playerNo].skillAmounts / 28f;
            weaponFill = Mathf.Lerp(weaponFill, weaponPercent, 0.1f);
            weaponBar.fillAmount = weaponFill;
            weaponBar.color = GameManager.players[playerNo].skillColor;
            if (GameManager.players[playerNo].skillConsumption == 0)
            {
                weaponRemain.text = "∞";
            }
            else
            {
                weaponRemain.text = Math.Floor(GameManager.players[playerNo].skillAmounts / GameManager.players[playerNo].skillConsumption).ToString();
            }

            HUDToggle toggle = this.GetComponent<HUDToggle>();
            toggle.value = (int)GameManager.players[playerNo].HP;
            toggle.alwaysShow = (GameManager.players[playerNo].HP != GameManager.players[playerNo].MaxHP) || alwaysShow || GameManager.players[playerNo].skillShow;
        }
    }
}
