using System;
using System.Collections.Generic;
using UnityEngine;
using MAXHelper;

public class AnalyticsManager : MonoBehaviour
{
    [SerializeField] private bool showDebugs = true;

    private static AnalyticsManager instance;
    public static AnalyticsManager Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SubscribeForAdsEvents()
    {
        AdsManager.Instance.OnAdShown += (info) =>
        {
            if (info.AdType != AdsManager.EAdType.REWARDED || (info.HasInternet && info.Availability == "watched"))
                LogAds("video_ads_watch", info);
        };
        AdsManager.Instance.OnAdAvailable += (info) => LogAds("video_ads_available", info);
        AdsManager.Instance.OnAdStarted += (info) => LogAds("video_ads_started", info);
    }

    private void LogAds(string eventString, AdInfo info) => LogAds(eventString, info.AdType.ToString(), info.Placement, info.Availability, info.HasInternet);
    private void LogAds(string eventString, string ad_type, string placement, string result, bool connection)
    {
        var sectionString = eventString;
        var data = new Dictionary<string, object>();
        data["ad_type"] = ad_type;
        data["placement"] = placement;
        data["result"] = result;
        data["connection"] = connection;

        AppMetrica.Instance.ReportEvent(sectionString, data);

        OnAnalyticSent($"{sectionString} | {data}");
    }

    public void Log(AnalyticSectionType section, string eventText)
    {
        AppMetricLog(section.ToString(), eventText);
    }

    private void AppMetricLog(string sectionString, string eventString)
    {
        var data = new Dictionary<string, object>();
        data[eventString] = null;
        AppMetrica.Instance.ReportEvent(sectionString, data);

        OnAnalyticSent($"{sectionString} | {eventString}");
    }

    private void OnAnalyticSent(string debugMessage)
    {
        if (showDebugs) Debug.Log("[analytic] " + debugMessage);
    }
}

[Serializable]
public struct AdAnalyticData
{
    public string ad_type;
    public string placement;
    public string result;
    public bool connection;

    public AdAnalyticData(string ad_type, string placement, string result, bool connection)
    {
        this.ad_type = ad_type;
        this.placement = placement;
        this.result = result;
        this.connection = connection;
    }
}

public enum AnalyticSectionType
{
    Tutorial,
    ArmyUpgrade,
    SpyingOnCountries,
    CountriesTakeovers,
    Economics,
    InApp,
    Misc,
    Ads
}