using System;
using UnityEngine;
using FunnyBlox;
using DG.Tweening;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using Zenject;

public class WorldEventService : MonoBehaviour, ISaver
{
    [SerializeField] private CommonConfig commonSettings;
    [SerializeField] private WorldEventType currentWorldEvent;
    public WorldEventType CurrentWorldEvent => currentWorldEvent;

    private DateTime nextWorldEventDateTime;

    private static WorldEventService instance;
    public static WorldEventService Instance => instance;

    private bool initialized = false;

    protected void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitService()
    {
        if (initialized) return;

        if (CommonData.CurrentGameVersionType == GameVersionType.VersionA) return;

        LoadData();

        if (DateTime.Now >= nextWorldEventDateTime) SetRandomWorldEvent();
        else StartWaitToRandomEvent();

        initialized = true;
    }

    private void StartWaitToRandomEvent()
    {
        Debug.Log("StartWaitToRandomEvent");
        if (CommonData.CurrentGameVersionType == GameVersionType.VersionA) return;

        float timeSeconds = (float)(nextWorldEventDateTime - DateTime.Now).TotalSeconds;
        DOVirtual.Float(0, 1, timeSeconds, (value) => { currentProgress = value; }).OnComplete(SetRandomWorldEvent).SetEase(Ease.Linear);
    }

    [SerializeField] private float currentProgress;

    private void SetRandomWorldEvent()
    {
        if (CommonData.CurrentGameVersionType == GameVersionType.VersionA) return;

        int eventIndex = UnityEngine.Random.Range(1, 5);
        currentWorldEvent = (WorldEventType)eventIndex;

        nextWorldEventDateTime = DateTime.Now.Add(commonSettings.WorldEventPeriod.ToTimeSpan());
        SaveData();

        StartWaitToRandomEvent();

        MessageService.Instance.ShowWorldEventMessage(currentWorldEvent);
        CurrencyService.Instance.RecalculateFinalIncome();
    }

    public bool CurrentWorldEventContains(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Economics:
                return currentWorldEvent == WorldEventType.WorldEconomicIncrease || currentWorldEvent == WorldEventType.WorldEconomicReduce;

            case UpgradeType.Army:
                return currentWorldEvent == WorldEventType.ArmyUpgradeCostIncrease || currentWorldEvent == WorldEventType.ArmyUpgradeCostReduce;

            case UpgradeType.Resistance:
                return false;
        }

        return false;
    }

    public float GetCurrentEventMultiplier()
    {
        float multiplier = 1;

        if (currentWorldEvent == WorldEventType.None) return multiplier;

        float currentValue = commonSettings.GetWorldEventValue(currentWorldEvent);

        switch (currentWorldEvent)
        {
            case WorldEventType.WorldEconomicIncrease:
            case WorldEventType.ArmyUpgradeCostIncrease:
                multiplier = 1 + (currentValue / 100);
                break;

            case WorldEventType.WorldEconomicReduce:
            case WorldEventType.ArmyUpgradeCostReduce:
            case WorldEventType.MorePlanes:
                multiplier = 1 - (currentValue / 100);
                break;
        }

        return multiplier;
    }

    #region Save Load

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_CURRENT_WORLD_EVENT))
        {
            currentWorldEvent = SaveManager.Load<WorldEventType>(CommonData.PREFSKEY_CURRENT_WORLD_EVENT);
        }
        else currentWorldEvent = WorldEventType.None;

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_NEXT_CHANGE_WORLD_EVENT_TIME))
        {
            nextWorldEventDateTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_NEXT_CHANGE_WORLD_EVENT_TIME);
        }
        else
        {
            nextWorldEventDateTime = DateTime.Now.Add(commonSettings.WorldEventPeriod.ToTimeSpan());
            SaveData();
        }
    }

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_CURRENT_WORLD_EVENT, currentWorldEvent);
        SaveManager.Save(CommonData.PREFSKEY_NEXT_CHANGE_WORLD_EVENT_TIME, nextWorldEventDateTime);
    }

    #endregion
}

public enum WorldEventType
{
    /// <summary> Нет никакого глобального события </summary>
    None,

    /// <summary> Бонус к доходу 15% </summary>
    WorldEconomicIncrease,

    /// <summary> Бонус к доходу -15% </summary>
    WorldEconomicReduce,

    /// <summary> Увеличение цен на апгрейды армии рандомного типа на 30% </summary>
    ArmyUpgradeCostIncrease,

    /// <summary> Уменьшение цен на апгрейды армии рандомного типа на 20% </summary>
    ArmyUpgradeCostReduce,

    /// <summary> Уменьшает паузу между самолётами до 20 секунд </summary>
    MorePlanes
}