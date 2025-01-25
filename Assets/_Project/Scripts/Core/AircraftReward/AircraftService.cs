using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.FlyUI;

namespace FunnyBlox.PlaneReward
{
    // todo полностью переделать, сделано плохо
    public class AircraftService : MonoSingleton<AircraftService>
    {
        private Coroutine _timerCoroutine;
        private Coroutine _updateCoroutine;

        private Camera _camera;
        [SerializeField] private AircraftRoot _aircraftRoot;
        [SerializeField] private ParticleSystem _smokeParticle; 
        [SerializeField] private ParticleSystem _explosionParticle;
        [SerializeField] private CommonConfig commonConfig;
        
        private float _baseCameraHeight;
        private const int _maxRespawnInterations = 5;
        private const float _rotationScalingConst = 2.5f;
        private const int MinReward = 1;
        private const float GiveRewardDelay = 1;
        private bool needGiveReward = false; // нужно ли выдать награду за самолёт (может висеть на фоне, как бы в ожидании когда мы откроем главный экран)

        [SerializeField] private List<Aircraft> _aircrafts;
        private Aircraft _crrAircraft;

        [Space]
        [SerializeField] private float _timeDelay;
        [SerializeField] private float _aircraftSpeed;

        private float _spawnHeight = 2;

        [SerializeField] private float _spawnRadius;
        [SerializeField] private float _despawnRadius;
        [SerializeField] private float _gainPercent;

        [Inject] private readonly GuiController gui;
        [Inject] private readonly FlyUIContainer flyUI;

        public GuiController GUI => gui;

        private float GetCameraRealRotation => Mathf.Abs(CameraService.Instance.transform.rotation.eulerAngles.x - 360f);
        private float GetRotationScaling => Mathf.Sin(GetCameraRealRotation * Mathf.Deg2Rad);
        
        public AircraftRoot GetAircraftRoot => _aircraftRoot;
        public float GetSpawnRadius => _spawnRadius + (GetRotationScaling * _spawnRadius * _rotationScalingConst);
        public float GetDespawnRadius => _despawnRadius + (GetRotationScaling * _despawnRadius * _rotationScalingConst);
        public float GetSpawnHeight => Mathf.Clamp(_spawnHeight + (_camera.transform.position.y - _baseCameraHeight), 0, 1000);
        private Aircraft GetRandomAircraft => _aircrafts[UnityEngine.Random.Range(0, _aircrafts.Count)];
        public float GetAircraftSpeed => _aircraftSpeed;

        public event Action OnAircraftClick;
        public event Action OnAircraftExplode;

        public float FinalDelay
        {
            get
            {
                float bonusMultiplier;

                if (WorldEventService.Instance.CurrentWorldEvent == WorldEventType.MorePlanes) bonusMultiplier = WorldEventService.Instance.GetCurrentEventMultiplier();
                else bonusMultiplier = 1;

                return _timeDelay * bonusMultiplier;
            }
        }

        private void Start()
        {
            _camera = Camera.main;
            _baseCameraHeight = _camera.transform.position.y; // Возможно, лучше получать из CameraService значения самого первого CameraMode

            BakeAircrafts();
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void BakeAircrafts()
        {
            foreach (Aircraft aircraft in _aircrafts)
            {
                aircraft.Initialize(this, OnDeSpawn, OnExplode);
            }
        }

        private void OnExplode()
        {
            _explosionParticle.Play();
            _updateCoroutine = null;

            SetNeedGiveReward();
            OnEndFly();

            OnAircraftExplode?.Invoke();
        }

        private void OnDeSpawn()
        {
            _updateCoroutine = null;
            OnEndFly();
        }

        private void OnEndFly()
        {
            _aircraftRoot.OnEndFly();
        }

        private IEnumerator TimerCoroutine()
        {
            while (true)
            {
                yield return new WaitWhile(() => _updateCoroutine != null);
                yield return new WaitForSecondsRealtime(FinalDelay);

                StartFly();
            }
        }

        private void StartFly()
        {
            _crrAircraft = GetRandomAircraft;
            RespawnAircraft(_crrAircraft);

            _smokeParticle.gameObject.SetActive(false);
            _explosionParticle.Stop();
            _updateCoroutine = StartCoroutine(_crrAircraft.Update());
            _aircraftRoot.OnStartFly();
        }

        private void RespawnAircraft(Aircraft aircraft)
        {
            int iterations = 0;

            while (++iterations < _maxRespawnInterations)
            {
                aircraft.Spawn();

                if (IsSpawnedCorrectly()) break;
            }
        }

        private bool IsSpawnedCorrectly()
        {
            Vector3 screenPoint = _camera.WorldToViewportPoint(_aircraftRoot.transform.position);
            return screenPoint.x is > 0 and < 1 && screenPoint. y is > 0 and < 1;
        }

        private const float DropAngle = 35;
        
        public void OnAircraftSelected()
        {
            if (_crrAircraft != null && _crrAircraft.TryDrop(DropAngle)) _smokeParticle.gameObject.SetActive(true);

            OnAircraftClick?.Invoke();
        }

        private void SetNeedGiveReward()
        {
            needGiveReward = true;
            TryGiveReward(out _);
        }

        public void TryGiveReward(out bool success)
        {
            if (needGiveReward && gui.CurrentScreen.Root)
            {
                int reward = Math.Max((int)Math.Round(CommonData.MoneyPerSecondBase * _gainPercent), MinReward);

                flyUI.FlyToCounter(_aircraftRoot, CurrencyType.Money, reward);

                needGiveReward = false;
            }

            success = false;
        }

        // Вычисление точки относительно которой будет выпускаться и деспауниться самолет
        // будет в пересечении луча выпущенного из центра Viewport и плоскости y = 0.0f
        public Vector3 GetAlignment()
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var alignmentHeight = _camera.transform.position.y - GetSpawnHeight;
            var dst = alignmentHeight / Mathf.Abs(Mathf.Cos(GetCameraRealRotation * Mathf.Deg2Rad));
            return ray.GetPoint(dst);
        }

        private void OnDrawGizmosSelected()
        {
            if (_camera == null) return;

            var alignment = GetAlignment();
            if (alignment == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(alignment, GetSpawnRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(alignment, GetDespawnRadius);
        }
    }
}