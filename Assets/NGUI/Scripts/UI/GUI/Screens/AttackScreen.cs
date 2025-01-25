using UnityEngine;
using FunnyBlox;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class AttackScreen : GuiScreen, ITutorialStarter
    {
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton attackButton;

        [SerializeField] private InfoEnemyArmyElementList groundElement;
        [SerializeField] private InfoEnemyArmyElementList resistanceElement;
        [SerializeField] private TextMeshProUGUI countryName;

        private Country _countryToAttack;

        private GuiController gui;
        private VisualCountryController countries;
        private BattleInGameService battle;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();
            battle = cts.Get<BattleInGameService>();

            closeButton.Init(OnCloseClick);
            attackButton.Init(OnAttackClick);
        }

        protected override void OnShow()
        {
            base.OnShow();

            TryShowTutorial();
        }

        protected override void OnHide()
        {
            base.OnHide();

            var tutor = gui.TutorContainer;
            tutor.BreakTutorial();
        }

        public void UpdateInfoToAttack(Country countryToAttack)
        {
            _countryToAttack = countryToAttack;

            countryName.text = countryToAttack.LocalCountryData.Name;

            // ARMY
            groundElement.gameObject.SetActive(true);
            groundElement.ForceLabel.text =
                string.Format($"Force {(int)countryToAttack.ArmyForce} / {BattleInGameService.ArmyMaxForce}");
            groundElement.ForceStars.SetProgress((float)countryToAttack.ArmyForce / BattleInGameService.ArmyMaxForce);

            // RESISTANCE
            resistanceElement.gameObject.SetActive(false);
            attackButton.gameObject.SetActive(true);
        }

        public void UpdateInfoForResistance(Country country)
        {
            groundElement.gameObject.SetActive(false);
            resistanceElement.gameObject.SetActive(true);

            resistanceElement.ForceLabel.text =
                string.Format($"Force {(int)(country.LocalResistanceLevel)} / {BattleInGameService.ResistanceMaxForce}");
            resistanceElement.ForceStars.SetProgress((float)country.LocalResistanceLevel / BattleInGameService.ResistanceMaxForce);

            attackButton.gameObject.SetActive(false);
        }

        public void OnAttackClick()
        {
            var tutor = gui.TutorContainer;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.EnemyAttackTutorID) tutor.CompleteTutorial();

            battle.Battle(countries.PlayerBaseCountry, _countryToAttack);
        }

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.EnemyAttackTutorID))
            {
                focusTran = attackButton.transform;
                tutor.TryShowInUI(TutorContainer.EnemyAttackTutorID, focusTran);
            }
        }

        public void OnCloseClick() => gui.Exit();
    }
}