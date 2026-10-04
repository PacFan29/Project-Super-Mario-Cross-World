using UnityEngine;

public class Yashichi : MonoBehaviour
{
    [Header("エフェクト")]
    public GameObject effect;

    [Header("コイン")]
    public GameObject coinSpawner;
    
    void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Player"){
            PlayerInfo player = col.GetComponent<PlayerInfo>();

            if (player.GotYashichi(this.transform.position))
            {
                CoinSpawner c = Instantiate(coinSpawner, this.transform.position, Quaternion.identity).GetComponent<CoinSpawner>();
                c.SetUp(10, player);
            }

            Instantiate(effect, this.transform.position, Quaternion.identity);
            
            Destroy(gameObject);
        }
    }
}
