using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [Header("プレイヤーデータ")]
    public GameData gameData;
    public PlayerPrefabs playerPrefabs;
    [Header("UI")]
    public CharacterSelectPlayer[] characterSelects;
    public GameObject primaryIconGroup;
    public GameObject primaryIcon;
    public Sprite lockedSprite;

    private int[] selectIndexes = new int[4];
    private float beforeAxis = 0f;

    private bool acceptable = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        selectIndexes[0] = (int)gameData.characterIds[0];
        selectIndexes[1] = (int)gameData.characterIds[1];
        selectIndexes[2] = (int)gameData.characterIds[2];
        selectIndexes[3] = (int)gameData.characterIds[3];

        foreach (Transform obj in primaryIconGroup.transform) {
            if ( 0 <= obj.gameObject.name.LastIndexOf("Clone") ) {
                Destroy(obj.gameObject);
            }
        }
        primaryIcon.transform.localPosition = new Vector2(-0.5f * primaryIcon.GetComponent<RectTransform>().sizeDelta.x * (playerPrefabs.playerData.Length - 3), 0f);
        primaryIcon.GetComponent<Image>().sprite = playerPrefabs.playerData[0].livesIcon;
        for (int i = 1; i < playerPrefabs.playerData.Length - 2; i++)
		{
			RectTransform iRect = (RectTransform)Instantiate(primaryIcon).transform;
			iRect.SetParent(primaryIconGroup.transform, false);
			iRect.localPosition = primaryIcon.transform.transform.localPosition;

			iRect.localPosition = new Vector2(
				primaryIcon.transform.localPosition.x + (primaryIcon.GetComponent<RectTransform>().sizeDelta.x * i),
				primaryIcon.transform.localPosition.y
			);

            iRect.gameObject.GetComponent<Image>().sprite = playerPrefabs.playerData[i].livesIcon;
		}
    }

    // Update is called once per frame
    void Update()
    {
        float view_axis;
        bool judge;
        (view_axis, judge) = AxisOnce("Horizontal");

        if (view_axis < 0 && judge){
            selectIndexes[0]--;
            if (selectIndexes[0] < 0) {
                selectIndexes[0] = playerPrefabs.playerData.Length - 3;
            }
        } else if (view_axis > 0 && judge){
            selectIndexes[0]++;
            if (selectIndexes[0] > playerPrefabs.playerData.Length - 3) {
                selectIndexes[0] = 0;
            }
        }
        
        if (playerPrefabs.playerData[selectIndexes[0]].unlocked) {
            characterSelects[0].index = selectIndexes[0];
            acceptable = true;
        } else {
            acceptable = false;
        }
        if (selectIndexes[1] >= 0) {
            characterSelects[1].index = selectIndexes[1];
        }
        if (selectIndexes[2] >= 0) {
            characterSelects[2].index = selectIndexes[2];
        }
        if (selectIndexes[3] >= 0) {
            characterSelects[3].index = selectIndexes[3];
        }
    }

    (float a, bool b) AxisOnce(string axis){
        bool judge = false;
        float view_axis = Input.GetAxis(axis);

        //RTが押されたら視点を変える：前フレームの入力値が0の場合のみ実施
        judge = Math.Abs(view_axis) > 0 && beforeAxis == 0.0f;
        beforeAxis = view_axis;

        return (view_axis, judge);
    }
}
