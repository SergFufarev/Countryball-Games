using System;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using TheSTAR.Sound;

public class CountryGroupElement : MonoBehaviour
{
    [SerializeField] private CountryBallType countryBallType;
    [SerializeField] private StarsProgress stars;
    [SerializeField] private PointerButton goToButton;

    public Transform GoToButtonTran => goToButton.transform;

    public void Init(Action goToAction, bool visibleGoToButton)
    {
        goToButton.gameObject.SetActive(visibleGoToButton);
        goToButton.Init(goToAction);
    }

    public void SetProgress(float progress) => stars.SetProgress(progress);
}