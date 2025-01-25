using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TheSTAR.Utility;
using TheSTAR.GUI;
using TheSTAR.GUI.FlyUI;
using TheSTAR.GUI.Screens;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace FunnyBlox
{
    public class VisualCountryController : MonoBehaviour, ISaver, IController, IUpgradeReactable
    {
        [SerializeField] private CountriesVisualType visualType;
        [SerializeField] public CountriesData _countriesDataConfig;
        [SerializeField] private CommonConfig _commonSettingsConfig;
        [SerializeField] private BallPositionHelper _ballPositionHelper;
        [SerializeField] private bool _testOpenWorld;
        [SerializeField] private Transform countriesContainer;
        [SerializeField] private Country countryPrefab;
        [SerializeField] private CountryBall countryBallPrefab;
        [SerializeField] private CountryBuilding buildingPrefab;
        [SerializeField] private RandomAnimationQueue randomAnimationQueue;

        public Country PlayerBaseCountry { get; set; }
        public List<Country> _countries;
        public List<Country> _playerCountries; // территории, принадлежащие игроку (в том числе базовый регион)
        public List<Country> _openCountries; // исследованные территории
        public List<Country> _openEnemyCountries; // открытые вражеские территории
        private Country _country;
        private int playerBaseCountryID = -1;
        public int PlayerBaseCountryID => playerBaseCountryID;

        private bool gameWasCompleted = false;
        private bool gamePlusMode = false; // находится ли игрок в режиме New Game +

        [Inject] private readonly GameController game;
        [Inject] private readonly GuiController gui;
        [Inject] private readonly RateUsController rateUs;
        [Inject] private readonly UpgradeService upgrades;
        [Inject] private readonly BattleInGameService battle;
        [Inject] private readonly FlyUIContainer flyUI;

        public CountryBall CountryBallPrefab => countryBallPrefab;
        public CountryBuilding CountryBuildingPrefab => buildingPrefab;

        public bool GamePlusMode
        {
            get
            {
                if (game.GameVersion == GameVersionType.VersionA) return false;

                return gamePlusMode;
            }
        }

        public bool TestOpenWorld => _testOpenWorld;
        public CountriesVisualType VisualType => visualType;

        // период запуска рандомной анимации кантрибола (в секундах)
        [Header("Random country ball anims")]
        [LabelText("Period Min")][SerializeField] private float RandomCountryBallAnimPeriodMin = 2;
        [LabelText("Period Max")] [SerializeField] private float RandomCountryBallAnimPeriodMax = 5;

        [Obsolete]
        private readonly List<int> microForceExceptions = new()
        {
            5, // Antigua and Barbuda
            11, // Багамы
            17, // Belize
            14, // Barbados
            28, // Cabo Verde
            37, // Comoros
            39, // Costa Rica
            41, // Куба
            47, // Dominika
            48, // Dominican Republic
            49, // Ecuador
            51, // El Salvador
            52, // Equatorial Guinea
            57, // Federated States of Micronesia
            67, // Grenada
            68, // Гватемала
            70, // Guinea-Bissau
            71, // Guyana
            72, // Haiti
            73, // Гондурас
            75, // Iceland
            84, // Jamaica
            89, // Kiribati
            107, // Marshall Islands
            119, // Nauru
            123, // Nicaragua
            131, // Palau
            132, // Panama
            143, // Saint Kitts and Nevis
            144, // Saint Lucia
            145, // saint vincent and the grenadies
            146, // Samoa
            147, // San Marino
            148, // São Tomé and Príncipe
            152, // Seychelles
            165, // Suriname
            173, // Tonga
            174, // Trinidad and Tobago
            178 // Tuvalu
        };
        private const int MicroForceValue = 6;

        #region Unity

        #endregion

        public void Init()
        {
            CountryBall.SetRandomAnimPeriods(RandomCountryBallAnimPeriodMin, RandomCountryBallAnimPeriodMax);

            CameraService.Instance.NearTrigger.OnNearCountryBall += (ball) => randomAnimationQueue.Add(ball);
            CameraService.Instance.NearTrigger.OnEndNearCountryBall += (ball) => randomAnimationQueue.Remove(ball);

            CountryBallVisualService.Instance.OnSetCustomisationEvent += UpdateCustomizationsForPlayerCountries;
        }

        public void LoadWorld()
        {
            // generation to start new game
            if (!PlayerPrefs.HasKey(CommonData.PREFSKEY_BASE_COINTRY_ID) || SaveManager.Load<int>(CommonData.PREFSKEY_BASE_COINTRY_ID) == -1)
            {
                GenerateCountriesForNewGame();
            }

            // load old game
            else LoadData();

            //WaitToRandomCountryBallAnim();
        }

        public CountryData.CountryBallPositionDatas LoadCountryBallPositionsFromConfig(int countryID)
        {
            var countryInConfig = _countriesDataConfig.CountryData[countryID];
            return countryInConfig.countryBallDatas;
        }

        public CountryData.BuildingPositionDatas LoadCountryBuildingPositionsFromConfig(int countryID)
        {
            var countryInConfig = _countriesDataConfig.CountryData[countryID];
            return countryInConfig.buildingsDatas;
        }

        public void SelectCountryToPlay(int countryId)
        {
            gui.Show<SelectColorScreen>();

            TimeUtility.WaitAsync(0.1f, () =>
            {
                OnSelectCountryToPlay(countryId);
                _country = GetCountry(countryId);
                SaveData();

                CameraService.Instance.SetDefaultCameraMode();
            });
        }

        /// <summary>
        /// Генерация для новой игры.
        /// Генерация всех стран с общими данными, столица не существует. При этом все кантриболы раскрашены в свой материал
        /// </summary>
        private void GenerateCountriesForNewGame()
        {
            // clear old
            if (_countries != null && _countries.Count > 0)
            {
                foreach (var country in _countries) Destroy(country.gameObject);

                _countries = new();
            }

            // generate new
            _openCountries = new();
            _openEnemyCountries = new();
            playerBaseCountryID = -1;

            foreach (CountryData data in _countriesDataConfig.CountryData)
            {
                CountryData tempData = new (data);

                tempData.Owner = tempData.Id;

                if (_testOpenWorld)
                {
                    //tempData.UseAirArmy = true;
                    //tempData.UseNavalArmy = true;
                    //tempData.UseResistance = true;
                }

                CreateCountry(tempData);
            }
        }

        public void OnSelectCountryToPlay(int id)
        {
            playerBaseCountryID = id;

            var selectedCountry = _countries[id];
            CountryData tempData = selectedCountry.LocalCountryData;

            tempData.Owner = CommonData.PlayerID;

            tempData.IsBaseCountry = true;
            tempData.Factories = 4;
            tempData.faceType = CountryBallVisualService.Instance.GetPlayerFaceType;
            tempData.hatType = CountryBallVisualService.Instance.GetPlayerHatType;
            selectedCountry.UseCustomisation();

            PlayerBaseCountry = _countries[id];
            PlayerBaseCountry.OnSelectForPlay();
            PlayerBaseCountry.Open(true);

            _playerCountries.Add(PlayerBaseCountry);
            CommonData.PlayerCountriesCount = _playerCountries.Count;

            //PlayerBaseCountry.SpawnIntelligence();

            // снова пробегаемся по странам, визуально закрываем их
            foreach (var c in _countries) c.Open(c.LocalCountryData.OpenState);

            if (game.GameVersion == GameVersionType.VersionA) return;

            // new game +

            if (SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_WAS_COMPLETED))
            {
                gamePlusMode = true;
                SaveManager.Save(CommonData.PREFSKEY_GAME_PLUS, gamePlusMode);

                int newGamePlusIndex = SaveManager.Load<int>(CommonData.PREFSKEY_GAME_PLUS_INDEX) + 1;
                SaveManager.Save(CommonData.PREFSKEY_GAME_PLUS_INDEX, newGamePlusIndex);

                if (newGamePlusIndex <= 5) AnalyticsManager.Instance.Log(AnalyticSectionType.Misc, $"new level {newGamePlusIndex}");
            }
        }

        /// <summary>
        /// Создание страны для начала игры
        /// </summary>
        private void CreateCountry(CountryData countryData)
        {
            _country = CreateCountryObject();
            countryData.BaseForce = GetRandomStartForce(ConvertFactoriesCountToForceType(countryData.Factories));
            _country.SetData(game, this, gui, rateUs, upgrades, battle, countryData, new(), new(), 0, false, _testOpenWorld);
            _countries.Add(_country);
        }

        /// <summary>
        /// Загрузка страны
        /// </summary>
        private void LoadCountry(CountryCollectedData data)
        {
            _country = CreateCountryObject();

            var ballPositionsData = LoadCountryBallPositionsFromConfig(data.CountryData.Id);
            data.CountryData.CloneBallPositions(ballPositionsData);

            var buildingPositionsData = LoadCountryBuildingPositionsFromConfig(data.CountryData.Id);
            data.CountryData.CloneBuildingPositions(buildingPositionsData);

            if (data.CountryData.Owner == CommonData.PlayerID) _playerCountries.Add(_country);

            _country.SetData(game, this, gui, rateUs, upgrades, battle, data.CountryData, data.factoryUpgrades, data.armyUpgrades, data.resistanceLevel, data.wonderBuilded, _testOpenWorld);
            _countries.Add(_country);

            if (_country.LocalCountryData.OpenState)
            {
                _openCountries.Add(_country);

                if (!_country.LocalCountryData.IsPlayerOwner) _openEnemyCountries.Add(_country);
            }

            if (_country.LocalCountryData.IsBaseCountry)
            {
                PlayerBaseCountry = _country;
                _country.LocalCountryData.Factories = Country.FullFactoriesCount;
            }
        }

        private Country CreateCountryObject()
        {
            return Instantiate(countryPrefab, countriesContainer);
        }

        public Country GetCountry(int countryId)
        {
            if (countryId == -1) return null;
            if (countryId == CommonData.PlayerID) countryId = playerBaseCountryID;
            return _countries.Find(country => country.ID == countryId);
        }

        public CountryData GetCountryData(int countryId)
        {
            return _countriesDataConfig.CountryData.Find(country => country.Id == countryId);
        }

        /// <summary>
        /// Перерасчёт полного заработка игрока
        /// </summary>
        public void RecalculatePlayerIncome()
        {
            CurrencyService.Instance.SetMoneyPerSecondValue(GetPlayerIncomeFromTerritories());
        }

        /// <summary>
        /// Суммарный доход игрока с территорий
        /// </summary>
        public float GetPlayerIncomeFromTerritories()
        {
            float defaultEffect = upgrades.GetDefaultEffect;
            float wonderEffect = upgrades.GetWonderEffect;
            float fullPlayerIncomeFromTerritories = 0f;
            float incomeFromCountry;

            CountryCollectedData countryData;

            foreach (Country country in _playerCountries)
            {
                countryData = CountrySaveLoad.LoadCountry(country.ID);
                incomeFromCountry = countryData.GetFactoryUpgradeLevels() * defaultEffect;

                if (countryData.wonderBuilded) incomeFromCountry += wonderEffect;

                if (upgrades.tradeRegions.Contains(country.ID)) incomeFromCountry *= upgrades.TradeEffectMultiplier;

                fullPlayerIncomeFromTerritories += incomeFromCountry;
            }

            return fullPlayerIncomeFromTerritories;
        }

        /// <summary>
        /// Получить рандомное значение силы страны на начало игры (расчитывается из количества экономики)
        /// </summary>
        public int GetRandomStartForce(CountryForceType countryForceType)
        {
            var range = _countriesDataConfig.countryForceData.Get(countryForceType);
            return Random.Range(range.min, range.max + 1);
        }

        public CountryForceType ConvertFactoriesCountToForceType(int factoriesCount)
        {
            return factoriesCount switch
            {
                1 => CountryForceType.Weak,
                2 => Random.Range(0f, 1f) < 0.5f ? CountryForceType.Small : CountryForceType.Middle,
                3 => CountryForceType.Big,
                4 => CountryForceType.Huge,
                _ => CountryForceType.Weak,
            };
        }

        #region SaveLoad

        [ContextMenu("SaveData")]
        public void SaveData()
        {
            // защита, иначе может быть такое что потеряем данные по странам
            if (_countries == null || _countries.Count == 0 || _countries.Count < 190) return;

            CountrySaveLoad.SavePlayerBaseCountryID(playerBaseCountryID);
            CountrySaveLoad.SaveCountriesCount(_countries.Count);

            foreach (Country country in _countries) country.SaveCountry();
        }

        /*
        // OLD
        public void SaveCountryWithConvertFromUpgradesToDictionary(Country country)
        {
            CountryCollectedData csd = new();

            csd.CountryData = country.LocalCountryData;

            // new factory save
            if (country.CountryBalls.Contains(CountryBallType.Factory))
            {
                csd.factoryUpgrades = ConvertCountryBallUpgradesToDictionary(country.CountryBalls.Get(CountryBallType.Factory));
            }

            // new army save
            if (country.CountryBalls.Contains(CountryBallType.GroundArmy))
            {
                csd.armyUpgrades = ConvertCountryBallUpgradesToDictionary(country.CountryBalls.Get(CountryBallType.GroundArmy));
            }

            // new resistance save
            if (country.CountryBalls.Contains(CountryBallType.Resistance))
            {
                int resistanceLevel = 0;
                var resistanceUpgrades = ConvertCountryBallUpgradesToDictionary(country.CountryBalls.Get(CountryBallType.Resistance));
                foreach (var resistance in resistanceUpgrades) resistanceLevel += resistance.Value;
                csd.resistanceLevel = resistanceLevel;
            }

            CountrySaveLoad.SaveCountry(country.ID, csd);
        }
        */

        /*
        private Dictionary<int, int> ConvertCountryBallUpgradesToDictionary(CountryBall ball)
        {
            Dictionary<int, int> data = new();

            for (int i = 0; i < ball.CountryBallUpgrades.Length; i++)
            {
                if (ball.CountryBallUpgrades[i].CurrentLevel > 0)
                {
                    data.Add(i, ball.CountryBallUpgrades[i].CurrentLevel);
                }
            }

            return data;
        }
        */

        private bool worldLoading = false;
        public bool WorldLoading => worldLoading;

        /// <summary>
        /// Загрузка игрового мира со всеми данными
        /// </summary>
        public void LoadData()
        {
            worldLoading = true;

            _openCountries = new();
            _openEnemyCountries = new();

            if (CountrySaveLoad.HasPlayerBaseCountyID) playerBaseCountryID = CountrySaveLoad.LoadPlayerBaseCountryID;

            if (CountrySaveLoad.HasCountriesCount)
            {                
                int countriesCount = CountrySaveLoad.LoadCountriesCount;

                if (countriesCount == 0)
                {
                    for (int i = 0; ; i++)
                    {
                        if (CountrySaveLoad.HasCountry(i)) countriesCount = i + 1;
                        else break;
                    }
                }

                CountryData configCountryData;

                for (int i = 0; i < countriesCount; i++)
                {
                    CountryCollectedData loadedCountryData = CountrySaveLoad.LoadCountry(i);

                    // парсим данные для новой версии если это необходимо
                    if (loadedCountryData.NeedUpgradeForBigBattle) loadedCountryData.ParseDataForBigBattleVersion();

                    // достаём из конфига данные, которые не сохраняем

                    configCountryData = _countriesDataConfig.CountryData[i];

                    loadedCountryData.CountryData.ScaleFactor = configCountryData.ScaleFactor;
                    loadedCountryData.CountryData.Color = configCountryData.Color;
                    loadedCountryData.CountryData.useBuildings = configCountryData.useBuildings;
                    loadedCountryData.CountryData.SetWonder(configCountryData.WonderType);

                    if (loadedCountryData.CountryData.IsPlayerOwner)
                    {
                        loadedCountryData.CountryData.hatType = CountryBallVisualService.Instance.GetPlayerHatType;
                        loadedCountryData.CountryData.faceType = CountryBallVisualService.Instance.GetPlayerFaceType;
                    }
                    else
                    {
                        CountryBallVisualService.Instance.GetCustomisationByOwner(loadedCountryData.CountryData.Owner,
                            out loadedCountryData.CountryData.hatType,
                            out loadedCountryData.CountryData.faceType);
                    }

                    LoadCountry(loadedCountryData);
                }

                CommonData.PlayerCountriesCount = _playerCountries.Count;
            }

            worldLoading = false;

            RecalculatePlayerIncome();
        }

        private void LoadGamePlusData()
        {
            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_GAME_WAS_COMPLETED))
            {
                gameWasCompleted = SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_WAS_COMPLETED);
            }

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_GAME_PLUS))
            {
                gamePlusMode = SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_PLUS);
            }
        }

        /// <summary>
        /// Удаляет данные о территориях
        /// </summary>
        public static void ClearData()
        {
            if (CountrySaveLoad.HasCountriesCount)
            {
                int countriesCount = CountrySaveLoad.LoadCountriesCount;

                if (countriesCount == 0)
                {
                    for (int i = 0; ; i++)
                    {
                        if (CountrySaveLoad.HasCountry(i)) countriesCount = i + 1;
                        else break;
                    }
                }

                for (int i = 0; i < countriesCount; i++) CountrySaveLoad.DeleteCountry(i);

                CountrySaveLoad.DeleteCountriesCount();
            }

            CountrySaveLoad.DeleteBaseCountryID();
        }

        #endregion

        #region Materials

        public Material GetMaterialByID(int id)
        {
            if (id == -1) return null;

            if (id >= _countriesDataConfig.FlagMaterials.Count) return _countriesDataConfig.FlagMaterials[0];

            return _countriesDataConfig.FlagMaterials[id];
        }

        public Material GetTerritoryUnexploredMaterial
        {
            get
            {
                if (visualType == CountriesVisualType.Minimum) return _countriesDataConfig._minMaterialUnexplored;
                else return _countriesDataConfig._unexploredTerritoryMaterial;
            }
        }

        public Material GetTerritoryMaterial(int territoryID)
        {
            return _countriesDataConfig._countryMaterialsFull[territoryID];
        }

        #endregion

        public Country FindEnemyForCountry(Country countryToAttack)
        {
            float baseForce = GetCountryData(countryToAttack.ID).BaseForce; // GetFullForceProgress; // изначальная сила страны до каких-либо взаимодействий с ней
            float minForce = baseForce * BattleInGameService.EnemyAttackMinForcePercent;
            float maxForce = baseForce * BattleInGameService.EnemyAttackMaxForcePercent;

            Country bestEnemyCandidate = null;

            bool candidatInDiapasonFound = false;

            foreach (var candidate in _countries)
            {
                if (candidate.LocalCountryData.Owner == countryToAttack.LocalCountryData.Owner) continue;

                if (bestEnemyCandidate == null)
                {
                    bestEnemyCandidate = candidate;
                    continue;
                }

                if (MathUtility.InBounds(candidate.LocalCountryData.BaseForce, minForce, maxForce))
                {
                    if (bestEnemyCandidate == null) bestEnemyCandidate = candidate;
                    else
                    {
                        if (!candidatInDiapasonFound || Vector3.Distance(candidate.transform.position, countryToAttack.transform.position) <
                            Vector3.Distance(bestEnemyCandidate.transform.position, countryToAttack.transform.position))
                        {
                            bestEnemyCandidate = candidate;
                        }
                    }

                    candidatInDiapasonFound = true;
                }
                else if (!candidatInDiapasonFound)
                {
                    if (Vector3.Distance(candidate.transform.position, countryToAttack.transform.position) <
                            Vector3.Distance(bestEnemyCandidate.transform.position, countryToAttack.transform.position))
                    {
                        bestEnemyCandidate = candidate;
                    }
                }
            }

            return bestEnemyCandidate;
        }

        public Country FindEnemyInForceRange(IntRange forceRange, bool countryMustBeClosed = true)
        {
            Country candidate;
            Country bestCandidate = null;

            for (int i = 0; i < _countries.Count; i++)
            {
                candidate = _countries[i];
                if (candidate == null || candidate.LocalCountryData.Owner == CommonData.PlayerID) continue;

                bool completeOpenClosedCondition = !countryMustBeClosed || !candidate.LocalCountryData.OpenState;
                bool completeForceCondition = MathUtility.InBounds(candidate.LocalCountryData.BaseForce, forceRange);

                if (completeOpenClosedCondition && completeForceCondition)
                {
                    if (bestCandidate == null) bestCandidate = candidate;
                    else
                    {
                        float bestCandidateDistance = Vector3.Distance(bestCandidate.transform.position, PlayerBaseCountry.transform.position);
                        float thisCandidateDistance = Vector3.Distance(candidate.transform.position, PlayerBaseCountry.transform.position);
                        if (thisCandidateDistance < bestCandidateDistance) bestCandidate = candidate;
                    }
                }
            }

            return bestCandidate;
        }

        public void OnCreateNewBuilding(CountryBuilding building)
        {
            building.PrepareForBuildAnimation();
            gui.HideCurrentScreen(() =>
            {
                CameraService.Instance.MoveTo(building.transform, () =>
                {
                    building.AnimateBuild(() => gui.Show(gui.CurrentScreen));
                });
            });
            gui.HideCurrentUniversalElements();
        }

        /// <summary>
        /// Обновить все кастомки для всех территорий игрока
        /// </summary>
        public void UpdateCustomizationsForPlayerCountries()
        {
            foreach (var pc in _playerCountries) pc.UpdateCustomisation();
            foreach (var oc in _openCountries) oc.UpdateCustomisation();
        }

        /// <summary>
        /// Проверяем, захватил ли игрок вообще все территории. Если да, открываем новый режим (активируется при следующем рестарте)
        /// </summary>
        public void CheckGameComplete()
        {
            if (_playerCountries.Count >= _countries.Count)
            {
                gameWasCompleted = true;
                SaveManager.Save(CommonData.PREFSKEY_GAME_WAS_COMPLETED, gameWasCompleted);
            }
        }

        public Country FindWeekCountry()
        {
            var result = FindCountryForForce(CountryForceType.Weak);
            if (result == null) FindCountryForForce(CountryForceType.Small);
            if (result == null) FindCountryForForce(CountryForceType.Middle);
            if (result == null) FindCountryForForce(CountryForceType.Big);
            if (result == null) FindCountryForForce(CountryForceType.Huge);
            return result;
        }

        public Country FindCountryForForce(CountryForceType forceType)
        {
            var neededForceRange = _countriesDataConfig.countryForceData.Get(forceType);

            if (forceType == CountryForceType.Weak) neededForceRange.min = 0;

            return FindEnemyInForceRange(neededForceRange);
        }

#if UNITY_EDITOR

        [ContextMenu("Load World")]
        private void RegenerateWorld()
        {
            GenerateCountriesForNewGame();
        }

        [ContextMenu("Save World")]
        private void ParseAllToConfig()
        {
            ParseScaleFactorsToConfig();
            ParseBallPositionsToConfig();
            ParseBuildingsToConfig();

            Debug.Log($"Full Parse Complete [{System.DateTime.Now}]");
        }

        private void ParseScaleFactorsToConfig()
        {
            for (int i = 0; i < _countries.Count; i++)
            {
                _countriesDataConfig.CountryData[i].ScaleFactor = _countries[i].LocalCountryData.ScaleFactor;
            }

            _countriesDataConfig.Save();
        }

        private void ParseBallPositionsToConfig()
        {
            _ballPositionHelper.ParseToConfig(_countries.ToArray(), _countriesDataConfig);
        }

        private void ParseBuildingsToConfig()
        {
            CountryBuilding building;
            Vector3 localPos;
            float x;
            float y;
            float z;
            float rotation;

            for (int countryIndex = 0; countryIndex < _countries.Count; countryIndex++)
            {
                var parseCountry = _countries[countryIndex];

                if (!parseCountry.LocalCountryData.useBuildings) continue;

                // buildings
                for (int buildingIndex = 0; buildingIndex < parseCountry.buildings.Count; buildingIndex++)
                {
                    if (buildingIndex >= parseCountry.buildings.Count) break;

                    building = parseCountry.buildings[buildingIndex];
                    localPos = building.GetLocalPos;
                    x = localPos.x;
                    y = localPos.y;
                    z = localPos.z;
                    rotation = building.transform.localEulerAngles.y;
                    _countriesDataConfig.CountryData[countryIndex].buildingsDatas.Set(buildingIndex, x, y, z, rotation);
                }

                if (parseCountry.wonder == null) continue;

                building = parseCountry.wonder;
                localPos = building.GetLocalPos;
                x = localPos.x;
                y = localPos.y;
                z = localPos.z;
                rotation = building.transform.localEulerAngles.y;

                // wonder
                _countriesDataConfig.CountryData[countryIndex].buildingsDatas.wonderPosition.Set(x, y, z, rotation);
            }

            _countriesDataConfig.Save();
        }

        //[ContextMenu("ParseBallLocalScalesAsScaleFactor")]
        private void ParseBallLocalScalesAsScaleFactor()
        {
            for (int countryIndex = 0; countryIndex < _countries.Count; countryIndex++)
            {
                var country = _countries[countryIndex];

                float localSF = country.CountryBalls.Get(CountryBallType.Main).GetLocalScaleFactor;
                _countriesDataConfig.CountryData[countryIndex].ScaleFactor = localSF;
            }

            _countriesDataConfig.Save();

            Debug.Log("ParseBallLocalScalesAsScaleFactor complete");
        }

        //[ContextMenu("UpdateCountryBallMaxSize")]
        private void UpdateCountryBallMaxSize()
        {
            float scaler = 1.3f;

            for (int countryIndex = 0; countryIndex < _countries.Count; countryIndex++)
            {
                _countriesDataConfig.CountryData[countryIndex].MaxSizeValue /= scaler;
            }

            _countriesDataConfig.Save();

            Debug.Log("UpdateCountryBallMaxSize complete");
        }

        public void SyncBallPositionsY()
        {
            foreach (var country in _countries)
            {
                country.SyncBallPositionsY();
            }
        }

        //[ContextMenu("TestMoveDownAllCountryBalls")]
        public void TestMoveDownAllCountryBalls()
        {
            foreach (var country in _countries)
            {
                country.TestDownCountryBalls();
            }
        }

        //[ContextMenu("TestFly")]
        private void TestFly()
        {
            flyUI.FlyToCounter(PlayerBaseCountry.CountryBalls.Get(CountryBallType.Main), CurrencyType.Money, 10);
        }
#endif

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        public void OnWonderBuilded(int countryID)
        {
            var countryData = CountrySaveLoad.LoadCountry(countryID);
            var visualCountry = _countries[countryID];
            visualCountry.SetWonderData(countryData.wonderBuilded);
            visualCountry.UpdateBallProgress(CountryBallType.Factory);
            visualCountry.UpdateBuildings();
            RecalculatePlayerIncome();
        }

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
        {
            var countryData = CountrySaveLoad.LoadCountry(countryID);
            var visualCountry = _countries[countryID];

            switch (upgradeType)
            {
                case UpgradeType.Economics:

                    visualCountry.SetFactoryData(countryData.factoryUpgrades);
                    // stars progress
                    visualCountry.UpdateBallProgress(CountryBallType.Factory);

                    // update economics
                    visualCountry.UpdateBuildings();
                    RecalculatePlayerIncome();
                    break;

                case UpgradeType.Army:

                    visualCountry.SetArmyData(countryData.armyUpgrades);
                    CountryBallType[] armyBallTypes = new CountryBallType[]
                    {
                        CountryBallType.GroundArmy,
                        CountryBallType.AirArmy,
                        CountryBallType.NavalArmy
                    };

                    foreach (var ballType in armyBallTypes)
                    {
                        visualCountry.UpdateBallProgress(ballType);
                    }

                    break;

                case UpgradeType.Resistance:
                    visualCountry.SetResistanceData(countryData.resistanceLevel);
                    // stars progress
                    visualCountry.UpdateBallProgress(CountryBallType.Resistance);

                    break;
            }
        }

        public void OnBuyTrade(int countryID)
        {
            RecalculatePlayerIncome();
        }

        public void OnRocketBuy(int totalRocketsCount) { }

        #endregion
    }
}

public enum CountriesVisualType
{
    /// <summary> Используется минимальный набор материалов </summary>
    Minimum,

    /// <summary>  Используются все материалы (отдельные на каждую страну) </summary>
    Full
}