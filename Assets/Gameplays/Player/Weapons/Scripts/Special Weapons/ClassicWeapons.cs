using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicWeapons : WeaponMovements
{
    /*
    変数index
    0＝ファイヤーストーム
    1＝アイススラッシャー
    2＝サンダービーム
    3＝クイックブーメラン
    4＝バブルリード
    */
    [Header("武器インデックス")]
    public int index = 0;
    [Header("サンダービームエフェクト")]
    public Transform thunder1;
    public Transform thunder2;
    private Vector3 startPos1;
    private Vector3 startPos2;
    private Vector3 goalPos1;
    private Vector3 goalPos2;
    private float tBeamTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        switch (index)
        {
            case 0:
                weaponType = WeaponTypes.Fire;
                velocity = this.transform.forward * 50f;
                StartCoroutine("LifeTime");
                break;

            case 1:
                weaponType = WeaponTypes.Ice;
                velocity = this.transform.forward * 50f;
                StartCoroutine("LifeTime");
                break;

            case 2:
                weaponType = WeaponTypes.Thunder;
                velocity = this.transform.forward * 60f;
                StartCoroutine("LifeTime");
                break;

            case 3:
                weaponType = WeaponTypes.Boomerang;
                break;

            case 4:
                weaponType = WeaponTypes.Water;
                break;
        }

        startPos1 = Vector3.zero;
        startPos2 = Vector3.zero;
        goalPos1 = new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f));
        goalPos2 = new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f));
    }

    // Update is called once per frame
    void Update()
    {
        if (thunder1 != null && thunder2 != null)
        {
            tBeamTime += 100 * Time.deltaTime;
            if (tBeamTime >= 1f)
            {
                startPos1 = goalPos1;
                startPos2 = goalPos2;
                goalPos1 = new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f));
                goalPos2 = new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f));
                tBeamTime = 0f;
            }
            thunder1.localPosition = Vector3.Lerp(startPos1, goalPos1, tBeamTime);
            thunder2.localPosition = Vector3.Lerp(startPos2, goalPos2, tBeamTime);
        }
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(2f);

        if (isAttacking)
        {
            Destroy(gameObject);
        }
    }
}
