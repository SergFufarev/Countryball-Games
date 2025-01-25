using System;
using UnityEngine;
using FunnyBlox;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

public class RateUsController : MonoBehaviour, IController, ISaver
{
    public static string GetRateUsURL => GetGooglePlayURL;
    private static string GetGooglePlayURL => $"https://play.google.com/store/apps/details?id={Application.identifier}";

    private bool plannedRateUs;
    private DateTime timeToShowRateUs;

    [Inject] private GuiController gui;

    private void Awake()
    {
        LoadData();
    }

    public void TryShowRateUs(out bool successful)
    {
        successful = false;

        if (CommonData.GameWasRated) return;

        bool conditionForOccupation = ((CommonData.PlayerOccupationCounter == 2) && CommonData.RateUsLevel != CommonData.PlayerOccupationCounter);
        bool conditionForPlan = plannedRateUs && DateTime.Now > timeToShowRateUs;

        if (conditionForOccupation || conditionForPlan)
        {
            successful = true;
            gui.Show<RateUsScreen>();

            CommonData.RateUsLevel = CommonData.PlayerOccupationCounter;
            SaveManager.Save(CommonData.PREFSKEY_RATE_US_LEVEL, CommonData.RateUsLevel);

            if (conditionForPlan) BreakRateUsPlan();
        }
    }

    /// <summary>
    /// Запланировать показ рейт аза на определённое время
    /// </summary>
    public void ScheduleRateUs()
    {
        plannedRateUs = true;
        timeToShowRateUs = DateTime.Now.AddDays(3);
        SaveData();
    }

    private void BreakRateUsPlan()
    {
        plannedRateUs = false;
        SaveData();
    }

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_NEED_SHOW_RATE_US_IN_TIME, plannedRateUs);
        SaveManager.Save(CommonData.PREFSKEY_TIME_TO_SHOW_RATE_US, timeToShowRateUs);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_NEED_SHOW_RATE_US_IN_TIME))
        {
            plannedRateUs = SaveManager.Load<bool>(CommonData.PREFSKEY_NEED_SHOW_RATE_US_IN_TIME);
        }
        else
        {
            plannedRateUs = false;
            SaveManager.Save(CommonData.PREFSKEY_NEED_SHOW_RATE_US_IN_TIME, false);
        }

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_TIME_TO_SHOW_RATE_US))
        {
            timeToShowRateUs = SaveManager.Load<DateTime>(CommonData.PREFSKEY_TIME_TO_SHOW_RATE_US);
        }
        else
        {
            timeToShowRateUs = new DateTime();
            SaveManager.Save(CommonData.PREFSKEY_TIME_TO_SHOW_RATE_US, timeToShowRateUs);
        }
    }

    #region Comparable

    public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
    public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

    #endregion
}