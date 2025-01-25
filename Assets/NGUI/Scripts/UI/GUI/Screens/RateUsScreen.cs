using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class RateUsScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton notMuchButton;
        [SerializeField] private PointerButton rateButton;

        private GuiController gui;
        private RateUsController rateUs;
        //private IARManager iar;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            rateUs = cts.Get<RateUsController>();
            //iar = cts.Get<IARManager>();

            closeButton.Init(Close);
            notMuchButton.Init(Close);
            rateButton.Init(Rate);
        }

        public void Close()
        {
            gui.Exit();
            rateUs.ScheduleRateUs();
        }

        public void Rate()
        {
            CommonData.GameWasRated = true;
            SaveManager.Save(CommonData.PREFSKEY_GAME_WAS_RATED, CommonData.GameWasRated);

            AnalyticsManager.Instance.Log(AnalyticSectionType.Tutorial, "5_rate_us");

            Application.OpenURL(RateUsController.GetRateUsURL);
            //IARManager.Instance.ShowInAppReview();
            //iar.ShowInAppReview();
            gui.ShowMainScreen();
        }
    }
}