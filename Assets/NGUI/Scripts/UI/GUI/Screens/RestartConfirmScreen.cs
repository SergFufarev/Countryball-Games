using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox.Saver;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class RestartConfirmScreen : GuiScreen
    {
        [SerializeField] private PointerButton yesButton;
        [SerializeField] private PointerButton noButton;
        [SerializeField] private PointerButton closeButton;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            yesButton.Init(OnYesClick);
            noButton.Init(OnNoClick);
            closeButton.Init(OnCloseClick);
        }

        public void OnYesClick() => RestartServise.Instance.Restart();

        public void OnNoClick() => Close();

        public void OnCloseClick() => Close();

        private void Close() => gui.Show<SettingsScreen>();
    }
}