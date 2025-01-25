using UnityEngine;
using SPSDigital.ADS;
using TheSTAR.Sound;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class IncomeAccelerationScreen : GuiScreen
    {
        private const int StarsCost = 100;
        private const int BoostTimeSeconds = 180;

        private GuiController gui;
        //private SoundController sounds;

        [SerializeField] private PointerButton exitButton;
        [SerializeField] private PointerButton buyForAdsButton;
        [SerializeField] private PointerButton buyForHardButton;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            //sounds = cts.Get<SoundController>();

            exitButton.Init(OnCloseClick);
            buyForAdsButton.Init(OnBuyForAdClick);
            buyForHardButton.Init(OnBuyForStarsClick);
        }

        public void OnCloseClick()
        {
            gui.Exit();
            AdsService.Instance.ShowInterstitial("close_income_acceleration");
        }

        public void OnBuyForStarsClick()
        {
            CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, StarsCost, () =>
            {
                GiveReward();
            });
        }

        public void OnBuyForAdClick() => AdsService.Instance.ShowRewarded(AdsService.AD_PLACE_INCOME_BOOST);

        public void GiveReward()
        {
            CurrencyService.Instance.SetIncomeBoost(BoostTimeSeconds);
            gui.Exit();
            SoundController.Instance.PlaySound(SoundType.Purchase);

            AnalyticsManager.Instance.Log(AnalyticSectionType.Misc, "x2speed_bonus");
        }
    }
}