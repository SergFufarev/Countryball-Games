using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using Lean.Touch;
using DG.Tweening;

public class CameraZoomHelper : MonoBehaviour
{
    [SerializeField] private LeanPinchWithoutCamera leanPinch;
    private float currentZoom;
    private bool isInit = false;
    private Action<float> onZoomChangedAction;

    public float MinZoom => leanPinch.ClampMin;
    public float MaxZoom => leanPinch.ClampMax;

    public void Init(float startZoom, Action<float> onZoomChangedAction)
    {
        Set(startZoom);
        this.onZoomChangedAction = onZoomChangedAction;
        isInit = true;
    }

    public void Set(float value)
    {
        leanPinch.Zoom = value;
        currentZoom = value;
    }

    void Update()
    {
        if (!isInit) return;

        if (currentZoom != leanPinch.Zoom)
        {
            float value = leanPinch.Zoom;

            if (!MathUtility.InBounds(value, MinZoom, MaxZoom))
            {
                value = MathUtility.Limit(value, MinZoom, MaxZoom);
                leanPinch.Zoom = value;
            }

            currentZoom = value;
            onZoomChangedAction?.Invoke(value);
        }
    }

    public void ZoomTo(float value)
    {
        DOVirtual.Float(leanPinch.Zoom, value, CameraService.CameraMoveTime, value => leanPinch.Zoom = value).SetEase(Ease.OutSine);
    }
}