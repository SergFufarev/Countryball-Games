using System;
using UnityEngine;
using FunnyBlox;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using TheSTAR.Sound;
using Zenject;
using MAXHelper;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour, ISaver, IController
{
    [SerializeField] private GameVersionType _gameVersion;
    [SerializeField] private bool useMinimumUiInGameScreen;
    [SerializeField] private bool showVisualOnlyForNearCountryBalls;
    [SerializeField] private CommonConfig commonConfig;
    [SerializeField] private bool useCheats;
    [SerializeField] private AdaptiveQuality adaptiveQuality;

    [Inject] private readonly GuiController gui;
    [Inject] private readonly UpgradeService upgrades;
    [Inject] private readonly VisualCountryController countries;
    [Inject] private readonly BattleInGameService battle;

    private bool firstSessionInDay = false;
    public bool FirstSessionInDay => firstSessionInDay;
    public bool UseCheats => useCheats;

    public BattleInGameService Battle => battle;

    private bool gdprWasAccepted = false;
    private int sessionIndex = 0;

    public GameVersionType GameVersion => _gameVersion;

    public bool UseMinimumUiInGameScreen => useMinimumUiInGameScreen;
    public bool ShowVisualOnlyForNearCountryBalls => showVisualOnlyForNearCountryBalls;

    private void Start()
    {
        LoadData();
        InitService();
        ShowGameLoading();
    }

    private void ShowGameLoading()
    {
        Sprite fonSprite;

        if (!gdprWasAccepted) fonSprite = commonConfig.firstSessionFon;
        else
        {
            fonSprite = commonConfig.loadFons[sessionIndex % commonConfig.loadFons.Length];
            sessionIndex++;
        }

        var loadScreen = gui.FindScreen<LoadScreen>();
        loadScreen.Init(fonSprite, LoadGameWorld, TryShowGdprOrStartGame);
        gui.Show(loadScreen);
    }

    private void ShowChooseCountryUI()
    {
        CameraService.Instance.SetCameraMode(1);
        gui.Show<ChooseCountryScreen>();
    }

    private void TryShowGdprOrStartGame()
    {
        if (!gdprWasAccepted)
        {
            SoundController.Instance.PlayMusic(MusicType.MainTheme);
            gui.Show<GDPRScreen>();
        }
        else
        {
            InitAds();

            if (countries.PlayerBaseCountryID == -1) ShowChooseCountryUI();
            else
            {
                bool loadFromBigBattle = CommonData.LoadGameSceneFromBigBattle;
                if (loadFromBigBattle) CommonData.LoadGameSceneFromBigBattle = false;

                StartMoveCamera(loadFromBigBattle);

                bool useCustomColor = SaveManager.Load<bool>(CommonData.PREFSKEY_USE_CUSTOM_COUNTRY_COLOR);
                if (useCustomColor)
                {
                    SoundController.Instance.PlayMusic(MusicType.MainTheme);
                    if (loadFromBigBattle) gui.HideCurrentScreen();
                    else ShowPlayGameUI();
                }
                else gui.Show<SelectColorScreen>();
            }
        }
    }

    private static bool adsInitializes = false;

    private void InitAds()
    {
        if (adsInitializes) return;

        AnalyticsManager.Instance.SubscribeForAdsEvents();
        AdsManager.Instance.Init();

        adsInitializes = true;
    }

    private void InitService()
    {
        // todo в будущем избавиться от всех синглтонов
        CountryBallVisualService.Instance.InitService();
        MessageService.Instance.InitService();
        countries.Init();

        Invoke(nameof(DelayInitWorldEventService), 3);

        gui.Init(out var urs, out var trs);
        CurrencyService.Instance.Init(trs);

        urs.Add(countries);
        upgrades.Init(urs);
        battle.Init();

        CommonData.CurrentGameVersionType = _gameVersion;

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_USE_HEIGHT_QUALITY))
        {
            bool useHeightQuality = SaveManager.Load<bool>(CommonData.PREFSKEY_USE_HEIGHT_QUALITY);
            adaptiveQuality.SetQuality(useHeightQuality ? AdaptiveQualityType.Height : AdaptiveQualityType.Low);
        }
        else
        {
            var autoQualityLevel = adaptiveQuality.AutoUpdateQuality();
            bool useHeightQuality = autoQualityLevel == AdaptiveQualityType.Height;
            SaveManager.Save<bool>(CommonData.PREFSKEY_USE_HEIGHT_QUALITY, useHeightQuality);
            CommonData.UseHighQuality = useHeightQuality;
        }
    }

    private void DelayInitWorldEventService()
    {
        WorldEventService.Instance.InitService();
    }

    public void OnAcceptGDPR()
    {
        gdprWasAccepted = true;

        SaveData();
        InitAds();
        ShowChooseCountryUI();
    }

    public void LoadGameWorld(Action endAction)
    {
        // check first session in day
        if (!PlayerPrefs.HasKey(CommonData.PREFSKEY_PREVIOUS_SESSION_DATE_TIME) || DateTime.Today > SaveManager.Load<DateTime>(CommonData.PREFSKEY_PREVIOUS_SESSION_DATE_TIME))
        {
            firstSessionInDay = true;
            SaveManager.Save(CommonData.PREFSKEY_PREVIOUS_SESSION_DATE_TIME, DateTime.Today);
        }
        else firstSessionInDay = false;

        countries.LoadWorld();

        endAction?.Invoke();
    }

    public void ShowPlayGameUI()
    {
        gui.ShowMainScreen();
        Invoke(nameof(DelayLookBallsToCamera), 0.1f);
    }

    private void DelayLookBallsToCamera()
    {
        CameraService.Instance.ForceOnCameraMove();
    }

    private void StartMoveCamera(bool loadFromBigBattle)
    {
        Country focusCountry;
        CountryBallType focusBallType;

        if (loadFromBigBattle)
        {
            int bigBattleEnemyCountryID = SaveManager.Load<int>(CommonData.PREFSKEY_BIG_BATTLE_RED_COUNTRY_ID);
            int bigBattlePlayerCountryID = SaveManager.Load<int>(CommonData.PREFSKEY_BIG_BATTLE_GREEN_COUNTRY_ID);

            var defenderCountry = countries.GetCountry(bigBattleEnemyCountryID);
            var attackerCountry = countries.GetCountry(bigBattlePlayerCountryID);

            focusCountry = defenderCountry;
            focusBallType = CountryBallType.GroundArmy;

            battle.AnimateExitFromBigBattle(attackerCountry, defenderCountry);
        }
        else
        {
            focusCountry = countries.PlayerBaseCountry;
            focusBallType = CountryBallType.Main;
        }

        CameraService.Instance.MoveTo(focusCountry, focusBallType, null, false);
    }

    #region SaveLoad

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_GDPR_WAS_ACCEPTED, gdprWasAccepted);
        SaveManager.Save(CommonData.PREFSKEY_SESSION_INDEX, sessionIndex);
        SaveManager.Save(CommonData.PREFSKEY_GAME_WAS_RATED, CommonData.GameWasRated);
        SaveManager.Save(CommonData.PREFSKEY_PLAYER_OCCUPATION_COUNTER, CommonData.PlayerOccupationCounter);
        SaveManager.Save(CommonData.PREFSKEY_RATE_US_LEVEL, CommonData.RateUsLevel);

        SaveCommonData();
    }

    public void LoadData()
    {
        gdprWasAccepted = SaveManager.Load<bool>(CommonData.PREFSKEY_GDPR_WAS_ACCEPTED);
        sessionIndex = SaveManager.Load<int>(CommonData.PREFSKEY_SESSION_INDEX);
        CommonData.GameWasRated = SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_WAS_RATED);
        CommonData.PlayerOccupationCounter = SaveManager.Load<int>(CommonData.PREFSKEY_PLAYER_OCCUPATION_COUNTER);
        CommonData.RateUsLevel = SaveManager.Load<int>(CommonData.PREFSKEY_RATE_US_LEVEL);
        CommonData.RocketsCount = SaveManager.Load<int>(CommonData.PREFSKEY_BIG_BATTLE_ROCKETS_COUNT);

        LoadCommonData();
    }

    public void SaveCommonData()
    {
        SaveManager.Save(CommonData.PREFSKEY_MUSIC_ON, CommonData.MusicOn);
        SaveManager.Save(CommonData.PREFSKEY_SOUNDS_ON, CommonData.SoundsOn);
        SaveManager.Save(CommonData.PREFSKEY_VIBRATION_ON, CommonData.VibrationsOn);
        SaveManager.Save(CommonData.PREFSKEY_NOTIFICATIONS_ON, CommonData.NotificationsOn);
    }

    public void LoadCommonData()
    {
        CommonData.MusicOn = LoadOption(CommonData.PREFSKEY_MUSIC_ON, true);
        CommonData.SoundsOn = LoadOption(CommonData.PREFSKEY_SOUNDS_ON, true);
        CommonData.VibrationsOn = LoadOption(CommonData.PREFSKEY_VIBRATION_ON, true);
        CommonData.NotificationsOn = LoadOption(CommonData.PREFSKEY_NOTIFICATIONS_ON, true);
    }

    private bool LoadOption(string key, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(key)) return SaveManager.Load<bool>(key);
        else return defaultValue;
    }

    #endregion

    #region Comparable

    public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
    public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

    #endregion

    public void OpenScene(SceneType sceneType)
    {
        SceneManager.LoadScene(sceneType.ToString());
    }
}

public enum GameVersionType
{
    VersionA,
    VersionB
}

public enum SceneType
{
    Game,
    Battle
}