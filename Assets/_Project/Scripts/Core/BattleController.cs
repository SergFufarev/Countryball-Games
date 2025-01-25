using UnityEngine;
using Zenject;
using TheSTAR.GUI;
using UnityEngine.SceneManagement;
using TheSTAR.GUI.Screens;
using TheSTAR.Utility;
using System.Collections.Generic;
using FunnyBlox;
using System;
using DG.Tweening;
using TheSTAR.Sound;
using SPSDigital.ADS;

namespace Battle
{
    public class BattleController : MonoBehaviour, IController, IUpgradeReactable
    {
        private Dictionary<BattleSideType, int> countryIDs;

        [SerializeField] private CommonConfig commonConfig;

        [SerializeField] private UnityDictionary<BattleSideType, BattleBuilding[]> buildings;
        [SerializeField] private bool generateArmiesOnStart;
        [SerializeField] private BattleBuilding redBase;
        [SerializeField] private BattleConfig battleConfig;
        [SerializeField] private ArmyDragHelper dragHelper;
        [SerializeField] private UnityDictionary<BattleSideType, BattleRocketContainer> rocketContainers;
        [SerializeField] private GameObject rocketPlane;
        [SerializeField] private DamageAoE damageAoePrefab;
        [SerializeField] private Transform damageAoeContainer;
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private AdaptiveQuality adaptiveQuality;
        [SerializeField] private BattleWinEffect winEffect;

        [Header("Banners")]
        [SerializeField] private Material[] bannerRandomMaterials;

        [Inject] private readonly ArmiesController armies;
        [Inject] private readonly GuiController gui;
        [Inject] private readonly BulletsContainer bullets;
        [Inject] private readonly CurrencyService currency;
        [Inject] private readonly UpgradeService upgrades;
        [Inject] private readonly BattleCameraController battleCameraController;

        public BattleBuilding RedBase => redBase;
        public ArmyDragHelper DragHelper => dragHelper;
        public ArmiesController Armies => armies;

        public event Action<int> OnChangeTotalRocketsCountEvent;
        public event Action<int> OnChangeCurrentLoadedRocketsInBattle;
        public event Action<float> RocketRechargingProgressEvent;
        public event Action FullRocketLoadComplete;
        public event Action OnPlayerRocketAttackEvent;

        private const int MaxRocketsInBattle = 3;
        private const float RocketRechargingDuration = 3;
        private const float EnemyRocketRechargingDuration = 5;

        private int rocketForce;
        private int rocketRadius;

        private readonly bool forceSetCameraModeForStartBattle = true;
        private readonly int forceCameraModeForStartBattle = 2;

        [SerializeField] private int totalAvailableRockets; // сколько всего у игрока ракет
        [SerializeField] private int currentLoadedRocketsInBattle; // сколько ракет загружено на данный момент
        [SerializeField] private int totalLoadedRocketsInBattle; // сколько ракет загружено суммарно в рамках боя
        [SerializeField] private int totalMaxRocketsForCurrentBattle;

        private int availableEnemyRockets;

        public int CurrentLoadedRocketsInBattle => currentLoadedRocketsInBattle;
        public int TotalAvailableRockets => totalAvailableRockets;
        public AdaptiveQuality AdaptiveQuality => adaptiveQuality;

        private bool inBattle = false;
        private bool battleFinished = false;

        public bool GamePlusMode
        {
            get
            {
                if (CommonData.CurrentGameVersionType == GameVersionType.VersionA) return false;

                return SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_PLUS);
            }
        }

        void Start()
        {
            inBattle = false;

            SetRandomBannerMaterial();

            Init();

            if (generateArmiesOnStart) LoadBattle();

            battleCameraController.Init();
            battleCameraController.SetVirtualCameraType(VirtualCameraType.Put);

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_USE_HEIGHT_QUALITY))
            {
                bool useHeightQuality = SaveManager.Load<bool>(CommonData.PREFSKEY_USE_HEIGHT_QUALITY);
                adaptiveQuality.SetQuality(useHeightQuality ? AdaptiveQualityType.Height : AdaptiveQualityType.Low);
            }
            else
            {
                var autoQualityLevel = adaptiveQuality.AutoUpdateQuality();
                bool useHeightQuality = autoQualityLevel == AdaptiveQualityType.Height;
                SaveManager.Save<bool>(CommonData.PREFSKEY_USE_HEIGHT_QUALITY, useHeightQuality);
                CommonData.UseHighQuality = useHeightQuality;
            }
        }

        private void Update()
        {
            bullets.Simulate();
        }

        private void Init()
        {
            CountryBallVisualService.Instance.InitService();

            gui.Init(out var urs, out var trs);
            gui.ShowMainScreen();
            armies.Init();

            urs.Add(armies);
            urs.Add(this);

            bullets.Init();
            currency.Init(trs);
            upgrades.Init(urs);

            redBase.onDieEvent += _ => Win();

            SetRocketsForStartBattle(CommonData.RocketsCount);
        }

        private void LoadBattle()
        {
            countryIDs = new();
            countryIDs.Add(BattleSideType.Green, SaveManager.Load<int>(CommonData.PREFSKEY_BIG_BATTLE_GREEN_COUNTRY_ID));
            countryIDs.Add(BattleSideType.Red, SaveManager.Load<int>(CommonData.PREFSKEY_BIG_BATTLE_RED_COUNTRY_ID));

            var allSides = EnumUtility.GetValues<BattleSideType>();
            foreach (var side in allSides)
            {
                foreach (var building in buildings.Get(side))
                {
                    building.Init(this, battleConfig.GetBuildingData(building.BuildingType));
                }
            }

            GenerateArmies();

            rocketForce = battleConfig.RocketData.Damage;
            rocketRadius = battleConfig.RocketData.Radius;
        }

        private void GenerateArmies()
        {
            armies.GenerateArmyOfSide(BattleSideType.Green, countryIDs[BattleSideType.Green], countryIDs[BattleSideType.Red], false);
            armies.GenerateArmyOfSide(BattleSideType.Red, countryIDs[BattleSideType.Red], countryIDs[BattleSideType.Green], true);
        }

        public void MoveSquadIntoTower(Squad squad, out List<Unit> unitsWithoutPlaceException)
        {
            unitsWithoutPlaceException = new();
            var buildingsOfSide = buildings.Get(squad.BattleSideType);
            BattleBuilding building;

            for (int i = 0; i < buildingsOfSide.Length; i++)
            {
                building = buildingsOfSide[i];
                if (!building.IsFull)
                {
                    building.SetShooterUnits(squad, out unitsWithoutPlaceException);
                    return;
                }
            }
        }

        public void GoToGameScene()
        {
            CommonData.LoadGameSceneFromBigBattle = true;

            SoundController.Instance.Stop(SoundType.BattleWinFireworks);
            SoundController.Instance.Stop(SoundType.BattleWin);
            SoundController.Instance.Stop(SoundType.BattleDefeat);

            TotalSaveGame();

            int sessionIndex = SaveManager.Load<int>(CommonData.PREFSKEY_SESSION_INDEX);
            Sprite fonSprite = commonConfig.loadFons[sessionIndex % commonConfig.loadFons.Length];

            var load = gui.FindScreen<LoadScreen>();
            load.Init(fonSprite, OpenGameScene, null);
            gui.Show(load);

            AdsService.Instance.InterstititalService.SkipDelay();

            void OpenGameScene(Action end)
            {
                OpenScene(SceneType.Game);
            }
        }

        private void OpenScene(SceneType sceneType)
        {
            SceneManager.LoadScene(sceneType.ToString());
        }

        public void SetGridColliderActivity(BattleSideType side, bool active)
        {
            armies.GetGrid(side).SetColliderActivity(active);
        }

        private void SavePlayerPlacement()
        {
            int[] playerPlacement = armies.GetCurrentPlacement(BattleSideType.Green);
            SaveManager.Save(CommonData.PREFSKEY_BIG_BATTLE_PLAYER_PLACEMENT, playerPlacement);
        }

        public void ShowGetUnitScreen()
        {
            upgrades.ShowUpgradeScreen(countryIDs[BattleSideType.Green], UpgradeType.Army);
        }

        public void StartBattle()
        {
            SoundController.Instance.PlayMusic(MusicType.BattleTheme);
            inBattle = true;

            if (totalAvailableRockets > 0) StartRocketRecharging();

            battleCameraController.SetVirtualCameraType(VirtualCameraType.Battle);
            if (forceSetCameraModeForStartBattle) battleCameraController.SetMode(forceCameraModeForStartBattle);

            rocketPlane.SetActive(true);

            SavePlayerPlacement();

            // start battle
            gui.Show<BattleScreen>();
            armies.StartBattle(BattleSideType.Green, redBase.transform);
            armies.StartBattle(BattleSideType.Red, null);
            bullets.StartSimulate();

            foreach (var building in buildings.Get(BattleSideType.Red))
            {
                building.OnStartBattle();
            }

            availableEnemyRockets = GetRocketsCountByForce(armies.GetSquadsCount(BattleSideType.Red));
            StartEnemyRocketRecharging();
        }

        private void StartEnemyRocketRecharging()
        {
            DOVirtual.Float(0, 1, EnemyRocketRechargingDuration, value => { }).OnComplete(() =>
            {
                if (TryEnemyRocketAttack()) StartEnemyRocketRecharging();
            });
        }

        private int GetRocketsCountByForce(int force)
        {
            return ((force - 1) / 10) + 1;
        }

        public void LeaveBattle()
        {
            battleFinished = true;
            SoundController.Instance.StopMusic();
            SoundController.Instance.PlaySound(SoundType.LeaveBattle);

            if (inBattle)
            {
                StopBattle(false);
                CalculateUnitResults();
                SaveRockets();
                GoToGameScene();
            }
            else
            {
                SavePlayerPlacement();
                GoToGameScene();
            }
        }

        [ContextMenu("Win")]
        private void Win()
        {
            if (battleFinished) return;

            battleFinished = true;

            winEffect.Play();
            SoundController.Instance.StopMusic();
            SoundController.Instance.PlaySound(SoundType.BattleWin);
            SoundController.Instance.PlaySound(SoundType.BattleWinFireworks);

            StopBattle(true);
            CalculateUnitResults();

            // завоевание территории
            var enemyCountry = CountrySaveLoad.LoadCountry(countryIDs[BattleSideType.Red]);
            enemyCountry.CountryData.Owner = CommonData.PlayerID;
            enemyCountry.armyUpgrades = new();
            enemyCountry.resistanceLevel = 0;
            //enemyCountry.CountryData.Resistance = 0;
            CountrySaveLoad.SaveCountry(countryIDs[BattleSideType.Red], enemyCountry);

            CommonData.PlayerOccupationCounter++;
            SaveManager.Save(CommonData.PREFSKEY_PLAYER_OCCUPATION_COUNTER, CommonData.PlayerOccupationCounter);

            SaveRockets();

            gui.HideCurrentScreen();
            gui.HideCurrentUniversalElements();

            Invoke(nameof(GoToGameScene), 6);

            //gui.Show<BattleWinScreen>();
        }

        [ContextMenu("Defeat")]
        public void Defeat()
        {
            if (battleFinished) return;

            battleFinished = true;

            StopBattle(false);
            CalculateUnitResults();

            SaveRockets();

            SoundController.Instance.StopMusic();
            SoundController.Instance.PlaySound(SoundType.BattleDefeat);

            gui.HideCurrentScreen();
            gui.HideCurrentUniversalElements();

            Invoke(nameof(GoToGameScene), 4);
        }

        private void SaveRockets()
        {
            if (totalAvailableRockets < 0) totalAvailableRockets = 0;

            CommonData.RocketsCount = totalAvailableRockets;
            SaveManager.Save(CommonData.PREFSKEY_BIG_BATTLE_ROCKETS_COUNT, CommonData.RocketsCount);
        }

        private void StopBattle(bool win)
        {
            armies.StopBattle(win);
            bullets.StopSimulate();
        }

        /// <summary>
        /// Расчёт оставшейся армии игрока
        /// </summary>
        private void CalculateUnitResults()
        {
            var battleUnitResults = armies.GetResultUnitCounts();
            var greenUnitResults = ConvertToUnitDictionary(battleUnitResults[BattleSideType.Green]);
            Dictionary<int, int> greenSquadResults = ConvertToSquadResults(greenUnitResults);

            // потери обычных солдат
            var countryData = SaveManager.Load<CountryCollectedData>("Country_" + countryIDs[BattleSideType.Green]);
            countryData.armyUpgrades = greenSquadResults;
            SaveManager.Save<CountryCollectedData>("Country_" + countryIDs[BattleSideType.Green], countryData);

            // потери повстанцев
            var enemyCountry = SaveManager.Load<CountryCollectedData>("Country_" + countryIDs[BattleSideType.Red]);
            if (greenUnitResults.ContainsKey((int)UnitType.Rebels)) enemyCountry.resistanceLevel = greenUnitResults[(int)UnitType.Rebels];
            else enemyCountry.resistanceLevel = 0;
            SaveManager.Save<CountryCollectedData>("Country_" + countryIDs[BattleSideType.Red], enemyCountry);

            Dictionary<int, int> ConvertToUnitDictionary(Dictionary<UnitType, int> intDictionary)
            {
                Dictionary<int, int> result = new();

                for (int i = 0; i < 10; i++)
                {
                    if (intDictionary.ContainsKey((UnitType)i))
                    {
                        result.Add(i, intDictionary[(UnitType)i]);
                    }
                }

                return result;
            }

            Dictionary<int, int> ConvertToSquadResults(Dictionary<int, int> unitResults)
            {
                Dictionary<int, int> squadResults = new();

                foreach (var id in unitResults.Keys)
                {
                    int squadSize = battleConfig.GetUnitData((UnitType)id).SquadSize;

                    squadResults.Add(id,
                        unitResults[id] / squadSize +
                        (unitResults[id] % squadSize == 0 ? 0 : 1));
                }

                return squadResults;
            }
        }

        private void TotalSaveGame()
        {
            currency.SaveData();
        }

        public void TryPlayerRocketAttack()
        {
            if (battleFinished || currentLoadedRocketsInBattle <= 0 || rocketContainers.Get(BattleSideType.Green).InAttack) return;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            var hits = Physics.RaycastAll(ray);
            RaycastHit rocketPlaneHit = new();

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("RocketPlane"))
                {
                    rocketPlaneHit = hit;
                    break;
                }
            }

            ReducePlayerRocket(() =>
            {
                RocketAttack(BattleSideType.Green, rocketPlaneHit.point);
                OnPlayerRocketAttackEvent?.Invoke();
            });
        }

        public bool TryEnemyRocketAttack()
        {
            if (battleFinished || availableEnemyRockets <= 0) return false;

            RocketAttack(BattleSideType.Red, armies.GetRandomActiveUnit(BattleSideType.Green).transform.position);
            availableEnemyRockets--;

            return true;
        }

        public readonly Vector3 ExplosionEffectOffset = new Vector3(0, 0.1f, 0);

        private void RocketAttack(BattleSideType side, Vector3 pos)
        {
            SoundController.Instance.PlaySound(SoundType.BombLaunch);

            rocketContainers.Get(side).Attack(pos, () =>
            {
                CreateAreaOfEffectDamage(side, pos, rocketForce, rocketRadius);
                PlayExplosionEffect(pos + ExplosionEffectOffset);
            });
        }

        public void SetRandomBannerMaterial()
        {
            var redBuildings = buildings.Get(BattleSideType.Red);
            List<BattleBanner> redBanners = new List<BattleBanner>();

            for (int i = 0; i < redBuildings.Length; i++)
            {
                for (int bannerIndex = 0; bannerIndex < redBuildings[i].Banners.Length; bannerIndex++)
                {
                    redBanners.Add(redBuildings[i].Banners[bannerIndex]);
                }
            }

            int count = redBanners.Count;
            Material[] randomMaterials = ArrayUtility.GetRandomValues(bannerRandomMaterials, count);

            for (int i = 0; i < redBanners.Count; i++)
            {
                redBanners[i].SetBannerMaterial(randomMaterials[i]);
            }
        }

        #region Rocket

        private bool inRocketRecharging = false;

        private void StartRocketRecharging()
        {
            if (inRocketRecharging) return;

            inRocketRecharging = true;

            DOVirtual.Float(0, 1, RocketRechargingDuration,
                value => RocketRechargingProgressEvent?.Invoke(value)).
                OnComplete(EndRecharging).SetEase(Ease.Linear);

            void EndRecharging()
            {
                inRocketRecharging = false;
                RocketRechargingProgressEvent?.Invoke(1);
                AddRocket();

                if (totalLoadedRocketsInBattle < totalMaxRocketsForCurrentBattle) StartRocketRecharging();
            }
        }

        private void AddRocket()
        {
            currentLoadedRocketsInBattle++;
            totalLoadedRocketsInBattle++;
            OnChangeCurrentLoadedRocketsInBattle?.Invoke(currentLoadedRocketsInBattle);

            if (totalLoadedRocketsInBattle >= MaxRocketsInBattle) FullRocketLoadComplete.Invoke();
        }

        private void ReducePlayerRocket(Action completeAction)
        {
            if (currentLoadedRocketsInBattle <= 0 || totalAvailableRockets <= 0) return;

            currentLoadedRocketsInBattle--;
            totalAvailableRockets--;
            totalMaxRocketsForCurrentBattle--;
            OnChangeTotalRocketsCountEvent?.Invoke(totalAvailableRockets);
            OnChangeCurrentLoadedRocketsInBattle?.Invoke(currentLoadedRocketsInBattle);

            completeAction?.Invoke();
        }

        private void SetRocketsForStartBattle(int totalRocketsCount)
        {
            totalAvailableRockets = totalRocketsCount;
            currentLoadedRocketsInBattle = 0;
            totalLoadedRocketsInBattle = 0;
            totalMaxRocketsForCurrentBattle = Math.Min(MaxRocketsInBattle, totalAvailableRockets);

            OnChangeTotalRocketsCountEvent?.Invoke(totalRocketsCount);
            OnChangeCurrentLoadedRocketsInBattle?.Invoke(currentLoadedRocketsInBattle);
        }

        #endregion

        public void CreateAreaOfEffectDamage(BattleSideType side, Vector3 pos, int force, float radius)
        {
            DamageAoE aoe = Instantiate(damageAoePrefab, pos, Quaternion.identity, damageAoeContainer);
            aoe.Init(side, force, radius);
            aoe.DoEffect();
        }

        public void PlayExplosionEffect(Vector3 pos)
        {
            explosionEffect.Stop();
            explosionEffect.transform.position = pos;
            explosionEffect.Play();
        }

        public void OnBuildingDestroy(BattleBuilding building)
        {
            PlayExplosionEffect(building.transform.position + ExplosionEffectOffset);
        }

        #region Reactable

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
        {
        }

        public void OnBuyTrade(int countryID)
        {
        }

        public void OnWonderBuilded(int countryID)
        {
        }

        public void OnRocketBuy(int totalRocketsCount)
        {
            SetRocketsForStartBattle(totalRocketsCount);
        }

        #endregion

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }

    public enum RocketAttackStatus
    {
        Ready,
        Wait
    }
}