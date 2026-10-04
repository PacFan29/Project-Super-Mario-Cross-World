using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    protected PlayerInfo info;

    [Header("エフェクト")]
    public GameObject deathEffect;
    protected PlayerVoiceManager voices;

    private float time = 0f;
    public GameObject charge1;
    public GameObject charge2;
    private GameObject[] actives = new GameObject[2];
    public GameObject buster;

    [Header("効果音")]
    public AudioClip busterSound;
    public AudioClip mBusterSound;
    public AudioClip chargeShotSound;

    // Start is called before the first frame update
    void Start()
    {
        info = GetComponent<PlayerInfo>();
        info.Stompable = false;
        info.playerType = 2;
        info.desacelerar = true;

        voices = GetComponent<PlayerVoiceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.players.Count > info.playerNumber) {
            //プレイヤークラスの値を設定
            // GameManager.players[info.playerNumber].setStatus(info.playerId, info.maxHP, info.HP, info.lives, info.localScore);
            GameManager.players[info.playerNumber].setStatus(info.playerId, info.maxHP, info.HP, (uint)info.localScore);
        }

        if (info != null){
            //プレイヤーIDを8に設定
            info.setPlayerId(8);
        }

        if (info.ButtonsDown["X"]){
            info.SoundPlay(busterSound);

            BusterShot(0);
        }

        if (info.Buttons["X"]) {
            if (time > 0 && time < 1.5){
                time += Time.deltaTime;
                if (time > 0.5 && actives[0] == null) {
                    actives[0] = Instantiate(charge1, transform.position, Quaternion.identity, this.transform);
                } else if (time >= 1.5 && time < 2){
                    time = 1.7f;
                    Destroy(actives[0]);
                    actives[1] = Instantiate(charge2, transform.position, Quaternion.identity, this.transform);
                }
            } else if (time <= 0){
                time = Time.deltaTime;
            }
        }
        
        if (!info.Buttons["X"] && time > 0){
            Destroy(actives[0]);
            Destroy(actives[1]);
            if (time >= 1.5) {
                voices.Attack();
                
                info.SoundPlay(chargeShotSound);
                BusterShot(2);
            } else if (time >= 0.5) {
                info.SoundPlay(mBusterSound);
                BusterShot(1);
            }
            time = 0;
        }
    }

    public virtual void BusterShot(byte level) {
        GameObject solarbrit = Instantiate(buster, transform.position, info.skin.rotation);
        MegaBuster bust = solarbrit.GetComponent<MegaBuster>();
        bust.player = info;
        bust.level = level;

        bust.power = 1f;
        bust.powerUp = 0;
    }

    public IEnumerator DamageAnimation(Vector3 direction) {
        info.canInput = false;
        float time = 0.3f;
        Vector3 damageVelocity = direction.normalized * 30f;
        damageVelocity.y = 0;
        
        while (time > 0) {
            info.VelocitySetUp(damageVelocity);
            info.skin.transform.forward = -damageVelocity.normalized;
            time -= Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        info.VelocitySetUp(Vector3.zero);
        yield return new WaitForSeconds(0.7f);

        info.canInput = true;
        info.tookDamage = false;
        yield return null;
    }
    public IEnumerator DeathAnimation() {
        info.activePhysics = false;
        GameManager.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.7f);

        GameManager.timeScale = 1;
        info.StopAllSounds();
        info.skinGroup.SetActive(false);
        GetComponent<CapsuleCollider>().isTrigger = true;
        GameObject d_effect = Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (GameManager.is3D()) {
            //3Dの場合、X軸に90°回転する。
            d_effect.transform.rotation = Quaternion.Euler(90, 0, 0);
        } else if (GameManager.dimension == DimensionType.ZWay2D) {
            d_effect.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        voices.StartCoroutine(voices.VoiceEcho(voices.DeathVoices));
    }
}
