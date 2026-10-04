using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeItems : MonoBehaviour
{
    [Serializable]
    public enum ItemType
    {
        FruitTarget,
        DoubleCookie,
        SuperCookie
    }
    [Header("セット")]
    public MazeManager manager;
    public ItemType type;
    public int index;
    [Header("スキン")]
    public Transform fruitParent;
    public GameObject[] fruitPrefabs = new GameObject[10];
    private List<GameObject> fruitSkins = new List<GameObject>();
    public GameObject doubleCookie;
    public GameObject superCookie;

    [Header("時間制限")]
    public float timeLimit = 0f;

    private uint points;
    private GameObject effect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject f in fruitPrefabs)
        {
            fruitSkins.Add(Instantiate(f.transform.GetChild(0).gameObject, fruitParent.position, fruitParent.rotation, fruitParent));
        }
    }

    void FixedUpdate()
    {
        fruitParent.Rotate(0f, -3f, 0f, Space.Self);
        // fruitParent.Rotate(0f, 0f, -3f, Space.Self);
    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case ItemType.FruitTarget:
            points = fruitPrefabs[index].GetComponent<FruitTarget>().score;
            effect = fruitPrefabs[index].GetComponent<FruitTarget>().effect;
            break;
            
            case ItemType.DoubleCookie:
            points = 7650;
            effect = null;
            break;

            case ItemType.SuperCookie:
            points = 1000;
            effect = null;
            break;
        }

        fruitParent.gameObject.SetActive(type == ItemType.FruitTarget && timeLimit > 0);
        for (int i = 0; i < fruitSkins.Count; i++)
        {
            fruitSkins[i].SetActive(i == index);
        }
        doubleCookie.gameObject.SetActive(type == ItemType.DoubleCookie && timeLimit > 0);
        superCookie.gameObject.SetActive(type == ItemType.SuperCookie && timeLimit > 0);

        if (timeLimit > 0f)
        {
            timeLimit -= Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.GetComponent<MazePlayer>() != null && timeLimit > 0f){
            int combo = 0; //Good
            if (type == ItemType.FruitTarget)
            {
                if (index >= 2) {combo++;} //Great
                if (index >= 4) {combo++;} //Super
                if (index >= 6) {combo++;} //Fantastic
                if (index >= 8) {combo++;} //Excellent
            }

            timeLimit = 0f;
            col.gameObject.GetComponent<MazePlayer>().scorePopUp(points, this.transform.position, combo);

            switch (type)
            {
                case ItemType.FruitTarget:
                this.GetComponent<AudioSource>().Play();

                if (effect != null)
                {
                    GameObject e = Instantiate(effect, this.transform.position, Quaternion.identity);
                    e.SetActive(true);
                }
                break;
                
                case ItemType.DoubleCookie:
                manager.ghostRateUp = true;
                col.gameObject.GetComponent<MazePlayer>().PowerUp(true);
                break;

                case ItemType.SuperCookie:
                col.gameObject.GetComponent<MazePlayer>().SpeedUp();
                break;
            }
            // GameManager.fruits[index]++;
        }
    }
}
