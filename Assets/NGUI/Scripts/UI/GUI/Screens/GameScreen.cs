using System;
using System.Collections;
using UnityEngine;
using SPSDigital.ADS;
using SPSDigital.IAP;
using FunnyBlox.OfflineReward;
using FunnyBlox.PlaneReward;
using DG.Tweening;
using TheSTAR.Utility;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using UnityEngine.UI;
using TheSTAR.Sound;

namespace TheSTAR.GUI.Screens
{
    public class GameScreen : GuiScreen, ISaver, ITutorialStarter
    {
        [Space]
        [SerializeField] private GameObject fullUiContainer;
        [SerializeField] private GameObject smallUiContainer;

        [Space]
        [SerializeField] private PointerButton settingsButton;
        [SerializeField] private PointerButton cameraButton;

        [SerializeField] private PointerButton tradeButton;
        [SerializeField] private PointerButton shopButton;
        [SerializeField] private PointerButton dailyBonusButton;

        [SerializeField] private PointerButton coinsForAdButton;
        [SerializeField] private PointerButton messagesButton;

        [SerializeField] private PointerButton planetButton;
        [SerializeField] private PointerButton quickIntelligenceButton;
        [SerializeField] private PointerButton bombButton;

        [SerializeField] private PointerButton homeButton;

        [Header("Explorer Lock")]
        [SerializeField] private PointerButton explorerLockButton;
        [SerializeField] private Image explorerProgress;
        [SerializeField] private TimeText explorerTimeText;

        [Header("Bomb Lock")]
        [SerializeField] private PointerButton bombLockBtn;
        [SerializeField] private Image bombProgress;
        [SerializeField] private TimeText bombTimerText;
        [SerializeField] private GameObject skipBombCdForAdObject;
        [SerializeField] private GameObject skipBombCdForHardObject;

        [Space]
        [SerializeField] private GameObject tradeLightObject; // подсветка для tradeButton

        [Space]
        [SerializeField] private CommonConfig commonSettings;

        private DateTime previousBombAttackTime;
        private DateTime incomeBoosterEndTime;

        private bool firstShowTradeButton;
        //private bool plannedRateUs;
        //private DateTime timeToShowRateUs;
        
        private bool needPerSecondUpdate = false;
        public bool availableSkipBombForAds = true;

        private GameController game;
        private VisualCountryController countries;
        private BattleInGameService battle;
        private TutorContainer tutor;
        private RateUsController rateUs;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            game = cts.Get<GameController>();
            countries = cts.Get<VisualCountryController>();
            battle = cts.Get<BattleInGameService>();
            tutor = GuiController.Instance.TutorContainer;
            rateUs = cts.Get<RateUsController>();

            settingsButton.Init(OnClickSettingsScreen);
            cameraButton.Init(OnCameraClick);
            tradeButton.Init(OnTradeClick);
            shopButton.Init(OnShopClick);
            dailyBonusButton.Init(OnDailyBonusClick);
            coinsForAdButton.Init(CoinsForAdClick);
            messagesButton.Init(OnMessagesClick);
            planetButton.Init(OnCountriesClick);
            explorerLockButton.Init(OnExplorerClick);
            quickIntelligenceButton.Init(OnExplorerClick);
            bombButton.Init(OnBombClick);
            bombLockBtn.Init(OnLockedBombClick);
            homeButton.Init(OnHomeClick);

            CameraService.Instance.NearTrigger.OnNearAircraft += OnNearAircraft;
            CameraService.Instance.NearTrigger.OnEndNearAircraft += OnEndNearAircraft;
            AircraftService.Instance.OnAircraftClick += OnClickAircraft;
            AircraftService.Instance.OnAircraftExplode += OnExplodeAircraft;
        }

        private bool isNearAircraft;

        private void OnNearAircraft()
        {
            isNearAircraft = true;

            if (tutor.InTutorial) return;

            TryShowTutorial();
        }

        private void OnEndNearAircraft()
        {
            isNearAircraft = false;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.AircraftTutorID) tutor.BreakTutorial();
        }

        private void OnClickAircraft()
        {
            isNearAircraft = false;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.AircraftTutorID) tutor.CompleteTutorial();
        }

        private void OnExplodeAircraft()
        {
            TryShowTutorial();
        }

        #region Show/Hide

        protected override void OnShow()
        {
            base.OnShow();

            LoadData();
            UpdateUI();

            Invoke(nameof(TryShowTutorial), 0.01f);
            Invoke(nameof(TryShowAdditionalScreens), 0.5f);
        }

        protected override void OnHide()
        {
            base.OnHide();
            GuiController.Instance.TutorContainer.BreakTutorial();
        }

        private void TryShowAdditionalScreens()
        {
            if (!isShow || GuiController.Instance.CurrentScreen is not GameScreen) return;

            rateUs.TryShowRateUs(out bool success);

            if (success) return;

            var gemShop = GuiController.Instance.FindScreen<ShopScreen>().GemShop;

            // если это первая сессия за день и активен премиум, то выдаём ежедневный премиальный бонус (5000 золота)
            if (!gemShop.premiumBonusReceived && game.FirstSessionInDay && GemShop.IsPremiumActive)
            {
                gemShop.ShowDailyPremiumBonus();
            }
            else
            {
                OfflineRewardServices.Instance.GetAvailableReward(out success);

                if (success) return;

                AircraftService.Instance.TryGiveReward(out success);

                if (success) return;

                AdsService.Instance.InterstititalService.TryShowInterPopup();
            }
        }

        public void UpdateUI()
        {
            // daily bonus

            bool needShowDailyBonus = DailyBonusService.Instance.NeedShowDailyBonus;
            dailyBonusButton.gameObject.SetActive(needShowDailyBonus);

            // coins for ads button

            bool needShowCoinsForAdButton = AdsService.Instance.NeedShowCoinsForAdButton;
            coinsForAdButton.gameObject.SetActive(needShowCoinsForAdButton);

            UpdateBombButton(out bool activateBomb);
            UpdateExplorerButton(out bool activateExplorer);

            // trade
            var tutor = GuiController.Instance.TutorContainer;
            bool needShowTradeButton = countries._playerCountries.Count > 1 && tutor.IsComplete(TutorContainer.ResistanceTutorID);
            tradeButton.gameObject.SetActive(needShowTradeButton);

            tradeLightObject.SetActive(false);

            if (!activateBomb || !activateExplorer)
            {
                needPerSecondUpdate = true;
                StopAllCoroutines();
                StartCoroutine(PerSecondCor());
            }
        }

        public void TryShowTutorial()
        {
            if (!IsShow) return;

            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.FactoryTutorID))
            {
                focusTran = countries.PlayerBaseCountry.CountryBalls.Get(CountryBallType.Factory).BodyTran;
                tutor.TryShowInWorld(TutorContainer.FactoryTutorID, focusTran);
                CameraService.Instance.MoveTo(countries.PlayerBaseCountry, CountryBallType.Factory);
            }
            else if (!tutor.IsComplete(TutorContainer.ArmyTutorID))
            {
                focusTran = countries.PlayerBaseCountry.CountryBalls.Get(CountryBallType.GroundArmy).BodyTran;
                tutor.TryShowInWorld(TutorContainer.ArmyTutorID, focusTran);
                CameraService.Instance.MoveTo(countries.PlayerBaseCountry, CountryBallType.GroundArmy);
            }
            else if (!tutor.IsComplete(TutorContainer.IntelligenceTutorID) && countries._playerCountries.Count == 1)
            {
                focusTran = countries.PlayerBaseCountry.CountryBalls.Get(CountryBallType.Intelligence).BodyTran;
                tutor.TryShowInWorld(TutorContainer.IntelligenceTutorID, focusTran);
                CameraService.Instance.MoveTo(countries.PlayerBaseCountry, CountryBallType.Intelligence);
            }
            else if (!tutor.IsComplete(TutorContainer.EnemyAttackTutorID))
            {
                var playerBaseCountryData = CountrySaveLoad.LoadCountry(countries.PlayerBaseCountry.ID);

                for (int i = 0; i < countries._openCountries.Count; i++)
                {
                    var country = countries._openCountries[i];
                    var countryData = CountrySaveLoad.LoadCountry(country.ID);
                    
                    if (country.LocalCountryData.IsPlayerOwner || countryData.GetArmyUpgradeLevels() >= 6) continue;

                    focusTran = country.CountryBalls.Get(CountryBallType.GroundArmy).BodyTran;
                    tutor.TryShowInWorld(TutorContainer.EnemyAttackTutorID, focusTran);
                    CameraService.Instance.MoveTo(country, CountryBallType.GroundArmy);
                }
            }
            else if (!tutor.IsComplete(TutorContainer.NewFactoryTutorID) && countries._playerCountries.Count > 1)
            {
                Country country = countries._playerCountries.Find(info => !info.LocalCountryData.IsBaseCountry);
                focusTran = country.CountryBalls.Get(CountryBallType.Factory).BodyTran;
                tutor.TryShowInWorld(TutorContainer.NewFactoryTutorID, focusTran);
                CameraService.Instance.MoveTo(country, CountryBallType.Factory);
            }
            else if (tutor.IsComplete(TutorContainer.NewFactoryTutorID) && !tutor.IsComplete(TutorContainer.ResistanceTutorID))
            {
                Country country = countries._playerCountries.Find(info => !info.LocalCountryData.IsBaseCountry);
                if (country != null)
                {
                    focusTran = country.CountryBalls.Get(CountryBallType.Resistance).BodyTran;
                    tutor.TryShowInWorld(TutorContainer.ResistanceTutorID, focusTran);
                    CameraService.Instance.MoveTo(country, CountryBallType.Resistance);
                }
            }
            else if (tutor.IsComplete(TutorContainer.ResistanceTutorID) && !tutor.IsComplete(TutorContainer.TradeTutorID))
            {
                focusTran = tradeButton.transform;
                tutor.TryShowInUI(TutorContainer.TradeTutorID, focusTran, true);
                tradeLightObject.SetActive(true);
            }
            else if (!tutor.IsComplete(TutorContainer.CountriesScreenTutorID))
            {
                focusTran = planetButton.transform;
                tutor.TryShowInUI(TutorContainer.CountriesScreenTutorID, focusTran, false, TutorContainer.CursorViewType.BottomEnge);
                return;
            }
            else if (DailyBonusService.Instance.NeedShowDailyBonus && !tutor.InTutorial && !tutor.IsComplete(TutorContainer.DailyBonusTutorID) && countries._playerCountries.Count >= 3)
            {
                focusTran = dailyBonusButton.transform;
                tutor.TryShowInUI(TutorContainer.DailyBonusTutorID, focusTran);
            }
            else if (isNearAircraft)
            {
                focusTran = AircraftService.Instance.GetAircraftRoot.transform;
                tutor.TryShowInWorld(TutorContainer.AircraftTutorID, focusTran, true);
            }
            else if (!tutor.IsComplete(TutorContainer.BuyCustomHatTutorID) && CommonData.PlayerCountriesCount >= 5)
            {
                if (GemShop.IsPremiumActive)
                {
                    tutor.CompleteTutorial(TutorContainer.BuyCustomHatTutorID);
                    return;
                }

                focusTran = shopButton.transform;
                tutor.TryShowInUI(TutorContainer.BuyCustomHatTutorID, focusTran, true);
            }
        }

        #endregion

        #region PerSecondUpdate

        // todo в будущем можно сделать на реактаблах (эталон - Кайдзю), корутина в экране плохая идея
        private IEnumerator PerSecondCor()
        {
            while (needPerSecondUpdate)
            {
                PerSecondUpdateUI();
                yield return new WaitForSeconds(1);
            }
        }

        private void PerSecondUpdateUI()
        {
            UpdateBombButton(out bool activateBomb);
            UpdateExplorerButton(out bool activateExplorer);

            bool stopPerSecondCor = activateBomb && activateExplorer;

            if (stopPerSecondCor)
            {
                needPerSecondUpdate = false;
                StopAllCoroutines();
            }
        }

        #endregion

        public void OnBombActivate()
        {
            previousBombAttackTime = DateTime.Now;
            SaveData();
        }

        public void SkipBombCdForAd()
        {
            var recoveryTime = battle.BattleConfig.BombRecoverTime;
            var recoveryTimeSpan = new TimeSpan(recoveryTime.Hours, recoveryTime.Minutes, recoveryTime.Seconds);
            SaveManager.Save(CommonData.PREFSKEY_BOMB_ATTACK_TIME, DateTime.Now - recoveryTimeSpan);
            NotificationManager.Instance.CancelBombNotification();
            availableSkipBombForAds = false;
        }

        #region ClickActions

        public void OnCameraClick() => CameraService.Instance.SwitchCameraMode();
        public void OnMessagesClick() => GuiController.Instance.Show<MessagesScreen>();
        public void OnLockedBombClick()
        {
            if (availableSkipBombForAds) AdsService.Instance.ShowRewarded(AdsService.AD_PLACE_SKIT_BOMB_CD);
            else BuySkipBombForHard();
        }
        public void OnClickSettingsScreen() => GuiController.Instance.Show<SettingsScreen>();
        public void OnShopClick() => GuiController.Instance.Show<ShopScreen>();
        public void OnHomeClick() => CameraService.Instance.MoveTo(countries.PlayerBaseCountry);
        public void OnDailyBonusClick() => GuiController.Instance.Show<DailyBonusScreen>();
        
        public void OnBombClick()
        {
            if (battle.InBattle) return;

            GuiController.Instance.Show<BombAttackScreen>();
        }

        public void OnShopForCurrencyClick()
        {
            var shop = GuiController.Instance.FindScreen<ShopScreen>();
            shop.PrepareForBuyCurrency();
            GuiController.Instance.Show(shop);
        }

        public void CoinsForAdClick()
        {
            coinsForAdButton.gameObject.SetActive(false);
            var coinsForAdScreen = GuiController.Instance.FindScreen<CoinsForAdScreen>();
            int reward = AdsService.Instance.RewardCoinsForAd;
            coinsForAdScreen.Init(reward);
            GuiController.Instance.Show<CoinsForAdScreen>();
        }

        public void OnCountriesClick() => GuiController.Instance.Show<CountriesScreen>();

        public void OnExplorerClick() => GuiController.Instance.Show<QuickIntelligenceScreen>();

        public void OnTradeClick()
        {
            firstShowTradeButton = false;
            SaveData();
            GuiController.Instance.Show<TradeScreen>();
        }

        #endregion

        private void BuySkipBombForHard()
        {
            CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, SkipWaitAtomBombScreen.Cost, () =>
            {
                var recoveryTime = battle.BattleConfig.BombRecoverTime;
                var recoveryTimeSpan = new TimeSpan(recoveryTime.Hours, recoveryTime.Minutes, recoveryTime.Seconds);
                SaveManager.Save(CommonData.PREFSKEY_BOMB_ATTACK_TIME, DateTime.Now - recoveryTimeSpan);
                NotificationManager.Instance.CancelBombNotification();
                availableSkipBombForAds = true;
                Hide();
                Show();
            }, () =>
            {
                var shop = GuiController.Instance.FindScreen<ShopScreen>();
                shop.PrepareForBuyCurrency();
                GuiController.Instance.Show(shop);
            });
        }

        #region Save/Load

        public void SaveData()
        {
            SaveManager.Save(CommonData.PREFSKEY_BOMB_ATTACK_TIME, previousBombAttackTime);
            SaveManager.Save(CommonData.PREFSKEY_COINS_FOR_AD_CLICK_TIME, AdsService.Instance.previousCoinsForAdClickTime);
            //SaveManager.Save(CommonData.PREFSKEY_TRADE_FIRST_SHOW, firstShowTradeButton);
        }

        public void LoadData()
        {
            previousBombAttackTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_BOMB_ATTACK_TIME);
            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_COINS_FOR_AD_CLICK_TIME))
            {
                AdsService.Instance.previousCoinsForAdClickTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_COINS_FOR_AD_CLICK_TIME);
            }
            else
            {
                AdsService.Instance.previousCoinsForAdClickTime = DateTime.Now;
                SaveManager.Save(CommonData.PREFSKEY_COINS_FOR_AD_CLICK_TIME, AdsService.Instance.previousCoinsForAdClickTime);
            }

            /*
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
            */

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME))
            {
                incomeBoosterEndTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME);
            }
            else
            {
                incomeBoosterEndTime = new DateTime();
                SaveManager.Save(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME, incomeBoosterEndTime);
            }
        }

        #endregion

        private void UpdateBombButton(out bool activateBomb)
        {
            var timeDifference = DateTime.Now - previousBombAttackTime;
            var neededTimeDifference = battle.BattleConfig.BombRecoverTime.ToTimeSpan();

            activateBomb = timeDifference.TotalSeconds >= neededTimeDifference.TotalSeconds;

            if (activateBomb)
            {
                bombButton.gameObject.SetActive(true);
                bombLockBtn.gameObject.SetActive(false);
            }
            else
            {
                bombButton.gameObject.SetActive(false);
                bombLockBtn.gameObject.SetActive(true);
                float progress = (float)timeDifference.TotalSeconds / (float)neededTimeDifference.TotalSeconds;
                bombProgress.fillAmount = progress;

                bombTimerText.SetValue(neededTimeDifference - timeDifference);
                skipBombCdForAdObject.SetActive(availableSkipBombForAds);
                skipBombCdForHardObject.SetActive(!availableSkipBombForAds);
            }
        }

        private void UpdateExplorerButton(out bool activateExplorer)
        {
            if (DateTime.Now >= Intelligence.Instance.UnlockTime)
            {
                quickIntelligenceButton.gameObject.SetActive(true);
                explorerLockButton.gameObject.SetActive(false);
                activateExplorer = true;
                explorerProgress.fillAmount = 0;
            }
            else
            {
                var timeLeft = Intelligence.Instance.UnlockTime - DateTime.Now;

                quickIntelligenceButton.gameObject.SetActive(false);
                explorerLockButton.gameObject.SetActive(true);
                float progress = 1 - ((float)timeLeft.TotalSeconds - 1) / (Intelligence.Instance.UnlockWaitTimeFull);

                DOVirtual.Float(explorerProgress.fillAmount, progress, 0.5f, value => explorerProgress.fillAmount = value).SetEase(Ease.OutSine);

                explorerTimeText.SetValue(timeLeft);
                activateExplorer = false;
            }
        }

        public void ShowFullUI(bool full)
        {
            fullUiContainer.SetActive(full);
        }
    }
}