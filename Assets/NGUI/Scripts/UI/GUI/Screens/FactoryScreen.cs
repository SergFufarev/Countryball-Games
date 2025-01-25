using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using TMPro;
using System;
using System.Collections.Generic;

namespace TheSTAR.GUI.Screens
{
    public class FactoryScreen : GuiScreen, ITutorialStarter, ITransactionReactable, IUpgradeReactable
    {
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton previousButton;
        [SerializeField] private PointerButton nextButton;

        private UpgradeData[] _upgradeDatas;
        private UpgradeData _wonderData;
        private int _countryID;
        private Country _country;

        protected GuiController gui;
        private UpgradeService _upgrades;
        private VisualCountryController countries;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            closeButton.Init(OnCloseClick);
            previousButton.Init(OnPreviousButtonClick);
            nextButton.Init(OnNextButtonClick);

            gui = cts.Get<GuiController>();
            _upgrades = cts.Get<UpgradeService>();
            countries = cts.Get<VisualCountryController>();
        }

        protected override void OnShow()
        {
            base.OnShow();

            //UpdateVisual();
            TryShowTutorial();
        }

        protected override void OnHide()
        {
            base.OnHide();

            var tutor = gui.TutorContainer;

            tutor.BreakTutorial();
        }

        public void SetData(int countryID, UpgradeData[] upgradeDatas)
        {
            _countryID = countryID;
            _country = countries.GetCountry(_countryID);
            _upgradeDatas = upgradeDatas;
            _wonderData = _country.GenerateWonderDataForCountry();

            countryName.text = _country.LocalCountryData.Name;

            UpdateVisual();
        }

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.FactoryTutorID))
            {
                focusTran = CreatedDefaultUpgrades[0].BuyOnceTran;
                tutor.TryShowInUI(TutorContainer.FactoryTutorID, focusTran, true);
            }
            else if (!tutor.IsComplete(TutorContainer.ArmyTutorID))
            {
                focusTran = closeButton.transform;
                tutor.TryShowInUI(TutorContainer.ArmyTutorID, focusTran);
            }
            else if (!tutor.IsComplete(TutorContainer.NewFactoryTutorID))
            {
                var countryData = CountrySaveLoad.LoadCountry(_countryID).CountryData;
                if (countryData == null || countryData.IsBaseCountry) return;

                focusTran = CreatedDefaultUpgrades[0].BuyOnceTran;
                tutor.TryShowInUI(TutorContainer.NewFactoryTutorID, focusTran, true);
            }
            else if (tutor.IsComplete(TutorContainer.NewFactoryTutorID) && !tutor.IsComplete(TutorContainer.ResistanceTutorID))
            {
                focusTran = closeButton.transform;
                tutor.TryShowInUI(TutorContainer.ResistanceTutorID, focusTran);
            }
        }

        protected void UpdateVisual()
        {
            int limit = _country.LocalCountryData.IsBaseCountry ? Country.FullFactoriesCount : _country.LocalCountryData.Factories;
            bool tutorialState = !gui.TutorContainer.IsComplete(TutorContainer.FactoryTutorID);
            bool trade = _upgrades.tradeRegions.Contains(_countryID);
            float tradeEffectMultiplier = trade ? _upgrades.TradeEffectMultiplier : 1;
            var factoryUpgrades = _upgrades.Upgrades.Get(UpgradeType.Economics);

            //float costMultiplier = 1;
            //if (WorldEventService.Instance.CurrentWorldEventContains(UpgradeType.Economics)) costMultiplier = WorldEventService.Instance.GetCurrentEventMultiplier();

            for (int i = 0; i < _upgradeDatas.Length; i++)
            {
                _upgradeDatas[i].Description = factoryUpgrades.UpgradeDataList[i].Description;
                _upgradeDatas[i].Cost = factoryUpgrades.UpgradeDataList[i].Cost;
                _upgradeDatas[i].AmountLevels = factoryUpgrades.UpgradeDataList[i].AmountLevels;
                _upgradeDatas[i].EffectPerLevel = factoryUpgrades.UpgradeDataList[i].EffectPerLevel * tradeEffectMultiplier;
                _upgradeDatas[i].Icon = factoryUpgrades.UpgradeDataList[i].Icon;
            }

            // set data to visual
            SetDataToList(
                ArrayUtility.LimitArraySize(_upgradeDatas, limit),
                OnBuyClick,
                tutorialState);
        }

        private void OnBuyClick(FactoryUpgradeUIElement upgradeElement)
        {
            if (upgradeElement.ID == -1) _upgrades.BuyWonder(_countryID);
            else _upgrades.BuyUpgrade(_countryID, UpgradeType.Economics, upgradeElement.ID, 1);
        }

        #region List

        [Header("List")]
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private FactoryUpgradeUIElement _listElement;
        [SerializeField] private FactoryUpgradeUIElement wonderListElement;

        private List<FactoryUpgradeUIElement> _createdDefaultUpgrades = new();
        public List<FactoryUpgradeUIElement> CreatedDefaultUpgrades => _createdDefaultUpgrades;

        public RectTransform Container => _contentTransform;

        public void SetDataToList(
            UpgradeData[] list,
            Action<FactoryUpgradeUIElement> buyAction,
            bool tutorialState)
        {
            // defaults

            if (_createdDefaultUpgrades == null) _createdDefaultUpgrades = new();

            int maxValue = Math.Max(list.Length, _createdDefaultUpgrades.Count);
            for (int i = 0; i < maxValue; i++)
            {
                if (i < list.Length)
                {
                    var data = list[i];

                    bool lockForTutor = tutorialState && i != 0;

                    if (i < _createdDefaultUpgrades.Count) _createdDefaultUpgrades[i].Init(i, data, buyAction, lockForTutor);
                    else
                    {
                        var element = Instantiate(_listElement, Vector3.zero, Quaternion.identity, _contentTransform);
                        element.transform.localScale = Vector3.one;
                        element.Init(i, data, buyAction, lockForTutor);
                        element.transform.localPosition = Vector3.zero;

                        _createdDefaultUpgrades.Add(element);

                        wonderListElement.transform.SetAsLastSibling();
                    }

                    _createdDefaultUpgrades[i].gameObject.SetActive(true);
                }
                else if (i < _createdDefaultUpgrades.Count) _createdDefaultUpgrades[i].gameObject.SetActive(false);
            }

            // wonder

            _wonderData.CurrentLevel = _country.WonderAlreadyBuilded ? 1 : 0;
            _wonderData.EffectPerLevel = 5 * (_upgrades.tradeRegions.Contains(_countryID) ? _upgrades.TradeEffectMultiplier : 1);
            wonderListElement.Init(-1, _wonderData, buyAction, false);
        }

        public void UpdateAvailableToBuy(float money)
        {
            for (int i = 0; i < _createdDefaultUpgrades.Count; i++)
            {
                _createdDefaultUpgrades[i].UpdateAmountAvailableForBuy(money);
            }
        }

        #endregion

        #region Click

        public virtual void OnCloseClick() => gui.Exit();
        public void OnPreviousButtonClick() => _upgrades.ShowOtherArmyInfo(false);
        public void OnNextButtonClick() => _upgrades.ShowOtherArmyInfo(true);

        #endregion

        #region Reacts

        public void ChangeMoneyPerSecondReact(float valueFerSecond)
        {
        }

        public void TransactionReact(CurrencyType currency, float value)
        {
            if (currency == CurrencyType.Money) UpdateAvailableToBuy(value);
        }

        public void IncomeIncreaseTick(TimeSpan timeLeft)
        {
        }

        public void EndIncomeIncrease()
        {
        }

        public void OnWonderBuilded(int countryID)
        {
            if (this._countryID != countryID) return;

            wonderListElement.SetCurrentValue(1);
        }

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
        {
            if (this._countryID != countryID || upgradeType != UpgradeType.Economics) return;

            // wonder
            if (upgradeID == 4)
            {
                wonderListElement.SetCurrentValue(1);
            }
            else
            {
                _createdDefaultUpgrades[upgradeID].SetCurrentValue(finalValue);

                if (gui.TutorContainer.InTutorial &&
                    (gui.TutorContainer.CurrentTutorialID == TutorContainer.FactoryTutorID || gui.TutorContainer.CurrentTutorialID == TutorContainer.NewFactoryTutorID) &&
                    finalValue == 5)
                {
                    gui.TutorContainer.CompleteTutorial();
                    BreakForceLock();
                    TryShowTutorial();
                }
            }
        }

        private void BreakForceLock()
        {
            for (int i = 0; i < _createdDefaultUpgrades.Count; i++) _createdDefaultUpgrades[i].BreakForceLock();
        }

        public void OnBuyTrade(int countryID) {}

        public void OnRocketBuy(int totalRocketsCount) {}

        #endregion
    }
}