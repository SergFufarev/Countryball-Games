using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility;
using Zenject;
using Random = UnityEngine.Random;

namespace Battle
{
    public class ArmiesController : MonoBehaviour, IUpgradeReactable
    {
        [SerializeField] private Squad squadPrefab;
        [SerializeField] private UnityDictionary<BattleSideType, Grid> grids;
        [SerializeField] private UnityDictionary<BattleSideType, UnityDictionary<UnitType, Unit>> unitPrefabs;
        [SerializeField] private UnityDictionary<BattleSideType, UnityDictionary<UnitType, UnitVisual[]>> unitVisualPrefabs;
        [SerializeField] private UnityDictionary<UnitType, int> playerArmyData;
        [SerializeField] private UnityDictionary<UnitType, int> enemyArmyData;
        [SerializeField] private UnityDictionary<BattleSideType, Transform> squadContainers;
        [SerializeField] private UnityDictionary<BattleSideType, Transform> unitContainers;
        [SerializeField] private UnityDictionary<BattleSideType, UnityDictionary<UnitType, DestructibleIntoPieces>> unitDestructibles;
        [SerializeField] private BattleConfig battleConfig;
        [SerializeField] private CountriesData countriesConfig;
        [SerializeField] private Transform airHeightTran;
        
        [Inject] private readonly BattleController battle;
        [Inject] private readonly BulletsContainer bullets;

        private Dictionary<BattleSideType, List<Squad>> createdSquads;
        private Dictionary<BattleSideType, List<Unit>> createdUnits;
        private Dictionary<BattleSideType, List<Unit>> activeUnits;
        public Dictionary<BattleSideType, List<Unit>> ActiveUnits => activeUnits;

        public Grid GetGrid(BattleSideType side) => grids.Get(side);

        public event Action<Unit> OnUnitDieEvent;

        private bool inBattle = false;

        BattleSideType[] allBattleSides;

        public int GetSquadsCount(BattleSideType side)
        {
            if (createdSquads == null || !createdSquads.ContainsKey(side)) return 0;
            else return createdSquads[side].Count;
        }

        private const int RED_SQUADS_ON_TOWERS = 4;
        private const int GREEN_SQUADS_ON_TOWERS = 0;

        public void Init()
        {
            allBattleSides = EnumUtility.GetValues<BattleSideType>();

            createdSquads = new();
            createdUnits = new();
            activeUnits = new();

            foreach (var battleSide in allBattleSides)
            {
                createdSquads.Add(battleSide, new ());
                createdUnits.Add(battleSide, new ());
                activeUnits.Add(battleSide, new ());

                if (unitDestructibles.Contains(battleSide))
                {
                    var unitDestructiblesInSide = unitDestructibles.Get(battleSide);

                    foreach (var ud in unitDestructiblesInSide.Values) ud.Init();
                }
            }
        }

        private void Update()
        {
            if (!inBattle) return;

            foreach (var side in allBattleSides)
            {
                foreach (var unit in createdUnits[side])
                {
                    unit.Simulate();
                }
            }
        }

        private Material playerFlagMaterial;
        private Material enemyFlagMaterial;

        public void GenerateArmyOfSide(BattleSideType side, int countryID, int enemyID, bool randomizePositions)
        {
            var grid = GetGrid(side);
            var countryData = CountrySaveLoad.LoadCountry(countryID);

            CountryBallVisualService.Instance.GetCustomisationByOwner(countryData.CountryData.Owner, out _, out CustomizationFaceType unitFaceType);

            int squadsOnTowers = side == BattleSideType.Red ? RED_SQUADS_ON_TOWERS : GREEN_SQUADS_ON_TOWERS;

            // load placement
            bool isPlayer = side == BattleSideType.Green;
            bool useLoadedPlacement = false;
            int[] loadedPlacement = null;

            if (isPlayer)
            {
                if (PlayerPrefs.HasKey(CommonData.PREFSKEY_BIG_BATTLE_PLAYER_PLACEMENT))
                {
                    loadedPlacement = SaveManager.Load<int[]>(CommonData.PREFSKEY_BIG_BATTLE_PLAYER_PLACEMENT);
                    useLoadedPlacement = true;
                }
            }

            // load army data
            var enemyCountryData = CountrySaveLoad.LoadCountry(enemyID);
            Dictionary<int, int> armySaveData = countryData.armyUpgrades;

            UnityDictionary<UnitType, int> armyData;

            Material currentFlagMaterial = countriesConfig.FlagMaterials[countryID];

            if (isPlayer)
            {
                playerArmyData = ConvertToUnitDictionary(armySaveData);
                armyData = playerArmyData;
                playerFlagMaterial = currentFlagMaterial;
            }
            else
            {
                int armyCount = countryData.GetArmyUpgradeLevels();
                enemyArmyData = ConvertToUnitDictionary(armySaveData);// countriesConfig.CountryData[countryID].armyData.GetResultUpgrades(armyCount);
                armyData = enemyArmyData;
                enemyFlagMaterial = currentFlagMaterial;
            }

            // calculate all squads
            int allSquadsCount = 0;
            foreach (var squadData in armyData.Values) allSquadsCount += squadData;

            // prepare total indexes (for enemy shuffle)
            int[] totalIndexes = new int[Grid.TotalSize];
            for (int i = 0; i < Grid.TotalSize; i++) totalIndexes[i] = i;
            if (randomizePositions)
            {
                int[] totalIndexesInUnitBounds = new int[allSquadsCount];
                for (int i = 0; i < totalIndexesInUnitBounds.Length; i++) totalIndexesInUnitBounds[i] = i;
                ArrayUtility.Randomize(totalIndexesInUnitBounds);
                for (int i = 0; i < totalIndexesInUnitBounds.Length; i++) totalIndexes[i] = totalIndexesInUnitBounds[i];
            }

            if (useLoadedPlacement)
            {
                for (int i = 0; i < loadedPlacement.Length; i++)
                {
                    if (loadedPlacement[i] != -1)
                    {
                        var unitType = (UnitType)loadedPlacement[i];
                        
                        if (armyData.Contains(unitType) && armyData.Get(unitType) > 0)
                        {
                            GenerateSquad(grid, side, totalIndexes[i], unitType, unitFaceType, currentFlagMaterial, GetCurrentUnitVisual(side, unitType));
                            armyData.Set(unitType, armyData.Get(unitType) - 1);
                        }
                    }
                }
            }

            int cellIndex = 0;

            int squadsForTowers = squadsOnTowers;

            List<Unit> shootersWithoutTowerException = new List<Unit>();

            // обычные сквады солдат
            foreach (var armyKeyValue in armyData.KeyValues)
            {
                for (int squadOfArmyTypeIndex = 0; squadOfArmyTypeIndex < armyKeyValue.Value; squadOfArmyTypeIndex++)
                {
                    while (grid.GetCellByTotalIndex(totalIndexes[cellIndex]).CurrentSquadType != null) cellIndex++;

                    var squad = GenerateSquad(grid, side, totalIndexes[cellIndex], armyKeyValue.Key, unitFaceType, currentFlagMaterial, GetCurrentUnitVisual(side, armyKeyValue.Key));

                    if (squadsForTowers > 0 && (squad.SquadType == UnitType.Infantry || squad.SquadType == UnitType.Snipers))
                    {
                        squadsForTowers--;
                        battle.MoveSquadIntoTower(squad, out var unitsWithoutPlace);

                        foreach (var uwp in unitsWithoutPlace) shootersWithoutTowerException.Add(uwp);
                    }
                    else cellIndex++;
                }
            }

            // сквады револьтов
            for (int i = 0; i < enemyCountryData.resistanceLevel; i++)
            {
                GenerateRevoltSquad(i, unitFaceType, currentFlagMaterial);
            }

            activeUnits[side] = new List<Unit>();
            foreach (var createdUnit in createdUnits[side])
            {
                if (!createdUnit.IsShooterWithoutTowerException) activeUnits[side].Add(createdUnit);
            }

            //foreach (var shooterWithoutTower in shootersWithoutTowerException) activeUnits[side].Remove(shooterWithoutTower);

            void GenerateRevoltSquad(int index, CustomizationFaceType unitFaceType, Material flagMaterial)
            {
                var cell = grid.RevoltCells[index];
                var squad = Instantiate(squadPrefab, cell.transform.position, Quaternion.identity, squadContainers.Get(side));
                squad.Init(side, UnitType.Rebels, unitFaceType);
                squad.GenerateUnits(this, bullets, battleConfig.GetUnitData(UnitType.Rebels), side == BattleSideType.Green ? battle.RedBase.transform : null, unitPrefabs.Get(side).Get(UnitType.Rebels), flagMaterial, GetCurrentUnitVisual(side, UnitType.Rebels));

                foreach (var unit in squad.Units) createdUnits[side].Add(unit);

                squad.name = "RebSquad";
                if (side == BattleSideType.Red) squad.transform.localEulerAngles = new Vector3(0, 180, 0);

                // add to dictionary
                createdSquads[side].Add(squad);
                cell.SetSquad(squad);
                squad.SetCurrentCell(cell);
                squad.SetColliderActivity(false);
            }
        }

        private Squad GenerateSquad(Grid grid, BattleSideType side, int totalIndex, UnitType unitType, CustomizationFaceType unitFaceType, Material flagMaterial, UnitVisual visualPrefab)
        {
            var cell = grid.GetCellByTotalIndex(totalIndex);
            var squad = Instantiate(squadPrefab, cell.transform.position, Quaternion.identity, squadContainers.Get(side));
            squad.Init(side, unitType, unitFaceType);
            squad.GenerateUnits(this, bullets, battleConfig.GetUnitData(unitType), side == BattleSideType.Green ? battle.RedBase.transform : null, unitPrefabs.Get(side).Get(unitType), flagMaterial, visualPrefab);

            foreach (var unit in squad.Units) createdUnits[side].Add(unit);

            squad.name = unitType.ToString() + "Squad";
            if (side == BattleSideType.Red) squad.transform.localEulerAngles = new Vector3(0, 180, 0);

            // add to dictionary
            createdSquads[side].Add(squad);
            cell.SetSquad(squad);
            squad.SetCurrentCell(cell);

            return squad;
        }

        private void AddNewUnit(BattleSideType side, int countryID, UnitType unitType)
        {
            CountryBallVisualService.Instance.GetCustomisationByOwner(countryID, out _, out CustomizationFaceType unitFaceType);

            var grid = GetGrid(side);
            int insertIndex = FindFirstEmptyCellTotalIndex(grid);
            var squad = GenerateSquad(grid, side, insertIndex, unitType, unitFaceType, side == BattleSideType.Green ? playerFlagMaterial : enemyFlagMaterial, GetCurrentUnitVisual(side, unitType));

            foreach (var unit in squad.Units) activeUnits[side].Add(unit);
        }

        int greenTankCurrentVisualIndex = 0;
        int redTankCurrentVisualIndex = 0;
        int greenApcCurrentVisualIndex = 0;
        int redApcCurrentVisualIndex = 0;

        public UnitVisual GetCurrentUnitVisual(BattleSideType side, UnitType unitType)
        {
            int visualIndex = 0;

            if (unitType == UnitType.Tank)
            {
                if (side == BattleSideType.Green)
                {
                    visualIndex = greenTankCurrentVisualIndex;
                    greenTankCurrentVisualIndex++;
                    if (greenTankCurrentVisualIndex >= 3) greenTankCurrentVisualIndex = 0;
                }
                else
                {
                    visualIndex = redTankCurrentVisualIndex;
                    redTankCurrentVisualIndex++;
                    if (redTankCurrentVisualIndex >= 3) redTankCurrentVisualIndex = 0;
                }
            }
            else if (unitType == UnitType.APC)
            {
                if (side == BattleSideType.Green)
                {
                    visualIndex = greenApcCurrentVisualIndex;
                    greenApcCurrentVisualIndex++;
                    if (greenApcCurrentVisualIndex >= 3) greenApcCurrentVisualIndex = 0;
                }
                else
                {
                    visualIndex = redApcCurrentVisualIndex;
                    redApcCurrentVisualIndex++;
                    if (redApcCurrentVisualIndex >= 3) redApcCurrentVisualIndex = 0;
                }
            }

            return unitVisualPrefabs.Get(side).Get(unitType)[visualIndex];
        }

        private int FindFirstEmptyCellTotalIndex(Grid grid)
        {
            int totalIndex = 0;

            // find empty cell
            foreach (var cellLine in grid.CellLines)
            {
                foreach (var cell in cellLine.Cells)
                {
                    if (cell.IsEmpty) return totalIndex;
                    else totalIndex++;
                }
            }

            return -1;
        }

        private UnityDictionary<UnitType, int> ConvertToUnitDictionary(Dictionary<int, int> intDictionary)
        {
            UnityDictionary<UnitType, int> result = new();

            for (int i = 0; i < 10; i++)
            {
                if (intDictionary.ContainsKey(i))
                    result.Add((UnitType)i, intDictionary[i]);
            }

            return result;
        }

        public void SetSquadsColliderActivity(BattleSideType battleSideType, bool active)
        {
            for (int i = 0; i < createdSquads[battleSideType].Count; i++)
            {
                var squad = createdSquads[battleSideType][i];
                if (squad.SquadType != UnitType.Rebels) squad.SetColliderActivity(active);
            }

            battle.SetGridColliderActivity(battleSideType, !active);
        }

        public void StartBattle(BattleSideType side, Transform destination)
        {
            var container = unitContainers.Get(side);
            foreach (var unit in createdUnits[side])
            {
                if (unit.HeightLevel == BattleHeightLevel.Air) unit.transform.position = new Vector3(unit.transform.position.x, airHeightTran.position.y, unit.transform.position.z);

                unit.OnStartBattle();
                if (!unit.IsTowerShooter)
                {
                    unit.SetMoveGoal(destination);
                    unit.transform.parent = container;
                }
            }

            squadContainers.Get(side).gameObject.SetActive(false);

            inBattle = true;

            GetGrid(side).gameObject.SetActive(false);
        }

        public void StopBattle(bool win)
        {
            foreach (var unit in createdUnits[BattleSideType.Green])
            {
                unit.OnEndBattle(win);
            }

            inBattle = false;
        }

        public void OnUnitDie(Unit unit)
        {
            activeUnits[unit.Side].Remove(unit);

            if (unit.Side == BattleSideType.Green && activeUnits[unit.Side].Count == 0) battle.Defeat();

            OnUnitDieEvent?.Invoke(unit);

            // destructible
            if (unit.IsTowerShooter) return;
            if (!unitDestructibles.Contains(unit.Side)) return;
            var unitDestructiblesInSide = unitDestructibles.Get(unit.Side);
            if (!unitDestructiblesInSide.Contains(unit.UnitType)) return;
            var unitDestructible = unitDestructiblesInSide.Get(unit.UnitType);
            unitDestructible.Destruct(unit.transform);
        }

        public Dictionary<BattleSideType, Dictionary<UnitType, int>> GetResultUnitCounts()
        {
            Dictionary<BattleSideType, Dictionary<UnitType, int>> result = new();
            
            foreach (var side in allBattleSides)
            {
                Dictionary<UnitType, int> resultsInSide = new();

                foreach (var unit in activeUnits[side])
                {
                    if (!resultsInSide.ContainsKey(unit.UnitType)) resultsInSide.Add(unit.UnitType, 0);

                    resultsInSide[unit.UnitType] = resultsInSide[unit.UnitType] + 1;
                }

                result.Add(side, resultsInSide);
            }

            return result;
        }

        public int[] GetCurrentPlacement(BattleSideType side)
        {
            var grid = GetGrid(side);
            int[] placement = new int[30];

            int cellIndex = 0;

            foreach (var line in grid.CellLines)
            {
                foreach (var cell in line.Cells)
                {
                    UnitType? squadType = cell.CurrentSquadType;
                    placement[cellIndex] = squadType == null ? -1 : (int)((UnitType)squadType);
                    cellIndex++;
                }
            }

            return placement;
        }

        public Unit GetRandomActiveUnit(BattleSideType side)
        {
            var list = activeUnits[side];
            var index = Random.Range(0, list.Count);
            return list[index];
        }

        public Unit GetRandomActiveUnit_NoTowerPrioritized(BattleSideType side)
        {
            var list = activeUnits[side];
            int startIndex = list.Count > 16 ? 16 : 0;
            var index = Random.Range(startIndex, list.Count);
            return list[index];
        }

        #region Reacts

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
        {
            if (upgradeType != UpgradeType.Army) return;

            AddNewUnit(BattleSideType.Green, countryID, (UnitType)upgradeID);
        }

        public void OnBuyTrade(int countryID)
        {}

        public void OnWonderBuilded(int countryID)
        {}

        public void OnRocketBuy(int totalRocketsCount)
        { }

        #endregion
    }

    public enum BattleSideType
    {
        Green,
        Red
    }

    public enum UnitType
    {
        Infantry,
        Snipers,
        Mortar,
        Hummer,
        APC,
        Tank,
        Artillery,
        MobileLauncher,
        HelicopterWithMachineGuns,
        HelicopterWithMissiles,

        Rebels
    }

    public enum BuildingType
    {
        RedBase,
        RedTower
    }

    public enum BattleHeightLevel
    {
        Ground,
        Air
    }
}