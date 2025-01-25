using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Sound;

namespace Battle
{
    public class BattleBuilding : BattleElement
    {
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private Transform damagePos;
        [SerializeField] private BuildindSquadPlace[] squadPlaces;
        [SerializeField] private Animator anim;
        [SerializeField] private BattleBanner[] banners;
        [SerializeField] private ParticleSystem hitParticle;
        [SerializeField] private DestructibleIntoPieces destructible;

        public BattleBanner[] Banners => banners;

        public override Transform DamageTransform => damagePos;
        public BuildingType BuildingType => buildingType;

        private float shootDistanceMultiplier;

        private BattleController battle;

        public bool IsFull
        {
            get
            {
                foreach (var place in squadPlaces)
                {
                    if (!place.IsFull) return false;
                }

                return true;
            }
        }

        public void Init(BattleController battle, BuildingConfigData buildingData)
        {
            this.battle = battle;
            UseBalanceData(buildingData);
            InitStats();

            if (destructible) destructible.Init();
        }

        private void UseBalanceData(BuildingConfigData data)
        {
            maxHp = data.Hp;
            shootDistanceMultiplier = data.ShootDistanceMultiplier;
            foreach (var place in squadPlaces) place.Init(shootDistanceMultiplier);
        }

        public void SetShooterUnits(Squad squad, out List<Unit> shootersWithoutTowerException)
        {
            shootersWithoutTowerException = new List<Unit>();

            foreach (var place in squadPlaces)
            {
                if (!place.IsFull)
                {
                    place.SetShooterUnits(squad, out shootersWithoutTowerException);
                    return;
                }
            }
        }

        public override void OnStartBattle()
        {
            base.OnStartBattle();

            foreach (var place in squadPlaces) place.OnStartBattle();
        }

        public override void OnEndBattle(bool win)
        {
            base.OnEndBattle(win);

            foreach (var place in squadPlaces) place.OnEndBattle(win);
        }

        public override void OnDie()
        {
            base.OnDie();

            foreach (var place in squadPlaces) place.OnDie();

            SoundController.Instance.PlaySound(SoundType.BuildingDestroy);

            battle.OnBuildingDestroy(this);

            if (destructible) destructible.Destruct(transform);
        }

        public override void Damage(Shooter shooter, float value, DamageReason reason)
        {
            base.Damage(shooter, value, reason);
            anim.SetTrigger("Damage");
            hitParticle.Play();
        }

        public void SetBannerMaterial(Material mat)
        {
            foreach (var rend in banners) rend.SetBannerMaterial(mat);
        }

        [Serializable]
        public class BuildindSquadPlace
        {
            [SerializeField] private Transform[] shooterPositions;
            public Transform[] ShooterPositions => shooterPositions;
            private List<Unit> shooters = new();

            private bool isFull = false;
            public bool IsFull => isFull;

            private float shootDistanceMultiplier;

            public void Init(float shootDistanceMultiplier)
            {
                this.shootDistanceMultiplier = shootDistanceMultiplier;
            }

            public void SetShooterUnits(Squad squad, out List<Unit> unitsWithoutPlace)
            {
                unitsWithoutPlace = new List<Unit>();
                bool shooterWithoutTowerException = false;
                foreach (var unit in squad.Units)
                {
                    SetShooterUnit(unit, out shooterWithoutTowerException);
                    if (shooterWithoutTowerException) unitsWithoutPlace.Add(unit);
                }

                squad.gameObject.SetActive(false);
                isFull = true;
            }

            private void SetShooterUnit(Unit shooter, out bool shooterWithoutTowerException)
            {
                if (shooters.Count == shooterPositions.Length)
                {
                    shooter.MakeTowerShooter(shootDistanceMultiplier, false);
                    shooter.gameObject.SetActive(false);
                    shooterWithoutTowerException = false;
                }
                else
                {
                    shooter.transform.position = shooterPositions[shooters.Count].position;
                    shooter.transform.parent = shooterPositions[shooters.Count];
                    shooters.Add(shooter);
                    shooter.MakeTowerShooter(shootDistanceMultiplier, true);
                    shooterWithoutTowerException = true;
                }
            }

            public void OnStartBattle()
            {
                foreach (var shooter in shooters) shooter.OnStartBattle();
            }

            public void OnEndBattle(bool win)
            {
                foreach (var shooter in shooters) shooter.OnEndBattle(win);
            }

            public void OnDie()
            {
                foreach (var shooter in shooters) shooter.OnDie();
            }
        }
    }
}