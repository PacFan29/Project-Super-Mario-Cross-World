using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _00BoomBoom : BossManager
{
    [Header("専用効果音")]
    public AudioClip attackSignSound;
    public GameObject spinningSound;
    public AudioClip attackEndSound;
    public AudioClip confusedSound;
    public AudioClip recoverJumpSound;
    public AudioClip changeShellSound;
    public AudioClip shellStartSound;
    public GameObject shellSound;
    public AudioClip shellLandSound;
    public AudioClip shellHitSound;
    public AudioClip recoverShellSound;
    public AudioClip faintSound;
    [Header("専用ボイス")]
    public VoiceManager vManager;
    public AudioClip[] attackSignVoices; //インデックス：0
    public AudioClip[] attackEndVoices; //インデックス：1
    public AudioClip[] recoveryShakeVoices; //インデックス：2
    public AudioClip[] recoveryJumpVoices; //インデックス：3
    public AudioClip[] blowDamageLandVoices; //インデックス：4
    public AudioClip[] diePressDownVoices; //インデックス：5～7
    public AudioClip[] dieBlowVoices; //インデックス：8～10
    private int attackTrigger = 0;
    [Header("消えるエフェクト")]
    public GameObject vanishEffect;

    private int shellGroundCount = 0;
    private bool charging = false;
    private float spinVel = 0f;
    private float shellVel = 0f;

    private Vector3 objNomalVector = Vector3.zero;
    RaycastHit wHit;
    private float wallHitDistance = 0f;
    private Vector3 reflectForward;

    public override IEnumerator Attack()
    {
        // gravity = 1.875f;
        attackTrigger = 0;

        yield return new WaitForSeconds(0.75f);

        while (true)
        {
            actionId = 1;
            yield return new WaitForSeconds(1f);
            spinningSound.SetActive(true);
            attackTrigger = 1;
            yield return new WaitForSeconds(3f);
            spinningSound.SetActive(false);
            attackTrigger = 0;
            actionId = 2;
            yield return new WaitForSeconds(5f);
            // yield return new WaitForSeconds(1f);
            actionId = 0;
            yield return new WaitForSeconds(2f);
        }
    }
    public override void OverrideFixedUpdate() {
        gravity = 1.875f;

        Transform skin = this.transform.GetChild(0);
        Vector3 staticVel = new Vector3(0f, velocity.y, 0f);
        switch (attackTrigger)
        {
            case 1:
                skin.localRotation *= Quaternion.Euler(0f, -30f, 0f);
                velocity += FacePlayer(true);
                if (new Vector3(velocity.x, 0f, velocity.z).magnitude > 17.5f)
                {
                    velocity = SpeedReachToTop(velocity, 17.5f);
                }
                // skin.localRotation *= Quaternion.Euler(0f, -30f, 0f);
                // velocity += FacePlayer(true) * 2f;
                // if (new Vector3(velocity.x, 0f, velocity.z).magnitude > 40f)
                // {
                //     velocity = SpeedReachToTop(velocity, 40f);
                // }
                break;
                
            case 2:
            case 3:
                if (charging)
                {
                    this.transform.forward = Vector3.Lerp(this.transform.forward, FacePlayer(true), 0.5f);
                }

                if (spinVel < 30f)
                {
                    spinVel += 3f;
                }
                if (attackTrigger == 3)
                {
                    if (shellVel < 40f)
                    {
                        shellVel += 4f;
                    }
                } else
                {
                    if (shellVel > 0f)
                    {
                        shellVel -= 4f;
                    }
                }
                skin.localRotation *= Quaternion.Euler(0f, -spinVel, 0f);
                
                Vector3 shellV = this.transform.forward * shellVel;
                velocity = new Vector3(shellV.x, velocity.y, shellV.z);

                if (Physics.Raycast(this.transform.position, this.transform.forward, out wHit, 6f, GroundLayer))
                {
                    wallHitDistance = wHit.distance;
                    objNomalVector = wHit.normal;

                    if (objNomalVector.y < 0.1f)
                    {
                        // wallHit = true;
                        Vector3 reflectVec = Vector3.Reflect(this.transform.forward, objNomalVector);
                        reflectForward = reflectVec.normalized;

                        this.transform.forward = reflectForward;
                        SoundPlay(new AudioClip[1]{shellHitSound});
                    }
                }
                break;

            default:
                skin.localRotation = Quaternion.Euler(0f, 0f, 0f);
                velocity = Vector3.Lerp(velocity, staticVel, 0.5f);
                this.transform.forward = Vector3.back;
                break;
        }

        if (Grounded && shellGroundCount > 0)
        {
            velocity.y = 7.5f * shellGroundCount;
            shellGroundCount--;
            Grounded = false;
            SoundPlay(new AudioClip[1]{shellLandSound});
        }

        //ボイス
        if (vManager.voiceIndex > -1)
        {
            switch (vManager.voiceIndex)
            {
                case 0:
                    VoicePlay(attackSignVoices);
                    SoundPlay(new AudioClip[1]{attackSignSound});
                    break;
                    
                case 1:
                    VoicePlay(attackEndVoices);
                    SoundPlay(new AudioClip[1]{confusedSound});
                    break;

                case 2:
                    VoicePlay(recoveryShakeVoices);
                    break;

                case 3:
                    VoicePlay(recoveryJumpVoices);
                    break;

                case 4:
                    VoicePlay(blowDamageLandVoices);
                    break;
                    
                case 5:
                case 6:
                case 7:
                    VoicePlay(new AudioClip[1]{diePressDownVoices[vManager.voiceIndex - 5]});
                    if (vManager.voiceIndex == 6)
                    {
                        SoundPlay(new AudioClip[1]{faintSound});
                    }
                    break;
                    
                case 8:
                case 9:
                case 10:
                    VoicePlay(new AudioClip[1]{dieBlowVoices[vManager.voiceIndex - 8]});
                    if (vManager.voiceIndex == 9)
                    {
                        SoundPlay(new AudioClip[1]{faintSound});
                    }
                    break;
            }

            vManager.voiceIndex = -1;
        }
        //効果音
        if (vManager.soundIndex > -1)
        {
            switch (vManager.soundIndex)
            {
                case 0:
                    SoundPlay(new AudioClip[1]{recoverJumpSound});
                    break;
                    
                case 1:
                    SoundPlay(new AudioClip[1]{attackEndSound});
                    break;
            }
            vManager.soundIndex = -1;
        }
    }
    public override IEnumerator DamageAnimation()
    {
        spinningSound.SetActive(false);
        
        velocity = new Vector3(0f, velocity.y, 0f);
        attackTrigger = 0;
        invincible = true;
        StopCoroutine("Attack");
        if (damageTrigger == 2)
        {
            yield return new WaitForSeconds(1.85f);
        } else
        {
            yield return new WaitForSeconds(1f);
        }
        SoundPlay(new AudioClip[1]{changeShellSound});
        actionId = 3;
        yield return new WaitForSeconds(0.45f);


        //甲羅形態　着地まで待機
        spinVel = 0f;
        shellVel = 0f;
        charging = true;
        attackTrigger = 2;
        shellGroundCount = 3;
        yield return new WaitUntil(() => shellGroundCount <= 0 && Grounded);

        //チャージ
        SoundPlay(new AudioClip[1]{shellStartSound});
        yield return new WaitForSeconds(1f);

        //突進開始
        charging = false;
        velocity.y = 17.5f;
        Grounded = false;
        yield return new WaitUntil(() => Grounded);

        shellSound.SetActive(true);

        attackTrigger = 3;
        yield return new WaitForSeconds(5.25f);

        //突進終わり
        attackTrigger = 2;
        yield return new WaitForSeconds(0.75f);

        shellSound.SetActive(false);
        SoundPlay(new AudioClip[1]{recoverShellSound});

        attackTrigger = 0;
        actionId = 0;
        invincible = false;
        yield return new WaitForSeconds(0.25f);
        if (!invincible)
        {
            StartCoroutine("Attack");
        }
    }
    public override IEnumerator DefeatedAnimation(){
        StopCoroutine("DamageAnimation");
        if (damageTrigger == 2)
        {
            yield return new WaitForSeconds(2.25f);
        } else
        {
            yield return new WaitForSeconds(4f);
        }
        Instantiate(vanishEffect, this.transform.position + Vector3.down * 3f, Quaternion.Euler(-90, 0, 0));
        Destroy(gameObject);
    }
}
