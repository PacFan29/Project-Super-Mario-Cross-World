using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    // new CinemachineVirtualCamera camera;
    // CinemachineOrbitalTransposer transposer;
    CinemachineCamera cam;
    Transform targetPlayer;
    // Vector3 cameraRotation;

    // public bool is3D;
    public enum CameraMode
    {
        OpenWorld3D,
        Linear3D,
        XWay2D,
        ZWay2D
    }
    [Header("カメラの挙動")]
    public CameraMode cameraMode;

    private Quaternion defaultRotation = Quaternion.Euler(20f, 0f, 0f);
    private Vector3 defaultOffset = new Vector3(0, 20f, -50f);
    private Vector3 default3DOffset = new Vector3(0, 20f, -50f);
    private Vector3 defaultXWay2DOffset = new Vector3(0, 0f, -55f);
    private Vector3 defaultZWay2DOffset = new Vector3(55f, 0f, 0f);
    [Header("カメラの向き")]
    public Quaternion currentForceRotation = Quaternion.Euler(20f, 0f, 0f);
    public Vector3 currentForceOffset = new Vector3(0, 20f, -50f);
    public bool enableForceRotation = false;

    private Quaternion cameraRotation;
    private Vector3 cameraOffset;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        // camera = GetComponent<CinemachineVirtualCamera>();
        // if (camera.GetCinemachineComponent<CinemachineOrbitalTransposer>() != null) {
        //     transposer = camera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        // }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PlayerInfo[] allPlayers = GameObject.FindObjectsByType<PlayerInfo>(FindObjectsSortMode.None);
        for (int i = 0; i < allPlayers.Length; i++){
            //1P着目
            if (allPlayers[i].playerNumber == 0 && allPlayers[i].primary){
                targetPlayer = allPlayers[i].gameObject.transform;
            }
        }

        BossManager[] goArray = GameObject.FindObjectsByType<BossManager>(FindObjectsSortMode.None);
        Transform bossObj = goArray.Length > 0 ? goArray[0].gameObject.transform : null;
        
        // if (is3D && (!GameManager.death || GameManager.falling)) {
        if (cameraMode == CameraMode.OpenWorld3D && (!GameManager.death || GameManager.falling)) {
            cam.LookAt = targetPlayer;
        } else if (GameManager.death) {
            cam.LookAt = null;
        } else {
            // if (bossObj != null) {
            //     camera.LookAt = bossObj;
            // } else {
            //     camera.LookAt = null;
            // }
            cam.LookAt = null;
        }

        if (GameManager.death) {
            cam.Follow = null;
        } else {
            cam.Follow = targetPlayer;

            // cameraRotation.x = Input.GetAxisRaw("Camera X") * 600;
            // cameraRotation.z = Input.GetAxisRaw("Camera Y") * 70;

            // if (cameraRotation.magnitude >= 0.1 && transposer != null && is3D){
            //     transposer.m_Heading.m_Bias += cameraRotation.x * 3f; //Biasを操作
            //     transposer.m_FollowOffset.y -= cameraRotation.z / 8f; //Follow Offsetを操作

            //     if (transposer.m_FollowOffset.y < -12f) {
            //         transposer.m_FollowOffset.y = -12f;
            //     } else if (transposer.m_FollowOffset.y > 40f) {
            //         transposer.m_FollowOffset.y = 40f;
            //     }
            // }
        }

        switch (cameraMode)
        {
            case CameraMode.Linear3D:
            cameraRotation = Quaternion.Euler(
                Math.Abs(Input.GetAxis("Camera Y")) >= 0.025f ? (Input.GetAxis("Camera Y") - 0.025f * Math.Sign(Input.GetAxis("Camera Y"))) / 0.075f * 12.5f : 0f, 
                Math.Abs(Input.GetAxis("Camera X")) >= 0.025f ? (Input.GetAxis("Camera X") - 0.025f * Math.Sign(Input.GetAxis("Camera X"))) / 0.075f * 12.5f : 0f, 
                0f
            );
            defaultOffset = default3DOffset;
            break;
            
            case CameraMode.XWay2D:
            defaultOffset = defaultXWay2DOffset;
            break;

            case CameraMode.ZWay2D:
            defaultOffset = defaultZWay2DOffset;
            break;
        }
        cameraRotation *= enableForceRotation ? currentForceRotation : defaultRotation;
        cameraOffset = enableForceRotation ? currentForceOffset : defaultOffset;

        if (cameraMode != CameraMode.OpenWorld3D)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, cameraRotation, 0.1f);
            this.GetComponent<CinemachineFollow>().FollowOffset = Vector3.Lerp(this.GetComponent<CinemachineFollow>().FollowOffset, cameraOffset, 0.1f);
        }

        /*
        Yrotation 0 = Offset (0, 10, -25)
        Yrotation 90 = Offset (-25, 10, 0)
        Yrotation 180 = Offset (0, 10, 25)
        Yrotation -90 = Offset (25, 10, 0)

        Offset Y = X or Z * 0.4f
        */
    }
}
