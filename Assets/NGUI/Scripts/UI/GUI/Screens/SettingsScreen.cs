using UnityEngine;
using FunnyBlox;
using TheSTAR.Sound;
using TheSTAR.Utility.Pointer;
using Battle;

namespace TheSTAR.GUI.Screens
{
    public class SettingsScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeBtn;
        [SerializeField] private PointerButton rateUsBtn;
        [SerializeField] private PointerButton privacyBtn;
        [SerializeField] private PointerButton restartBtn;

        [SerializeField] private SettingsUiOption musicOption;
        [SerializeField] private SettingsUiOption soundsOption;
        [SerializeField] private SettingsUiOption vibrationOption;
        [SerializeField] private SettingsUiOption notificationOption;
        [SerializeField] private SettingsUiOption qualityOption;

        [Space]
        [SerializeField] private GameObject cheatsContainer;

        [Space]
        [SerializeField] private bool inBattle;

        private GuiController gui;

        private ControllerStorage cts;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            this.cts = cts;
            gui = cts.Get<GuiController>();

            closeBtn.Init(OnCloseClick);
            rateUsBtn.Init(OnRateUsClick);
            privacyBtn.Init(onPrivacyClick);
            restartBtn.Init(OnRestartClick);

            musicOption.Init(OnMusicClick);
            soundsOption.Init(OnSoundsClick);
            vibrationOption.Init(OnVibrationClick);
            notificationOption.Init(OnNotificationsClick);
            qualityOption.Init(OnQualityClick);
        }

        protected override void OnShow()
        {
            base.OnShow();
            UpdateVisual();
        }

        protected override void OnHide()
        {
            base.OnHide();

            SaveData();
        }

        #region Clicks

        public void OnCloseClick()
        {
            gui.Exit();
        }

        public void OnRestartClick()
        {
            gui.Show<RestartConfirmScreen>();
        }

        public void OnMusicClick()
        {
            CommonData.MusicOn = !CommonData.MusicOn;
            UpdateVisual();

            if (CommonData.MusicOn) SoundController.Instance.PlayMusic(MusicType.MainTheme);
            else SoundController.Instance.StopMusic();
        }

        public void OnSoundsClick()
        {
            CommonData.SoundsOn = !CommonData.SoundsOn;
            UpdateVisual();
        }

        public void OnVibrationClick()
        {
            CommonData.VibrationsOn = !CommonData.VibrationsOn;
            UpdateVisual();
        }

        public void OnNotificationsClick()
        {
            CommonData.NotificationsOn = !CommonData.NotificationsOn;
            UpdateVisual();
        }

        public void OnQualityClick()
        {
            CommonData.UseHighQuality = !CommonData.UseHighQuality;
            UpdateVisual();

            if (inBattle) cts.Get<BattleController>().AdaptiveQuality.SetQuality(CommonData.UseHighQuality ? AdaptiveQualityType.Height : AdaptiveQualityType.Low);
        }

        public void OnRateUsClick()
        {
            gui.Show<RateUsScreen>();
        }

        public void onPrivacyClick()
        {
            Application.OpenURL(CommonData.PRIVACY_URL);
        }

        public void GiveTestCoins10k()
        {
            CurrencyService.Instance.AddCurrency(CurrencyType.Money, 10000);
        }

        public void GiveTestCoins1M()
        {
            CurrencyService.Instance.AddCurrency(CurrencyType.Money, 1000000);
        }

        #endregion

        private void SaveData()
        {
            SaveManager.Save(CommonData.PREFSKEY_MUSIC_ON, CommonData.MusicOn);
            SaveManager.Save(CommonData.PREFSKEY_SOUNDS_ON, CommonData.SoundsOn);
            SaveManager.Save(CommonData.PREFSKEY_VIBRATION_ON, CommonData.VibrationsOn);
            SaveManager.Save(CommonData.PREFSKEY_NOTIFICATIONS_ON, CommonData.NotificationsOn);
            SaveManager.Save(CommonData.PREFSKEY_USE_HEIGHT_QUALITY, CommonData.UseHighQuality);
        }

        private void UpdateVisual()
        {
            musicOption.SetValue(CommonData.MusicOn);
            soundsOption.SetValue(CommonData.SoundsOn);
            vibrationOption.SetValue(CommonData.VibrationsOn);
            notificationOption.SetValue(CommonData.NotificationsOn);
            qualityOption.SetValue(CommonData.UseHighQuality);
        }
    }
}