using UnityEngine;
using FunnyBlox;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class MessageBattlePrepareScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI attackerName;
        [SerializeField] private TextMeshProUGUI defenderName;
        [SerializeField] private TextMeshProUGUI attackerForceLabel;
        [SerializeField] private TextMeshProUGUI defenderForceLabel;
        [SerializeField] private Renderer enemyFlag;
        [SerializeField] private Renderer playerFlag;
        [SerializeField] private PointerButton backButton;
        [SerializeField] private PointerButton exitButton;
        [SerializeField] private PointerButton viewButton;

        private Country _playerCountry;

        private GuiController gui;
        private VisualCountryController countries;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();

            backButton.Init(OnBackClick);
            exitButton.Init(OnClickCloseScreen);
            viewButton.Init(OnGoToClick);
        }

        public void SetInfo(int enemyCountryID, int playerCountryID)
        {
            bool isResistance = enemyCountryID == playerCountryID;
            title.text = isResistance ? "The rebels are going to attack!" : "An attack is being prepared!";

            Country enemyCountry = countries.GetCountry(enemyCountryID);
            Country playerCountry = countries.GetCountry(playerCountryID);

            _playerCountry = playerCountry;
            attackerName.text = enemyCountry.LocalCountryData.Name;
            defenderName.text = playerCountry.LocalCountryData.Name;

            int enemyForce = isResistance ? (int)enemyCountry.GetResistanceForceValue : enemyCountry.ArmyForce;
            int playerForce = CountrySaveLoad.LoadCountry(playerCountryID).GetArmyUpgradeLevels();

            attackerForceLabel.text = $"Force: {enemyForce}";
            defenderForceLabel.text = $"Force: {playerForce}";

            enemyFlag.material = isResistance ? enemyCountry.GetBaseOwnerMaterial : enemyCountry.GetCountryOwnerMaterial;
            playerFlag.material = countries.PlayerBaseCountry.GetCountryOwnerMaterial;
        }

        public void OnClickCloseScreen()
        {
            if (TryShowRevelsMessage()) return;

            gui.ShowRootScren();
        }

        public void OnBackClick()
        {
            if (TryShowRevelsMessage()) return;

            gui.Show<MessagesScreen>();
        }

        public void OnGoToClick()
        {
            if (TryShowRevelsMessage()) return;

            gui.ShowMainScreen();
            CameraService.Instance.MoveTo(_playerCountry);
        }

        private bool TryShowRevelsMessage()
        {
            var tutor = gui.TutorContainer;

            if (!tutor.IsComplete(TutorContainer.RabelsTutorID))
            {
                tutor.CompleteTutorial(TutorContainer.RabelsTutorID);
                MessageService.Instance.ShowRebelsTutorMessage();
                return true;
            }

            return false;
        }
    }
}