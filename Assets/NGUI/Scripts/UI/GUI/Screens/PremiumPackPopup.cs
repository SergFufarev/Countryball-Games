using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPSDigital.ADS;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class PremiumPackPopup : GuiScreen
    {
        [SerializeField] private string productSKU;
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton buyButton;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            closeButton.Init(OnCloseClick);
            buyButton.Init(OnConfirmClick);
        }

        public void OnConfirmClick()
        {
            gui.Exit();
            gui.FindScreen<ShopScreen>().GemShop.BuyRealProduct(productSKU);
        }

        public void OnCloseClick()
        {
            gui.Exit();
            AdsService.Instance.ShowInterstitial("close_premium_pack_popup");
        }
    }
}