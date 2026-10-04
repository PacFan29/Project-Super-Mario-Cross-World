using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MovingPlatformParent
{
    //Code by codinginflow(GitHub)
    private GameObject[] waypoints;
    public float speed = 1f;
    public float waitTime = 0f;
    float waitingTime = 0f;

    int currentWaypointIndex = 0;

    public enum PlatformType
    {
        AlwaysMoving, //常に動いている
        WhenYouRideMove, //乗ると動く
        MoveWhileRiding, //乗っている間は動く
        MoveWhenNotRiding //乗っていない間は動く
    }
    public PlatformType platformType;

    bool moveTrigger = false;

    void Start()
    {
        int length = this.transform.parent.transform.childCount - 1;
        waypoints = new GameObject[length];

        for (int i = 1; i <= length; i++) {
            waypoints[i-1] = this.transform.parent.GetChild(i).gameObject;
        }
        waitingTime = 0f;
    }

    // Update is called once per frame
    void Update() {
        bool isRiding = playerList.Count > 0;
        switch (platformType)
        {
            case PlatformType.AlwaysMoving:
            moveTrigger = true;
            break;

            case PlatformType.WhenYouRideMove:
            if (isRiding)
            {
                moveTrigger = true;
            }
            break;
            
            case PlatformType.MoveWhileRiding:
            moveTrigger = isRiding;
            break;
            
            case PlatformType.MoveWhenNotRiding:
            moveTrigger = !isRiding;
            break;
        }

        if (waitingTime > 0f)
        {
            waitingTime -= Time.deltaTime;
            return;
        }
        if (!moveTrigger)
        {
            return;
        }
        if (waypoints[currentWaypointIndex] != null) {
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position) < 0.1f){
                currentWaypointIndex++;
                waitingTime = waitTime;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
        }
    }

    // void OnCollisionEnter(Collision other) {
    //     if (other.gameObject.GetComponent<PlayerInfo>() != null) {
    //         other.gameObject.transform.parent = transform;
    //     }
    // }
    // void OnCollisionExit(Collision other) {
    //     if (other.gameObject.GetComponent<PlayerInfo>() != null) {
    //         other.gameObject.transform.parent = null;
    //     }
    // }
}
