using System;
using System.Collections.Generic;
using TheSTAR.Utility;
using TMPro;
using UnityEngine;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
#if UNITY_EDITOR
using Newtonsoft.Json;
#endif

namespace FunnyBlox
{
    public class Country : MonoBehaviour
    {
        [SerializeField] private TextMeshPro countryNameTMP;
        [SerializeField] private Transform countryBallsContainer;
        [SerializeField] private Transform buildingsContainer;

        public CountryData LocalCountryData;
        public UnityDictionary<CountryBallType, CountryBall> CountryBalls;
        public List<CountryBuilding> buildings;
        public CountryBuilding wonder;

        private Dictionary<int, int> localFactoryUpgrades;
        public Dictionary<int, int> LocalFactoryUpgrades => localFactoryUpgrades;

        private Dictionary<int, int> localArmyUpgrades;
        public Dictionary<int, int> LocalArmyUpgrades => localArmyUpgrades;
        private bool localWonderBuilded;

        private int localResistanceLevel;
        public int LocalResistanceLevel => localResistanceLevel;

        public int GetResistanceForceValue => localResistanceLevel;
        public int ID => LocalCountryData.Id;

        public const int FullFactoriesCount = 4;
        private const float MaxCountryNameSize = 1;
        private const float MinCountryNameSize = 0.5f;
        private const float MicroCountryNameSize = 0.2f;

        public int GetArmyUpgradeLevels() => DictionaryConvertUtility.GetTotalValue(localArmyUpgrades);
        public int GetFactoryUpgradeLevels() => DictionaryConvertUtility.GetTotalValue(localFactoryUpgrades);
        public float GetFactoryUpgradesProgress()
        {
            float progress;
            float defaultBuildingsProgess = (float)GetFactoryUpgradeLevels() / (LocalCountryData.Factories * 5);
            float wonderProgress = localWonderBuilded ? 1 : 0;
            progress = ((defaultBuildingsProgess * LocalCountryData.Factories) + wonderProgress) / (LocalCountryData.Factories + 1);

            return progress;
        }

        private readonly string[] BuildingNames = new string[]
        {
            "farm",
            "sawmill",
            "factory",
            "bank"
        };

        public Material GetCountryOwnerMaterial
        {
            get
            {
                if (LocalCountryData.IsPlayerOwner) return GetPlayerCountryMaterial;
                else return countriesController.GetMaterialByID(LocalCountryData.Owner);
            }
        }
        public Material GetBallOwnerMaterial(CountryBallType countryBallType)
        {
            if (countryBallType == CountryBallType.Resistance)
            {
                if (LocalCountryData.IsPlayerOwner) return countriesController.GetMaterialByID(ID);
                else return GetPlayerCountryMaterial;
            }
            else return GetCountryOwnerMaterial;
        }
        public Material GetBaseOwnerMaterial => countriesController.GetMaterialByID(ID);
        public Material GetPlayerCountryMaterial
        {
            get
            {
                int baseID = countriesController.PlayerBaseCountryID;
                return countriesController.GetMaterialByID(baseID);
            }
        }

        private Renderer territoryRenderer;

        public int ArmyForce => GetArmyUpgradeLevels();

        private Dictionary<int, int> ConvertTotalLevelsIntoDictionary(int totalLevels)
        {
            return DictionaryConvertUtility.ConvertTotalLevelsIntoDictionary(totalLevels, BattleInGameService.ArmyMaxUpgradeLevel);
        }

        #region Spawn

        private GameController game;
        private RateUsController rateUs;
        private GuiController gui;
        private UpgradeService upgrades;
        private VisualCountryController countriesController;
        private BattleInGameService battle;

        public float GetWonderForce => upgrades.GetWonderEffect;

        public void SetFactoryData(Dictionary<int, int> factoryData)
        {
            localFactoryUpgrades = factoryData == null ? new () : new(factoryData);
        }
        public void SetArmyData(Dictionary<int, int> armyData)
        {
            localArmyUpgrades = armyData == null ? new() : new(armyData);
        }
        public void SetResistanceData(int resistanceLevel)
        {
            this.localResistanceLevel = resistanceLevel;
        }
        public void SetWonderData(bool isBuildid)
        {
            this.localWonderBuilded = isBuildid;
        }
        public void SetData(
            GameController game,
            VisualCountryController countries,
            GuiController gui,
            RateUsController rateUs,
            UpgradeService upgrades,
            BattleInGameService battle,
            CountryData data,
            Dictionary<int, int> factoryUpgrades,
            Dictionary<int, int> armyUpgrades,
            int resistanceLevel,
            bool wonderBuilded,
            bool testOpenAll)
        {
            this.game = game;
            this.countriesController = countries;
            this.gui = gui;
            this.rateUs = rateUs;
            this.upgrades = upgrades;
            this.battle = battle;

            name = data.Name;

            CountryBalls = new();
            LocalCountryData = data;

            // upgrades
            SetFactoryData(factoryUpgrades);
            SetArmyData(armyUpgrades);
            SetResistanceData(resistanceLevel);
            SetWonderData(wonderBuilded);

            LoadCountryCollider();

            Open(testOpenAll ? true : LocalCountryData.OpenState);

            UpdateElements();

            UpdateCountryNameText();
        }

        public void UpdateCustomisation()
        {
            CountryBallVisualService.Instance.GetCustomisationByOwner(LocalCountryData.Owner,
                out LocalCountryData.hatType,
                out LocalCountryData.faceType);

            UseCustomisation();
        }

        [ContextMenu("UseCustomization")]
        public void UseCustomisation()
        {
            CountryBalls.Get(CountryBallType.Main).SetHat(LocalCountryData.hatType);

            foreach (var ball in CountryBalls.GetAllValues())
            {
                if (ball.CountryBallType == CountryBallType.Resistance) ball.SetFace(GetRevoltFaceType);
                else ball.SetFace(LocalCountryData.faceType);
            }
        }

        private CustomizationFaceType GetRevoltFaceType
        {
            get
            {
                CountryBallVisualService.Instance.GetCustomisationByOwner(LocalCountryData.IsPlayerOwner ? ID : CommonData.PlayerID,
                    out _,
                    out CustomizationFaceType revoltFace);

                return revoltFace;
            }
        }

        private void UpdateCountryNameText()
        {
            countryNameTMP.text = LocalCountryData.Name;
            countryNameTMP.transform.localPosition = Vector3.zero;

            float size;

            if (LocalCountryData.ScaleFactor < 1) size = MicroCountryNameSize;
            else
            {
                float step = ((MaxCountryNameSize - MinCountryNameSize) / 4);
                size = (MinCountryNameSize + (LocalCountryData.ScaleFactor - 1) * step);
            }

            countryNameTMP.transform.localScale = new Vector3(size, size, size);
        }

        private void CreateFactoryBuilding(int index)
        {
            var position = LocalCountryData.PositionForBuilding(index);
            var rotation = LocalCountryData.RotationForBuilding(index);

            CountryBuilding building;

            while (buildings.Count <= index) buildings.Add(null);

            if (buildings[index] != null) return;

            building = CreateBuildingObject();
            buildings[index] = building;

            building.transform.localPosition = position;
            building.transform.localEulerAngles = new Vector3(0, rotation, 0);
            building.SetSize(LocalCountryData.ScaleFactor);

            building.GenerateVisual(countriesController._countriesDataConfig.GetBuildingVisual(index));

            building.name = BuildingNames[index];

            if (!countriesController.WorldLoading) countriesController.OnCreateNewBuilding(building);
        }

        private void CreateWonderBuilding()
        {
            if (wonder != null) return;

            var position = LocalCountryData.PositionForWonder();
            var rotation = LocalCountryData.RotationForWonder();

            wonder = CreateBuildingObject();
            wonder.transform.localPosition = position;
            wonder.transform.localEulerAngles = new Vector3(0, rotation, 0);
            wonder.SetSize(LocalCountryData.ScaleFactor);

            wonder.GenerateVisual(countriesController._countriesDataConfig.GetWonderVisual(LocalCountryData.WonderType));

            wonder.name = "wonder";

            if (!countriesController.WorldLoading) countriesController.OnCreateNewBuilding(wonder);
        }

        private CountryBuilding CreateBuildingObject() => Instantiate(countriesController.CountryBuildingPrefab, buildingsContainer);

        public Vector3 IntelligenceHomePos
        {
            get
            {
                var pos = LocalCountryData.PositionForBall(CountryBallType.Intelligence);
                return pos == null ? new Vector3(0, ((Vector3)LocalCountryData.PositionForBall(CountryBallType.Main)).y, 0) : (Vector3)pos;

            }
        }

        private CountryBall CreateBallObject(Vector3 localPos)
        {
            var ball = Instantiate(countriesController.CountryBallPrefab, countryBallsContainer);
            ball.transform.localPosition = localPos;
            ball.transform.localRotation = Quaternion.identity;
            ball.SetVisualActivity(!game.ShowVisualOnlyForNearCountryBalls);
            return ball;
        }

        public void SetMainBallScaleMinLimit(float minLimit)
        {
            CountryBalls.Get(CountryBallType.Main).SetScaleMinLimit(minLimit);
        }

        public Vector3 PositionForBall(CountryBallType ballType)
        {
            var tempPos = LocalCountryData.PositionForBall(ballType);
            Vector3 position = tempPos != null ? (Vector3)tempPos : CountryBallVisual.PositionForBallType(ballType);

            return position;
        }

        public int BuildingsCount => LocalCountryData.Factories; // количество обычных строений (без чуда света)

        /// <summary>
        /// Обновляет элементы региона
        /// </summary>
        [ContextMenu("UpdateElements")]
        public void UpdateElements()
        {
            //CountryCollectedData data = CountrySaveLoad.LoadCountry(ID);

            if (LocalCountryData.OpenState)
            {
                UpdateCountryBalls();

                if (LocalCountryData.useBuildings) UpdateBuildings();
            }
            else UpdateBall(CountryBallType.Main);
        }

        public void UpdateCountryBalls()
        {
            var allCountryBallTypes = EnumUtility.GetValues<CountryBallType>();
            for (int i = 0; i < allCountryBallTypes.Length; i++) UpdateBall(allCountryBallTypes[i]);
        }

        private void UpdateBall(CountryBallType ballType)
        {
            if (!LocalCountryData.CheckUseBall(ballType)) return;

            // create
            CountryBall _countryBall;
            if (!CountryBalls.Contains(ballType))
            {
                _countryBall = CreateBallObject(PositionForBall(ballType));
                CountryBalls.Add(ballType, _countryBall);
                _countryBall.UpdateLook(CameraService.Instance.CurrentCameraMode, false);
            }
            else _countryBall = CountryBalls.Get(ballType);

            _countryBall.Init(ballType, this, OnCountryBallSelect);

            if (ballType == CountryBallType.Main) _countryBall.SetHat(LocalCountryData.hatType);

            if (ballType == CountryBallType.Resistance)
            {
                _countryBall.SetFace(GetRevoltFaceType);
            }
            else _countryBall.SetFace(LocalCountryData.faceType);

            _countryBall.gameObject.SetActive(ballType == CountryBallType.Main || LocalCountryData.OpenState);

            // update stars

            UpdateBallProgress(ballType);
        }

        public void UpdateBuildings()
        {
            if (!LocalCountryData.useBuildings) return;

            bool testOpen = countriesController.TestOpenWorld;

            for (int i = 0; i < Mathf.Min(BuildingsCount, FullFactoriesCount); i++)
            {
                if (IsNeedShowBuilding(i) || testOpen)
                    CreateFactoryBuilding(i);
            }

            if (IsNeedShowWonder || testOpen)
                CreateWonderBuilding();
        }

        private bool IsNeedShowBuilding(int index)
        {
            var countryData = CountrySaveLoad.LoadCountry(ID);

            return countryData != null && countryData.factoryUpgrades != null && countryData.factoryUpgrades.ContainsKey(index) && countryData.factoryUpgrades[index] == 5; //CountryBalls.Get(CountryBallType.Factory).CountryBallUpgrades[index].IsFullUpgraded;
        }

        private bool IsNeedShowWonder => WonderAlreadyBuilded;

        public bool WonderAlreadyBuilded
        {
            get
            {
                if (CountrySaveLoad.HasCountry(ID))
                {
                    return CountrySaveLoad.LoadCountry(ID).wonderBuilded;
                }
                else return false;
            }
        }

        #endregion

        public void OnSelectForPlay()
        {
            localFactoryUpgrades = new();
            localArmyUpgrades = new();
            localResistanceLevel = 0;
            localWonderBuilded = false;
        }

        public void Open(bool state)
        {
            if (!LocalCountryData.OpenState && !LocalCountryData.IsPlayerOwner)
            {
                SetArmyData(ConvertTotalLevelsIntoDictionary(LocalCountryData.BaseForce));
            }

            LocalCountryData.OpenState = state;

            SetElementsVisibility(state);
            UpdateTerritoryMaterial();

            if (state) UpdateElements();

            if (LocalCountryData.IsBaseCountry)
            {
                if (CountryBalls.Contains(CountryBallType.Resistance)) CountryBalls.Get(CountryBallType.Resistance).gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR

        [ContextMenu("TestGetPlayerPrefsData")]
        private void TestGetPlayerPrefsData()
        {
            var data = CountrySaveLoad.LoadCountry(ID);
            string savedData = JsonConvert.SerializeObject(data);
            Debug.Log($"Data for {ID}:");
            Debug.Log(savedData);
        }

#endif

        public void UpdateTerritoryMaterial()
        {
            bool isPlayer = LocalCountryData.Owner == CommonData.PlayerID;
            int territoryID = (isPlayer ? countriesController.PlayerBaseCountryID : LocalCountryData.Owner);

            if (!LocalCountryData.OpenState) territoryRenderer.material = countriesController.GetTerritoryUnexploredMaterial;
            else
            {
                if (countriesController.VisualType == CountriesVisualType.Minimum)
                {
                    territoryRenderer.material =
                        isPlayer ?
                        countriesController._countriesDataConfig._minMaterialPlayer :
                        countriesController._countriesDataConfig._minMaterialEnemy;
                }
                else
                {
                    if (isPlayer)
                    {
                        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_USE_CUSTOM_COUNTRY_COLOR) &&
                            PlayerPrefs.HasKey(CommonData.PREFSKEY_CUSTOM_COUNTRY_COLOR_INDEX) &&
                            SaveManager.Load<bool>(CommonData.PREFSKEY_USE_CUSTOM_COUNTRY_COLOR))
                        {
                            int index = SaveManager.Load<int>(CommonData.PREFSKEY_CUSTOM_COUNTRY_COLOR_INDEX);
                            territoryRenderer.material = countriesController._countriesDataConfig._customPlayerMaterials[index];
                        }
                    }
                    else territoryRenderer.material = countriesController.GetTerritoryMaterial(territoryID);
                }
            }
        }

        private bool elementsVisible = true;

        public void SetElementsVisibility(bool visible)
        {
            foreach (var ball in CountryBalls.GetAllValues())
            {
                if (ball.CountryBallType == CountryBallType.Main) continue;

                bool isResistance = ball.CountryBallType == CountryBallType.Resistance;

                if (!isResistance) ball.gameObject.SetActive(visible);
                else
                {
                    if (visible)
                    {
                        if (LocalCountryData.IsBaseCountry) continue;

                        ball.gameObject.SetActive(visible);
                        ball.UpdateOwnerMaterial();
                    }
                    else ball.gameObject.SetActive(visible);
                }
            }

            buildingsContainer.gameObject.SetActive(visible);

            elementsVisible = visible;
        }

        private void LoadCountryCollider()
        {
            var countryColliderPrefab = countriesController._countriesDataConfig.CountryPrefabs[ID].transform;
            transform.position = countryColliderPrefab.transform.position;

            Transform countryColliderTransform = Instantiate(countryColliderPrefab, transform.GetChild(0));
            countryColliderTransform.localPosition = Vector3.zero;
            countryColliderTransform.name = LocalCountryData.Name;
            territoryRenderer = countryColliderTransform.GetChild(0).GetComponent<Renderer>();
        }

        #region Losses

        public void ArmyLosses(int force)
        {
            DictionaryConvertUtility.ReduceFromEnd(localArmyUpgrades, 10, force);
            UpdateBallProgress(CountryBallType.GroundArmy);
            UpdateBallProgress(CountryBallType.AirArmy);
            UpdateBallProgress(CountryBallType.NavalArmy);
        }

        public void ResistanceLosses(int force)
        {
            localResistanceLevel -= force;
            if (localResistanceLevel < 0) localResistanceLevel = 0;
            UpdateBallProgress(CountryBallType.Resistance);
        }

        public void BombLosses(int force)
        {
            if (force <= 0) return;

            if (LocalCountryData.OpenState)
            {
                ArmyLosses(force);
                int resultArmyForce = GetArmyUpgradeLevels();
                if (resultArmyForce > 0)
                {
                    UpdateAllBallStarProgressesByCollectedCountryData();
                    SaveCountry();
                }
                else BombOccupate();
            }
            else
            {
                LocalCountryData.BaseForce -= game.Battle.BattleConfig.BombForce;

                if (LocalCountryData.BaseForce <= 0)
                {
                    LocalCountryData.BaseForce = 0;
                    BombOccupate();
                }
                else SaveCountry();
            }

            void BombOccupate()
            {
                Occupate(CommonData.PlayerID);
                rateUs.TryShowRateUs(out _);
            }
        }

        #endregion

        public void Occupate(int newOwner)
        {
            if (LocalCountryData.Owner == newOwner) return;

            var loadedCountry = CountrySaveLoad.LoadCountry(ID);

            // игрок захватил территорию
            if (newOwner == CommonData.PlayerID)
            {
                // open

                if (!LocalCountryData.OpenState) Open(true);

                loadedCountry.armyUpgrades.Clear();
                loadedCountry.factoryUpgrades.Clear();
                loadedCountry.resistanceLevel = 0;

                UpdateBallProgress(CountryBallType.GroundArmy);
                UpdateBallProgress(CountryBallType.AirArmy);
                UpdateBallProgress(CountryBallType.NavalArmy);

                localArmyUpgrades = new();
                localResistanceLevel = 0;

                countriesController._playerCountries.Add(this);
                countriesController._openEnemyCountries.Remove(this);

                switch (countriesController._playerCountries.Count - 1)
                {
                    case 1:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "1_country_takeover 1");
                        break;

                    case 5:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "2_country_takeover 5");
                        break;

                    case 10:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "3_country_takeover 10");
                        break;

                    case 15:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "4_spying_country 15");
                        break;

                    case 20:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "5_spying_country 20");
                        break;

                    case 30:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "6_spying_country 30");
                        break;

                    case 40:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "7_spying_country 40");
                        break;

                    case 49:
                        AnalyticsManager.Instance.Log(AnalyticSectionType.CountriesTakeovers, "8_spying_country 49");
                        break;
                }

                CommonData.PlayerOccupationCounter++;
                game.SaveData();

                countriesController.CheckGameComplete();

                if (gui.CurrentScreen is GameScreen gameScreen) gameScreen.TryShowTutorial();
            }

            // у игрока захватили территорию
            else if (LocalCountryData.IsPlayerOwner)
            {
                loadedCountry.armyUpgrades.Clear();
                loadedCountry.factoryUpgrades.Clear();
                loadedCountry.resistanceLevel = 0;

                localResistanceLevel = 0;
                localFactoryUpgrades = new();

                countriesController._playerCountries.Remove(this);
                countriesController._openEnemyCountries.Add(this);

                // потеря торгового соглашения с территорией
                if (upgrades.tradeRegions.Contains(ID))
                {
                    upgrades.tradeRegions.Remove(ID);
                    upgrades.SaveData();
                }

                // выставляем дефолтные апгрейды армии как в начале игры

                LocalCountryData.BaseForce = countriesController.GetRandomStartForce(countriesController.ConvertFactoriesCountToForceType(countriesController._countriesDataConfig.CountryData[LocalCountryData.Id].Factories)); //countriesController._countriesDataConfig.CountryData[LocalCountryData.Id].BaseForce;

                SetArmyData(ConvertTotalLevelsIntoDictionary(LocalCountryData.BaseForce));
            }

            CommonData.PlayerCountriesCount = countriesController._playerCountries.Count;

            LocalCountryData.Owner = newOwner;

            // update balls

            foreach (var ball in CountryBalls.GetAllValues())
            {
                ball.UpdateOwnerMaterial();
                ball.UpdateFace();
            }

            CountryBallVisualService.Instance.GetCustomisationByOwner(newOwner, out LocalCountryData.hatType, out LocalCountryData.faceType);
            UseCustomisation();

            UpdateTerritoryMaterial();

            countriesController.RecalculatePlayerIncome();

            loadedCountry.CountryData = LocalCountryData;
            CountrySaveLoad.SaveCountry(ID, loadedCountry);

            UpdateAllBallStarProgressesByCollectedCountryData();
        }

        public void UpdateAllBallStarProgressesByCollectedCountryData()
        {
            foreach (var ball in CountryBalls.Values)
            {
                UpdateBallProgress(ball.CountryBallType);
            }
        }

        public void UpdateBallProgress(CountryBallType ballType)
        {
            float progress = 0;

            switch (ballType)
            {
                case CountryBallType.Main:
                    progress = 0;
                    break;

                case CountryBallType.GroundArmy:
                case CountryBallType.AirArmy:
                case CountryBallType.NavalArmy:
                    progress = (float)GetArmyUpgradeLevels() / BattleInGameService.ArmyMaxForce;
                    break;

                case CountryBallType.Resistance:
                    progress = (float)localResistanceLevel / BattleInGameService.ResistanceMaxForce;
                    break;

                case CountryBallType.Factory:
                    progress = GetFactoryUpgradesProgress();
                    break;

                case CountryBallType.Intelligence:
                    progress = 0;
                    break;
            }

            if (CountryBalls.Contains(ballType))
            {
                CountryBall visualCountryBall = CountryBalls.Get(ballType);
                visualCountryBall.SetStarsProgress(progress);
            }
        }

        public void Revolt(Action completeAction)
        {
            if (LocalCountryData.Owner != CommonData.PlayerID || LocalCountryData.IsBaseCountry) return;
            
            // восстание
            battle.Battle(this, this, completeAction);
        }

        public void SyncBallPositionsY()
        {
            float y = 0;

            foreach (var ball in CountryBalls.Values)
            {
                if (ball.CountryBallType == CountryBallType.Main)
                {
                    y = ball.transform.localPosition.y;
                    continue;
                }

                ball.transform.localPosition = new Vector3(ball.transform.localPosition.x, y, ball.transform.localPosition.z);
            }
        }

        public void OnCountryBallSelect(CountryBall ball, bool goFromCountriesScreen = false)
        {
            GuiScreen currentScreen = gui.CurrentScreen;

            SelectCountryBallType selectType = SelectCountryBallType.None;

            if (currentScreen is GameScreen || (goFromCountriesScreen && currentScreen is CountriesScreen)) selectType = SelectCountryBallType.SelectInGame;
            else if (currentScreen is ChooseCountryScreen) selectType = SelectCountryBallType.SelectToStartNewGame;
            else if (currentScreen is BombAttackScreen) selectType = SelectCountryBallType.SelectToBomb;
            else return;

            if (selectType == SelectCountryBallType.None) return;

            switch (selectType)
            {
                case SelectCountryBallType.SelectToStartNewGame:
                    countriesController.SelectCountryToPlay(ID);
                    break;

                case SelectCountryBallType.SelectToBomb:
                    gui.FindScreen<BombAttackScreen>().BombAttack(ID);
                    break;

                case SelectCountryBallType.SelectInGame:
                    if (LocalCountryData.OpenState)
                    {
                        bool isPlayerOwner = LocalCountryData.Owner == CommonData.PlayerID;

                        if (ball.CountryBallType == CountryBallType.Resistance)
                        {
                            if (isPlayerOwner)
                            {
                                // tutor
                                var tutor = gui.TutorContainer;
                                if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.ResistanceTutorID) tutor.CompleteTutorial();

                                var screen = gui.FindScreen<AttackScreen>();
                                screen.UpdateInfoForResistance(this);
                                gui.Show(screen);
                            }
                            else upgrades.ShowUpgradeScreen(ball.Country.ID, UpgradeService.StaticConvertToUpgradeType(ball.CountryBallType));
                        }
                        else
                        {
                            if (isPlayerOwner)
                            {
                                if (ball.CountryBallType == CountryBallType.Intelligence) gui.Show<QuickIntelligenceScreen>();
                                else upgrades.ShowUpgradeScreen(ball.Country.ID, UpgradeService.StaticConvertToUpgradeType(ball.CountryBallType));
                            }
                            else battle.ShowPlayerAttackInfo(this);
                        }
                    }
                    else Intelligence.Instance.PrepareIntelligence(this);
                    break;
            }
        }

        public UpgradeData GenerateWonderDataForCountry()
        {
            UpgradeData wonderData = new(upgrades.Upgrades.Get(UpgradeType.Economics).UpgradeDataList[^1]);

            var countryData = CountrySaveLoad.LoadCountry(ID);

            var wonderType = countryData.CountryData.WonderType;
            wonderData.Icon = countriesController._countriesDataConfig.GetWonderIcon(wonderType);
            bool wonderAlreadyBuilded = countryData.wonderBuilded;
            wonderData.CurrentLevel = wonderAlreadyBuilded ? 1 : 0;

            return wonderData;
        }

        public void SaveCountry()
        {
            CountryCollectedData csd = new();

            csd.CountryData = LocalCountryData;
            csd.factoryUpgrades = LocalFactoryUpgrades;
            csd.armyUpgrades = LocalArmyUpgrades;
            csd.resistanceLevel = localResistanceLevel;
            csd.wonderBuilded = localWonderBuilded;

            CountrySaveLoad.SaveCountry(ID, csd);
        }

        [ContextMenu("Test get army force")]
        private void TestGetArmyForce()
        {
            Debug.Log($"Force: {ArmyForce}");
        }

#if UNITY_EDITOR
        public void TestDownCountryBalls()
        {
            foreach (var ball in CountryBalls.Values) ball.TestMoveDown();
        }

        [ContextMenu("Reuse Scale Factor")]
        public void ReuseScaleFactor()
        {
            float scaleFactor = LocalCountryData.ScaleFactor;
            foreach (var ball in CountryBalls.Values) ball.SetSize(scaleFactor);
            foreach (var building in buildings) building.SetSize(scaleFactor);
            wonder.SetSize(scaleFactor);
        }
#endif
    }
}