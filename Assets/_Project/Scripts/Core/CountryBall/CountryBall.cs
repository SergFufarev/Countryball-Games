using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.GUI.FlyUI;
using TheSTAR.Utility;
using System.Linq;

namespace FunnyBlox
{
    public class CountryBall : CountryElement, IUiFlySender 
    {
        [SerializeField] private CountryBallType _countryBallType;
        [SerializeField] private CountryBallVisual visual;

        public float GetLocalScaleFactor => visual.GetLocalScaleFactor;
        public bool IsEmotionIdle => visual.IsEmotionIdle;

        private bool visualIsActive;
        public bool VisualIsActive => visualIsActive;

        private CountryBallCustomElement _currentCustomElement = null;

        public CountryBallVisual Visual => visual;

        private float progress;
        public float Progress => progress;

        public CountryBallType CountryBallType => _countryBallType;
        public Transform BodyTran => visual.BodyTransform;
        public Transform FaceTran => visual.FaceAnim.transform;

        public Transform startSendPos => transform;

        private Action<CountryBall, bool> onSelectAction;

        private static float randomAnimMinPeriod;
        private static float randomAnimMaxPeriod;

        public static void SetRandomAnimPeriods(float min, float max)
        {
            randomAnimMinPeriod = min;
            randomAnimMaxPeriod = max;
        }

        private float randomAnimPeriod;
        public float RandomAnimPeriod => randomAnimPeriod;

        #region Size

        public void SetSize(float size) => visual.SetSize(size);

        public void SetScaleMinLimit(float minLimit) => visual.SetScaleMinLimit(minLimit);

        #endregion Size

        private bool initialized = false;

        public void Init(
            CountryBallType ballType,
            Country country,
            Action<CountryBall, bool> onSelectAction)
        {
            if (initialized) return;

            this.onSelectAction = onSelectAction;

            _country = country;

            _countryBallType = ballType;
            visual.SetMaterial(_country.GetBallOwnerMaterial(_countryBallType));

            bool useStars = false;
            string objectName = "ball";

            switch (_countryBallType)
            {
                case CountryBallType.Main:
                    objectName = "main";
                    useStars = false;
                    break;
                case CountryBallType.Intelligence:
                    objectName = "intelligence";
                    useStars = false;
                    break;

                case CountryBallType.GroundArmy:
                    objectName = "ground";
                    useStars = true;
                    break;
                case CountryBallType.AirArmy:
                    objectName = "air";
                    useStars = true;
                    break;
                case CountryBallType.NavalArmy:
                    objectName = "fleet";
                    useStars = true;
                    break;
                case CountryBallType.Resistance:
                    objectName = "resistance";
                    useStars = true;
                    break;
                case CountryBallType.Factory:
                    objectName = "factory";
                    useStars = true;
                    break;
            }

            visual.SetStarsActivity(useStars);
            visual._useStars = useStars;

            // create role visual

            SpawnVisualElement(_countryBallType);

            // anim

            if (CameraService.Instance) CameraService.Instance.OnChangeModeEvent += (mode) => UpdateLook(mode, true);

            visual.Init(this);

            name = objectName;
            initialized = true;
        }

        public void InitForIngelligence(Transform parent, Country country, Action<CountryBall, bool> onSelectAction)
        {
            transform.parent = parent;

            _country = country;
            _countryBallType = CountryBallType.Intelligence;

            visual.SetStarsActivity(false);
            visual._useStars = false;

            this.onSelectAction = onSelectAction;

            visual.Init(this);

            name = "intelligence";
        }

        public void SetStarsProgress(float starsProgress)
        {
            progress = starsProgress;
            visual.SetStarsProgress(starsProgress);
        }

        public void OnSelect(bool goFromCountriesScreen = false) => onSelectAction?.Invoke(this, goFromCountriesScreen);

        #region Look

        public void UpdateLook(CameraMode cameraMode, bool animate) => visual.UpdateLook(cameraMode, animate);

        #endregion

        #region Visual

        public void UpdateOwnerMaterial()
        {
            if (_countryBallType == CountryBallType.Resistance)
            {
                if (_country.LocalCountryData.Owner == CommonData.PlayerID) visual.SetMaterial(_country.GetBaseOwnerMaterial);
                else
                {
                    var mat = _country.GetPlayerCountryMaterial;
                    if (mat != null) visual.SetMaterial(mat);
                }
            }
            else visual.SetMaterial(_country.GetCountryOwnerMaterial);
        }

        public void UpdateFace()
        {
            CustomizationFaceType face;

            if (CountryBallType == CountryBallType.Resistance)
            {
                if (Country.LocalCountryData.Owner == CommonData.PlayerID) face = CustomizationFaceType.Basic;
                else face = CountryBallVisualService.Instance.GetPlayerFaceType;
            }
            else
            {
                if (Country.LocalCountryData.Owner == CommonData.PlayerID) face = CountryBallVisualService.Instance.GetPlayerFaceType;
                else face = CustomizationFaceType.Basic;
            }

            SetFace(face);
        }

        [Obsolete]
        public void UpdateHat()
        {
            if (CountryBallType != CountryBallType.Main) return;

            if (_country.LocalCountryData.Owner == CommonData.PlayerID) SetHat(CountryBallVisualService.Instance.GetPlayerHatType);
            else SetHat(CustomizationHatType.None);
        }

        public void SetHat(CustomizationHatType hat)
        {
            if (_currentCustomElement) Destroy(_currentCustomElement.gameObject);

            if (hat == CustomizationHatType.None) return;

            var visualPrefab = CountryBallVisualService.Instance.GetCountryBallCustomizationPrefab(hat);
            if (visualPrefab) _currentCustomElement = Instantiate(visualPrefab, visual.VisualElementParent);
        }

        public void SetFace(CustomizationFaceType face)
        {
            visual.FaceAnim.SetFace(face);
            finalRandomAnimations = GetFinalAvailableRandomAnimations(face);
        }

        #endregion

        #region Anim

        public void PlayAnim(CountryBallEmotionType emotionType, float delay = 0)
        {
            if (!visual.gameObject.activeSelf) visual.gameObject.SetActive(true);

            visual.PlayAnim(emotionType, delay);
        }

        private bool inJump = false;

        public void Jump(Vector3 to, Action fullCompleteAction)
        {
            inJump = true;
            visual.BodyAnim.enabled = true;
            visual.BodyAnim.Play("Up");

            if (visual._useStars) visual.SetStarsActivity(false);

            TimeUtility.WaitAsync(AnimationUtility.GetClipLength(visual.BodyAnim, "rig|jumping.up"), () =>
            {
                transform.position = to;
                if (visual._useStars) visual.SetStarsActivity(true);

                TimeUtility.WaitAsync(AnimationUtility.GetClipLength(visual.BodyAnim, "rig|jumping.down"), () =>
                {
                    fullCompleteAction?.Invoke();
                    visual.BodyAnim.Play("Idle");
                    inJump = false;
                });
            });
        }

        #endregion

        public void SpawnVisualElement(CountryBallType countryBallType)
        {
            if (CountryBallVisualService.Instance == null) return;

            var visualPrefab = CountryBallVisualService.Instance.GetCountryBallRolePrefab(countryBallType);
            if (visualPrefab) Instantiate(visualPrefab, visual.VisualElementParent);
        }

#if UNITY_EDITOR

        public void PrepareColliderToTestMaxSize()
        {
            GetComponentInChildren<SphereCollider>().isTrigger = true;
            var r = gameObject.AddComponent<Rigidbody>();
            r.useGravity = false;
        }

        [ContextMenu("TestMoveDown")]
        public void TestMoveDown()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 0.5f, transform.localPosition.z);
        }
#endif

        public void PlayRandomAnim()
        {
            CountryBallEmotionType randomEmotion = ArrayUtility.GetRandomValue(finalRandomAnimations);
            PlayAnim(randomEmotion);
        }

        private readonly CountryBallEmotionType[] baseRandomAnimations = new CountryBallEmotionType[]
        {
            CountryBallEmotionType.Happy,
            CountryBallEmotionType.Nervous,
            CountryBallEmotionType.Shocked,
            CountryBallEmotionType.Sleeping,
            CountryBallEmotionType.Yawning
        };

        private CountryBallEmotionType[] finalRandomAnimations;

        private CountryBallEmotionType[] GetFinalAvailableRandomAnimations(CustomizationFaceType faceType)
        {
            var allAvailableEmotions = CountryBallVisualService.Instance.GetAvailableEmotionsForFaceType(faceType);

            List<CountryBallEmotionType> finalAvailableRandomEmotions = new();

            for (int i = 0; i < allAvailableEmotions.Length; i++)
            {
                if (baseRandomAnimations.Contains(allAvailableEmotions[i]))
                {
                    finalAvailableRandomEmotions.Add(allAvailableEmotions[i]);
                }
            }

            return finalAvailableRandomEmotions.ToArray();
        }

        public void SetVisualActivity(bool active)
        {
            if (inJump) return; // если шарик прыгает, его визуал трогать нельзя

            visualIsActive = active;

            visual.gameObject.SetActive(active);
            if (active) OnActivateVisual();
        }

        private bool visualWasActivated = false; // был ли визуал активирован хотя бы раз

        private void OnActivateVisual()
        {
            PlayAnim(CountryBallEmotionType.Idle, CountryBallVisual.GetRandomIdleAnimDelay);

            if (!visualWasActivated)
            {
                randomAnimPeriod = UnityEngine.Random.Range(randomAnimMinPeriod, randomAnimMaxPeriod);
                visualWasActivated = true;
            }
        }
    }

    public enum CountryBallLookType
    {
        Forward,
        Up
    }
}

public enum SelectCountryBallType
{
    None,
    SelectToStartNewGame,
    SelectInGame,
    SelectToBomb
}