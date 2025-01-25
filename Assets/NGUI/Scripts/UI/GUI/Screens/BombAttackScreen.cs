using FunnyBlox;
using UnityEngine;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class BombAttackScreen : GuiScreen
    {
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton cameraButton;

        private GuiController gui;
        private VisualCountryController countries;
        private BattleInGameService battle;
        //private Sound.SoundController sounds;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();
            battle = cts.Get<BattleInGameService>();
            //sounds = cts.Get<Sound.SoundController>();

            closeButton.Init(OnCloseClick);
            cameraButton.Init(OnCameraClick);
        }

        public void BombAttack(int countryId)
        {
            var countryToAttack = countries.GetCountry(countryId);
            if (countryToAttack.LocalCountryData.Owner == CommonData.PlayerID) return;

            battle.BombAttack(countryToAttack);

            var gameScreen = gui.FindScreen<GameScreen>();
            gameScreen.OnBombActivate();
            gameScreen.LoadData();
            gui.Exit();
        }

        public void OnCloseClick() => gui.Exit();

        public void OnCameraClick() => CameraService.Instance.SwitchCameraMode();
    }
}