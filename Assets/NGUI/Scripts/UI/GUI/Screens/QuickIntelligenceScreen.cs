using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class QuickIntelligenceScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton weakButton;
        [SerializeField] private PointerButton smallButton;
        [SerializeField] private PointerButton middleButton;
        [SerializeField] private PointerButton bigButton;
        [SerializeField] private PointerButton hugeButton;

        private IntRange weakTutorForce = new(0, 5);

        private bool inWeakTutor;

        private GuiController gui;
        private VisualCountryController countries;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();

            closeButton.Init(OnCloseClick);
            weakButton.Init(OnClick_Weak);
            smallButton.Init(OnClick_Small);
            middleButton.Init(OnClick_Middle);
            bigButton.Init(OnClick_Big);
            hugeButton.Init(OnClick_Huge);
        }

        protected override void OnShow()
        {
            base.OnShow();

            var tutor = gui.TutorContainer;
            tutor.TryShowInUI(TutorContainer.IntelligenceTutorID, weakButton.transform, out inWeakTutor, true);

            weakButton.SetInteractalbe(inWeakTutor ? true : countries.FindCountryForForce(CountryForceType.Weak));
            smallButton.SetInteractalbe(inWeakTutor ? false : countries.FindCountryForForce(CountryForceType.Small));
            middleButton.SetInteractalbe(inWeakTutor ? false : countries.FindCountryForForce(CountryForceType.Middle));
            bigButton.SetInteractalbe(inWeakTutor ? false : countries.FindCountryForForce(CountryForceType.Big));
            hugeButton.SetInteractalbe(inWeakTutor ? false : countries.FindCountryForForce(CountryForceType.Huge));
        }

        protected override void OnHide()
        {
            base.OnHide();

            var tutor = gui.TutorContainer;
            if (tutor.InTutorial) tutor.BreakTutorial();
        }

        public void OnCloseClick() => gui.Exit();
        public void OnClick_Weak() => OnChooseCountryClick(CountryForceType.Weak);
        public void OnClick_Small() => OnChooseCountryClick(CountryForceType.Small);
        public void OnClick_Middle() => OnChooseCountryClick(CountryForceType.Middle);
        public void OnClick_Big() => OnChooseCountryClick(CountryForceType.Big);
        public void OnClick_Huge() => OnChooseCountryClick(CountryForceType.Huge);

        private void OnChooseCountryClick(CountryForceType countryForceType)
        {
            var tutor = gui.TutorContainer;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.IntelligenceTutorID) tutor.CompleteTutorial();

            Country bestCandidate = null;

            if (inWeakTutor)
            {
                bestCandidate = countries.FindEnemyInForceRange(weakTutorForce);
                if (bestCandidate == null) bestCandidate = countries.FindCountryForForce(countryForceType);
            }
            else bestCandidate = countries.FindCountryForForce(countryForceType);

            if (bestCandidate == null) return;

            gui.CurrentScreen.Hide();
            CameraService.Instance.MoveTo(bestCandidate, () => Intelligence.Instance.PrepareIntelligence(bestCandidate));
        }
    }
}

public enum CountryForceType
{
    Weak,
    Small,
    Middle,
    Big,
    Huge
}