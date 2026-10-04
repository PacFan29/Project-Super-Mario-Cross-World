using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public GameData data;
    public CinemachineCamera openWorld3D;
    public CinemachineCamera linear3D;
    public CinemachineCamera XWay2D;
    public CinemachineCamera ZWay2D;

    [HideInInspector] public bool underwater;

    [Header("ＢＧＭ管理")]
    public GameObject musicManager;

    [Header("効果音")]
    public GameObject pacInvulSound;

    private List<GameObject> objList;

    public float XZLockTime = 0f;
    
    [Header("カメラの向き")]
    public Quaternion currentForceRotation = Quaternion.Euler(20f, 0f, 0f);
    public Vector3 currentForceOffset = new Vector3(0, 20f, -50f);
    public bool enableForceRotation = false;
    // Start is called before the first frame update
    void Start()
    {
        objList = new List<GameObject>();

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            // if (obj.GetComponent<PlayerInfo>() == null || obj.GetComponent<BossManager>() == null)
            if (
                obj.GetComponent<EnemyManager>() != null || 
                // obj.GetComponent<CoinManager>() != null || 
                obj.GetComponent<ItemManager>() != null || 
                obj.GetComponent<MonitorManager>() != null || 
                obj.GetComponent<ItemCapsule>() != null
                )
            {
                objList.Add(obj);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        openWorld3D.Priority.Enabled = false;
        linear3D.Priority.Enabled = false;
        XWay2D.Priority.Enabled = false;
        ZWay2D.Priority.Enabled = false;

        openWorld3D.Priority.Value = 0;
        linear3D.Priority.Value = 0;
        XWay2D.Priority.Value = 0;
        ZWay2D.Priority.Value = 0;

        if (XZLockTime > 0)
        {
            XZLockTime -= Time.deltaTime;
        }
        openWorld3D.gameObject.GetComponent<CinemachineOrbitalFollow>().enabled = XZLockTime <= 0f;
        linear3D.gameObject.GetComponent<CinemachineFollow>().enabled = XZLockTime <= 0f;
        XWay2D.gameObject.GetComponent<CinemachineFollow>().enabled = XZLockTime <= 0f;
        ZWay2D.gameObject.GetComponent<CinemachineFollow>().enabled = XZLockTime <= 0f;

        linear3D.gameObject.GetComponent<CameraController>().enableForceRotation = enableForceRotation;

        if (!GameManager.death)
        {
            if (enableForceRotation)
            {
                linear3D.Priority.Enabled = true;
                linear3D.Priority.Value = 1;
                linear3D.gameObject.GetComponent<CameraController>().currentForceRotation = currentForceRotation;
                linear3D.gameObject.GetComponent<CameraController>().currentForceOffset = currentForceOffset;
            } else
            {
                switch (GameManager.dimension)
                {
                    case DimensionType.Normal3D:
                        //3D
                        if (data.openWorldAsDefault)
                        {
                            //オープンワールド
                            openWorld3D.Priority.Enabled = true;
                            openWorld3D.Priority.Value = 1;
                        }
                        else
                        {
                            linear3D.Priority.Enabled = true;
                            linear3D.Priority.Value = 1;
                        }
                        break;

                    case DimensionType.XWay2D:
                        //2D(X方向)
                        XWay2D.Priority.Enabled = true;
                        XWay2D.Priority.Value = 1;
                        break;

                    case DimensionType.ZWay2D:
                        //2D(Z方向)
                        ZWay2D.Priority.Enabled = true;
                        ZWay2D.Priority.Value = 1;
                        break;

                    case DimensionType.FreeWay3D:
                        //3D(方向指定)
                        linear3D.Priority.Enabled = true;
                        linear3D.Priority.Value = 1;
                        break;
                }
            }
        }

        if (musicManager != null)
        {
            musicManager.GetComponent<AudioLowPassFilter>().enabled = underwater;
        }

        bool leastOnePowerPellet = false;
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<PlayerInfo>() != null)
            {
                if (player.GetComponent<PlayerInfo>().giant && player.GetComponent<PlayerInfo>().giantTrigger == 1)
                {
                    leastOnePowerPellet = true;
                }
            }
        }
        pacInvulSound.SetActive(leastOnePowerPellet);

        Vector2 viewportPoint;
        foreach (GameObject obj in objList)
        {
            if (obj == null)
            {
                objList.Remove(obj);
                break;
            }

            viewportPoint = Camera.main.WorldToViewportPoint(obj.transform.position);

            bool active = -0.25f <= viewportPoint.x && viewportPoint.x <= 1.25f
                && -0.25f <= viewportPoint.y && viewportPoint.y <= 1.25f;
            
            if (obj.GetComponent<EnemyManager>() != null)
            {
                if (obj.GetComponent<EnemyManager>().HP <= 0)
                {
                    active = true;
                }
            }
            if (obj.GetComponent<MonitorManager>() != null || obj.GetComponent<ItemCapsule>() != null)
            {
                if (obj.tag == "Untagged")
                {
                    active = true;
                }
            }
            
            obj.SetActive(active);
        }
    }
}
