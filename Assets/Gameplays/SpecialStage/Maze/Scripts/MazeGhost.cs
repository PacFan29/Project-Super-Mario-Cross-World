using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GhostType {
    Blinky,
    Pinky,
    Inky,
    Clyde,
    Sue,
    Funky,
    Spunky
}

public class MazeGhost : MazeCharacter
{
    private Transform targetPlayer;
    [Header("ゴーストの小屋")]
    // public Transform ghostHouse;
    public GhostHouse ghostHouse;
    [Header("分散ターゲット")]
    public Transform scatterTarget;
    [Header("ゴーストの性格")]
    public GhostType ghostType;

    public enum GhostMode
    {
        Normal,
        Frightened,
        Eaten
    }
    [Header("ゴーストの状態")]
    public GhostMode ghostMode;

    private int turnStep = 0;
    private bool triggerState = true;

    private Vector3 targetPos;
    private Vector3 targetPos2;

    private bool inGhostHouse = false;


    public float confusedTime = 0f;

    public bool isReadyToOut = true;
    private int inOutTrigger = 0; //0＝無、1＝小屋から出る、-1＝小屋に戻る
    [Header("エフェクト")]
    public GameObject effect;

    void Update()
    {
        int giantCount = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player.GetComponent<MazePlayer>() != null) {
                if (targetPlayer == null) {
                    targetPlayer = player.transform;
                }
                if (player.GetComponent<MazePlayer>().isInvincible)
                {
                    giantCount++;
                }
            }
        }

        //小屋判定
        inGhostHouse = 
            this.transform.position.x >= ghostHouse.GetLeft() && 
            this.transform.position.x <= ghostHouse.GetRight() && 
            this.transform.position.z <= ghostHouse.GetTop() && 
            this.transform.position.z >= ghostHouse.GetBottom();

        if (isReadyToOut)
        {
            stay = inGhostHouse && Vector3.Distance(this.transform.position, inititalPosition) <= 1.5f && ghostMode == GhostMode.Frightened;
        } else
        {
            stay = true;
        }

        //スピード調整
        if (confusedTime > 0f)
        {
            confusedTime -= Time.deltaTime;
            speed = 0f;
        } else
        {
            switch (ghostMode)
            {
                case GhostMode.Normal:
                    if ((ghostType == GhostType.Blinky && manager.almostThere) || ghostType == GhostType.Sue)
                    {
                        speed = 25f;
                    } else
                    {
                        speed = 22.5f;
                    }
                    break;
                    
                case GhostMode.Frightened:
                    speed = 10f;
                    break;

                case GhostMode.Eaten:
                    speed = 50f;
                    break;
            }
        }

        //ジャンプの高さ制限
        if (ghostType == GhostType.Funky)
        {
            yVelRate = 0.475f;
        } else
        {
            yVelRate = 1f;
        }

        //ターゲットの特定
        float minDis = Mathf.Infinity;
        int dirIndex = 0;
        List<int> randomDirList = new List<int>();
        for (int i = 0; i < dirs.Length; i++) {
            float distance = GetDistance(targetPlayer, dirs[i]);
            if (distance < minDis) {
                minDis = distance;
                dirIndex = i;
            }
            if (dirs[i] != -direction)
            {
                randomDirList.Add(i);
            }
        }

        //1人でも巨大になっていたらイジケる
        if (manager.giantCount > 0 && ghostMode == GhostMode.Normal)
        {
            TurnAround();
            ghostMode = GhostMode.Frightened;
        } else if (manager.giantCount <= 0 && ghostMode == GhostMode.Frightened)
        {
            ghostMode = GhostMode.Normal;
        }


        //イジケ状態の時の行先ランダム
        int randomIndex = randomDirList[UnityEngine.Random.Range(0, randomDirList.Count)];
        if (ghostMode == GhostMode.Frightened && !inGhostHouse)
        {
            dirIndex = randomIndex;
        }

        //曲がるか反対に向くか
        if (turnStep > 0)
        {
            turnStep--;
        }
        if (inOutTrigger == 0)
        {
            // Debug.Log(Vector3.Distance(this.transform.position, ghostHouse.GetOutPosition()));
            if (inGhostHouse)
            {
                if (ghostMode != GhostMode.Frightened && Vector3.Distance(this.transform.position, ghostHouse.GetOutPosition()) <= 3f && isReadyToOut)
                {
                    inOutTrigger = 1;
                }
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i] != -direction && dirs[i] != direction && turnStep <= 0)
                {
                    if (i == dirIndex && canRotate(dirs[i]))
                    {
                        //曲がる
                        if (!inGhostHouse)
                        {
                            gridPos();
                        }
                        direction = dirs[i];
                        turnStep = (int)(15 * Math.Ceiling(22.5f / speed));
                        break;
                    }
                    if (new Vector3(velocity.x, 0f, velocity.z) == Vector3.zero && canRotate(dirs[i]) && i != dirIndex)
                    {
                        //行き止まりだったら反対側に
                        if (!inGhostHouse)
                        {
                            gridPos();
                        }
                        direction = dirs[i];
                        turnStep = (int)(15 * Math.Ceiling(22.5f / speed));
                        break;
                    }
                }
            }
        } else
        {
            if (inOutTrigger == 1)
            {
                //小屋から出る
                this.transform.position = new Vector3(ghostHouse.GetBasePosition().x, this.transform.position.y, this.transform.position.z);
                rb.position = this.transform.position;

                direction = dirs[0];
                turnStep = (int)(15 * Math.Ceiling(22.5f / speed));
                if (Vector3.Distance(this.transform.position, ghostHouse.GetBasePosition()) <= 2.25f)
                {
                    inOutTrigger = 0;
                }
            } else
            {
                //小屋に戻る
                this.transform.position = new Vector3(ghostHouse.GetBasePosition().x, this.transform.position.y, this.transform.position.z);
                rb.position = this.transform.position;

                direction = dirs[2];
                turnStep = (int)(15 * Math.Ceiling(22.5f / speed));

                if (Vector3.Distance(this.transform.position, ghostHouse.GetOutPosition()) <= 2.25f)
                {
                    if (manager.giantCount > 0)
                    {
                        ghostMode = GhostMode.Frightened;
                    } else
                    {
                        ghostMode = GhostMode.Normal;
                    }
                    inOutTrigger = 0;
                }
            }
        }
        this.GetComponent<CapsuleCollider>().enabled = inOutTrigger == 0;

        // if (Vector3.Distance(this.transform.position, ghostHouse.position) <= 2f && ghostMode == GhostMode.Eaten)
        if (
            Vector3.Distance(this.transform.position, ghostHouse.GetBasePosition()) <= 3f 
            && ghostMode == GhostMode.Eaten
            && inOutTrigger == 0)
        {
            inOutTrigger = -1;
            // if (manager.giantCount > 0)
            // {
            //     ghostMode = GhostMode.Frightened;
            // } else
            // {
            //     ghostMode = GhostMode.Normal;
            // }
        }
    }

    float GetDistance(Transform target, Vector2 offset) {
        Vector3 playerPosition = target.position;
        Vector3 start = this.transform.position + (new Vector3(offset.x, 0, offset.y) * tilePixel);
        targetPos2 = playerPosition;

        Transform blinkyType = null;
        foreach (MazeGhost g in FindObjectsByType<MazeGhost>(FindObjectsSortMode.None))
        {
            if (g.ghostType == GhostType.Blinky)
            {
                blinkyType = g.gameObject.transform;
            }
        }

        if (inOutTrigger == 1)
        {
            //小屋から出る
            playerPosition = ghostHouse.GetBasePosition();
        } else if (inOutTrigger == -1)
        {
            //小屋に戻る
            playerPosition = ghostHouse.GetOutPosition();
        } else
        {
            if (inGhostHouse)
            {
                if (ghostMode == GhostMode.Frightened)
                {
                    //イジケ状態のときに小屋に戻ると、閉じこもる。
                    playerPosition = inititalPosition;
                } else
                {
                    playerPosition = ghostHouse.GetOutPosition();
                }
            } else
            {
                if (ghostMode == GhostMode.Eaten)
                {
                    // playerPosition = ghostHouse.position;
                    playerPosition = ghostHouse.GetBasePosition();
                } else
                {
                    if (manager.ghostState == MazeManager.GhostState.Chase)
                    {
                        switch (ghostType)
                        {
                            case GhostType.Pinky:
                                playerPosition = target.position + target.gameObject.GetComponent<MazeCharacter>().skin.forward * tilePixel * 2f;
                                targetPos2 = playerPosition;
                                break;
                            
                            case GhostType.Inky:
                                if (blinkyType != null)
                                {
                                    playerPosition = (target.position + target.gameObject.GetComponent<MazeCharacter>().skin.forward * tilePixel) * 2 - blinkyType.position;
                                    targetPos2 = target.position + target.gameObject.GetComponent<MazeCharacter>().skin.forward * tilePixel;
                                }
                                break;

                            case GhostType.Clyde:
                                if (Vector3.Distance(playerPosition, start) <= 25f)
                                {
                                    playerPosition = scatterTarget.position;
                                } else
                                {
                                    playerPosition = target.position;
                                }
                                targetPos2 = target.position;
                                break;
                            
                            case GhostType.Spunky:
                                if (!Grounded)
                                {
                                    playerPosition = blinkyType.position;
                                }
                                break;
                        }
                    } else
                    {
                        playerPosition = scatterTarget.position;
                        targetPos2 = playerPosition;
                    }
                }
            }
        }

        targetPos = playerPosition;
        
        return Vector3.Distance(playerPosition, start);
    }

    public void TurnAround()
    {
        if (confusedTime > 0)
        {
            return;
        }
        if (ghostMode == GhostMode.Normal)
        {
            direction = -direction;
        }
    }

    void OnTriggerStay(Collider col){
        if (col.gameObject.GetComponent<MazePlayer>() != null && triggerState && !stop && SpecialStageManager.ready){
            if (ghostMode == GhostMode.Frightened) {
                triggerState = false;
                StartCoroutine("GetEaten");

                manager.ghostCombo++;
                uint pts = 400 * (uint)Math.Pow(2, Math.Max(0, Math.Min(4, manager.ghostCombo - 1)));
                if (manager.ghostRateUp)
                {
                    //800(Good) → 3200(Super) → 12800(Excellent) → 12800(Wonderful) ...
                    pts = 800 * (uint)Math.Pow(4, Math.Max(0, Math.Min(2, manager.ghostCombo - 1)));
                } else
                {
                    if (manager.ghostCombo >= 5) {
                        //7650点
                        pts += 1250;
                    }
                }
                col.gameObject.GetComponent<MazePlayer>().scorePopUp(
                    pts, 
                    this.transform.position, 
                    manager.ghostRateUp ? (manager.ghostCombo - 1) * 2 : manager.ghostCombo - 1
                );
            } else if (ghostMode == GhostMode.Normal) {
                //ミス
                StartCoroutine(col.gameObject.GetComponent<MazePlayer>().LostLife());
            }
        }
    }

    IEnumerator GetEaten()
    {
        hide = true;
        var efMain = Instantiate(effect, this.transform.position, Quaternion.Euler(-90, 0, 0)).GetComponent<ParticleSystem>().main;
        efMain.useUnscaledTime = true;

        foreach (Transform other in effect.transform) {
            var otherEff = other.gameObject.GetComponent<ParticleSystem>().main;
            otherEff.useUnscaledTime = true;
        }

        this.GetComponent<AudioSource>().Play();
        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(0.5f);

        hide = false;
        triggerState = true;
        Time.timeScale = 1f;
        ghostMode = GhostMode.Eaten;
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + new Vector3(1, 0, 0) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(transform.position + new Vector3(-1, 0, 0) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0, 0, 1) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0, 0, -1) * tilePixel * 0.75f, Vector3.one * tilePixel * 0.75f);

        if (ghostMode != GhostMode.Frightened)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPos, tilePixel * 0.25f);
        }

        if (ghostMode == GhostMode.Normal)
        {
            if (ghostType == GhostType.Inky)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(targetPos2, tilePixel * 0.25f);
            }
            if (ghostType == GhostType.Clyde)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(this.transform.position, 25f);
            }
        }
    }

    public void BusterHit()
    {
        confusedTime = 2f;
    }

    public void GhostJump()
    {
        if ((ghostType != GhostType.Funky && ghostType != GhostType.Spunky) || ghostMode != GhostMode.Normal)
        {
            return;
        }

        Grounded = false;
        jumpVel = 40f * yVelRate;
    }
}
