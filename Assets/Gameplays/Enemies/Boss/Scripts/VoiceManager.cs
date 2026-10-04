using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    // private AudioSource source;
    public int voiceIndex = -1;
    public int soundIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // source = GetComponent<AudioSource>();
        voiceIndex = -1;
        soundIndex = -1;
    }

    // Update is called once per frame
    // public void VoicePlay(AudioClip voiceClip) {
    //     if (voiceClip != null) {
    //         // int voiceIndex = UnityEngine.Random.Range(0, voiceClips.Length);

    //         //効果音の再生
    //         // source.clip = voiceClips[voiceIndex];
    //         source.clip = voiceClip;
    //         source.Play();
    //     }
    // }
    public void VoicePlay(int index)
    {
        voiceIndex = index;
    }
    public void SoundPlay(int index)
    {
        soundIndex = index;
    }
}
