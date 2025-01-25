using System;
using DG.Tweening;
using UnityEngine;
using Zenject;
using Lean.Common;
using Lean.Touch;
using TheSTAR.Utility;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using TheSTAR.GUI.UniversalElements;

namespace FunnyBlox
{
    [System.Serializable]
    public struct CameraMode
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public CountryBallLookType BallLookType;
        public float limitBallSize;
        public bool checkFovToBallScale;
        public float triggerSize;
        public Vector3 triggerCenter;
    }

    public class CameraService : MonoSingleton<CameraService>
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private Camera _subCamera;
        [SerializeField] private CameraZoomHelper _zoomHelper;

        [Space]
        [SerializeField] private LeanDragCamera _leanDragCamera;

        private Transform _cameraPivotTransform;

        [SerializeField] private CameraMode[] _cameraModes;

        [SerializeField] private float _speedMove;
        [SerializeField] private LeanPlane leanPlane;
        [SerializeField] private NearTrigger nearTrigger;
        [SerializeField] private Transform rotator;

        public NearTrigger NearTrigger => nearTrigger;
        public Camera UiCamera => _uiCamera;

        [Inject] private readonly GameController game;
        [Inject] private readonly GuiController gui;
        [Inject] private readonly VisualCountryController _countryService;

        private float _cameraEdgeOffsetTop;
        private float _cameraEdgeOffsetBottom;
        private float _cameraEdgeOffsetHorizontal;

        private int _currentModeIndex;
        private Vector3 _previousCameraPos;
        private float _previousCameraRotation;

        public event Action OnCameraMoveEvent;

        private const float HideBallsDistance = 40; // на какой дистанции скрываем кантриболы
        private const float NormalCameraToContentRatio = 0.9325f;
        private const float SpecialCameraToContentRatio = 1.7f;
        private const float SpecialOffsetRation = 0.416667f;
        private const float MaxHorizontalOffset = 23; // для расстояния камеры 100 оффсет равен 23

        private const float MaxBallSize = 8.8f;
        public const float DefaultBallSize = 1;
        private const float MinBallSize = 0.23f;


        // Sensitivity
        private const float MinDistanceSensitivity = 0.3f;
        private const float DefaultDistanceSensitivity = 0.6f;
        private const float MaxDistanceSensitivity = 1;

        public float DefaultCameraDistance => _cameraModes[0].Position.y;
        public float MinCameraDistance => _zoomHelper.MinZoom;
        public float MaxCameraDistance => _zoomHelper.MaxZoom;

        private bool _subCameraIsActive = false;
        private bool currentCountryBallsVisibility = true;

        public event Action<CameraMode> OnChangeModeEvent;

        public CameraMode CurrentCameraMode => _cameraModes[_currentModeIndex];

        void Start()
        {
            _cameraPivotTransform = _mainCamera.transform.parent;
            _mainCamera.transform.localPosition = _cameraModes[_currentModeIndex].Position;
            _cameraPivotTransform.rotation = Quaternion.Euler(_cameraModes[_currentModeIndex].Rotation);

            _previousCameraPos = transform.position;
            _previousCameraRotation = transform.rotation.x;

            _zoomHelper.Init(_cameraModes[_currentModeIndex].Position.y, ChangeZoom);
            nearTrigger.Init(game.ShowVisualOnlyForNearCountryBalls);
            nearTrigger.Set(_cameraModes[_currentModeIndex].triggerSize, _cameraModes[_currentModeIndex].triggerCenter);

            CalculateCameraEdgeOffset(_currentModeIndex);
        }

        private void Update()
        {
            // sub camera
            if (_subCameraIsActive) _subCamera.fieldOfView = _mainCamera.fieldOfView;

            // other
            if (transform.position != _previousCameraPos || transform.rotation.x != _previousCameraRotation) OnCameraPosOrRotationChanged();

            void OnCameraPosOrRotationChanged()
            {
                _previousCameraPos = transform.position;
                _previousCameraRotation = transform.rotation.x;
                OnCameraMoveEvent?.Invoke();

                // проверяем, вышла ли камера за рамки

                bool inBoundsMaxZ = transform.position.z + _cameraEdgeOffsetTop < leanPlane.MaxY;
                bool inBoundsMinY = transform.position.z - _cameraEdgeOffsetBottom > leanPlane.MinY;
                bool inBoundsZ = inBoundsMaxZ && inBoundsMinY;

                bool inBoundsMaxX = transform.position.x + _cameraEdgeOffsetHorizontal < leanPlane.MaxX;
                bool inBoundsMinX = transform.position.x - _cameraEdgeOffsetHorizontal > leanPlane.MinX;
                bool inBoundsX = inBoundsMaxX && inBoundsMinX;

                if (!inBoundsZ)
                {
                    float moveOffsetZ = 0;

                    if (!inBoundsMaxZ) moveOffsetZ = (transform.position.z + _cameraEdgeOffsetTop) - leanPlane.MaxY;
                    else if (!inBoundsMinY) moveOffsetZ = (transform.position.z - _cameraEdgeOffsetBottom) - leanPlane.MinY;

                    transform.position = new Vector3(transform.position.x, _countryService._countries[0].transform.position.y, transform.position.z - moveOffsetZ);
                }

                if (!inBoundsX)
                {
                    float x = 0;

                    if (!inBoundsMaxX) x = leanPlane.MinX + _cameraEdgeOffsetHorizontal;
                    else if (!inBoundsMinX) x = leanPlane.MaxX - _cameraEdgeOffsetHorizontal;

                    transform.position = new Vector3(x, _countryService._countries[0].transform.position.y, transform.position.z);
                }
            }
        }

        public void ChangeZoom(float value)
        {
            _mainCamera.transform.localPosition = new Vector3(0, value, 0);

            bool visible = value < HideBallsDistance;
            bool visibleChanged = currentCountryBallsVisibility != visible;
            currentCountryBallsVisibility = visible;
            float minSizeLimit;

            if (value > DefaultCameraDistance)
            {
                float progress = MathUtility.GetProgress(value, DefaultCameraDistance, _zoomHelper.MaxZoom);
                minSizeLimit = MathUtility.ProgressToValue(progress, DefaultBallSize, MaxBallSize);
            }
            else
            {
                float progress = MathUtility.GetProgress(value, _zoomHelper.MinZoom, DefaultCameraDistance);
                minSizeLimit = MathUtility.ProgressToValue(progress, MinBallSize, DefaultBallSize);
            }

            // sensitivity

            if (value > DefaultCameraDistance)
            {
                float progress = MathUtility.GetProgress(value, DefaultCameraDistance, MaxCameraDistance);
                _leanDragCamera.Sensitivity = MathUtility.ProgressToValue(progress, DefaultDistanceSensitivity, MaxDistanceSensitivity);
            }
            else
            {
                float progress = MathUtility.GetProgress(value, MinCameraDistance, DefaultCameraDistance);
                _leanDragCamera.Sensitivity = MathUtility.ProgressToValue(progress, MinDistanceSensitivity, DefaultDistanceSensitivity);
            }

            foreach (Country country in _countryService._countries)
            {
                country.SetMainBallScaleMinLimit(Math.Min(minSizeLimit, country.LocalCountryData.MaxSizeValue));
                if (visibleChanged && country.LocalCountryData.OpenState) country.SetElementsVisibility(visible);
            }

            CalculateCameraEdgeOffset(GetNearCameraModeIndex());

            float differenceMultiplier = value / _cameraModes[_currentModeIndex].Position.y;
            nearTrigger.Set(
                _cameraModes[_currentModeIndex].triggerSize * differenceMultiplier,
                _cameraModes[_currentModeIndex].triggerCenter * differenceMultiplier);
        }

        public void ForceOnCameraMove()
        {
            OnCameraMoveEvent?.Invoke();
        }

        private void CalculateCameraEdgeOffset(int modeIndex)
        {
            var currentModeData = _cameraModes[modeIndex];

            if (currentModeData.Rotation.x == 0)
            {
                float offsetSum = currentModeData.Position.y * NormalCameraToContentRatio;
                _cameraEdgeOffsetBottom = _cameraEdgeOffsetTop = offsetSum / 2;
            }
            else if (currentModeData.Rotation.x == -45)
            {
                float offsetSum = currentModeData.Position.y * SpecialCameraToContentRatio;

                _cameraEdgeOffsetBottom = offsetSum * SpecialOffsetRation;
                _cameraEdgeOffsetTop = offsetSum - _cameraEdgeOffsetBottom;
            }
            else Debug.LogError("Угол не поддерживается! Необходимо рассчитать значения");

            _cameraEdgeOffsetHorizontal = currentModeData.Position.y / 100 * MaxHorizontalOffset;
        }

        private int GetNearCameraModeIndex()
        {
            int? nearCameraModeIndex = null; // к какому моду камера расположена сейчас ближе всего

            for (int i = 0; i < _cameraModes.Length; i++)
            {
                if (_cameraModes[i].Rotation.x != _cameraModes[_currentModeIndex].Rotation.x) continue;

                if (nearCameraModeIndex == null)
                {
                    nearCameraModeIndex = i;
                    continue;
                }

                float dif = MathUtility.Difference(_cameraModes[i].Position.y, _mainCamera.transform.localPosition.y);
                if (dif < MathUtility.Difference(_cameraModes[(int)nearCameraModeIndex].Position.y, _mainCamera.transform.localPosition.y))
                {
                    nearCameraModeIndex = i;
                }
            }

            return (int)nearCameraModeIndex;
        }

        private bool currentUseFullUI = true;

        public void SwitchCameraMode()
        {
            if (game.UseMinimumUiInGameScreen && currentUseFullUI && gui.CurrentScreen is GameScreen)
            {
                currentUseFullUI = false;
                gui.FindScreen<GameScreen>().ShowFullUI(currentUseFullUI);
                gui.FindUniversalElement<IncomeContainer>().ShowFullUI(currentUseFullUI);
            }
            else
            {
                if (game.UseMinimumUiInGameScreen)
                {
                    currentUseFullUI = true;
                    gui.FindScreen<GameScreen>().ShowFullUI(currentUseFullUI);
                    gui.FindUniversalElement<IncomeContainer>().ShowFullUI(currentUseFullUI);
                }

                SetCameraMode(GetNearCameraModeIndex() + 1);
            }
        }

        public void SetDefaultCameraMode() => SetCameraMode(0);

        public void SetCameraMode(int newIndex)
        {
            _currentModeIndex = newIndex;
            if (_currentModeIndex >= _cameraModes.Length) _currentModeIndex = 0;
            _mainCamera.transform.DOLocalMove(_cameraModes[_currentModeIndex].Position, _speedMove).SetEase(Ease.OutCirc);
            rotator.DORotate(_cameraModes[_currentModeIndex].Rotation, _speedMove).SetEase(Ease.OutCirc);

            CalculateCameraEdgeOffset(_currentModeIndex);

            // sensitivity

            if (_cameraModes[_currentModeIndex].Position.y > DefaultCameraDistance)
            {
                float progress = MathUtility.GetProgress(_cameraModes[_currentModeIndex].Position.y, DefaultCameraDistance, MaxCameraDistance);
                _leanDragCamera.Sensitivity = MathUtility.ProgressToValue(progress, DefaultDistanceSensitivity, MaxDistanceSensitivity);
            }
            else
            {
                float progress = MathUtility.GetProgress(_cameraModes[_currentModeIndex].Position.y, MinCameraDistance, DefaultCameraDistance);
                _leanDragCamera.Sensitivity = MathUtility.ProgressToValue(progress, MinDistanceSensitivity, DefaultDistanceSensitivity);
            }

            bool visible = _cameraModes[_currentModeIndex].Position.y < HideBallsDistance;
            bool visibleChanged = visible != currentCountryBallsVisibility;
            currentCountryBallsVisibility = visible;


            foreach (Country country in _countryService._countries)
            {
                if (visibleChanged && country.LocalCountryData.OpenState) country.SetElementsVisibility(visible);

                country.SetMainBallScaleMinLimit(Mathf.Min(_cameraModes[_currentModeIndex].limitBallSize, country.LocalCountryData.MaxSizeValue));
            }

            OnChangeModeEvent?.Invoke(_cameraModes[_currentModeIndex]);

            _zoomHelper.Set(_cameraModes[_currentModeIndex].Position.y);
            nearTrigger.Set(_cameraModes[_currentModeIndex].triggerSize, _cameraModes[_currentModeIndex].triggerCenter);
        }

        public const float CameraMoveTime = 1;

        #region MoveTo

        public void MoveTo(Country country, Action completeAction = null, bool animate = true) => MoveTo(country, CountryBallType.Main, completeAction, animate);

        public void MoveTo(Country country, CountryBallType countryBallType, Action completeAction = null, bool animate = true)
        {
            ZoomToCountry(country);
            MoveTo(country.CountryBalls.Get(countryBallType).BodyTran, completeAction, animate);
        }

        public void MoveTo(Country country, Country attackerCountry, Action completeAction = null, bool animate = true)
        {
            ZoomToCountry(country.LocalCountryData.ScaleFactor > attackerCountry.LocalCountryData.ScaleFactor ? country : attackerCountry);
            MoveTo(country.CountryBalls.Get(CountryBallType.GroundArmy).BodyTran, completeAction, animate);
        }

        public void MoveTo(Transform to, Action completeAction = null, bool animate = true)
        {
            if (to == null)
            {
                completeAction?.Invoke();
                return;
            }

            if (animate)
            {
                StopLeanDragCamera();

                Action end = () =>
                {
                    completeAction?.Invoke();
                    if (gui.CurrentScreen == null || gui.CurrentScreen.DraggableCamera) PlayLeanDragCamera();
                };

                bool equalPositions = transform.position.x == to.position.x && transform.position.z == to.position.z;

                if (!equalPositions)
                {
                    transform.DOKill();
                    transform.DOMoveX(to.position.x, CameraMoveTime).SetEase(Ease.OutSine);
                    transform.DOMoveZ(to.position.z, CameraMoveTime).SetEase(Ease.OutSine).OnComplete(end.Invoke);
                }
                else end.Invoke();
            }
            else
            {
                transform.position = to.position;
                completeAction?.Invoke();
            }
        }

        private void ZoomToCountry(Country country)
        {
            float factor = country.LocalCountryData.ScaleFactor;
            float minZoom = MinCameraDistance;
            float mediumZoom = DefaultCameraDistance;
            float zoom = MathUtility.ProgressToValue(factor, minZoom, mediumZoom);
            zoom = MathUtility.Limit(zoom, minZoom, mediumZoom);
            _zoomHelper.Set(_mainCamera.transform.localPosition.y);
            _zoomHelper.ZoomTo(zoom);
        }

        #endregion //  MoveTo

        public void ActivateSubCamera()
        {
            _subCameraIsActive = true;
            _subCamera.gameObject.SetActive(true);
        }

        public void DeactivateSubCamera()
        {
            _subCameraIsActive = false;
            _subCamera.gameObject.SetActive(false);
        }

        public void StopLeanDragCamera() => _leanDragCamera.enabled = false;
        public void PlayLeanDragCamera() => _leanDragCamera.enabled = true;
    }
}