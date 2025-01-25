using System.Collections;
using UnityEngine;
using TheSTAR.Sound;
using FunnyBlox;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class GetCoinsRewardForAdScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI rewardLable;
        [SerializeField] private PointerButton okButton;

        private float reward;
        private GuiController gui;
        //private SoundController sounds;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            //sounds = cts.Get<SoundController>();

            okButton.Init(OnOKClick);
        }

        public void Init(int rewardValue)
        {
            reward = rewardValue;
        }

        protected override void OnShow()
        {
            base.OnShow();

            rewardLable.text = $"+{reward}";
            CurrencyService.Instance.AddCurrency(CurrencyType.Money, reward);
        }

        public void OnOKClick()
        {
            gui.ShowRootScren();

            SoundController.Instance.PlaySound(SoundType.Purchase);
        }
    }
}