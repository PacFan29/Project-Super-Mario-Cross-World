using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpreader : MonoBehaviour
{
    public GameObject ringLoss;
    private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }
    
    public void SpreadRings(uint rings, bool hyper, bool easyMode) {
        source.Play();

        int maxHalfRings = easyMode ? 10 : 16;

        uint ring_amount = 1;
        byte ring_counter = 0;
        // float ring_angle = 11.25f;
        float ring_angle = 360f / (maxHalfRings * 2);
        bool flip = false;
        float speed = 20f;

        if (hyper) {
            ring_angle = 22.5f;
            if (rings > 8) ring_amount = (uint)Math.Floor((float)rings / 8f);
        }

        while (ring_counter < rings) {
            Vector3 yUp = hyper ? Vector3.up * 2f : Vector3.zero;

            MovingRingManager oneRing = Instantiate(ringLoss, transform.position + yUp, Quaternion.identity).GetComponent<MovingRingManager>();
            oneRing.angle = ring_angle;
            oneRing.speed = speed;
            if (hyper) {
                oneRing.amount = ring_amount;
                oneRing.hyper = true;
            }
            oneRing.setSpeed();

            ring_angle *= -1;
            if (flip) {
                if (hyper) {
                    ring_angle += 45f;
                } else {
                    // ring_angle += 22.5f;
                    ring_angle += 360f / maxHalfRings;
                }
            }
            flip = !flip;

            ring_counter++;
            
            if (hyper && ring_counter == 8) {
                break;
            } else if (ring_counter == maxHalfRings) {
                speed = 10f;
                // ring_angle = 11.25f;
                ring_angle = 360f / (maxHalfRings * 2);
            } else if (ring_counter == maxHalfRings * 2) {
                break;
            }
        }
    }
}
