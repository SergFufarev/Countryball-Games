using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FunnyBlox;
using SPSDigital.IAP;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

namespace SPSDigital.ADS
{
    //[Obsolete]
    public class ADSInterstititalService : MonoBehaviour
    {
        [SerializeField]
        private float interstititalDelay = 60f;

        [SerializeField] private GameObject uiPopupInterReward;

        private bool interIsReady;

        private int currentInterPopupIndex; // индекс текущего интер-попапа

        private List<Type> showedInterPopupTypes = new(); // какие типы интер-попапов были показаны в текущей сессии

        /// <summary>
        /// Попапы, которые открываются раз в минуту. Каждый из них при закрытии тригерит нефорсированный интер
        /// </summary>
        private readonly InterPopupData[] interPopups = new InterPopupData[]
        {
            new(typeof(StarterPackPopup), true),
            new(typeof(PremiumPackPopup), true),
            new(typeof(NoAdsPopup), true),
            new(typeof(CoinsForAdScreen), false),
            new(typeof(IncomeAccelerationScreen), false)
        };

        private struct InterPopupData
        {
            private Type popupType;
            private bool oneTimeInSession;

            public Type PopupType => popupType;
            public bool OneTimeInSession => oneTimeInSession;

            public InterPopupData(Type popupType, bool oneTimeInSession)
            {
                this.popupType = popupType;
                this.oneTimeInSession = oneTimeInSession;
            }
        }

        // todo в будущем выпилить все методы Start кроме одного, за эталон взять Кайдзю
        void Start()
        {
            AdsService.Instance.InterDismissed += OnInterDismissed;
            AdsService.Instance.FinishRewarded += (complete) => RestartDelay();

            RestartDelay();
        }

        public void TryShowInterPopup()
        {
            //game.GameVersion == GameVersionType.VersionB
            if (!interIsReady || (GuiController.Instance.CurrentScreen is not GameScreen && GuiController.Instance.CurrentScreen is not BattlePrepareScreen)) return;

            ShowInterPopup();
        }

        private void ShowInterPopup()
        {
            interIsReady = false;
            interRewarded = true;

            var interPopupType = interPopups[currentInterPopupIndex].PopupType;
            bool oneTimeShowInSession = interPopups[currentInterPopupIndex].OneTimeInSession;
            currentInterPopupIndex++;
            if (currentInterPopupIndex >= interPopups.Length) currentInterPopupIndex = 0;

            bool available = true;

            // тут не очень хорошо с точки зрения расширения без модификации

            // check avaible
            if (interPopupType == typeof(NoAdsPopup))
            {
                available = !PlayerPrefs.HasKey(CommonData.PREFSKEY_ADS_REMOVED) && !GemShop.IsPremiumActive;
            }
            else if (interPopupType == typeof(StarterPackPopup))
            {
                available = !GemShop.IsStarterPackPurchased;
            }
            else if (interPopupType == typeof(PremiumPackPopup))
            {
                available = !GemShop.IsPremiumActive;
            }
            else if (interPopupType == typeof(IncomeAccelerationScreen))
            {
                available = !CurrencyService.Instance.IsIncomeBoost;
            }

            if (oneTimeShowInSession && showedInterPopupTypes.Contains(interPopupType)) available = false;

            if (!available)
            {
                ShowInterPopup();
                return;
            }

            if (interPopupType == typeof(CoinsForAdScreen))
            {
                int reward = AdsService.Instance.RewardCoinsForAd;
                (GuiController.Instance.FindScreen(interPopupType) as CoinsForAdScreen).Init(reward);
            }

            GuiController.Instance.Show(interPopupType);

            if (!showedInterPopupTypes.Contains(interPopupType)) showedInterPopupTypes.Add(interPopupType);

            RestartDelay();
        }

        public void RewardedInterCancelled()
        {
            RestartDelay();
        }

        public void RestartDelay()
        {
            //if (game.GameVersion == GameVersionType.VersionB) return;

            interIsReady = false;
            interRewarded = false;

            StopAllCoroutines();
            StartCoroutine(Delay());
        }

        [ContextMenu("SkipDelay")]
        public void SkipDelay()
        {
            StopCoroutine(Delay());
            interIsReady = true;
            TryShowInterPopup();
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(interstititalDelay);
            interIsReady = true;
            TryShowInterPopup();
        }

        private bool interRewarded = false; // интеры, которые вызываются каждые n секунд дают игроку награду. Но не каждый интер должен давать награду (например интер для бомбы)

        private void OnInterDismissed()
        {
            if (interRewarded)
            {
                float rewardValue = GetInterReward;
                var getRewardScreen = GuiController.Instance.FindScreen<GetCoinsRewardForAdScreen>();
                getRewardScreen.Init((int)rewardValue);
                GuiController.Instance.Show(getRewardScreen);
                interRewarded = false;
            }

            RestartDelay();
        }

        public float GetInterReward => Mathf.Max(CommonData.MoneyPerSecondBase * 30, 1);
    }
}