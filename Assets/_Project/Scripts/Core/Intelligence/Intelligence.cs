using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

namespace FunnyBlox
{
    public class Intelligence : MonoSingleton<Intelligence>, ISaver
    {
        [LabelText("Скорость движения шара м/сек")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private CommonConfig _commonSettingsConfig;

        [Inject] private readonly GameController game;
        [Inject] private readonly GuiController gui;
        [Inject] private readonly VisualCountryController countries;
        [Inject] private readonly BattleInGameService battle;

        public CountryBall IntelligenceBall => countries.PlayerBaseCountry.CountryBalls.Get(CountryBallType.Intelligence);

        private Country _intelligenceCountry;
        private int spyingCountriesCount = 0;
        private DateTime unlockDateTime;
        private float unlockWaitTimeFull;

        public DateTime UnlockTime => unlockDateTime;
        public float UnlockWaitTimeFull => unlockWaitTimeFull;

        private bool intelligenceInProcess = false; // исследование находится в процессе

        [Obsolete] public Vector3 DefaultHomeCountryOffset => new (-1.5f, countries.PlayerBaseCountry.CountryBalls.Get(CountryBallType.Main).transform.localPosition.y, 0f);

        void Start()
        {
            LoadData();
        }

        public void PrepareIntelligence(Country country)
        {
            if (intelligenceInProcess) return;

            _intelligenceCountry = country;

            var tutor = gui.TutorContainer;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.IntelligenceTutorID) tutor.CompleteTutorial();

            var intelligenceScreen = gui.FindScreen<IntelligenceScreen>();
            intelligenceScreen.Init(country.LocalCountryData);
            gui.Show(intelligenceScreen);
        }

        public void RunIntelligence(bool forHard)
        {
            float time
                = Vector3.Distance(IntelligenceBall.transform.position, _intelligenceCountry.transform.position)
                  / _moveSpeed;

            IntelligenceBall.transform.DOKill();

            // добавить смещение
            IntelligenceBall.transform.DOMove(_intelligenceCountry.CountryBalls.Get(CountryBallType.Main).transform.position + CommonData.BallBackOffset, time)
                .OnComplete(ReturnFromIntelligence);

            CameraService.Instance.ActivateSubCamera();
            intelligenceInProcess = true;

            if (game.GameVersion == GameVersionType.VersionB && !forHard) StartWaitToNextIntelligence();

            gui.ShowMainScreen();
        }

        private void StartWaitToNextIntelligence()
        {
            unlockWaitTimeFull = _commonSettingsConfig.IntelligenceTimes[countries._openCountries.Count];
            unlockDateTime = DateTime.Now.AddSeconds(unlockWaitTimeFull);
            SaveManager.Save(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME, unlockDateTime);
            SaveManager.Save(CommonData.PREFSKEY_UNLOCK_WAIT_TIME_FULL, unlockWaitTimeFull);
        }

        private void ReturnFromIntelligence()
        {
            spyingCountriesCount++;

            switch (spyingCountriesCount)
            {
                case 3:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "1_spying_country 3");
                    break;

                case 10:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "2_spying_country 10");
                    break;

                case 20:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "3_spying_country 20");
                    break;

                case 30:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "4_spying_country 30");
                    break;

                case 40:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "5_spying_country 40");
                    break;

                case 49:
                    AnalyticsManager.Instance.Log(AnalyticSectionType.SpyingOnCountries, "6_spying_country 49");
                    break;
            }

            _intelligenceCountry.Open(true);

            Vector3 homePos = countries.PlayerBaseCountry.transform.position + countries.PlayerBaseCountry.IntelligenceHomePos;

            float time = Vector3.Distance(IntelligenceBall.transform.position, homePos) / _moveSpeed;
            IntelligenceBall.transform.DOMove(homePos, time).OnComplete(() =>
            {
                CameraService.Instance.ActivateSubCamera();
            });

            if (!countries._openCountries.Contains(_intelligenceCountry))
            {
                countries._openCountries.Add(_intelligenceCountry);
                countries._openEnemyCountries.Add(_intelligenceCountry);

                battle.Mutual.Try();
            }

            gui.FindScreen<GameScreen>().TryShowTutorial();

            if (_intelligenceCountry.CountryBalls.Contains(CountryBallType.Resistance))
                _intelligenceCountry.CountryBalls.Get(CountryBallType.Resistance).UpdateFace();

            //countries.SaveCountryWithConvertFromUpgradesToDictionary(_intelligenceCountry);

            var loadedCountryData = CountrySaveLoad.LoadCountry(_intelligenceCountry.ID);
            _intelligenceCountry.UpdateAllBallStarProgressesByCollectedCountryData();

            _intelligenceCountry.SaveCountry();

            intelligenceInProcess = false;

            SaveData();
        }

        #region SaveLoad

        public void SaveData()
        {
            SaveManager.Save(CommonData.PREFSKEY_SPYING_COUNTRIES_COUNT, spyingCountriesCount);
        }

        public void LoadData()
        {
            spyingCountriesCount = SaveManager.Load<int>(CommonData.PREFSKEY_SPYING_COUNTRIES_COUNT);

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME))
                unlockDateTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME);
            else
                unlockDateTime = DateTime.Now.AddMinutes(-1);

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_UNLOCK_WAIT_TIME_FULL))
                unlockWaitTimeFull = SaveManager.Load<float>(CommonData.PREFSKEY_UNLOCK_WAIT_TIME_FULL);
            else unlockWaitTimeFull = 0;
        }

        #endregion
    }
}