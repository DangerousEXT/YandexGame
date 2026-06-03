using GameAnalyticsSDK;
using YG;
using UnityEngine;

public class InitGameAnalytics : MonoBehaviour
{
    private static bool isAnalyticsInitialized = false;
    private void Start()
    {
        if (!isAnalyticsInitialized)
        {
            var yandexId = YG2.envir.appID;
            if (!string.IsNullOrEmpty(yandexId))
                GameAnalytics.SetCustomId(yandexId);
            GameAnalytics.Initialize();
            isAnalyticsInitialized = true;
            Debug.Log("GameAnalytics инициализирован");
        }
    }
}
