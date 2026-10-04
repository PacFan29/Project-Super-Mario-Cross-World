using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaManager : MonoBehaviour
{
    public GameObject splashEffect;
    
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            PlayerInfo player = other.GetComponent<PlayerInfo>();
            if (player.shieldActive == 2 || player.metal)
            {
                //フレイムバリアあり：マグマの中に潜れる
                //注意！　マグマの中でダメージを受けると、無敵時間解除後に即死！
                player.underwaterButLava = true;
                player.underwaterTrigger = true;
            }
            else
            {
                //フレイムバリアなし：8ダメージ
                player.TakeDamage(8, player.gameObject.transform.position, (int)DamageType.Lava);
            }
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            PlayerInfo player = other.GetComponent<PlayerInfo>();
            player.underwaterTrigger = false;
        }
    }
}
