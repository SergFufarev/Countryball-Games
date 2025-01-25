using System;
using UnityEngine;
using UnityEngine.UI;
using TheSTAR.Utility;

namespace TheSTAR.GUI.Screens
{
    public class LoadScreen : GuiScreen
    {
        [SerializeField] private Image fon;

        private LoadDelegate loading;
        private Action endLoadingAction;
        private const float LoadDelayTime = 0.2f;
        private const float LoadTime = 0.8f;

        public delegate void LoadDelegate(Action endLoadAction);

        public void Init(Sprite fonSprite, LoadDelegate loading, Action endLoadingAction)
        {
            fon.sprite = fonSprite;
            Init(loading, endLoadingAction);
        }

        public void Init(LoadDelegate loading, Action endLoadingAction)
        {
            this.loading = loading;
            this.endLoadingAction = endLoadingAction;
        }

        protected override void OnShow()
        {
            base.OnShow();

            TimeUtility.WaitAsync(LoadDelayTime, () => loading(DelayEndLoadAction));
        }

        private void DelayEndLoadAction()
        {
            TimeUtility.WaitAsync(LoadTime, endLoadingAction);
        }
    }
}