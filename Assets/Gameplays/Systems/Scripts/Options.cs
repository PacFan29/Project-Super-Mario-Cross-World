using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Options", menuName = "ScriptableObjects/Options")]
public class Options : ScriptableObject
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
    public enum HUDOptions
    {
        Normal,
        AlwaysShow,
        AlwaysHide
    }
    public enum TimeFormat
    {
        Quotations,
        ColonsOnly,
        ColonsAndPeriod,
        QuotationsMilli3,
        ColonsOnlyMilli3,
        ColonsAndPeriodMilli3,
        MinutesSecondsOnly
    }
    public enum ScorePopUp
    {
        Normal,
        Full,
        NormalExceptSonic,
        FullExceptSonic,
        None
    }
    [Header("サウンド")]
    public float musicVolume;
    public float soundVolume;
    public float voiceVolume;

    [Header("難易度")]
    // public bool beginnerMode;
    // public bool hardMode;
    public Difficulty difficulty;
    [Header("HUD")]
    public HUDOptions hudOptions;
    public bool livesCoinsCross;
    public bool comma;
    public TimeFormat timeFormat;
    public ScorePopUp scorePopUp;
    public bool bossHealthBar;
    [Header("残機の有無")]
    public bool lives = false;
}
