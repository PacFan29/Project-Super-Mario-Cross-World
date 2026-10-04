using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

public class CharacterSelectPlayer : MonoBehaviour
{
    [Header("プレイヤーデータ")]
    public PlayerPrefabs playerPrefabs;
    public LocalizedStringTable playerNameTable;
    UnityEngine.Localization.Tables.StringTable playerNamePairs;
    [Header("インデックス")]
    public int index;
    [Header("3D UI")]
    public Transform ui3d;
    private bool changedTrigger = true;
    private int currentIndex;
    [Header("UI")]
    public Image artwork;
    public Text playerName;
    public Image lifeParam;
    public Image attackParam;
    public Image defenseParam;
    public Image speedParam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        changedTrigger = true;
        currentIndex = index;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefabs.PlayerData data = playerPrefabs.playerData[index];

        //アートワーク
        artwork.sprite = data.artwork;

        //3D UI
        if (currentIndex != index)
        {
            currentIndex = index;
            changedTrigger = true;
        }
        if (changedTrigger)
        {
            if (ui3d.childCount > 0)
            {
                Destroy(ui3d.GetChild(0).gameObject);
            }
            GameObject skin = Instantiate(data.prefab.GetComponent<PlayerInfo>().skin.gameObject, ui3d.position + data.prefab.GetComponent<PlayerInfo>().skin.localPosition, ui3d.rotation, ui3d);
            if (skin.GetComponent<Animator>() != null) {
                skin.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            ChangeLayer(ui3d);
            changedTrigger = false;
        }

        //名前
        playerNamePairs = playerNameTable.GetTable();
        playerName.text = playerNamePairs[data.name].Value;
        playerName.color = data.color;

        //ライフ：標準３（実際はハート6個分）
        lifeParam.fillAmount = (float)data.prefab.GetComponent<PlayerInfo>().maxHPSegment / 10f;
        //攻撃：標準３
        attackParam.fillAmount = 0.6f;
        if (data.prefab.GetComponent<_08MegaMan>() != null || data.prefab.GetComponent<_10ProtoMan>() != null)
        {
            attackParam.fillAmount = 0.2f;
        } else if (data.prefab.GetComponent<_11Bass>() != null)
        {
            attackParam.fillAmount = 0.1f;
        }
        //防御：標準１（1.25の場合、ダメージ率が80%になる）
        defenseParam.fillAmount = (1f / (float)data.prefab.GetComponent<PlayerInfo>().defenseRate) * 0.2f;
        //スピード：標準約２
        speedParam.fillAmount = (float)data.prefab.GetComponent<PlayerInfo>().TopSpeed / 50f;
    }

    public void ChangeLayer(Transform skin)
    {
        foreach (Transform s in skin)
        {
            s.gameObject.layer = LayerMask.NameToLayer("UI 3D Object");
            ChangeLayer(s);
        }
    }
}
