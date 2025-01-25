using UnityEngine;
using Battle;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class BattleScreen : GuiScreen, ITutorialStarter
    {
        [SerializeField] private PointerButton cameraButton;
        [SerializeField] private PointerButton settingsButton;
        [SerializeField] private PointerButton retreatButton;
        [SerializeField] private Pointer rocketAttackPointer;

        [Space]
        [SerializeField] private BattleRocketUiContainer rockets;

        private BattleController battle;
        private BattleCameraController battleCameras;
        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            battle = cts.Get<BattleController>();
            battleCameras = cts.Get<BattleCameraController>();
            gui = cts.Get<GuiController>();

            cameraButton.Init(battleCameras.SwitchMode);
            settingsButton.Init(() =>
            {
                gui.Show<SettingsScreen>();
            });

            retreatButton.Init(() =>
            {
                battle.LeaveBattle();
            });

            rockets.Init(() =>
            {
                if (rocketIconClicked) return;

                rocketIconClicked = true;
                gui.TutorContainer.BreakTutorial();
                TryShowTutorial();
            });

            rocketAttackPointer.InitPointer((pointer) => battle.TryPlayerRocketAttack());
        }

        private int loadedRocketsCount = 0;

        protected override void OnShow()
        {
            base.OnShow();

            battle.OnChangeCurrentLoadedRocketsInBattle += OnChangeCurrentLoadedRocketsInBattle;
            battle.RocketRechargingProgressEvent += OnChangeRocketProgress;
            //battle.FullRocketLoadComplete += () => rockets.SetProgressbarActivity(false);
            battle.Armies.OnUnitDieEvent += OnUnitDie;
            battle.OnPlayerRocketAttackEvent += OnPlayerRocketAttack;

            OnChangeCurrentLoadedRocketsInBattle(battle.CurrentLoadedRocketsInBattle);
        }

        protected override void OnHide()
        {
            base.OnHide();

            battle.OnChangeCurrentLoadedRocketsInBattle -= OnChangeCurrentLoadedRocketsInBattle;
            battle.RocketRechargingProgressEvent -= OnChangeRocketProgress;
            //battle.FullRocketLoadComplete -= () => rockets.SetProgressbarActivity(false);
            battle.Armies.OnUnitDieEvent -= OnUnitDie;
            battle.OnPlayerRocketAttackEvent -= OnPlayerRocketAttack;

            if (gui.TutorContainer.InTutorial) gui.TutorContainer.BreakTutorial();
        }

        private void OnChangeCurrentLoadedRocketsInBattle(int rocketsInBattle)
        {
            rockets.SetValue(rocketsInBattle);

            loadedRocketsCount = rocketsInBattle;

            if (!gui.TutorContainer.InTutorial) TryShowTutorial();
        }

        private void OnChangeRocketProgress(float progress)
        {
            rockets.SetProgress(progress);
        }

        private void OnUnitDie(Unit unit)
        {
            if (rocketIconClicked && unit == unitForRocketTutor)
            {
                TryShowTutorial();
            }
        }

        private void OnPlayerRocketAttack()
        {
            if (rocketIconClicked && gui.TutorContainer.InTutorial && gui.TutorContainer.CurrentTutorialID == TutorContainer.RocketAttackTutorID)
            {
                gui.TutorContainer.CompleteTutorial();
            }
        }

        private bool rocketIconClicked = false;
        private Unit unitForRocketTutor;

        public void TryShowTutorial()
        {
            if (!isShow) return;

            Transform focusTran;
            var tutor = gui.TutorContainer;

            if (!tutor.IsComplete(TutorContainer.RocketAttackTutorID) && loadedRocketsCount > 0)
            {
                if (!rocketIconClicked)
                {
                    focusTran = rockets.buttonTran;
                    tutor.TryShowInUI(TutorContainer.RocketAttackTutorID, focusTran);
                }
                else
                {
                    unitForRocketTutor = battle.Armies.ActiveUnits[BattleSideType.Red][^1];
                    tutor.TryShowInWorld(TutorContainer.RocketAttackTutorID, unitForRocketTutor.transform);
                }
            }
        }
    }
}