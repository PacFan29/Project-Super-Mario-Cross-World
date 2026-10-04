using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("オプション")]
    public Options options;
    [Header("AudioMixer")]
    public AudioMixer master;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        master.SetFloat("BGM", ConvertVolume2dB(options.musicVolume));
        master.SetFloat("SE", ConvertVolume2dB(options.soundVolume));
        master.SetFloat("Voice", ConvertVolume2dB(options.voiceVolume));
    }

    float ConvertVolume2dB(float volume)
    {
        return Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp(volume, 0f, 1f)), -80f, 0f);
    }
}
