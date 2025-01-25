using System;
using System.Collections.Generic;
using TheSTAR.GUI.Screens;
using TheSTAR.GUI.UniversalElements;
using UnityEngine;
using Zenject;
using FunnyBlox;
using DG.Tweening;

namespace TheSTAR.GUI.FlyUI
{
    public class FlyUIContainer : MonoBehaviour
    {
        [SerializeField] private HandfulFlyUI handfulPrefab;
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private AnimationCurve scaleCurve;

        [SerializeField] private float flyTime = 1;
        
        [Inject] private GuiController _gui;
        [Inject] private CurrencyService currency;
        [Inject] private CameraService cameraService;

        private List<FlyUIObject> _flyObjectsPool = new ();

        public void FlyToCounter(IUiFlySender from, CurrencyType currencyType, int value)
        {
            StartFlyTo(from, _gui.FindUniversalElement<IncomeContainer>().EndFlyCoinPos, currencyType, value);
        }
        
        private void StartFlyTo(IUiFlySender sender, RectTransform to, CurrencyType currencyType, int value)
        {
            var startPos = cameraService.UiCamera.ScreenToWorldPoint(Camera.main.WorldToScreenPoint(sender.startSendPos.position));
            var distance = to.position - startPos;

            Action endAction = () =>
            {
                currency.AddCurrency(currencyType, value);
                _gui.FindUniversalElement<IncomeContainer>().OnCompleteFlyCurrency(currencyType, value);
            };

            var handful = Instantiate(handfulPrefab, startPos, Quaternion.identity, transform);
            handful.transform.localPosition = new Vector3(handful.transform.localPosition.x, handful.transform.localPosition.y, 0);
            handful.Fly(7, to, endAction);
        }
    }

    public interface IUiFlySender
    {
        Transform startSendPos { get; }
    }
}