using System;
using UnityEngine;
using UnityEngine.AI;
using TheSTAR.Sound;
using Sirenix.OdinInspector;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Battle
{
    public class Unit : BattleElement
    {
        [SerializeField] private UnitType unitType;
        [SerializeField] private bool useShootSound;
        [SerializeField] [ShowIf("useShootSound")] private SoundType shootSound;
        [SerializeField] private bool useDamageSound;
        [SerializeField] [ShowIf("useDamageSound")] private SoundType damageSound;
        [SerializeField] private bool useDieSound;
        [SerializeField] [ShowIf("useDieSound")] private bool useRandomDefaultDieSound;
        [SerializeField] [ShowIf("@useDieSound && !useRandomDefaultDieSound")] private SoundType dieSound;
        [SerializeField] private BattleHeightLevel height;

        private float force;
        private float attackSpeed;

        [Space]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private UnitTrigger attentionTrigger;
        [SerializeField] private UnitTrigger attackTrigger;
        [SerializeField] private Collider col;

        [Space]
        [SerializeField] private Vector3 shootRotationOffset;
        [SerializeField] private bool rotateToGoalBeforeShoot;

        private UnitVisual visual;

        private bool inBattle = false;
        private bool isAlive = true;
        private bool isTowerShooter = false;
        private bool isShooterWithoutTowerException = false;
        public bool IsTowerShooter => isTowerShooter;
        public bool IsShooterWithoutTowerException => isShooterWithoutTowerException;

        public UnitType UnitType => unitType;
        public BattleHeightLevel HeightLevel => height;

        private BulletType bulletType;
        private Transform finalMoveGoal;
        private Transform currentMoveGoal;
        private AutoShooter shooter;
        private Transform GetShootingTran() { return visual.ShootingPos; }

        private ArmiesController armies;
        private BulletsContainer bullets;

        private MoveStatus moveStatus;

        private UnitConfigData data;

        public void GenerateVisual(UnitVisual visualPrefab)
        {
            visual = Instantiate(visualPrefab, transform.position, visualPrefab.transform.rotation, transform);
        }

        public void Init(
            ArmiesController armies,
            BulletsContainer bullets,
            UnitConfigData data,
            Transform finalMoveGoal,
            BattleSideType side,
            CustomizationFaceType faceType,
            Material flagMaterial)
        {
            this.data = data;

            UseBalanceData();

            this.armies = armies;
            this.bullets = bullets;
            this.finalMoveGoal = finalMoveGoal;
            this.side = side;

            if (attentionTrigger) attentionTrigger.Init(side, OnEnemyEnementFoundForAttention);
            attackTrigger.Init(side, OnEnemyEnementFoundForAttack);

            shooter = new(force, 1 / attackSpeed, GetShootingTran);
            shooter.ShootEvent += Shoot;
            shooter.ShootingEntry += OnShootingEntry;
            shooter.ShootingExit += OnShootingExit;

            visual.SetFace(faceType);

            InitStats();

            visual.TrySetCountryBallMaterial(flagMaterial);
        }

        private void UseBalanceData()
        {
            maxHp = data.Hp;
            attackSpeed = data.AttackSpeed;
            force = data.Damage;

            attackTrigger.SetRadius(data.AttackDistance * (isTowerShooter ? towerShootDistanceBonus : GetRandomRadiusMultiplier));
            if (attentionTrigger) attentionTrigger.SetRadius(data.AttentionDistance * GetRandomRadiusMultiplier);

            agent.speed = data.MoveSpeed;
            bulletType = data.BulletType;
        }

        private float GetRandomRadiusMultiplier => Random.Range(0.9f, 1.1f);

        public void PlayFaceAnim(CountryBallEmotionType emotionType, float delay) => visual.PlayFaceAnim(emotionType, delay);

        public void PlayBodyAnim(BodyAnimType bodyAnimType, float delay = 0)
        {
            if (lockAnimations) return;

            if (delay == 0) visual.PlayBodyAnim(bodyAnimType);
            else
            {
                delayAnimType = bodyAnimType;
                Invoke(nameof(DelayPlayBodyAnim), delay);
            }
        }

        public void InitPlayStaticAnimation(float delay)
        {
            Invoke(nameof(TryPlayStaticAnimation), delay);
        }

        private void TryPlayStaticAnimation()
        {
            if (inBattle) return;

            // либо запускаем рандомную анимацию статики, либо ждём 2 секунды и повторяем попытку
            if (Random.Range(0f, 1f) > 0.5f) PlayBodyAnim(BodyAnimType.Static);

            Invoke(nameof(TryPlayStaticAnimation), 2);
        }

        private BodyAnimType delayAnimType;

        private void DelayPlayBodyAnim()
        {
            if (inBattle && delayAnimType == BodyAnimType.Static) return;

            visual.PlayBodyAnim(delayAnimType);
        }

        private float towerShootDistanceBonus;

        // сделать юнита башенным стрелком (не может двигаться, имеет бонус к дальности атаки)
        public void MakeTowerShooter(float shootDistanceBonus, bool withoutTowerException)
        {
            isTowerShooter = true;
            this.isShooterWithoutTowerException = withoutTowerException;
            this.towerShootDistanceBonus = shootDistanceBonus;
            UseBalanceData();
        }

        #region Shooter

        private Transform currentShootingGoal = null;

        private void Shoot(Shooter shooter, IDamageOwner goal, float force)
        {
            Action doShootAction = () =>
            {
                bullets.Shoot(side, bulletType, shooter, goal, DamageReason.Unit, force);

                if (useShootSound) SoundController.Instance.PlaySound(shootSound);

                PlayBodyAnim(BodyAnimType.Shoot);
            };

            doShootAction();

            if (rotateToGoalBeforeShoot && currentShootingGoal != goal.DamageTransform) SmoothLookAt(goal.DamageTransform);
        }

        private void SmoothLookAt(Transform target)
        {
            var lookAtQuaternion = Quaternion.LookRotation(target.position - transform.position);
            var angles = lookAtQuaternion.eulerAngles;
            angles = new Vector3(0, angles.y, 0) + shootRotationOffset;
            transform.DORotate(angles, 0.1f);
        }

        private void OnChangeShootingGoal(Transform newGoal)
        {
            SmoothLookAt(newGoal);
            currentShootingGoal = newGoal;
        }

        private void OnShootingEntry()
        {
            StopMoving();
        }

        private void OnShootingExit()
        {
            GoToFinalGoal();
            PlayBodyAnim(BodyAnimType.Move);
        }

        private void OnEnemyEnementFoundForAttention(BattleElement element)
        {
            if (currentMoveGoal == finalMoveGoal)
            {
                SetMoveGoal(element.transform);
                element.onDieEvent += (e) =>
                {
                    if (((BattleElement)e).transform == currentMoveGoal) GoToFinalGoal();
                };
            }
        }

        private void OnEnemyEnementFoundForAttack(BattleElement element) => AddGoal(element);

        private void AddGoal(BattleElement element)
        {
            if (shooter.ContainsGoal(element)) return;

            shooter.AddGoal(element);
            element.onDieEvent += (e) =>
            {
                RemoveGoal(e);
            };
        }

        private void RemoveGoal(IDamageOwner goal)
        {
            shooter.RemoveGoal(goal);
        }

        #endregion

        public void Simulate()
        {
            if (!inBattle || !isAlive || isTowerShooter || agent == null) return;

            if (moveStatus == MoveStatus.Stop) return;
            else
            {
                if (side == BattleSideType.Green)
                {
                    if (currentMoveGoal != null) agent.SetDestination(currentMoveGoal.position);
                    else agent.SetDestination(transform.position);
                }
                else
                {
                    agent.SetDestination(transform.position + Vector3.back);
                }
            }
        }

        public override void OnStartBattle()
        {
            inBattle = true;
            attackTrigger.gameObject.SetActive(true);

            if (!isTowerShooter)
            {
                attentionTrigger.gameObject.SetActive(true);
                col.enabled = true;
                agent.enabled = true;
            }

            shooter.StartSimulateShooting();

            PlayFaceAnim(CountryBallEmotionType.Angry, 0);
            PlayBodyAnim(BodyAnimType.Move);

            visual.OnStartBattle();
        }

        public override void OnEndBattle(bool win)
        {
            if (!isTowerShooter) agent.enabled = false;
            shooter.StopSimulateShooting();
            StopMoving();

            if (win)
            {
                PlayBodyAnim(BodyAnimType.Happy);
                this.lockAnimations = true;
            }
        }

        private bool lockAnimations = false;

        #region Move

        public void StopMoving()
        {
            currentMoveGoal = transform;
            moveStatus = MoveStatus.Stop;

            agent.enabled = false;
        }

        public void GoToFinalGoal()
        {
            SetMoveGoal(finalMoveGoal);
        }

        public void SetMoveGoal(Transform goal)
        {
            currentMoveGoal = goal;
            moveStatus = MoveStatus.Move;
            if (!isTowerShooter) agent.enabled = true;
        }

        #endregion // Move

        public override void OnDie()
        {
            base.OnDie();
            shooter.StopSimulateShooting();
            isAlive = false;
            armies.OnUnitDie(this);

            if (isTowerShooter) return;

            if (useDieSound)
            {
                if (useRandomDefaultDieSound)
                {
                    SoundType sound = (SoundType)Random.Range((int)SoundType.DieDefault_1, (int)SoundType.DieDefault_4 + 1);
                    SoundController.Instance.PlaySound(sound);
                }
                else SoundController.Instance.PlaySound(dieSound);
            }
        }

        public override void Damage(Shooter shooter, float value, DamageReason reason)
        {
            if (isTowerShooter) return;

            base.Damage(shooter, value, reason);

            if (side != BattleSideType.Red || this.shooter.ContainsAnyGoal) return;

            if (shooter != null) SetMoveGoal(shooter.Transform);

            if (useDamageSound)
            {
                SoundController.Instance.PlaySound(damageSound);
            }
        }
    }

    public enum MoveStatus
    {
        Stop,
        Move
    }
}