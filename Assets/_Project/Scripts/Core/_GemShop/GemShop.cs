using System;
using SPSDigital.Metrica;
using WP;
using UnityEngine;
using UnityEngine.Purchasing;
using FunnyBlox;
using SPSDigital.ADS;
using TheSTAR.Sound;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

namespace SPSDigital.IAP
{
    public class GemShop : MonoBehaviour
    {
        public const string PREMIUM_PACK_ID = "premium_subscription_country.balls.i.w";
        public const string STARTER_PACK_ID = "starter_pack_country.balls.i.w";

        [SerializeField] private GemShopData gemShopData;
        [SerializeField] private Transform uiMenuItems;
        [SerializeField] private GemShopItem[] gemShopItems; // основные айтемы (покупка харды за реал)
        [SerializeField] private SoftShopItem[] softShopItems; // софт айтемы (покупка софты за харду)
        [SerializeField] private AdsShopItem removeAdsItem; // айтем на отключение рекламы
        [SerializeField] private GemShopItem[] packs; // объекты паков

        [SerializeField] private int idCatSkin = 2;
        [SerializeField] private int amountBossRushKeys = 12;
        [SerializeField] private int amountGoldRushKeys = 12;

        public static bool IsPremiumActive
        {
            get
            {
                bool value =
                    PlayerPrefs.HasKey(PREMIUM_PACK_ID) &&
                    SaveManager.Load<int>(PREMIUM_PACK_ID) == 1 &&
                    DateTime.Today < SaveManager.Load<DateTime>(CommonData.PREFSKEY_PREMIUM_END_TIME);

                return value;
            }
        }

        public static bool IsStarterPackPurchased => PlayerPrefs.HasKey(STARTER_PACK_ID) && SaveManager.Load<int>(STARTER_PACK_ID) == 1;

        private GuiController gui;
        //private SoundController sounds;

        public void Init(ControllerStorage cts)
        {
            gui = cts.Get<GuiController>();
            //sounds = cts.Get<SoundController>();
        }

        private void OnEnable()
        {
            MobileInAppPurchaser.Instance.OnPurchaseResult += PurchaseCompletedHandler;
            UpdateUI();
        }

        private void OnDisable()
        {
            if (MobileInAppPurchaser.Instance) MobileInAppPurchaser.Instance.OnPurchaseResult -= PurchaseCompletedHandler;
        }

        private void UpdateUI()
        {
            bool firstBuy = false; // !PlayerPrefs.HasKey(PLAYERPREFS_FIST_BUY);

            // main items

            for (int i = 0; i < gemShopItems.Length; i++) gemShopItems[i].Init(gemShopData.GemShopItemsList[i], BuyRealProduct, firstBuy);

            // soft items

            for (int i = 0; i < softShopItems.Length; i++) softShopItems[i].Init(gemShopData.SoftItemsList[i], BuySoftProduct);

            // remove ads

            if (removeAdsItem != null) removeAdsItem.Init(gemShopData.RemoveAdsData, BuyRemoveAds);
            UpdateAdsButtonActivity();

            // packs

            int packIndex = 0;
            foreach (SGemShopItem data in gemShopData.GemShopItemsList)
            {
                if (data.Type == EGemShopItemType.Pack)
                {
                    packs[packIndex].Init(data, BuyRealProduct, false);

                    if (SaveManager.Load<int>(data.SKU) == 1)
                    {
                        bool needShow = false;
                        if (data.SKU == PREMIUM_PACK_ID) needShow = !IsPremiumActive;

                        Array.Find(packs, pack => pack.SKU == data.SKU).SetVisible(needShow);
                    }

                    packIndex++;
                }
            }
        }

        private void UpdateAdsButtonActivity()
        {
            if (removeAdsItem != null)
            {
                bool visible = !PlayerPrefs.HasKey(CommonData.PREFSKEY_ADS_REMOVED) && !IsPremiumActive;
                removeAdsItem.SetVisible(visible);
            }
        }

        /// <summary>
        /// Покупка за реал
        /// </summary>
        public void BuyRealProduct(string sku)
        {
            if (Debug.isDebugBuild) GiveRewardForRealBuy(sku);
            else
            {
                MobileInAppPurchaser.Instance.BuyProduct(sku);
                AppMetricaIAP.ProductIDConsumable = sku;
            }
        }

        /// <summary>
        /// Покупка отключения рекламы
        /// </summary>
        private void BuyRemoveAds()
        {
            string sku = gemShopData.RemoveAdsData.SKU;
            MobileInAppPurchaser.Instance.BuyProduct(sku);
            AppMetricaIAP.ProductIDConsumable = sku;
        }

        /// <summary>
        /// Покупка софты за харду
        /// </summary>
        private void BuySoftProduct(int id)
        {
            var data = gemShopData.SoftItemsList.Find(item => item.Id == id);

            CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, data.Price, () =>
            {
                float reward = Math.Max(data.RewardHours * 60 * 60 * CurrencyService.Instance.FinalMoneyPerSecond, 1);
                CurrencyService.Instance.AddCurrency(CurrencyType.Money, reward);
                SoundController.Instance.PlaySound(SoundType.Purchase);
            });
        }

        //------EVENTS------

        private void PurchaseCompletedHandler(bool state, WPReceipt product)
        {
            if (!state) return;

            GiveRewardForRealBuy(product.SKU);
        }

        private void GiveRewardForRealBuy(string productSKU)
        {
            SGemShopItem item;

            if (productSKU == "remove_ads") item = gemShopData.RemoveAdsData;
            else item = gemShopData.GemShopItemsList.Find(item => item.SKU == productSKU);

#if SPSDIGITAL_METRICA
            AppMetricaBridge.ReportEvent(AppMetricaVariables.IAP_SUCCESS
                , AppMetricaBridge.SetParametrs(
                    inapp_id: item.SKU
                    , currency: "USD"
                    , price: item.Price.ToString()
                    , inapp_type: item.Type.ToString()
                ));
#endif

            switch (item.Type)
            {
                case EGemShopItemType.Hard:

                    /*
                    if (!PlayerPrefs.HasKey(PLAYERPREFS_FIST_BUY))
                    {
                        // bonus x2
                        CurrencyService.Instance.AddCurrency(CurrencyType.Stars, item.GemAmount * 2);
                        CurrencyService.Instance.AddCurrency(CurrencyType.Money, item.CoinAmount * 2);

                        foreach (GemShopItem shopItem in gemShopItems) shopItem.SetIsFirst(false);

                        SaveManager.Save(PLAYERPREFS_FIST_BUY, 1);
                    }
                    else
                    {
                    */
                        CurrencyService.Instance.AddCurrency(CurrencyType.Stars, item.GemAmount);
                        CurrencyService.Instance.AddCurrency(CurrencyType.Money, item.CoinAmount);
                    //}

                    for (int i = 0; i < gemShopData.GemShopItemsList.Count; i++)
                    {
                        if (gemShopData.GemShopItemsList[i].SKU == productSKU)
                        {
                            AnalyticsManager.Instance.Log(AnalyticSectionType.InApp, $"{i + 1}_buy hard {gemShopData.GemShopItemsList[i].Price}");
                            break;
                        }
                    }

#if SPSDIGITAL_METRICA
                    AppMetrica.Instance.ReportEvent(
                        string.Format(AppMetricaVariables.IAP_STUFF, item.Id, item.Price));
#endif

                    break;
                case EGemShopItemType.Pack:
                    if (!PlayerPrefs.HasKey(item.SKU) || PlayerPrefs.GetInt(item.SKU) == 0)
                    {
#if SPSDIGITAL_METRICA
                        AppMetrica.Instance.ReportEvent(AppMetricaVariables.IAP_STARTER);
#endif

                        // premium
                        if (item.ProductType == ProductType.Subscription)
                        {
                            RemoveAds();
                            SaveManager.Save(CommonData.PREFSKEY_PREMIUM_END_TIME, DateTime.Today.AddYears(10));

                            ShowDailyPremiumBonus();

                            AnalyticsManager.Instance.Log(AnalyticSectionType.InApp, "buy_premium");

                            if (!gui.TutorContainer.IsComplete(TutorContainer.BuyCustomHatTutorID))
                            {
                                gui.TutorContainer.CompleteTutorial(TutorContainer.BuyCustomHatTutorID);
                            }
                        }

                        // starter
                        else
                        {
                            CurrencyService.Instance.AddCurrency(CurrencyType.Stars, item.GemAmount);
                            CurrencyService.Instance.AddCurrency(CurrencyType.Money, item.CoinAmount);

                            // кастомка
                            CountryBallVisualService.Instance.OnBuyCustomisation(CustomizationHatType.Capcolonel);

                            // + сразу надеваем игроку, чтобы он сразу увидел награду
                            CountryBallVisualService.Instance.SetCustomisation(CustomizationHatType.Capcolonel, CountryBallVisualService.Instance.GetPlayerFaceType);

                            AnalyticsManager.Instance.Log(AnalyticSectionType.InApp, "buy_starter");
                        }

                        SaveManager.Save(item.SKU, 1);

                        if (packs != null && packs.Length > 0)
                        {
                            var pack = Array.Find(packs, pack => pack.SKU == item.SKU);
                            if (pack != null) pack.SetVisible(false);
                        }
                    }

                    break;
                case EGemShopItemType.NoAds:
                    if (!PlayerPrefs.HasKey(CommonData.PREFSKEY_ADS_REMOVED)) SaveManager.Save(CommonData.PREFSKEY_ADS_REMOVED, 1);

                    RemoveAds();
                    break;
            }

            CurrencyService.Instance.SaveData();
            SoundController.Instance.PlaySound(SoundType.Purchase); 
        }

        public bool premiumBonusReceived = false;

        public void ShowDailyPremiumBonus()
        {
            var item = gemShopData.GemShopItemsList.Find(info => info.SKU == PREMIUM_PACK_ID);
            var rewardScreen = gui.FindScreen<RewardScreen>();
            rewardScreen.SetData(item.CoinAmount);
            gui.Show(rewardScreen);
            premiumBonusReceived = true;
        }

        private void RemoveAds()
        {
            AdsService.Instance.allAdsRemoved = true;
            AdsService.Instance.HideBannerAds();

            UpdateAdsButtonActivity();
        }
    }
}