using UnityEngine;
using System;
using DG.Tweening;

public class CountryBuilding : CountryElement
{
    [SerializeField] private Transform animScaler;

    private const float AnimTime = 0.4f;
    private const float EndTime = 0.5f;

    private CountryBuildingVisual currentVisual;
    public CountryBuildingVisual CurrentVisual => currentVisual;

    public void GenerateVisual(CountryBuildingVisual visualPrefab)
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual.gameObject);
        }

        currentVisual = Instantiate(visualPrefab, animScaler);
    }

    public Vector3 GetLocalPos => transform.localPosition;

    public void SetSize(float scaleFactor)
    {
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    public void PrepareForBuildAnimation()
    {
        animScaler.localScale = Vector3.zero;
    }

    public void AnimateBuild(Action endAction)
    {
        DOVirtual.Float(0, 1, AnimTime, value =>
        {
            animScaler.localScale = new Vector3(value, value, value);
        }).SetEase(Ease.OutBack).OnComplete(() =>
        {
            DOVirtual.Float(0, 1, EndTime, null).OnComplete(() => endAction.Invoke());
        });
    }
}