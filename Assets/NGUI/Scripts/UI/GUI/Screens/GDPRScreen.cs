using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class GDPRScreen : GuiScreen
    {
        [SerializeField] private PointerButton privacyBtn;
        [SerializeField] private PointerButton termsBtn;
        [SerializeField] private PointerButton acceptBtn;

        private GameController game;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            game = cts.Get<GameController>();

            privacyBtn.Init(OnPrivacyClick);
            termsBtn.Init(OnTermsOfUseClick);
            acceptBtn.Init(Accept);
        }

        public void Accept()
        {
            MaxSdk.SetHasUserConsent(true);
            game.OnAcceptGDPR();
        }

        public void OnPrivacyClick()
        {
            Application.OpenURL(CommonData.PRIVACY_URL);
        }

        public void OnTermsOfUseClick()
        {
            Application.OpenURL(CommonData.TERMS_URL);
        }
    }
}