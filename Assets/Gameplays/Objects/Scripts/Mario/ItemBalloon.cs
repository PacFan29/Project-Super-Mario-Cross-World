using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemBalloonType {
    RestoreHP,
    RestoreWP,
    FireFlower,
    IceFlower,
    ThunderShroom,
    BoomerangFlower,
    BubbleFlower,
    WindFlower,
    SuperBallFlower,
    Shield,
    FireShield,
    BubbleShield,
    ThunderShield,
    PoisonShield,
    ForceShield,
    ArmageddonShield,
    RestoreHPMini,
    RestoreWPMini,
}

public class ItemBalloon : MonoBehaviour
{
    public ItemBalloonType itemType;
    private int skinIndex;
    
    [Header("アイコン")]
    public Sprite[] restoreHP = new Sprite[4];
    public Sprite restoreWP;
    public Sprite[] powerUps = new Sprite[3];
    public Sprite[] shields = new Sprite[4];

    [Header("エフェクト")]
    public GameObject healEffect;
    public GameObject effect;
    public GameObject pacGotEffect;
    public GameObject megaGotEffect;
    public GameObject amiboHpEffect;
    public GameObject amiboWpEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int index = 0;
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (allPlayers.Length < 2) {
            for (int i = 0; i < allPlayers.Length; i++){
                if (allPlayers[i].GetComponent<PlayerInfo>() != null){
                    if (allPlayers[i].GetComponent<PlayerInfo>().playerNumber == 0) {
                        index = allPlayers[i].GetComponent<PlayerInfo>().playerType;
                    }
                }
            }
        }

        skinIndex = 0;
        switch (itemType){
            case ItemBalloonType.RestoreHP:
            //スーパーキノコ（HP 8回復）
            //restoreHP[index].SetActive(true);
            skinIndex = index;
            break;

            case ItemBalloonType.RestoreWP:
            //アミーボ 武器（武器エネルギー 8回復）
            //restoreWP.SetActive(true);
            skinIndex = 2;
            break;
                
            case ItemBalloonType.FireFlower:
            //ファイアフラワー
            //powerUps[0].SetActive(true);
            break;
                
            case ItemBalloonType.IceFlower:
            //アイスフラワー
            //powerUps[1].SetActive(true);
            break;
                
            case ItemBalloonType.ThunderShroom:
            //サンダーキノコ
            //powerUps[2].SetActive(true);
            break;
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            switch (itemType){
                case ItemBalloonType.RestoreHP:
                //スーパーキノコ（HP 8回復）
                if (skinIndex == 0) {
                    if (player.HP == player.maxHP) {
                        Instantiate(effect, this.transform.position, Quaternion.identity);
                    } else {
                        Instantiate(healEffect, this.transform.position, Quaternion.identity);
                    }
                } else if (skinIndex == 1) {
                    MakeEffect();
                } else if (skinIndex == 2) {
                    Instantiate(amiboHpEffect, this.transform.position, Quaternion.identity);
                } else if (skinIndex == 3) {
                    ;
                }
                if (col.GetComponent<_16MSonic>() != null) {
                    col.GetComponent<_16MSonic>().BoostIncrease(50, false);
                } else {
                    player.GotShroom(this.transform.position);
                    player.HPRestore(8);
                }
                break;

                case ItemBalloonType.RestoreHPMini:
                //スーパーキノコ小（HP 2回復）
                if (skinIndex == 0) {
                    if (player.HP == player.maxHP) {
                        Instantiate(effect, this.transform.position, Quaternion.identity);
                    } else {
                        Instantiate(healEffect, this.transform.position, Quaternion.identity);
                    }
                } else if (skinIndex == 1) {
                    MakeEffect();
                } else if (skinIndex == 2) {
                    Instantiate(amiboHpEffect, this.transform.position, Quaternion.identity);
                } else if (skinIndex == 3) {
                    ;
                }
                if (col.GetComponent<_16MSonic>() != null) {
                    col.GetComponent<_16MSonic>().BoostIncrease(10, false);
                } else {
                    player.GotShroom(this.transform.position, true);
                    player.HPRestore(2);
                }
                break;

                case ItemBalloonType.RestoreWP:
                //アミーボ 武器（武器エネルギー 10回復）
                player.GotRestoreWP(10, this.transform.position);
                Instantiate(amiboWpEffect, this.transform.position, Quaternion.identity);
                break;

                case ItemBalloonType.RestoreWPMini:
                //アミーボ 武器小（武器エネルギー 2回復）
                player.GotRestoreWP(2, this.transform.position, true);
                Instantiate(amiboWpEffect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.FireFlower:
                //ファイアフラワー
                player.PowerUpAnim(1);
                MakeEffect();
                break;
                
                case ItemBalloonType.IceFlower:
                //アイスフラワー
                player.PowerUpAnim(2);
                MakeEffect();
                break;
                
                case ItemBalloonType.ThunderShroom:
                //サンダーキノコ
                player.PowerUpAnim(3);
                MakeEffect();
                break;
                
                case ItemBalloonType.BoomerangFlower:
                //ブーメランフラワー
                player.PowerUpAnim(4);
                MakeEffect();
                break;
                
                case ItemBalloonType.BubbleFlower:
                //アワフラワー
                player.PowerUpAnim(5);
                MakeEffect();
                break;
                
                case ItemBalloonType.WindFlower:
                //アワフラワー
                player.PowerUpAnim(6);
                MakeEffect();
                break;
                
                case ItemBalloonType.SuperBallFlower:
                //アワフラワー
                player.PowerUpAnim(7);
                MakeEffect();
                break;

                case ItemBalloonType.Shield:
                player.GotShield(1);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.FireShield:
                player.GotShield(2);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.BubbleShield:
                player.GotShield(3);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.ThunderShield:
                player.GotShield(4);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.PoisonShield:
                player.GotShield(5);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.ForceShield:
                player.GotShield(6);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
                
                case ItemBalloonType.ArmageddonShield:
                player.GotShield(7);
                Instantiate(effect, this.transform.position, Quaternion.identity);
                break;
            }

            Destroy(gameObject);
        }
    }

    void MakeEffect() {
        switch (skinIndex) {
            case 0:
            //マリオ
            Instantiate(effect, this.transform.position, Quaternion.identity);
            break;
            
            case 1:
            //パックマン
            Instantiate(pacGotEffect, this.transform.position, Quaternion.identity);
            break;
            
            case 2:
            //ロックマン
            Instantiate(megaGotEffect, this.transform.position, Quaternion.identity);
            break;

            case 3:
            //ソニック
            //Instantiate(ef, this.transform.position, Quaternion.identity);
            break;
        }
    }
}
