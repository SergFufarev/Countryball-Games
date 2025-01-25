using UnityEngine;
using SPSDigital.ADS;
using TMPro;
using TheSTAR.Utility.Pointer;
using MAXHelper;

namespace TheSTAR.GUI.Screens
{
    public class CoinsForAdScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeBtn;
        [SerializeField] private PointerButton watchAdBtn;
        [SerializeField] private TextMeshProUGUI rewardLable;

        private int rewardValue;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            closeBtn.Init(OnCloseClick);
            watchAdBtn.Init(OnConfirmClick);
        }

        public void Init(int rewardValue)
        {
            if (rewardValue < 1) rewardValue = 1;
            this.rewardValue = rewardValue;
        }

        protected override void OnShow()
        {
            base.OnShow();
            rewardLable.text = $"+{rewardValue}";
        }

        public void OnCloseClick()
        {
            gui.Exit();
            AdsService.Instance.ShowInterstitial("close_coins_for_ad");
        }

        public void OnConfirmClick()
        {
            AdsManager.ShowRewarded(gameObject, (success) =>
            {
                if (success) GiveRewardForAd();
            }, AdsService.AD_PLACE_COINS_FOR_AD);
        }

        public void GiveRewardForAd()
        {
            AdsService.Instance.OnCompletedRewardedCoinsForAds();

            var getRewardScreen = gui.FindScreen<GetCoinsRewardForAdScreen>();
            getRewardScreen.Init(rewardValue);
            gui.Show(getRewardScreen);

            AnalyticsManager.Instance.Log(AnalyticSectionType.Misc, "reward_chest");
        }
    }
}