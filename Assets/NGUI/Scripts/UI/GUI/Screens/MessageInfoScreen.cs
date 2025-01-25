using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    /// <summary>
    /// Информация о результатах боя
    /// </summary>
    public class MessageInfoScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private MessageInfoArmyElement groundArmyInfo;
        [SerializeField] private PointerButton exitButton;
        [SerializeField] private PointerButton backButton;

        private GuiController gui;
        private VisualCountryController countries;

        private Country _reportedCountry;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();

            exitButton.Init(OnClickCloseScreen);
            backButton.Init(OnBackClick);
        }

        public void Init(int countryID, bool win, int groundForce) // float? airForce, float? navalForce
        {
            _reportedCountry = countries.GetCountry(countryID);

            title.text = win ? "War report: you win!" : "War report: you lose!";

            // ground
            groundArmyInfo.SetValue(groundForce);
        }

        public void OnClickCloseScreen()
        {
            //if (!successful)
            //{
                //CameraService.Instance.ZoomToPlayersFOV(); todo если используется движение камеры в начале боя - раскомментировать
                gui.ShowRootScren();
            //}
        }

        public void OnBackClick()
        {
            gui.Exit();
        }

        public void OnGoToClick()
        {
            gui.Exit();
            CameraService.Instance.MoveTo(_reportedCountry);
        }
    }
}