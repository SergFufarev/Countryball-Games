using UnityEngine;
using MAXHelper;
using System;
using Sirenix.OdinInspector;
using SPSDigital.Metrica;
using FunnyBlox;
using SPSDigital.IAP;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using TheSTAR.Utility;

namespace SPSDigital.ADS
{
    [Obsolete] // потом убрать, сделать как в Кайдзю
    public class AdsService : MonoBehaviour
    {
        [SerializeField] private ADSInterstititalService interstititalService;
        [SerializeField] private CommonConfig commonSettings;
        public static AdsService Instance;

        [HideInInspector] public bool allAdsRemoved = false;

        public ADSInterstititalService InterstititalService => interstititalService;

        public bool NeedShowCoinsForAdButton => (DateTime.Now - previousCoinsForAdClickTime).TotalSeconds >= commonSettings.CoinsForAdWaitTime.TotalSeconds;

        // todo вырезать плейсменты
        public const string AD_PLACE_IN_GAME = "in_game";
        public const string AD_PLACE_COINS_FOR_AD = "coins_for_ad";
        public const string AD_PLACE_INCOME_BOOST = "income_boost_for_ad";
        public const string AD_PLACE_UPGRADE_FOR_AD = "upgrade_for_ad";
        public const string AD_PLACE_TRADE_FOR_AD = "trade_for_ad";
        public const string AD_PLACE_SKIT_BOMB_CD = "skip_bomb_cd";

        public DateTime previousCoinsForAdClickTime;
        public int RewardCoinsForAd
        {
            get
            {
                int rewardMultiplierIndex = (int)((DateTime.Now - previousCoinsForAdClickTime).TotalSeconds / commonSettings.CoinsForAdWaitTime.TotalSeconds) - 1;
                rewardMultiplierIndex = MathUtility.Limit(rewardMultiplierIndex, 0, commonSettings.CoinsForAdsRewardValues.Length - 1);
                int multiplier = commonSettings.CoinsForAdsRewardValues[rewardMultiplierIndex];
                int reward = (int)(CommonData.MoneyPerSecondBase * multiplier);
                return reward;
            }
        }

        public void OnCompletedRewardedCoinsForAds()
        {
            previousCoinsForAdClickTime = DateTime.Now;
            SaveManager.Save(CommonData.PREFSKEY_COINS_FOR_AD_CLICK_TIME, previousCoinsForAdClickTime);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            allAdsRemoved = PlayerPrefs.HasKey(CommonData.PREFSKEY_ADS_REMOVED) || GemShop.IsPremiumActive;
        }

        #region BANNER

        public Action<bool> ShowBanner;

        public void ShowBannerAds()
        {
            if (!Instance.allAdsRemoved)
            {
                Debug.Log("[Mad Pixel] Show banner");
                AdsManager.ToggleBanner(true);
                ShowBanner?.Invoke(true);
            }
            else HideBannerAds();
        }

        public void HideBannerAds()
        {
            Debug.Log("[Mad Pixel] Hide banner");
            AdsManager.ToggleBanner(false);
            ShowBanner?.Invoke(false);
        }

        #endregion

        #region REWARD

        public Action<bool> FinishRewarded;

        private string currentRewardedPlacement = "";

        [BoxGroup("Reward", centerLabel: true)]
        public void ShowRewarded(string rewardedPlacement)
        {
            currentRewardedPlacement = rewardedPlacement;

            if (allAdsRemoved)
            {
                // если у игрока отключена реклама, то награда за ревардед становится бесплатной
                GiveRewardForAd(rewardedPlacement);
                return;
            }

#if SPSDIGITAL_METRICA
            AppMetricaBridge.ReportEvent("video_ads_available"
                , AppMetricaBridge.SetParametrs(
                    ad_type: "rewarded"
                    , placement: rewardedPlacement
                    , result: (MAXHelper.AdsManager.HasLoadedAd(MAXHelper.AdsManager.EAdType.REWARDED)
                        ? "success"
                        : "not_available")
                    , connection: "1"
                    , level_number: ""
                    , level_name: ""
                    , level_count: ""
                    , level_diff: ""
                    , level_loop: ""
                    , level_random: ""
                    , level_type: ""
                )
            );
#endif

            AdsManager.EResultCode result = AdsManager.ShowRewarded(this.gameObject, OnFinishRewarded, rewardedPlacement);

            if (result != AdsManager.EResultCode.OK)
            {
                Debug.Log("[Mad Pixel] Ad has not been loaded yet");
                //TODO: здесь можно показать UI, что реклама не подгружена
            }
        }

        private bool CheckInternetConnection => Application.internetReachability != NetworkReachability.NotReachable;

        private void OnFinishRewarded(bool success)
        {
            if (success) GiveRewardForAd(currentRewardedPlacement);
            else
            {
                //TODO: Игрок не досмотрел рекламу до конца, ничего не давайте
                Debug.Log($"[Mad Pixel] User closed rewarded ad before it was finished");
            }

            FinishRewarded?.Invoke(success);
        }

        [Obsolete]
        // todo вырезать этот метод
        public void GiveRewardForAd(string placement)
        {
            Debug.Log($"[Mad Pixel] Give reward to user!");

            switch (placement)
            {
                case AD_PLACE_COINS_FOR_AD:
                    GuiController.Instance.FindScreen<CoinsForAdScreen>().GiveRewardForAd();
                    break;

                case AD_PLACE_INCOME_BOOST:
                    GuiController.Instance.FindScreen<IncomeAccelerationScreen>().GiveReward();
                    break;

                case AD_PLACE_UPGRADE_FOR_AD:
                    //upgrades.GiveUpgradeForAds();
                    break;

                case AD_PLACE_TRADE_FOR_AD:
                    //upgrades.GiveTradeForAds();
                    break;

                case AD_PLACE_SKIT_BOMB_CD:
                    var gameScreen = GuiController.Instance.FindScreen<GameScreen>();
                    gameScreen.SkipBombCdForAd();
                    gameScreen.Hide();
                    gameScreen.Show();
                    break;
            }
        }

        #endregion

        #region INTERSTITIAL

        public Action InterDismissed;

        [BoxGroup("Interstitital", centerLabel: true)] [SerializeField]
        private string interstititalPlacement = "inter_placement";

        private string currentInterPlacement;

        public void ShowInterstitial(string placement)
        {
            if (allAdsRemoved) return;

            currentInterPlacement = placement;

            //MyButton.enabled = false;

            AdsManager.EResultCode result =
                AdsManager.ShowInter(this.gameObject, OnInterDismissed, interstititalPlacement);
            switch (result)
            {
                case AdsManager.EResultCode.ADS_FREE:
                    Debug.Log("[Mad Pixel] User bought adsfree and has no inters");
                    //MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.NOT_LOADED:
                    Debug.Log("[Mad Pixel] Ad has not been loaded yet");
                    //MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.ON_COOLDOWN:
                    float Seconds = AdsManager.CooldownLeft;
                    Debug.Log($"[Mad Pixel] Cooldown for ad has not finished! Can show inter in {Seconds} seconds");
                    //MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.OK:
                    Debug.Log("[Mad Pixel] Inter was shown");
                    break;
            }
        }


        private void OnInterDismissed(bool success)
        {
            Debug.Log($"[Mad Pixel] User dismissed the interstitial ad");

            InterDismissed?.Invoke();
        }

        #endregion
    }
}