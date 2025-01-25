using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
using FunnyBlox;

public class NotificationManager : MonoSingleton<NotificationManager>, ISaver
{
    private AndroidNotificationChannel defaultChannel;

    private const string DefaultChannelID = "default_channel";

    private int _sentBombNotificationID; // id предстоящей нотификации о бомбе
    private int _sentDailyBonusNotificationID; // id предстоящей нотификации о дейлике

    private void Start()
    {
        LoadData();

        defaultChannel = new AndroidNotificationChannel()
        {
            Id = DefaultChannelID,
            Name = "Default Channel",
            Description = "For Generic notifications",
            Importance = Importance.Default
        };

        AndroidNotificationCenter.RegisterNotificationChannel(defaultChannel);
    }

    #region Register

    public void RegisterNotificationForBombRecharge(System.DateTime nextBombTime)
    {
        if (!CommonData.NotificationsOn) return;

        var notification = new AndroidNotification()
        {
            Title = "General, your nuclear strike is ready!",
            Text = "Come back and take over some countries with this!",
            SmallIcon = "default",
            LargeIcon = "default",
            FireTime = nextBombTime
        };

        CancelBombNotification();
        _sentBombNotificationID = AndroidNotificationCenter.SendNotification(notification, DefaultChannelID);
        SaveData();
    }

    public void RegisterNotificationForNextDailyBonus(System.DateTime nextDailyBonusTime)
    {
        if (!CommonData.NotificationsOn) return;

        var notification = new AndroidNotification()
        {
            Title = "Come and get your rewards!",
            Text = "Go get your daily reward!",
            SmallIcon = "default",
            LargeIcon = "default",
            FireTime = nextDailyBonusTime
        };

        if (_sentDailyBonusNotificationID != -1) AndroidNotificationCenter.CancelNotification(_sentDailyBonusNotificationID);
        _sentDailyBonusNotificationID = AndroidNotificationCenter.SendNotification(notification, DefaultChannelID);
        SaveData();
    }

    #endregion

    #region Cancel

    public void CancelBombNotification() => CancelNotification(ref _sentBombNotificationID);

    public void CancelDailyBonusNotification() => CancelNotification(ref _sentDailyBonusNotificationID);

    private void CancelNotification(ref int id)
    {
        if (id != -1)
        {
            AndroidNotificationCenter.CancelNotification(id);
            id = -1;
        }
    }

    public void CancelAllNotificatinos()
    {
        CancelBombNotification();
        CancelDailyBonusNotification();
    }

    #endregion

    #region SaveLoad

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_SENT_NOTIFICATION_BOMB_ID, _sentBombNotificationID);
        SaveManager.Save(CommonData.PREFSKEY_SENT_NOTIFICATION_DAILY_BONUS_ID, _sentDailyBonusNotificationID);
    }

    public void LoadData()
    {
        _sentBombNotificationID = SaveManager.Load<int>(CommonData.PREFSKEY_SENT_NOTIFICATION_BOMB_ID);
        _sentDailyBonusNotificationID = SaveManager.Load<int>(CommonData.PREFSKEY_SENT_NOTIFICATION_DAILY_BONUS_ID);
    }

    #endregion
}