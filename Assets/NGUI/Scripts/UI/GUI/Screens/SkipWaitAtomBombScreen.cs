using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class SkipWaitAtomBombScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton buyButton;

        private GuiController gui;
        private BattleInGameService battle;

        public const int Cost = 100;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            battle = cts.Get<BattleInGameService>();

            closeButton.Init(OnCloseClick);
            buyButton.Init(OnBuyClick);
        }

        public void OnCloseClick() => gui.Exit();

        public void OnBuyClick()
        {
            BuySkipBombForHard();
        }

        private void BuySkipBombForHard()
        {
            CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, Cost, () =>
            {
                var recoveryTime = battle.BattleConfig.BombRecoverTime;
                var recoveryTimeSpan = new TimeSpan(recoveryTime.Hours, recoveryTime.Minutes, recoveryTime.Seconds);
                SaveManager.Save(CommonData.PREFSKEY_BOMB_ATTACK_TIME, DateTime.Now - recoveryTimeSpan);
                NotificationManager.Instance.CancelBombNotification();
                gui.FindScreen<GameScreen>().availableSkipBombForAds = true;

                gui.Exit();
            },
            () =>
            {
                var shop = gui.FindScreen<ShopScreen>();
                shop.PrepareForBuyCurrency();
                gui.Show(shop);
            });
        }
    }
}