using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using Unity.Advertisement.IosSupport; // NOTE: Import "com.unity.ads.ios-support" from Package Manager, if it's missing
#endif

namespace MAXHelper {
    public class AppLovinComp : MonoBehaviour {
        #region Fields
        private MaxSdkBase.AdInfo ShowedInfo;
        private MAXCustomSettings Settings;
        private string RewardedID = "empty";
        private string BannerID = "empty";
        private string InterstitialID = "empty";
        private bool bInitialized;
        [SerializeField] private bool bShowDebug;
        #endregion


        #region Events Declaration
        public UnityAction OnInitComplete;
        public UnityAction<bool> onFinishAdsEvent;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, AdsManager.EAdType> onErrorEvent;
        public UnityAction onInterDismissedEvent;
        public UnityAction OnBannerInitialized;
        public UnityAction<bool> onAdLoadedEvent; // true = rewarded 

        public UnityAction<string, MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo> onBannerLoadedEvent;
        public UnityAction<string, MaxSdkBase.AdInfo> onBannerRevenueEvent;
        #endregion


        #region Initialization
        public void Init(MAXCustomSettings CustomSettings) {
            Settings = CustomSettings;
            if (string.IsNullOrEmpty(Settings.SDKKey)) {
                Debug.LogError("[MadPixel] Cant init SDK with a null SDK key!");
            }
            else {
                MaxSdkCallbacks.OnSdkInitializedEvent += OnAppLovinInitialized;
                InitSDK();
                MaxSdk.TargetingData.Keywords = GetKeywords();
            }
        }

        private void InitSDK() {
            MaxSdk.SetSdkKey(Settings.SDKKey);
            MaxSdk.InitializeSdk();
        }

        private string[] GetKeywords() {
            bool bUpper = true;
            if (SystemInfo.deviceType == DeviceType.Handheld) {

                bUpper = SystemInfo.systemMemorySize > 5000 &&
                         SystemInfo.graphicsMemorySize > 2000 &&
                         SystemInfo.processorCount >= 8 &&
                         Screen.currentResolution.height >= 1920 &&
                         Screen.currentResolution.width >= 1080;

            }
            return new[] { bUpper ? "tier_upper" : "tier_lower" };
        }


        private void OnAppLovinInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration) {
            if (Settings.bShowMediationDebugger) {
                MaxSdk.ShowMediationDebugger();
            }

            if (Settings.bUseBanners) {
                InitializeBannerAds();
            }

            if (Settings.bUseRewardeds) {
                InitializeRewardedAds();
            }

            if (Settings.bUseInters) {
                InitializeInterstitialAds();
            }

            Debug.Log("[MadPixel] AppLovin is initialized");
            bInitialized = true;

            OnInitComplete?.Invoke();
        }

        #endregion

        #region Banners
        public void InitializeBannerAds() {
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.BannerID)) {
                BannerID = Settings.BannerID;
            } else {
                Debug.LogError("[MadPixel] Banner ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.BannerID_IOS)) {
                BannerID = Settings.BannerID_IOS;
            } else {
                Debug.LogError("Banner ID in Settings is Empty!");
            }
#endif
            MaxSdk.CreateBanner(BannerID, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(BannerID, Settings.BannerBackground);

            OnBannerInitialized?.Invoke();
        }

        private void OnBannerAdLoadedEvent(string type, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdLoadedEvent invoked. {type}, {adInfo}");
            }
            onBannerLoadedEvent?.Invoke(type, adInfo, null);
        }

        private void OnBannerAdLoadFailedEvent(string type, MaxSdkBase.ErrorInfo errorInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdLoadFailedEvent invoked. {type}, {errorInfo}");
            }
            onBannerLoadedEvent?.Invoke(type, null, errorInfo);
        }

        private void OnBannerAdRevenuePaidEvent(string type, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdRevenuePaidEvent invoked. {type}, {adInfo}");
            }
            onBannerRevenueEvent?.Invoke(type, adInfo);
        }

        public void ShowBanner(bool bShow, MaxSdkBase.BannerPosition NewPosition = MaxSdkBase.BannerPosition.BottomCenter) {
            if (bInitialized) {
                if (bShow) {
                    MaxSdk.UpdateBannerPosition(BannerID, NewPosition);
                    MaxSdk.ShowBanner(BannerID);
                }
                else {
                    MaxSdk.HideBanner(BannerID);
                }
            }
        }
        #endregion

        #region Interstitials
        public void InitializeInterstitialAds() {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplayEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        public void CancelInterAd() {
            onInterDismissedEvent?.Invoke();
        }

        private void LoadInterstitial() {
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.InterstitialID)) {
                InterstitialID = Settings.InterstitialID;
            } else {
                Debug.LogError("[MadPixel] Interstitial ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.InterstitialID_IOS)) {
                InterstitialID = Settings.InterstitialID_IOS;
            } else {
                Debug.LogError("Interstitial ID in Settings is Empty!");
            }
#endif
            MaxSdk.LoadInterstitial(InterstitialID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(false);
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadInterstitial", 3);
            Debug.LogWarning("OnInterstitialFailedEvent");
        }

        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnInterstitialFailedToDisplayEvent invoked");
            }
            LoadInterstitial();

            onErrorEvent?.Invoke(adInfo, errorInfo, AdsManager.EAdType.INTER);

            Debug.LogWarning("InterstitialFailedToDisplayEvent");
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnInterstitialDismissedEvent invoked");
            }

            LoadInterstitial();
            onInterDismissedEvent?.Invoke();
        }
        #endregion

        #region Rewarded
        public void InitializeRewardedAds() {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first RewardedAd
            LoadRewardedAd();
        }

        public void CancelRewardedAd() {
            onFinishAdsEvent?.Invoke(false);
            ShowedInfo = null;
        }

        private void LoadRewardedAd() {
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.RewardedID)) {
                RewardedID = Settings.RewardedID;
            } else {
                Debug.LogError("[MadPixel] Rewarded ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.RewardedID_IOS)) {
                RewardedID = Settings.RewardedID_IOS;
            } else {
                Debug.LogError("Rewarded ID in Settings is Empty!");
            }
#endif
            MaxSdk.LoadRewardedAd(RewardedID);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdDisplayedEvent invoked");
            }
            ShowedInfo = adInfo;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(true);
            ShowedInfo = adInfo; 
            if (bShowDebug) {
                Debug.Log("OnRewardedAdLoadedEvent invoked");
            }
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadRewardedAd", 3); 
            if (bShowDebug) {
                Debug.Log("OnRewardedAdLoadFailedEvent invoked");
            }
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            // Rewarded ad failed to display. We recommend loading the next ad

            if (bShowDebug) {
                Debug.Log("OnRewardedAdFailedToDisplayEvent invoked");
            }

            OnError(adInfo, errorInfo);
            LoadRewardedAd();
        }

        private void OnError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo) {
            onErrorEvent?.Invoke(adInfo, EInfo, AdsManager.EAdType.REWARDED);
            ShowedInfo = null;
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdDismissedEvent invoked");
            }

            if (ShowedInfo != null) {
                onFinishAdsEvent?.Invoke(false);
            }
            
            ShowedInfo = null;
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdReceivedRewardEvent invoked");
            }

            onFinishAdsEvent?.Invoke(ShowedInfo != null);
            ShowedInfo = null;
        }

        #endregion

        #region Show Ads

        public bool ShowInterstitial() {
            if (bInitialized && MaxSdk.IsInterstitialReady(InterstitialID)) {
                MaxSdk.ShowInterstitial(InterstitialID);
                return true;
            }

            return false;
        }

        public void ShowRewarded() {
            if (bInitialized && MaxSdk.IsRewardedAdReady(RewardedID)) {
                MaxSdk.ShowRewardedAd(RewardedID);
            }
        }

        public bool IsReady(bool bIsRewarded) {
            if (bInitialized) {
                if (bIsRewarded) {
                    return MaxSdk.IsRewardedAdReady(RewardedID);
                }
                else {
                    return MaxSdk.IsInterstitialReady(InterstitialID);
                }
            }
            return false;
        }
        #endregion

        #region Unsubscribers

        void OnDestroy() {
            UnsubscribeAll();
        }
        public void UnsubscribeAll() {
            if (bInitialized) {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialDismissedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialFailedToDisplayEvent;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdDismissedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
            }
        }

        #endregion
    }
}