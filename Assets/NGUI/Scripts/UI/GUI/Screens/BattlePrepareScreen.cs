using UnityEngine;
using TheSTAR.Utility.Pointer;
using Battle;
using SPSDigital.ADS;

namespace TheSTAR.GUI.Screens
{
    public class BattlePrepareScreen : GuiScreen, ITutorialStarter
    {
        [SerializeField] private Pointer placementPointer;
        [SerializeField] private PointerButton attackButton;
        [SerializeField] private PointerButton leaveButton;
        [SerializeField] private PointerButton getUnitButton;
        [SerializeField] private PointerButton changeCameraButton;
        [SerializeField] private PointerButton settingsButton;
        [SerializeField] private PlacementTutorContainer placementTutorContainer;

        private BattleController battle;
        private BattleCameraController battleCameras;
        private GuiController gui;

        private bool inDrag = false;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            battle = cts.Get<BattleController>();
            battleCameras = cts.Get<BattleCameraController>();
            gui = cts.Get<GuiController>();

            placementPointer.InitPointer(
                (pointer) =>
                {
                    inDrag = true;
                    battle.DragHelper.OnDown();

                    if (gui.TutorContainer.InTutorial && gui.TutorContainer.CurrentTutorialID == TutorContainer.UnitPlacementTutorID)
                    {
                        gui.TutorContainer.CompleteTutorial();
                        placementTutorContainer.Hide();
                    }
                },
                (pointer) =>
                {
                    battle.DragHelper.OnDrag();
                },
                (pointer) =>
                {
                    inDrag = false;
                    battle.DragHelper.OnUp();

                    if (!gui.TutorContainer.InTutorial)
                        TryShowTutorial();
                });

            getUnitButton.Init(battle.ShowGetUnitScreen);
            attackButton.Init(() =>
            {
                if (battle.Armies.ActiveUnits[BattleSideType.Green].Count == 0) return;

                if (gui.TutorContainer.InTutorial && gui.TutorContainer.CurrentTutorialID == TutorContainer.BigBattleAttackTutorID)
                {
                    gui.TutorContainer.CompleteTutorial();
                }

                battle.StartBattle();
            });
            leaveButton.Init(battle.LeaveBattle);
            changeCameraButton.Init(battleCameras.SwitchMode);
            settingsButton.Init(() =>
            {
                gui.Show<SettingsScreen>();
            });
        }

        protected override void OnShow()
        {
            base.OnShow();

            TryShowTutorial();
            Invoke(nameof(TryShowAdditionalScreens), 0.5f);
        }

        private void TryShowAdditionalScreens()
        {
            if (gui.CurrentScreen is not BattlePrepareScreen) return;

            AdsService.Instance.InterstititalService.TryShowInterPopup();
        }

        protected override void OnHide()
        {
            base.OnHide();
            var tutor = gui.TutorContainer;
            if (tutor.InTutorial) tutor.BreakTutorial();

            if (inDrag)
            {
                battle.DragHelper.OnUp();
                inDrag = false;
            }
        }

        public void TryShowTutorial()
        {
            Transform focusTran;
            var tutor = gui.TutorContainer;

            if (!tutor.IsComplete(TutorContainer.GetUnitInBattlePrepareTutorID) && battle.Armies.GetSquadsCount(BattleSideType.Green) <= 3)
            {
                focusTran = getUnitButton.transform;
                tutor.TryShowInUI(TutorContainer.GetUnitInBattlePrepareTutorID, focusTran, false, TutorContainer.CursorViewType.BottomEnge);
            }
            else if (!tutor.IsComplete(TutorContainer.BuyRocketTutorID))
            {
                focusTran = getUnitButton.transform;
                tutor.TryShowInUI(TutorContainer.BuyRocketTutorID, focusTran, false, TutorContainer.CursorViewType.BottomEnge);
            }
            else if (!tutor.IsComplete(TutorContainer.UnitPlacementTutorID))
            {
                Transform fromCell = null;
                Transform toCell = null;

                var army = battle.Armies;
                var grid = army.GetGrid(BattleSideType.Green);

                bool sniperCellFound = false;
                bool emptyCellFound = false;

                int sniperFromLineIndex = 0;
                int lineIndex = 0;
                int cellIndex = 0;

                for (; lineIndex < grid.CellLines.Length; lineIndex++)
                {
                    if (sniperCellFound) break;

                    Battle.Grid.CellLine line = grid.CellLines[lineIndex];

                    for (; cellIndex < line.Cells.Length; cellIndex++)
                    {
                        Cell cell = line.Cells[cellIndex];
                        if (sniperCellFound) break;

                        if (cell.CurrentSquadType == UnitType.Snipers)
                        {
                            fromCell = cell.transform;
                            sniperCellFound = true;
                            sniperFromLineIndex = lineIndex;
                            break;
                        }
                    }
                }

                if (!sniperCellFound) return;

                lineIndex = sniperFromLineIndex + 1;
                for (; lineIndex < grid.CellLines.Length; lineIndex++)
                {
                    if (emptyCellFound) break;

                    Battle.Grid.CellLine line = grid.CellLines[lineIndex];

                    Cell cell = line.Cells[cellIndex];
                    if (cell.IsEmpty)
                    {
                        toCell = cell.transform;
                        emptyCellFound = true;
                        break;
                    }
                }

                if (!emptyCellFound) return;

                placementTutorContainer.Init(fromCell, toCell);
                placementTutorContainer.Show();

                tutor.TryShowInUI(TutorContainer.UnitPlacementTutorID, null, false, TutorContainer.CursorViewType.Invisible);
            }
            else if (!tutor.IsComplete(TutorContainer.BigBattleAttackTutorID))
            {
                focusTran = attackButton.transform;
                tutor.TryShowInUI(TutorContainer.BigBattleAttackTutorID, focusTran, false, TutorContainer.CursorViewType.BottomEnge);
            }
        }
    }
}