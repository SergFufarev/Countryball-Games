using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;

public class DailyBonusService : MonoSingleton<DailyBonusService>
{
    private const int MaxDay = 12;

    public bool NeedShowDailyBonus
    {
        get
        {
            DateTime previousShowTime = GetPreviousShowTime();
            TimeSpan timePassed = DateTime.Today - previousShowTime;

            int passedDays = (int)timePassed.TotalDays;
            int currentBonusIndex = GetCurrentBonusIndex();

            if (passedDays < 1) return false;
            else // if (passedDays == 1)
            {
                bool indexWasUpdated = GetWasBonusIndexWasUpdatedForThisDay();

                if (!indexWasUpdated)
                {
                    currentBonusIndex++;
                    if (currentBonusIndex >= MaxDay) currentBonusIndex = 0;

                    SetCurrentBonusIndex(currentBonusIndex);
                    SetWasBonusIndexWasUpdatedForThisDay(true);
                }
                
                return true;
            }
            /*
            else
            {
                currentBonusIndex = 0;
                SetCurrentBonusIndex(currentBonusIndex);
                return true;
            }*/
        }
    }

    private DateTime GetPreviousShowTime()
    {
        DateTime result = new DateTime();

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_PREVIOUS_DAILY_BONUS_TIME))
        {
            string prefsResult = PlayerPrefs.GetString(CommonData.PREFSKEY_PREVIOUS_DAILY_BONUS_TIME);
            result = DateTime.ParseExact(prefsResult, "u", CultureInfo.InvariantCulture);
        }
        else SetDateTime(result);

        return result;
    }

    private bool GetWasBonusIndexWasUpdatedForThisDay()
    {
        bool result = false;

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_BONUS_INDEX_WAS_UPDATED_FOR_THIS_DAY))
        {
            result = SaveManager.Load<bool>(CommonData.PREFSKEY_BONUS_INDEX_WAS_UPDATED_FOR_THIS_DAY);
        }
        else SetWasBonusIndexWasUpdatedForThisDay(result);

        return result;
    }

    private void SetWasBonusIndexWasUpdatedForThisDay(bool value)
    {
        SaveManager.Save(CommonData.PREFSKEY_BONUS_INDEX_WAS_UPDATED_FOR_THIS_DAY, value);
    }

    private void SetDateTime(DateTime time)
    {
        string result = time.ToString("u", CultureInfo.InvariantCulture);
        PlayerPrefs.SetString(CommonData.PREFSKEY_PREVIOUS_DAILY_BONUS_TIME, result);
        PlayerPrefs.Save();
    }

    public int GetCurrentBonusIndex()
    {
        int result = -1;

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_CURRENT_DAILY_BONUS_INDEX)) result = PlayerPrefs.GetInt(CommonData.PREFSKEY_CURRENT_DAILY_BONUS_INDEX);
        else SetCurrentBonusIndex(-1);

        return result;
    }

    private void SetCurrentBonusIndex(int i)
    {
        PlayerPrefs.SetInt(CommonData.PREFSKEY_CURRENT_DAILY_BONUS_INDEX, i);
        PlayerPrefs.Save();
    }

    public void OnGetDailyReward()
    {
        SetDateTime(DateTime.Today);
        SetWasBonusIndexWasUpdatedForThisDay(false);

        AnalyticsManager.Instance.Log(AnalyticSectionType.Tutorial, "4_visited_daily_bonus");

        // notification
        DateTime notificationTime = DateTime.Today.AddDays(1);
        notificationTime.AddHours(12); // о дейликах напоминаем в 12 часов
        NotificationManager.Instance.RegisterNotificationForNextDailyBonus(notificationTime);
    }
}