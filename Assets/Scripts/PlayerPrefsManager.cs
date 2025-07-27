using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsManager
{
    private const string TUTORIAL_COMPLETED_KEY = "HasCompletedTutorial";
    private const string FIRST_TIME_KEY = "FirstTime";
    public static bool HasCompletedTutorial
    {
        get => PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
        set => PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, value ? 1 : 0);
    }

    public static bool IsFirstTime
    {
        get => PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1;
        set => PlayerPrefs.SetInt(FIRST_TIME_KEY, value ? 1 : 0);
    }
}
