using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TheSTAR.Utility;
using Random = UnityEngine.Random;
using TheSTAR.Sound;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

namespace FunnyBlox
{
    public class BattleInGameService : MonoBehaviour, IController
    {
        [SerializeField] private Transform battleEffectsContainer;
        [SerializeField] private ParticleSystem[] battleParticles;
        [SerializeField] private BattleConfig battleConfig;
        [SerializeField] private AtomBomb bomb;
        [SerializeField] private ParticleSystem bombEffect;

        [Header("Enemy Attack")]
        [SerializeField] private EnemyRevolt revolt;
        [SerializeField] private EnemyInvasion invasion;
        [SerializeField] private EnemyMutual mutual;

        [Inject] private readonly GameController game;
        [Inject] private readonly GuiController gui;
        [Inject] private readonly VisualCountryController countries;

        public EnemyRevolt Revolt => revolt;
        public EnemyInvasion Invasion => invasion;
        public EnemyMutual Mutual => mutual;

        public EnemyAttackType? currentEnemyAttackType;

        public const float EnemyAttackMinForcePercent = 0.2f;
        public const float EnemyAttackMaxForcePercent = 0.4f;
        public const int ArmyMaxForce = 30;
        public const int ArmyMaxUpgradeLevel = 3;
        public const int ResistanceMaxForce = 3;

        private bool inBattle = false;
        public bool InBattle => inBattle;
        public BattleConfig BattleConfig => battleConfig;

        private static float[] armyUpgradeCostMultipliers = new float[]
        {
            0.25f,
            0.35f,
            0.4f
        };

        public static float ArmyUpgradeCostMutliplier(int levelIndex)
        {
            if (levelIndex >= armyUpgradeCostMultipliers.Length) levelIndex = armyUpgradeCostMultipliers.Length - 1;

            return armyUpgradeCostMultipliers[levelIndex];
        }

        // в каком порядке армии получают урон (первые в списке получают урон в первую очередь)
        private readonly CountryBallType[] armyDamagePriorities = new CountryBallType[]
        {
            CountryBallType.Resistance,
            CountryBallType.GroundArmy
        };

        public void Init()
        {
            revolt.Init(this);
            invasion.Init(this);
            mutual.Init(this);

            availableAttackTypes = new();

            if (revolt.Use) availableAttackTypes.Add(EnemyAttackType.Revolt);
            if (invasion.Use) availableAttackTypes.Add(EnemyAttackType.Invasion);
            if (mutual.Use) availableAttackTypes.Add(EnemyAttackType.Mutual);

            Invoke(nameof(TryLoadForStartGame), 1);
        }

        public void ShowPlayerAttackInfo(Country countryToAttack)
        {
            var screen = gui.FindScreen<AttackScreen>();
            screen.UpdateInfoToAttack(countryToAttack);
            gui.Show(screen);
        }

        public bool IsCountryInBattle(Country country)
        {
            if (!inBattle) return false;

            return country == attackerCountry || country == defenderCountry;
        }

        private Country attackerCountry, defenderCountry;

        public void Battle(Country attacker, Country defender, Action completeAction = null)
        {
            if (inBattle) return;

            SaveEnemyAttackStatus();

            bool attackerIsPlayer = attacker.LocalCountryData.Owner == CommonData.PlayerID;
            bool defenderIsPlayer = defender.LocalCountryData.Owner == CommonData.PlayerID;
            bool isRevolt = attacker == defender; // бунт, повстанцы страны нападают на пехоту
            bool aiFight = !attacker.LocalCountryData.IsPlayerOwner && !defender.LocalCountryData.IsPlayerOwner;

            bool groundArmyWasActivatedForMove = false;
            bool airArmyWasActivatedForMove = false;
            bool navalArmyWasActivatedForMove = false;

            // big battle (in battle scene)
            if (attackerIsPlayer && !defenderIsPlayer)
            {
                // prepare data
                CurrencyService.Instance.SaveData();
                SaveManager.Save(CommonData.PREFSKEY_BIG_BATTLE_GREEN_COUNTRY_ID, attacker.ID);
                SaveManager.Save(CommonData.PREFSKEY_BIG_BATTLE_RED_COUNTRY_ID, defender.ID);

                inBattle = true;
                attackerCountry = attacker;
                defenderCountry = defender;

                // animate army balls
                gui.CurrentScreen.Hide();
                attacker.UpdateCountryBalls();
                CameraService.Instance.MoveTo(defender, attacker, null);

                MoveBallsToEnemy(attacker, defender, () =>
                {
                    Vector3 battleEffectPos = MathUtility.MiddlePosition(
                        defender.CountryBalls.Get(CountryBallType.GroundArmy).Visual.BodyTransform.position,
                        attacker.CountryBalls.Get(CountryBallType.GroundArmy).Visual.BodyTransform.position);

                    //ParticlesPlay(battleEffectPos);
                    //StartCoroutine(WaitToCompleteBattleCor(battleConfig.BattleDuration));

                    // load battle scene
                    var loadScreen = gui.FindScreen<LoadScreen>();
                    loadScreen.Init(LoadBattleScene, null);
                    gui.Show(loadScreen);

                    void LoadBattleScene(Action endAction)
                    {
                        game.OpenScene(SceneType.Battle);
                    }
                });
            }

            // quick battle (in game scene)
            else
            {
                gui.CurrentScreen.Hide();

                if (!attackerIsPlayer) attacker.UpdateCountryBalls();
                if (!defenderIsPlayer) defender.UpdateCountryBalls();

                if (isRevolt && (!attackerIsPlayer || attacker.LocalCountryData.IsBaseCountry))
                {
                    Debug.Log("Бунт не может произойти в этой территории!");
                    return;
                }

                CameraService.Instance.MoveTo(defender, attacker, gui.ShowMainScreen);

                inBattle = true;
                attackerCountry = attacker;
                defenderCountry = defender;

                var attackerData = CountrySaveLoad.LoadCountry(attacker.ID);
                var defenderData = CountrySaveLoad.LoadCountry(defender.ID);

                MoveBallsToEnemy(attacker, defender, () =>
                {
                    Vector3 battleEffectPos = MathUtility.MiddlePosition(
                        defender.CountryBalls.Get(CountryBallType.GroundArmy).Visual.BodyTransform.position,
                        attacker.CountryBalls.Get(CountryBallType.GroundArmy).Visual.BodyTransform.position);

                    ParticlesPlay(battleEffectPos);
                    StartCoroutine(WaitToCompleteBattleCor(battleConfig.BattleDuration));
                });

                void CompleteBattle()
                {
                    if (CommonData.VibrationsOn) Handheld.Vibrate();

                    ParticlesStop();
                    MoveBallsToHome();

                    bool attackerWin;

                    int reportGroundForce = 0;

                    Country playerCountry = attackerIsPlayer ? attacker : defender;

                    if (isRevolt)
                    {
                        int revoltForce = attacker.LocalResistanceLevel;
                        int defenderForce = defenderData.GetArmyUpgradeLevels();

                        attackerWin = revoltForce > defenderForce;

                        // Потери
                        // Во время восстания потери несут повстанцы и пехота

                        defender.ArmyLosses(attacker.LocalResistanceLevel);
                        attacker.ResistanceLosses(attacker.LocalResistanceLevel);
                        reportGroundForce = playerCountry.ArmyForce;

                        // успешное восстание - мы теряем регион
                        if (attackerWin) attacker.Occupate(attacker.ID);

                        // report
                        MessageService.Instance.ShowBattleResultMessage(defender.ID, !attackerWin, reportGroundForce);
                    }
                    else if (aiFight)
                    {
                        float attackerForce = attackerData.GetArmyUpgradeLevels();
                        float defenderForce = defenderData.GetArmyUpgradeLevels();

                        attackerWin = attackerForce > defenderForce;

                        // захват территории
                        if (attackerWin) defender.Occupate(attacker.LocalCountryData.Owner);
                    }

                    // нападение врага на нашу территорию
                    else
                    {
                        float attackerForce = attackerData.GetArmyUpgradeLevels();
                        float defenderForce = defenderData.GetArmyUpgradeLevels();

                        if (attackerIsPlayer) attackerForce += defender.GetResistanceForceValue;

                        attackerWin = attackerForce > defenderForce;

                        int fullEnemyForce = attacker.ArmyForce;
                        int forceBeforeDamage;
                        forceBeforeDamage = defender.ArmyForce;
                        defender.ArmyLosses(fullEnemyForce);
                        fullEnemyForce -= forceBeforeDamage;

                        reportGroundForce = playerCountry.ArmyForce;

                        // захват территории
                        if (attackerWin) defender.Occupate(attacker.LocalCountryData.Owner);

                        if (attackerIsPlayer && attackerWin)
                        {
                            // если игрок захватил регион, который хотел на нас напасть, тогда перезапускаем вражескую атаку
                            if (invasion.EnemyAttackerCountry != null && defender.ID == invasion.EnemyAttackerCountry.ID)
                            {
                                invasion.Restart();
                            }

                            if (mutual.EnemyAttackerCountry != null && (defender.ID == mutual.EnemyAttackerCountry.ID || defender.ID == mutual.EnemyDefenderCountry.ID))
                            {
                                mutual.Restart();
                            }
                        }

                        // report
                        MessageService.Instance.ShowBattleResultMessage(defender.ID, attackerIsPlayer ? attackerWin : !attackerWin, reportGroundForce);
                    }

                    attackerCountry.SaveCountry();
                    defenderCountry.SaveCountry();

                    attackerCountry = null;
                    defenderCountry = null;

                    completeAction?.Invoke();
                }

                IEnumerator WaitToCompleteBattleCor(float timeSeconds)
                {
                    yield return new WaitForSeconds(timeSeconds);
                    CompleteBattle();
                }
            }

            #region Move

            void MoveBallsToEnemy(Country from, Country to, Action completeMoveAction)
            {
                // если необходимый для атаки болл изначально выключен, его нужно включить для атаки, а по завершении выключить обратно

                CountryBall ball;
                CountryBall goal;
                float time;

                if (isRevolt)
                {
                    ball = from.CountryBalls.Get(CountryBallType.Resistance);
                    goal = to.CountryBalls.Get(CountryBallType.GroundArmy);
                    time = Vector3.Distance(ball.transform.position, goal.transform.position) / battleConfig.MoveSpeed;

                    MoveToGoal(completeMoveAction);
                }
                else
                {
                    ball = from.CountryBalls.Get(CountryBallType.GroundArmy);
                    goal = to.CountryBalls.Get(CountryBallType.GroundArmy);
                    time = Vector3.Distance(ball.transform.position, goal.transform.position) / battleConfig.MoveSpeed;

                    if (!ball.gameObject.activeSelf)
                    {
                        ball.gameObject.SetActive(true);
                        groundArmyWasActivatedForMove = true;
                    }

                    MoveToGoal(completeMoveAction);

                    // если у врага есть авиация, то выдвигаем свою авиацию
                    if (from.LocalCountryData.UseAirArmy && to.LocalCountryData.UseAirArmy)
                    {
                        ball = from.CountryBalls.Get(CountryBallType.AirArmy);
                        goal = to.CountryBalls.Get(CountryBallType.AirArmy);

                        if (!ball.gameObject.activeSelf)
                        {
                            ball.gameObject.SetActive(true);
                            airArmyWasActivatedForMove = true;
                        }

                        MoveToGoal();
                    }

                    // если у врага есть флот, то выдвигаем свой флот
                    if (from.LocalCountryData.UseNavalArmy && to.LocalCountryData.UseNavalArmy)
                    {
                        ball = from.CountryBalls.Get(CountryBallType.NavalArmy);
                        goal = to.CountryBalls.Get(CountryBallType.NavalArmy);

                        if (!ball.gameObject.activeSelf)
                        {
                            ball.gameObject.SetActive(true);
                            navalArmyWasActivatedForMove = true;
                        }

                        MoveToGoal();
                    }

                    if (to.CountryBalls.Contains(CountryBallType.Resistance)) to.CountryBalls.Get(CountryBallType.Resistance).PlayAnim(CountryBallEmotionType.Angry);
                }

                void MoveToGoal(Action completeAction = null)
                {
                    ball.transform.DOKill();
                    ball.PlayAnim(CountryBallEmotionType.Angry);

                    float middleScaleFactor = (attacker.LocalCountryData.ScaleFactor + defender.LocalCountryData.ScaleFactor) / 2;

                    Vector3 battleOffset = CommonData.BallBackOffset * middleScaleFactor;
                    battleOffset = new Vector3(battleOffset.x, battleOffset.y, MathUtility.Limit(battleOffset.z, -MinBattleOffsetDistance, CommonData.BallBackOffset.z));

                    ball.Jump(goal.transform.position + battleOffset, completeAction);

                    goal.PlayAnim(CountryBallEmotionType.Crying);
                }
            }

            void MoveBallsToHome()
            {
                CountryBall ball;
                Country homeCountry = attacker;
                float time;

                if (isRevolt)
                {
                    ball = attacker.CountryBalls.Get(CountryBallType.Resistance);
                    time = Vector3.Distance(ball.transform.localPosition, attacker.PositionForBall(ball.CountryBallType)) / battleConfig.MoveSpeed;

                    MoveToHome(ExitFromFight);

                    attacker.CountryBalls.Get(CountryBallType.GroundArmy).PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                }
                else
                {
                    Country battleCountry = defender;
                    ball = homeCountry.CountryBalls.Get(CountryBallType.GroundArmy);
                    time = Vector3.Distance(ball.transform.localPosition, Vector3.right * 2.2f) / battleConfig.MoveSpeed;

                    MoveToHome(() =>
                    {
                        if (groundArmyWasActivatedForMove) homeCountry.CountryBalls.Get(CountryBallType.GroundArmy).gameObject.SetActive(false);
                        if (airArmyWasActivatedForMove) homeCountry.CountryBalls.Get(CountryBallType.AirArmy).gameObject.SetActive(false);
                        if (navalArmyWasActivatedForMove) homeCountry.CountryBalls.Get(CountryBallType.NavalArmy).gameObject.SetActive(false);

                        ExitFromFight();
                    });

                    if (homeCountry.LocalCountryData.UseAirArmy && battleCountry.LocalCountryData.UseAirArmy)
                    {
                        ball = homeCountry.CountryBalls.Get(CountryBallType.AirArmy);
                        MoveToHome();
                    }

                    if (homeCountry.LocalCountryData.UseNavalArmy && battleCountry.LocalCountryData.UseNavalArmy)
                    {
                        ball = homeCountry.CountryBalls.Get(CountryBallType.NavalArmy);
                        MoveToHome();
                    }

                    // animate enemies

                    battleCountry.CountryBalls.Get(CountryBallType.GroundArmy).PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                    if (battleCountry.CountryBalls.Contains(CountryBallType.AirArmy)) battleCountry.CountryBalls.Get(CountryBallType.AirArmy).PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                    if (battleCountry.CountryBalls.Contains(CountryBallType.NavalArmy)) battleCountry.CountryBalls.Get(CountryBallType.NavalArmy).PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                    if (battleCountry.CountryBalls.Contains(CountryBallType.Resistance)) battleCountry.CountryBalls.Get(CountryBallType.Resistance).PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                }

                void MoveToHome(Action completeAction = null)
                {
                    ball.PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);
                    ball.transform.DOKill();
                    ball.Jump(attacker.transform.position + homeCountry.PositionForBall(ball.CountryBallType), completeAction);
                }

                void ExitFromFight()
                {
                    inBattle = false;
                    TryNextEnemyAttack();
                }
            }

            #endregion

            #region Particles

            void ParticlesPlay(Vector3 pos)
            {
                battleEffectsContainer.position = pos;

                battleParticles[0].Play(true);
                battleParticles[1].Play(true);
            }

            void ParticlesStop()
            {
                battleParticles[0].Stop(true);
                battleParticles[1].Stop(true);
            }

            #endregion
        }

        public void AnimateExitFromBigBattle(Country attacker, Country defender)
        {
            float middleScaleFactor = (attacker.LocalCountryData.ScaleFactor + defender.LocalCountryData.ScaleFactor) / 2;

            Vector3 battleOffset = CommonData.BallBackOffset * middleScaleFactor;
            battleOffset = new Vector3(battleOffset.x, battleOffset.y, MathUtility.Limit(battleOffset.z, -MinBattleOffsetDistance, CommonData.BallBackOffset.z));

            TryAnimateBall(CountryBallType.GroundArmy, () => gui.Show<GameScreen>());
            TryAnimateBall(CountryBallType.NavalArmy);
            TryAnimateBall(CountryBallType.AirArmy);

            void TryAnimateBall(CountryBallType ballType, Action endAction = null)
            {
                if (!attacker.CountryBalls.Contains(ballType) || !defender.CountryBalls.Contains(ballType)) return;

                var attackerBall = attacker.CountryBalls.Get(ballType);
                var defenderBall = defender.CountryBalls.Get(ballType);

                attackerBall.transform.position = defenderBall.transform.position + battleOffset;
                attackerBall.Jump(attacker.transform.position + attacker.PositionForBall(ballType), endAction);
            }
        }

        private void TryLoadForStartGame()
        {
            LoadEnemyAttackStatus(out bool success);

            if (!success) TryNextEnemyAttack();
        }

        private void TryNextEnemyAttack()
        {
            EnemyAttackType nextEnemyAttack;

            if (currentEnemyAttackType == null) nextEnemyAttack = availableAttackTypes[0];
            else
            {
                int currentEnemyAttackIndexInList = availableAttackTypes.IndexOf((EnemyAttackType)currentEnemyAttackType);
                nextEnemyAttack = ArrayUtility.GetNextRound(availableAttackTypes, currentEnemyAttackIndexInList);
            }

            GetEnemyAttackByType(nextEnemyAttack).Try();
        }

        private List<EnemyAttackType> availableAttackTypes;

        private EnemyAttack GetEnemyAttackByType(EnemyAttackType attackType)
        {
            return attackType switch
            {
                EnemyAttackType.Invasion => invasion,
                EnemyAttackType.Mutual => mutual,
                _ => revolt,
            };
        }

        // минимальное расстояние, на котором может находится атакующий кантрибол от защищающегося
        private const float MinBattleOffsetDistance = 0.5f;

        #region Bomb

        public void BombAttack(Country country)
        {
            if (inBattle) return;

            bomb.Fly(countries.PlayerBaseCountry, country, battleConfig.BombSpeed, () =>
            {
                country.BombLosses(BattleConfig.BombForce);
                bombEffect.transform.position = country.CountryBalls.Get(CountryBallType.Main).transform.position;
                bombEffect.Play();
                SoundController.Instance.PlaySound(SoundType.Bomb);

                TryNextEnemyAttack();
            });

            // notification

            var bombRechangeGameTimeSpan = battleConfig.BombRecoverTime;
            TimeSpan bombRechangeTimeSpan = new TimeSpan(bombRechangeGameTimeSpan.Hours, bombRechangeGameTimeSpan.Minutes, bombRechangeGameTimeSpan.Seconds);
            NotificationManager.Instance.RegisterNotificationForBombRecharge(DateTime.Now.Add(bombRechangeTimeSpan));
        }

        #endregion

        /// <summary>
        /// Возвращает рандомный регион игрока, не являющийся столицей
        /// </summary>
        public Country GetRandomPlayersRegion()
        {
            if (countries._playerCountries == null || countries._playerCountries.Count <= 1) return null;

            List<Country> availableCounries = new(countries._playerCountries);
            availableCounries.Remove(countries.PlayerBaseCountry);

            return availableCounries[Random.Range(0, availableCounries.Count)];
        }

        /// <summary>
        /// Возвращает рандомную вражескую территорию
        /// </summary>
        public Country GetRandomOpenEnemyRegion(Country withoutCountry = null)
        {
            List<Country> availableCountries = new(countries._openEnemyCountries);

            if (withoutCountry) availableCountries.Remove(withoutCountry);
            if (availableCountries.Count == 0) return null;

            return availableCountries[Random.Range(0, availableCountries.Count)];
        }

        /// <summary>
        /// Возвращает рандомную пару вражеских территорий
        /// </summary>
        public void GetRandomEnemiesForMutualAttack(out Country attacker, out Country defender)
        {
            attacker = GetRandomOpenEnemyRegion();
            defender = GetRandomOpenEnemyRegion(attacker);
        }

        [ContextMenu("SaveEnemyAttackStatus")]
        private void SaveEnemyAttackStatus()
        {
            if (currentEnemyAttackType == null) return;

            SaveManager.Save<EnemyAttackType>(CommonData.PREFSKEY_CURRENT_ENEMY_ATTACK_TYPE, (EnemyAttackType)currentEnemyAttackType);
            var currentAttack = GetEnemyAttackByType((EnemyAttackType)currentEnemyAttackType);
            SaveManager.Save<EnemyAttackState>(CommonData.PREFSKEY_CURRENT_ENEMY_ATTACK_STATE, currentAttack.State);
            SaveManager.Save<float>(CommonData.PREFSKEY_WAIT_TO_ENEMY_ATTACK_TIME, currentAttack.WaitTime);

            // counries

            int countryID;
            countryID = revolt.RevoltRegion ? revolt.RevoltRegion.ID : -1;
            SaveManager.Save<int>(CommonData.PREFSKEY_ENEMY_ATTACK_REVOLT_COUNTRY_ID, countryID);

            countryID = invasion.EnemyAttackerCountry ? invasion.EnemyAttackerCountry.ID : -1;
            SaveManager.Save<int>(CommonData.PREFSKEY_ENEMY_ATTACK_INVASION_ATTACKER_COUNTRY_ID, countryID);

            countryID = invasion.CountryToEnemyAttack ? invasion.CountryToEnemyAttack.ID : -1;
            SaveManager.Save<int>(CommonData.PREFSKEY_ENEMY_ATTACK_INVASION_DEFENDER_COUNTRY_ID, countryID);

            countryID = mutual.EnemyAttackerCountry ? mutual.EnemyAttackerCountry.ID : -1;
            SaveManager.Save<int>(CommonData.PREFSKEY_ENEMY_ATTACK_MUTUAL_ATTACKER_COUNTRY_ID, countryID);

            countryID = mutual.EnemyDefenderCountry ? mutual.EnemyDefenderCountry.ID : -1;
            SaveManager.Save<int>(CommonData.PREFSKEY_ENEMY_ATTACK_MUTUAL_DEFENDER_COUNTRY_ID, countryID);
        }

        [ContextMenu("LoadEnemyAttackStatus")]
        private void LoadEnemyAttackStatus(out bool success)
        {
            success = false;

            if (!PlayerPrefs.HasKey(CommonData.PREFSKEY_CURRENT_ENEMY_ATTACK_TYPE)) return;

            revolt.Break();
            invasion.Break();
            mutual.Break();

            currentEnemyAttackType = SaveManager.Load<EnemyAttackType>(CommonData.PREFSKEY_CURRENT_ENEMY_ATTACK_TYPE);
            var currentAttack = GetEnemyAttackByType((EnemyAttackType)currentEnemyAttackType);
            EnemyAttackState loadedState = SaveManager.Load<EnemyAttackState>(CommonData.PREFSKEY_CURRENT_ENEMY_ATTACK_STATE);
            float loadedWaitTime = SaveManager.Load<float>(CommonData.PREFSKEY_WAIT_TO_ENEMY_ATTACK_TIME);
            currentAttack.Load(loadedState, loadedWaitTime);

            // countries

            revolt.LoadCountries(
                countries.GetCountry(SaveManager.Load<int>(CommonData.PREFSKEY_ENEMY_ATTACK_REVOLT_COUNTRY_ID)));

            invasion.LoadCountries(
                countries.GetCountry(SaveManager.Load<int>(CommonData.PREFSKEY_ENEMY_ATTACK_INVASION_ATTACKER_COUNTRY_ID)),
                countries.GetCountry(SaveManager.Load<int>(CommonData.PREFSKEY_ENEMY_ATTACK_INVASION_DEFENDER_COUNTRY_ID)));

            mutual.LoadCountries(
                countries.GetCountry(SaveManager.Load<int>(CommonData.PREFSKEY_ENEMY_ATTACK_MUTUAL_ATTACKER_COUNTRY_ID)),
                countries.GetCountry(SaveManager.Load<int>(CommonData.PREFSKEY_ENEMY_ATTACK_MUTUAL_DEFENDER_COUNTRY_ID)));

            success = true;
        }

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) SaveEnemyAttackStatus();
        }

        private void OnApplicationQuit()
        {
            SaveEnemyAttackStatus();
        }
    }

    public enum EnemyAttackType
    {
        Revolt, // сопротивление
        Invasion, // вторжение
        Mutual
    }

    public enum EnemyAttackState
    {
        /// <summary> Бездействие. Не проводится никаких ожиданий, подготовок (продолжительнось не ограничена) </summary>
        Idle = 0,

        /// <summary> Ожидание. Запущен таймер ожидания между нападениями (продолжительномть 3 минуты) </summary>
        Pause = 1,

        /// <summary> Ожидание завершено. Переход к BattlePreparation произойдёт сразуже, либо в момент когда это будет возможно (необходимо для того, чтобы выба возможна только 1 подготовка за раз)</summary>
        PauseComplete = 2,

        /// <summary> Подготовка. Расчитаны данные о предстоящем нападении, выводится сообщение (продолжительность 2 минуты). Этот статус может быть только у одного вида нападений за раз </summary>
        BattlePreparation = 3,

        /// <summary> Готов к бою. Если невозможно никакое нападение, ожидаем когда будет возможно. Если возможно нападение с подгоовленными данными, то происходит это нападение. Если невозможно нападение в паре, то атака считается придварительно отраженной, переходим к ожиданию следующего нападения </summary>
        ReadyToAttack = 4,

        /// <summary> Происходит атака </summary>
        InAttack = 5
    }
}