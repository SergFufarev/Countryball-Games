using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox.OfflineReward;
using TheSTAR.Sound;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class RewardScreen : GuiScreen
    {
        [SerializeField] private PointerButton claimBtn;
        [SerializeField] private ReducedBigText rewardCounterLabel;

        private int _reward;
        private GuiController gui;
        //private SoundController sounds;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            //sounds = cts.Get<SoundController>();
            claimBtn.Init(OnClaimClick);
        }

        public void SetData(int reward)
        {
            _reward = reward;
            rewardCounterLabel.SetValue(reward);
        }

        public void OnClaimClick()
        {
            CurrencyService.Instance.AddCurrency(CurrencyType.Money, _reward);
            gui.Exit();
            SoundController.Instance.PlaySound(SoundType.Purchase);
        }
    }
}