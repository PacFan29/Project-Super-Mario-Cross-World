using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedCoinCount : MonoBehaviour
{
    [Header("スプライト")]
    public SpriteRenderer main;
    public Sprite[] sprites = new Sprite[8];
    [Header("カウント")]
    public int count;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        int index = Math.Min(sprites.Length, Math.Max(0, count - 1));

        main.sprite = sprites[index];

        this.GetComponent<Animator>().SetBool("Perfect", count == 8);
    }
    
    public void destroyObject()
    {
        Destroy(gameObject);
    }
}
