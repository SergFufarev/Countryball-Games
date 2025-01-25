using System;
using UnityEngine;
using FunnyBlox;
using DG.Tweening;

public class CountryBallVisual : MonoBehaviour
{
    [SerializeField] private Transform scaler;
    [SerializeField] private Transform additionalScaler;
    [SerializeField] private Transform rotator;
    [SerializeField] private Renderer _bodyRenderer;
    [SerializeField] private CountryBallStars _stars;
    [SerializeField] private Transform _rotationHelper;
    [SerializeField] private Transform _visualElementParent;
    [Space]
    [SerializeField] private CountryBallAnim faceAnim;
    [SerializeField] private Animator bodyAnim;

    public Animator BodyAnim => bodyAnim;
    public Transform BodyTransform => bodyAnim.transform;
    public Transform VisualElementParent => _visualElementParent;
    public CountryBallAnim FaceAnim => faceAnim;

    public float GetLocalScaleFactor => scaler.localScale.x;

    private const float RotateTime = 1;
    private readonly Vector3 forwardAngles = new(-30, 180, 0);
    private readonly Vector3 upAngles = new(-90, 180, 0);

    private const float MainBallScaleMultilpier = 1.3f;
    public const float DefaultPosY = 1f; // дефолтная локальная высота кантрилоба
    public const float MainBallPosZ = 1.2f;
    private float _minLimit;
    private float minLimit
    {
        get
        {
            return _minLimit;
        }
        set
        {
            if (checkSize) Debug.Log("Set minLimit: " + value);
            _minLimit = value;
        }
    }
    private float idealSize; // скейл без учёта отдаления камеры

    public bool _useStars = false;

    private CountryBall target;

    public void Init(CountryBall target)
    {
        this.target = target;

        bool isMain = target.CountryBallType == CountryBallType.Main;
        if (isMain) minLimit = CameraService.DefaultBallSize;

        SetSize(target.Country.LocalCountryData.ScaleFactor);

        if (isMain) idealSize = target.Country.LocalCountryData.ScaleFactor;
    }

    public void SetMaterial(Material mat)
    {
        _bodyRenderer.sharedMaterial = mat;
    }

    public void SetStarsActivity(bool use)
    {
        _stars.gameObject.SetActive(use);
    }

    public void SetStarsProgress(float progress)
    {
        _stars.SetProgress(progress);
    }

    #region Size

    [Obsolete]
    public static Vector3 PositionForBallType(CountryBallType ballType)
    {
        Vector3 basePos = new(0, DefaultPosY, 0);

        int pos = 0;

        switch (ballType)
        {
            case CountryBallType.Main:
                pos = 0;
                break;
            case CountryBallType.GroundArmy:
                pos = 2;
                break;
            case CountryBallType.AirArmy:
                pos = 3;
                break;
            case CountryBallType.NavalArmy:
                pos = 4;
                break;
            case CountryBallType.Resistance:
                pos = -1;
                break;
            case CountryBallType.Factory:
                pos = 1;
                break;
        }

        return basePos + new Vector3(1.1f * pos, 0, 0);
    }

    public void SetScaleMinLimit(float minLimit)
    {
        this.minLimit = minLimit;
        if (minLimit > idealSize) SetSize(minLimit);
        else SetSize(idealSize);
    }

    [SerializeField] private bool checkSize;

    public void SetSize(float size)
    {
        SetSize(new Vector3(size, size, size));
    }

    private void SetSize(Vector3 size)
    {
        if (checkSize) Debug.Log("Set size: " + size);

        if (target.CountryBallType == CountryBallType.Main)
        {
            if (size.y > minLimit) scaler.transform.localScale = size;
            else scaler.transform.localScale = new Vector3(minLimit, minLimit, minLimit);
        }
        else scaler.transform.localScale = size;

        float additionalSize = (target.CountryBallType == CountryBallType.Main) ? MainBallScaleMultilpier : 1;
        additionalScaler.localScale = new Vector3(additionalSize, additionalSize, additionalSize);
    }

    #endregion Size

    #region Look

    public void UpdateLook(CameraMode cameraMode, bool animate)
    {
        switch (cameraMode.BallLookType)
        {
            case CountryBallLookType.Forward: LookForward(animate); break;
            case CountryBallLookType.Up: LookUp(animate); break;
        }
    }

    public void LookForward(bool animate)
    {
        rotator.DOKill();

        if (animate) rotator.DORotate(forwardAngles, RotateTime).SetEase(Ease.OutCirc);
        else rotator.localRotation = Quaternion.Euler(forwardAngles);
    }

    public void LookUp(bool animate)
    {
        rotator.DOKill();

        if (animate) rotator.DORotate(upAngles, RotateTime).SetEase(Ease.OutCirc);
        else rotator.localEulerAngles = upAngles;
    }

    #endregion

    #region Anim

    public static float GetRandomIdleAnimDelay
    {
        get
        {
            return UnityEngine.Random.Range(0f, 3f);
        }
    }

    public void PlayAnim(CountryBallEmotionType emotionType, float delay)
    {
        faceAnim.CurrentEmotion = emotionType;

        switch (faceAnim.CurrentEmotion)
        {
            case CountryBallEmotionType.Idle:
                bodyAnim.Play("Idle");
                break;
            case CountryBallEmotionType.Crying:
                bodyAnim.enabled = true;
                bodyAnim.Play("Cry");
                break;
        }

        if (delay == 0) DelayPlayAnim();
        else
        {
            faceAnim.PlayPreIdleAnim();
            Invoke(nameof(DelayPlayAnim), delay);
        }
    }

    public bool IsEmotionIdle => faceAnim.CurrentEmotion == CountryBallEmotionType.Idle;

    private void DelayPlayAnim()
    {
        faceAnim.PlayAnim(faceAnim.CurrentEmotion);
    }

    #endregion
}