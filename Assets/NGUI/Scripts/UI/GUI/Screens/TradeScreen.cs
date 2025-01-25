using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class TradeScreen : GuiScreen, ITutorialStarter, IUpgradeReactable
    {
        [SerializeField] private PointerButton closeButton;

        private const int MaxLevel = 1;

        private UpgradeService _upgrades;
        private GuiController gui;
        private VisualCountryController countries;

        private TradeData[] tradeDatas;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            closeButton.Init(OnCloseClick);

            gui = cts.Get<GuiController>();
            _upgrades = cts.Get<UpgradeService>();
            countries = cts.Get<VisualCountryController>();
        }

        protected override void OnShow()
        {
            //UpdateUI();

            base.OnShow();

            UpdateVisual();
            TryShowTutorial();
        }

        protected override void OnHide()
        {
            base.OnHide();

            var tutor = gui.TutorContainer;
            if (tutor.CurrentTutorialID == TutorContainer.TradeCloseTutorID) tutor.CompleteTutorial();
            else tutor.BreakTutorial();
        }

        public void SetData(TradeData[] tradeDatas, bool autoUpdateVisual = false)
        {
            this.tradeDatas = tradeDatas;

            if (autoUpdateVisual) UpdateVisual();
        }

        public void UpdateVisual()
        {
            var tutor = gui.TutorContainer;
            int commonCost = (_upgrades.tradeRegions.Count == 0 ? 500 : _upgrades.tradeRegions.Count * 1000);

            tradeDatas = new TradeData[countries._playerCountries.Count - 1];

            int countryIndex = 0;
            for (int i = 0; i < tradeDatas.Length;)
            {
                var playerCountry = countries._playerCountries[countryIndex];

                if (playerCountry.LocalCountryData.IsBaseCountry) countryIndex++;
                else
                {
                    int finalCost;
                    if (i == 0 && !tutor.IsComplete(TutorContainer.TradeTutorID)) finalCost = 0;
                    else finalCost = commonCost;

                    tradeDatas[i].upgradeData.Description = playerCountry.LocalCountryData.Name;
                    tradeDatas[i].upgradeData.Cost = finalCost;
                    tradeDatas[i].upgradeData.AmountLevels = MaxLevel;
                    tradeDatas[i].upgradeData.CurrentLevel = _upgrades.tradeRegions.Contains(playerCountry.ID) ? MaxLevel : 0;
                    tradeDatas[i].countryID = playerCountry.ID;

                    countryIndex++;
                    i++;
                }
            }

            SetDataToList(
                tradeDatas,
                BuyTrade, _upgrades.BuyTradeForAds, false);
        }

        private void BuyTrade(int countryID, int amount, float cost)
        {
            var tutor = gui.TutorContainer;
            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.TradeTutorID)
                tutor.CompleteTutorial();

            _upgrades.BuyForRegionTrade(countryID, cost);

            TryShowTutorial();
        }

        public void OnCloseClick() => gui.ShowMainScreen();

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.TradeTutorID))
            {
                focusTran = CreatedElements[0].BuyOnceTran;
                tutor.TryShowInUI(TutorContainer.TradeTutorID, focusTran, true);
            }
            else if (!tutor.IsComplete(TutorContainer.TradeCloseTutorID))
            {
                focusTran = closeButton.transform;
                tutor.TryShowInUI(TutorContainer.TradeCloseTutorID, focusTran);
            }
        }

        #region List

        [Header("List")]
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private TradeUpgradeUiElement _listElement;

        private List<TradeUpgradeUiElement> _createdElements = new();
        public List<TradeUpgradeUiElement> CreatedElements => _createdElements;

        public RectTransform Container => _contentTransform;

        public void SetDataToList(
            TradeData[] list,
            Action<int, int, float> buyAction,
            Action<int, int> buyForAdAction,
            bool tutorialState)
        {
            if (_createdElements == null) _createdElements = new();

            int maxValue = Math.Max(list.Length, _createdElements.Count);
            for (int i = 0; i < maxValue; i++)
            {
                if (i < list.Length)
                {
                    var data = list[i];

                    bool lockForTutor = tutorialState && i != 0;
                    //int countryID = countries._playerCountries[i].LocalCountryData.Id;

                    if (i < _createdElements.Count) _createdElements[i].Init(i, list[i].countryID, list[i].upgradeData, buyAction, buyForAdAction, lockForTutor);
                    else
                    {
                        var element = Instantiate(_listElement, Vector3.zero, Quaternion.identity, _contentTransform);
                        element.transform.localScale = Vector3.one;
                        element.Init(i, list[i].countryID, list[i].upgradeData, buyAction, buyForAdAction, lockForTutor);

                        element.transform.localPosition = Vector3.zero;

                        _createdElements.Add(element);
                    }

                    _createdElements[i].gameObject.SetActive(true);
                }
                else if (i < _createdElements.Count) _createdElements[i].gameObject.SetActive(false);
            }
        }

        public void UpdateAvailableToBuy(float money)
        {
            for (int i = 0; i < _createdElements.Count; i++)
            {
                _createdElements[i].UpdateAmountAvailableForBuy(money);
            }
        }

        #endregion

        #region React

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue) {}

        public void OnBuyTrade(int countryID)
        {
            UpdateVisual();
        }

        public void OnWonderBuilded(int countryID) {}

        public void OnRocketBuy(int totalRocketsCount) {}

        #endregion
    }

    public struct TradeData
    {
        public int countryID;
        public UpgradeData upgradeData;
    }
}