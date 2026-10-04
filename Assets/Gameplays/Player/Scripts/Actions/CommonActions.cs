using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonActions : MonoBehaviour
{
    protected PlayerInfo info;
    [Header("共通エフェクト")]
    public GameObject sprintEffect;
    public GameObject groundPoundEffect;
    private float sprintTime = 1f;
    private bool sprint;
    protected bool underwaterPrevious = false;
    protected bool gravityControl;

    //操作等
    [Header("アクションＩＤ")]
    public sbyte actionId = 0;

    //ボイス
    protected PlayerVoiceManager voices;
    [Header("共通効果音")]
    public AudioClip throwFireball;
    public AudioClip throwIceball;
    public AudioClip throwThunderBeam;
    public AudioClip throwBoomerang;
    public AudioClip throwBubble;
    public AudioClip throwWindStorm;
    public AudioClip throwSuperBall;
    public AudioClip hipDropPre;
    public AudioClip hipDropLand;

    protected bool groundPound = false;
    protected bool continueJumping = false;

    protected bool enabledDash = true;
    protected byte dashButton = 0; //0＝Ｘ／ＲＴ　1＝Ｘのみ　2＝ＲＴのみ
    protected bool enabledGroundPound = true;
    protected bool enabledFireball = true;
    protected bool enabledToSwim = true;
    protected bool crouchButton = false;

    protected bool initialStompable = true;

    protected List<GameObject> bulletList = new List<GameObject>();
    protected float reloadTime = 0f;

    // protected bool AButton;
    // protected bool AButtonDown;
    // protected bool AButtonUp;
    // protected bool BButton;
    // protected bool BButtonDown;
    // protected bool BButtonUp;
    // protected bool XButton;
    // protected bool XButtonDown;
    // protected bool XButtonUp;
    // protected bool YButton;
    // protected bool YButtonDown;
    // protected bool YButtonUp;
    
    [Header("リング散乱")]
    public RingSpreader spreader;
    public AudioClip blueRingSound;

    [HideInInspector] public bool blueRing = false;

    // Update is called once per frame
    public void commonActions()
    {
        // AButton = info.Buttons["A"];
        // BButton = info.Buttons["B"];
        // XButton = info.Buttons["X"];
        // YButton = info.Buttons["Y"];

        //リングアタックのみ
        if (GameManager.courseMode == CourseMode.RingAttack && this.GetComponent<_16MSonic>() == null) {
            if (GameManager.Coins > 0 && (info.HP > 0 && info.HP < info.maxHP)) {
                info.HP = info.maxHP;
            } else if (info.HP > 1 && GameManager.Coins <= 0) {
                info.HP = 1;
            }
        }

        //しゃがむ
        crouchButton = info.GetCrouchButton("RB");

        if (info.underwater && enabledToSwim)
        {
            //水中
            if (info.ButtonsDown["A"])
            {
                info.Swim();
            }
        }
        else
        {
            //ダッシュ（チームマリオ＆チームパックマン＆チームロックマンのみ）
            if (enabledDash)
            {
                if (info.Grounded)
                {
                    if (
                        ((dashButton == 0 || dashButton == 1) && info.Buttons["X"]) ||
                        ((dashButton == 0 || dashButton == 2) && info.Axises["LT/RT"] >= 0.25f)
                    )
                    {
                        if (info.giant)
                        {
                            if (sprintTime > 0)
                            {
                                sprintTime = 0f;
                                info.constantChange("top", info.TopSpeed * 2.25f); //例）30f
                            }
                        } else
                        {
                            if (sprintTime > 0 && info.XZmag >= info.TopSpeed - 2.5f)
                            {
                                sprintTime -= Time.deltaTime;
                                info.constantChange("top", info.TopSpeed * (1f + (1f - sprintTime) * 0.25f));
                            }
                            if (sprintTime <= 0 && !sprint)
                            {
                                if (!info.rolling) Instantiate(sprintEffect, transform.position, info.skin.rotation);
                                sprint = true;
                                info.ForwardSetUp(Vector3.zero, info.TopSpeed * 1.5f);
                                info.constantChange("top", info.TopSpeed * 1.5f); //例）30f
                            }
                        }
                    }
                    else
                    {
                        sprint = false;
                        sprintTime = 1f;
                        if (info.giant)
                        {
                            info.constantChange("top", info.TopSpeed * 1.5f);
                        } else
                        {
                            info.constantChange("top", info.TopSpeed);
                        }
                    }
                }
                else
                {
                    if (info.XZmag > info.TopSpeed * 1.25f)
                    {
                        sprintTime = 0f;
                    }
                    else
                    {
                        sprintTime = 1f;
                    }
                    sprint = false;
                }

                if (sprint)
                {
                    info.dustTrailEffect();
                }
            }

            //ジャンプ
            if (gravityControl && !info.gravityLock)
            {
                if (info.underwater)
                {
                    if ((info.finalVelocity.y >= 20 || continueJumping) && info.Buttons["A"])
                    {
                        info.constantChange("grv", info.U_Gravity / 2f); //例）0.625f
                    }
                    else
                    {
                        info.constantChange("grv", info.U_Gravity);
                    }
                }
                else
                {
                    if ((info.finalVelocity.y >= 20 || continueJumping) && info.Buttons["A"])
                    {
                        info.constantChange("grv", info.Gravity / 2f); //例）0.625f
                    }
                    else
                    {
                        info.constantChange("grv", info.Gravity);
                    }
                }
            }
            if (info.underwater)
            {
                if (info.XZmag >= 30)
                {
                    info.constantChange("jmp", info.U_JumpForce + 1.25f); //例）39.375f
                }
                else if (info.XZmag >= 20)
                {
                    info.constantChange("jmp", info.U_JumpForce + 0.625f); //例）36.875f
                }
                else if (info.XZmag >= 10)
                {
                    info.constantChange("jmp", info.U_JumpForce + 0.3125f); //例）35.625f
                }
                else
                {
                    info.constantChange("jmp", info.U_JumpForce); //例）34.375f
                }
            }
            else
            {
                if (info.XZmag >= 30)
                {
                    info.constantChange("jmp", info.JumpForce + 2.50f); //例）39.375f
                }
                else if (info.XZmag >= 20)
                {
                    info.constantChange("jmp", info.JumpForce + 1.25f); //例）36.875f
                }
                else if (info.XZmag >= 10)
                {
                    info.constantChange("jmp", info.JumpForce + 0.625f); //例）35.625f
                }
                else
                {
                    info.constantChange("jmp", info.JumpForce); //例）34.375f
                }
            }
        }

        //ヒップドロップ（チームマリオ＆チームロックマンのみ）
        if (enabledGroundPound)
        {
            if (!info.Grounded && info.GetCrouchButtonDown("RB") && !groundPound)
            {
                groundPound = true;
                StartCoroutine("GroundPound");
            }
        }

        //ファイアボール（チームマリオ＆チームパックマン＆チームソニックのみ）
        if (enabledFireball && !info.giant && reloadTime <= 0)
        {
            if (info.ButtonsDown["X"] && info.powerUpActive == 1)
            {
                info.SoundPlay(throwFireball);

                FireBall fire = Instantiate(info.fireBall, transform.position + info.skin.forward, info.skin.rotation).GetComponent<FireBall>();
                fire.player = info;
                bulletList.Add(fire.gameObject);

                if (bulletList.Count % 2 == 0)
                {
                    reloadTime = 0.75f;
                }
            }
            if (info.ButtonsDown["X"] && info.powerUpActive == 2)
            {
                info.SoundPlay(throwIceball);

                IceBall ice = Instantiate(info.iceBall, transform.position + info.skin.forward, info.skin.rotation).GetComponent<IceBall>();
                ice.player = info;
                bulletList.Add(ice.gameObject);

                if (bulletList.Count % 2 == 0)
                {
                    reloadTime = 0.75f;
                }
            }
            if (info.ButtonsDown["X"] && info.powerUpActive == 3)
            {
                info.SoundPlay(throwThunderBeam);

                ThunderBeam beam = Instantiate(info.thunderBeam, transform.position + info.skin.forward, info.skin.rotation).GetComponent<ThunderBeam>();
                beam.player = info;
                bulletList.Add(beam.gameObject);

                if (bulletList.Count % 2 == 0)
                {
                    reloadTime = 0.75f;
                }
            }
            if (info.ButtonsDown["X"] && info.powerUpActive == 7)
            {
                info.SoundPlay(throwSuperBall);

                BounceBall ball = Instantiate(info.bounceBall, transform.position + info.skin.forward, info.skin.rotation).GetComponent<BounceBall>();
                ball.player = info;
                ball.downward = !info.Grounded;
                ball.OnStart();
                bulletList.Add(ball.gameObject);

                if (bulletList.Count % 2 == 0)
                {
                    reloadTime = 0.75f;
                }
            }
        }
        foreach (GameObject b in bulletList)
        {
            if (b == null)
            {
                bulletList.Remove(b);
            }
        }
        if (reloadTime > 0)
        {
            reloadTime -= Time.deltaTime;
            if (bulletList.Count % 2 != 0)
            {
                reloadTime = 0f;
            }
        }

        //水中判定、ドルフィンジャンプ
        if (info.underwater != underwaterPrevious)
        {
            // if (!info.underwater && info.Buttons["A"])
            // {
            //     info.YvelSetUp(31.875f);
            // }
            if (!info.underwater)
            {
                if (info.Buttons["A"])
                {
                    info.YvelSetUp(Math.Min(80f, Math.Max(31.875f, info.finalVelocity.y * 4f)));
                }
                else
                {
                    info.YvelSetUp(info.finalVelocity.y * 2f);
                }
            }
            else if (info.underwater)
            {
                info.YvelSetUp(info.finalVelocity.y / 4f);
            }
        }
        underwaterPrevious = info.underwater;
    }

    public IEnumerator GroundPound()
    {
        info.SoundPlay(hipDropPre);
        info.VelocitySetUp(Vector3.zero);
        actionId = 5;
        info.axisInput = false;
        info.activePhysics = false;
        info.Stompable = true;

        voices.GroundPound();

        yield return new WaitForSeconds(0.3f);

        info.groundAttack = true;
        info.activePhysics = true;
        info.VelocitySetUp(Vector3.up * info.TerminalVelocity);

        yield return new WaitForSeconds(0.03f);

        while (!info.Grounded && actionId == 5)
        {
            yield return new WaitForSeconds(0f);

            if (info.finalVelocity.y > 0)
            {
                info.axisInput = true;
                groundPound = false;
                actionId = 0;
                yield break;
            }
        }

        if (groundPound) {
            info.SoundPlay(hipDropLand);
            Vector3 pos = transform.position - (transform.up * (info.ColliderHeight / 2f));
            Instantiate(groundPoundEffect, pos, Quaternion.FromToRotation(Vector3.up, info.GroundNormal));

            if (info.GroundNormal != Vector3.up)
            {
                info.groundAttack = false;
                info.ForwardSetUp(Vector3.zero, 20f);
                info.rolling = true;
            }
            else
            {
                if (!info.GetCrouchButton("RB"))
                {
                    info.groundAttack = false;
                }
                yield return new WaitForSeconds(0.5f);
                if (info.groundAttack)
                {
                    while (info.GetCrouchButton("RB") && actionId == 5)
                    {
                        yield return new WaitForSeconds(0f);
                    }

                    info.groundAttack = false;
                }
            }
        }

        info.axisInput = true;
        groundPound = false;
        info.Stompable = initialStompable;
        actionId = 0;
    }
    
    public void GetChildren(GameObject obj, string layer) {
        Transform children = obj.GetComponentInChildren<Transform>();
        //子要素がいなければ終了
        if (children.childCount == 0) {
            return;
        }
        foreach(Transform ob in children) {
            //ここに何かしらの処理
            ob.gameObject.layer = LayerMask.NameToLayer(layer);
            GetChildren(ob.gameObject, layer);
        }
    }

    public void RingSpreadRingAttackOnly() {
        if (GameManager.Coins > 0) {
            uint remained = (uint)Math.Floor(GameManager.Coins * Math.Min(0.8, ((double)GameManager.Coins / 1000) + ((double)GameManager.Coins / 200) * 0.6));
            if (GameManager.extra) remained = 0;
            
            if (info.defenseUp)
            {
                if (GameManager.extra)
                {
                    remained = GameManager.Coins >= 63 ? (uint)Math.Floor((float)GameManager.Coins * 0.2f) : Math.Max(0, GameManager.Coins - 50);
                } else
                {
                    remained = (uint)Math.Floor(GameManager.Coins * Math.Min(0.6, ((double)GameManager.Coins / 250)));
                }
            }

            if (info.options.difficulty == Options.Difficulty.Easy && GameManager.Coins > 20) {
                if (info.defenseUp)
                {
                    remained = Math.Max(0, GameManager.Coins - 10);
                } else
                {
                    remained = Math.Max(0, GameManager.Coins - 20);
                }
            }

            spreader.SpreadRings(GameManager.Coins - remained, blueRing, info.options.difficulty == Options.Difficulty.Easy);
            GameManager.Coins = remained;
            blueRing = false;
        }
    }
    public void ActiveBlueRing() {
        info.SoundPlay(blueRingSound);
        blueRing = true;
    }
}
