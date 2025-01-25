using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SPSDigital.ADS;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class NoAdsPopup : GuiScreen
    {
        [SerializeField] private PointerButton confirmButton;
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private string productSKU;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            confirmButton.Init(OnConfirmClick);
            closeButton.Init(OnCloseClick);
        }

        public void OnConfirmClick()
        {
            gui.Exit();
            gui.FindScreen<ShopScreen>().GemShop.BuyRealProduct(productSKU);
        }

        public void OnCloseClick()
        {
            gui.Exit();
            AdsService.Instance.ShowInterstitial("close_no_ads_pack_popup");
        }
    }
}