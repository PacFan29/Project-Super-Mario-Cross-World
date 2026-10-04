using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenStar : KeyItemManager
{
    [Header("スキン")]
    public GameObject[] skins = new GameObject[4];
    public GameObject[] pacManSkins = new GameObject[13];

    public override void Awake()
    {
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();

        switch (data.keyItem) {
            case Character.Mario:
            skinIndex = 0;
            volume = 1f;
            break;

            case Character.PacMan:
            skinIndex = 1;
            volume = 0.7f;
            break;

            case Character.RockMan:
            skinIndex = 2;
            volume = 1f;
            break;

            case Character.Sonic:
            skinIndex = 3;
            volume = 0.5f;
            break;
            
            case Character.Other:
            skinIndex = 4;
            volume = 0.5f;
            break;
        }
        source.clip = gotSounds[skinIndex];
        source.volume = volume;
    }

    public override void SkinManager()
    {
        foreach (GameObject obj in skins) {
            obj.SetActive(false);
        }
        skins[skinIndex].SetActive(true);

        foreach (GameObject pacman in pacManSkins) {
            pacman.SetActive(false);
        }
        if (data.greenStars.Length == 7) {
            pacManSkins[index + 6].SetActive(true);
        } else {
            pacManSkins[index].SetActive(true);
        }

        if (!gotIt) {
            isInvisible = GameManager.keyItems[index] && GameManager.courseMode != CourseMode.RingAttack;
            if (isInvisible && !invisibleTrigger) {
                void changeMaterial(Transform group) {
                    foreach (Transform child in group) {
                        Renderer[] childrenRenderer = child.gameObject.GetComponentsInChildren<Renderer>();
                        
                        if (childrenRenderer.Length < 1) {
                            changeMaterial(child);
                        } else {
                            for (int i = 0; i < childrenRenderer.Length; i++){
                                try {
                                    childrenRenderer[i].materials = new Material[1]{alreadyGotIt};
                                } catch {}
                            }
                        }
                    }
                }
                changeMaterial(group);
                effect.gameObject.SetActive(false);
                invisibleTrigger = true;
            }
        }
    }

    public override void UpdateElement()
    {
        source.pitch = GameManager.greenStarPitch;

        GameManager.keyItems[index] = true;

        if (skinIndex == 0)
        {
            GameManager.greenStarPitch += 0.05f;
        }
    }
}
