using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using YG;
using YG.Utils.LB;


[Serializable]
public class SurvivalLeaderboardService : MonoBehaviour
{
    public const string TechnicalName = "survivalMaxTime";
    public static void RegisterCurrentRunResult()
    {
        // Don't send invalid (zero or negative) scores to leaderboard
        int ms = PlayerData.Instance.BestSurvivalTimeMilliseconds;
        if (ms <= 0)
        {
            Debug.Log($"Skip registering survival run result: best time is {ms} ms");
            return;
        }

        // YG2.SetLBTimeConvert expects seconds (float), convert from milliseconds
        float seconds = ms / 1000f;
        YG2.SetLBTimeConvert(TechnicalName, seconds);
        Debug.Log($"Registered survival run result: {ms} ms ({seconds:F3} s)");
    }
}
