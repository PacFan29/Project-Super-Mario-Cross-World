using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [Header("オプション")]
    public GameData data;
    public Options options;
    [Header("ピッチ調整用")]
    public AudioMixer master;
    [Header("ステージBGM")]
    public AudioClip stageMusic;
    public float loopBegin;
    public float loopEnd;
    [Header("その他BGM")]
    public AudioClip speedUpMusic;
    public AudioClip invincibilityMusic;
    public AudioClip cSonicInvincibilityMusic;
    public AudioClip sonicInvincibilityMusic;
    public AudioClip megaMarioMusic;
    public AudioClip metalMarioMusic;
    public AudioClip pSwitchMusic;
    public AudioClip deathMusic;
    public AudioClip drowningMusic;
    public AudioClip hurryUpMusic;
    public AudioClip marioClearMusic;
    public AudioClip pacManClearMusic;
    public AudioClip megaManClearMusic;
    public AudioClip sonicClearMusic;

    private AudioClip currentMusic;
    public static int musicIndex = 0;
    public static bool musicFade = false;
    private float musicOpacity = 1f;
    private AudioSource musicManager;
    float Ttime;

    private bool musicPaused = false;

    float[] loopValues = new float[2];

    private bool hurryUp = false;
    private bool hurryUpTrigger = false;
    private bool hurryUpPlaying = false;

    private bool clearTrigger = false;
    // Start is called before the first frame update
    void Awake()
    {
        /*
        "All-Star Rest Area" = Begin : 0.582, End : 24.372

        変数名：musicIndex
        ０＝通常
        １＝やられBGM
        ２＝止める
        ３＝溺れる！！
        ４＝スピードアップ
        ５＝無敵
        ６＝無敵
        ７＝メタルマリオ
        ８＝クリア
        ９＝巨大マリオ
        */
        hurryUpTrigger = false;
        hurryUpPlaying = false;
        clearTrigger = false;

        musicIndex = 0;
        musicFade = false;
        musicManager = GetComponent<AudioSource>();
        musicManager.clip = stageMusic;
        musicManager.Play();

        currentMusic = stageMusic;
        loopValues[0] = loopBegin;
        loopValues[1] = loopEnd;
    }

    // Update is called once per frame
    void Update()
    {
        if (musicIndex == 8 && !clearTrigger)
        {
            clearTrigger = true;
            ClearMusic(GameManager.stageClearIndex);
        }

        if (GameManager.isPaused && FindObjectsByType<WorldMapManager>(FindObjectsSortMode.None).Length <= 0)
        {
            if (!musicPaused)
            {
                musicManager.Pause();
                musicPaused = true;
            }
        }
        else
        {
            if (musicPaused)
            {
                musicManager.Play();
                musicPaused = false;
            }
        }
        if (!hurryUpPlaying)
        {
            if (musicManager.loop)
            {
                if (musicManager.pitch > 0)
                {
                    if (musicManager.time >= loopEnd)
                    {
                        musicManager.time = loopBegin;
                        musicManager.Play();
                    }
                }
                else
                {
                    if (musicManager.time > loopEnd && loopEnd > 0)
                    {
                        musicManager.time = loopEnd;
                        musicManager.Play();
                    }
                    else if (musicManager.time <= loopBegin)
                    {
                        musicManager.time = loopEnd;
                        musicManager.Play();
                    }
                }
                Ttime = musicManager.time;
                if (musicIndex == 1)
                {
                    DeathMusic();
                }
                else if (musicIndex == 2)
                {
                    MusicStop();
                }
                else if (musicIndex == 3)
                {
                    DrowningMusic();
                }
                else if (musicIndex == 4)
                {
                    SpeedUpMusic();
                }
                else if (musicIndex == 5)
                {
                    InvincibilityMusic();
                }
                else if (musicIndex == 6)
                {
                    InvincibilityMusic(true);
                }
                else if (musicIndex == 7)
                {
                    MetalMusic();
                }
                else if (musicIndex == 9)
                {
                    MegaMusic();
                }
                else if (musicIndex == 10)
                {
                    PSwitchMusic();
                }
            }
            else
            {
                if (musicIndex == 0)
                {
                    musicManager.clip = currentMusic;
                    musicManager.Play();
                    musicManager.loop = true;
                }
            }
        }

        if (musicIndex == 0)
        {
            if (musicFade)
            {
                musicOpacity -= 2 * Time.deltaTime;
            }
            else
            {
                musicOpacity += 2 * Time.deltaTime;
            }
        }
        musicOpacity = Mathf.Clamp(musicOpacity, 0f, 1f);

        // musicManager.volume = 0.7f * musicOpacity * options.musicVolume;
        musicManager.volume = 0.6f * musicOpacity;

        // if (!hurryUp && (GameManager.TotalTime > 0 && GameManager.TotalTime <= 60000 && options.hardMode)) {
        if (!hurryUp && (GameManager.TotalTime > 0 && GameManager.TotalTime <= 60000 && GameManager.courseMode == CourseMode.RingAttack))
        {
            hurryUp = true;
            if (!hurryUpTrigger)
            {
                StartCoroutine("HurryUp");
                hurryUpTrigger = true;
            }
        }
        // if (hurryUp && (GameManager.TotalTime <= 0 && GameManager.TotalTime > 60000 && options.hardMode)) {
        if (hurryUp && (GameManager.TotalTime <= 0 && GameManager.TotalTime > 60000 && GameManager.courseMode == CourseMode.RingAttack))
        {
            hurryUp = false;
            hurryUpTrigger = false;
        }
        musicManager.pitch = (hurryUp && !hurryUpPlaying && musicManager.clip != deathMusic ? 1.5f : 1.0f) * (musicIndex == 4 ? 1.25f : 1f);
        // if (musicManager.pitch > 0)
        // {
        //     master.SetFloat("BGMPitch", 1f / musicManager.pitch);
        //     // master.SetFloat("BGMFFT", 1024f * musicManager.pitch);
        // }
    }

    void DeathMusic()
    {
        musicManager.clip = deathMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = deathMusic.length;
        }
        musicManager.Play();
        musicManager.loop = false;
    }

    void MusicStop()
    {
        musicManager.Stop();
        musicManager.loop = false;
    }

    void DrowningMusic()
    {
        musicManager.clip = drowningMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = drowningMusic.length;
        }
        musicManager.Play();
        musicManager.loop = false;
    }

    void SpeedUpMusic()
    {
        // musicManager.clip = speedUpMusic;
        // if (musicManager.pitch > 0) {
        //     musicManager.time = 0f;
        // } else {
        //     musicManager.time = speedUpMusic.length;
        // }
        // musicManager.Play();
        // musicManager.loop = false;
    }
    void InvincibilityMusic(bool sonic = false)
    {
        if (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss)
        {
            musicManager.clip = sonicInvincibilityMusic;
        }
        else
        {
            musicManager.clip = (sonic ? cSonicInvincibilityMusic : invincibilityMusic);
        }
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            if (data.HUDType == GameType.Sonic || data.HUDType == GameType.SonicBoss)
            {
                musicManager.time = sonicInvincibilityMusic.length;
            }
            else
            {
                musicManager.time = (sonic ? cSonicInvincibilityMusic.length : invincibilityMusic.length);
            }
        }
        musicManager.Play();
        musicManager.loop = false;
    }
    void MetalMusic()
    {
        musicManager.clip = metalMarioMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = metalMarioMusic.length;
        }
        musicManager.Play();
        musicManager.loop = false;
    }

    void ClearMusic(int index)
    {
        AudioClip clearMusic = marioClearMusic;
        switch (index)
        {
            case 1:
            clearMusic = pacManClearMusic;
            break;
            
            case 2:
            clearMusic = megaManClearMusic;
            break;
            
            case 3:
            clearMusic = sonicClearMusic;
            break;
        }
        musicOpacity = 1f;
        musicFade = false;

        musicManager.clip = clearMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = clearMusic.length;
        }
        musicManager.loop = false;
        musicManager.Play();
    }
    void MegaMusic()
    {
        musicManager.clip = megaMarioMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = megaMarioMusic.length;
        }
        musicManager.Play();
        musicManager.loop = false;
    }
    void PSwitchMusic()
    {
        musicManager.clip = pSwitchMusic;
        if (musicManager.pitch > 0)
        {
            musicManager.time = 0f;
        }
        else
        {
            musicManager.time = pSwitchMusic.length;
        }
        musicManager.Play();
        musicManager.loop = false;
    }

    public void ChangeMusic(AudioClip music, float begin, float end)
    {
        if (clearTrigger)
        {
            return;
        }
        musicIndex = 0;
        currentMusic = music;
        loopBegin = begin;
        loopEnd = end;
        musicOpacity = 1f;
        musicFade = false;
        if (musicManager.loop)
        {
            musicManager.clip = music;
            musicManager.Play();
        }
    }
    public void ReturnMusic()
    {
        if (clearTrigger)
        {
            return;
        }
        musicIndex = 0;
        musicOpacity = 1f;
        musicFade = false;

        currentMusic = stageMusic;
        loopBegin = loopValues[0];
        loopEnd = loopValues[1];
        if (musicManager.loop)
        {
            musicManager.clip = stageMusic;
            musicManager.Play();
        }
    }

    IEnumerator HurryUp()
    {
        if (clearTrigger)
        {
            yield break;
        }
        musicManager.clip = hurryUpMusic;
        musicManager.time = 0f;
        musicManager.loop = false;
        musicManager.Play();
        hurryUpPlaying = true;
        yield return new WaitWhile(() => musicManager.isPlaying);
        hurryUpPlaying = false;
        musicManager.loop = true;
        ReturnMusic();
    }
}
