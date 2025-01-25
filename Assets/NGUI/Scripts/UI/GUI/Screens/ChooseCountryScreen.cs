using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class ChooseCountryScreen : GuiScreen
    {
        [SerializeField] private PointerButton cameraBtn;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            cameraBtn.Init(OnCameraClick);
        }

        public void OnCameraClick() => CameraService.Instance.SwitchCameraMode();
    }
}