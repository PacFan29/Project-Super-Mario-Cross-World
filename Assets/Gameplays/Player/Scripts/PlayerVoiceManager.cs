using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVoiceManager : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] JumpVoices;
    public AudioClip[] AttackVoices;
    public AudioClip[] SuperAttackVoices;
    public AudioClip[] GroundPoundVoices;
    public AudioClip[] ExcellentVoices;
    public AudioClip[] ClearVoices;
    public AudioClip[] HurtVoices;
    public AudioClip[] BurningVoices;
    public AudioClip[] DeathVoices;
    public AudioClip[] DrownVoices;
    public AudioClip[] FallenVoices;
    public AudioClip[][] OtherVoices;

    private bool enableVoice = true;

    // private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        // source = GetComponent<AudioSource>();
        enableVoice = true;
    }

    public void Jump(){
        VoicePlay(JumpVoices);
    }
    public void Attack(){
        VoicePlay(AttackVoices);
    }
    public void SuperAttack(){
        VoicePlay(SuperAttackVoices);
    }
    public void GroundPound(){
        VoicePlay(GroundPoundVoices, true);
    }
    public void Clear(){
        VoicePlay(ClearVoices);
    }
    public void Hurt(){
        VoicePlay(HurtVoices, true);
    }
    public void Burning(){
        VoicePlay(BurningVoices, true);
    }
    public void Death(){
        VoicePlay(DeathVoices, true);
    }
    public void Fallen(){
        VoicePlay(FallenVoices, true);
    }
    public void Excellent(){
        VoicePlay(ExcellentVoices, true);
    }
    public void Other(byte index) {
        VoicePlay(OtherVoices[index]);
    }

    public void VoicePlay(AudioClip[] voices, bool force = false) {
        if (voices.Length < 1) return;
        
        byte voiceIndex = (byte)UnityEngine.Random.Range(0, voices.Length);

        if (voices[voiceIndex] != null && (enableVoice || force)) {
            source.clip = voices[voiceIndex];
            source.Play();
            StartCoroutine(VoicePlaying(voices[voiceIndex].length));
        }
    }

    public void VoiceStop()
    {
        source.Stop();
        enableVoice = true;
    }

    public IEnumerator VoiceEcho(AudioClip[] voices) {
        if (voices.Length < 1) yield break;

        byte voiceIndex = (byte)UnityEngine.Random.Range(0, voices.Length);

        for (byte i = 4; i > 0; i--) {
            float volume = 0.25f * i;
            source.PlayOneShot(voices[voiceIndex], volume);

            yield return new WaitForSeconds(0.7f);
        }
    }

    IEnumerator VoicePlaying(float length)
    {
        enableVoice = false;
        yield return new WaitForSeconds(length);
        enableVoice = true;
    }

    void OnDisable()
    {
        enableVoice = true;
    }
}
