using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _00Suzie : EnemyMovements
{
    public enum Direction { up, down, left, right };
    public Direction initialDirection = Direction.up;
    private Direction dir;
    public GameObject[] eyelids = new GameObject[2];
    private int eyelidTrigger = 0;

    void Awake()
    {
        dir = initialDirection;
        eyelidTrigger = 0;
    }

    public override void Movements()
    {
        RaycastHit hitInfo;
        switch (action)
        {
            case 0:
                manager.velocity = 15f * GetAxis(dir);
                Physics.Raycast(this.transform.position, GetAxis(dir), out hitInfo, 2.25f, manager.GroundLayer);
                if (hitInfo.collider != null)
                {
                    action = 1;
                }
                eyelidTrigger = 0;
                break;

            case 1:
                time += Time.deltaTime;
                if (time <= 0.05f || time >= 1.45f)
                {
                    eyelidTrigger = 1;
                }
                else
                {
                    eyelidTrigger = 2;
                }
                if (time >= 1.5f)
                {
                    switch (dir)
                    {
                        case Direction.up:
                            dir = Direction.down;
                            break;
                        case Direction.down:
                            dir = Direction.up;
                            break;
                        case Direction.left:
                            dir = Direction.right;
                            break;
                        case Direction.right:
                            dir = Direction.left;
                            break;
                    }
                    time = 0f;
                    action = 0;
                }
                break;
        }

        eyelids[0].SetActive(eyelidTrigger == 1);
        eyelids[1].SetActive(eyelidTrigger == 2);
    }

    public Vector3 GetAxis(Direction refAxis)
    {
        switch (refAxis)
        {
            case Direction.down:
                return -transform.up;
            case Direction.left:
                return -transform.right;
            case Direction.right:
                return transform.right;
        }

        // default is Vector3.up
        return transform.up;
    }

    // void OnCollisionStay(Collision col){
    //     if (LayerMask.LayerToName(col.gameObject.layer) == "Default" && action == 0) {
    //         RaycastHit hitInfo;
    //         Physics.Raycast(this.transform.position, GetAxis(dir), out hitInfo, 1f, manager.GroundLayer);
    //         if (hitInfo.collider != null)
    //         {
    //             action = 1;
    //         }
    //     }
    // }
}
