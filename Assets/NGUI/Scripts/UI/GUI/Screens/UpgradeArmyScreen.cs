using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using TMPro;
using System;
using System.Collections.Generic;

namespace TheSTAR.GUI.Screens
{
    public delegate bool GetCanBuyForAdDelegate();

    public class UpgradeArmyScreen : GuiScreen, IUpgradeReactable, ITutorialStarter, ITransactionReactable
    {
        [SerializeField] private StarsProgress stars;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI forceText;
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton previousBtn;
        [SerializeField] private PointerButton nextBtn;
        [SerializeField] private bool forceLockNavigationButtons = false;
        [SerializeField] private BattleConfig battleConfig;
        [SerializeField] private bool inBattle;

        protected UpgradeData[] _upgradeDatas;
        protected string _titleKey;
        protected int _countryID;

        private GuiController gui;
        private UpgradeService _upgrades;
        private UpgradeType upgradeType;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            _upgrades = cts.Get<UpgradeService>();

            closeButton.Init(OnCloseButtonClick);
            previousBtn.Init(GoToPreviousScreen);
            nextBtn.Init(GoToNextScreen);
        }

        protected override void OnShow()
        {
            base.OnShow();
            UpdateVisual();

            TryShowTutorial();

            var countryData = CountrySaveLoad.LoadCountry(_countryID);
            bool needShowNavigationButtons = !forceLockNavigationButtons && countryData.CountryData.Owner == CommonData.PlayerID;

            previousBtn.gameObject.SetActive(needShowNavigationButtons);
            nextBtn.gameObject.SetActive(needShowNavigationButtons);
        }

        protected override void OnHide()
        {
            base.OnHide();

            gui.TutorContainer.BreakTutorial();
        }

        #region React

        public void OnWonderBuilded(int countryID)
        {
        }

        public void ChangeMoneyPerSecondReact(float valueFerSecond) { }

        public void TransactionReact(CurrencyType currency, float value)
        {
            if (currency == CurrencyType.Money)
            {
                for (int i = 0; i < _createdElements.Count; i++)
                {
                    _createdElements[i].UpdateAmountAvailableForBuy(value);
                }
            }

            rocketPanel.UpdateAmountAvailableForBuy(value);
        }

        public void IncomeIncreaseTick(TimeSpan timeLeft) { }

        public void EndIncomeIncrease() { }

        public void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
        {
            if (countryID != this._countryID || upgradeType != this.upgradeType) return;

            _createdElements[upgradeID].SetCurrentValue(finalValue);
            _upgradeDatas[upgradeID].CurrentLevel = finalValue;

            if (upgradeType == UpgradeType.Army)
            {
                bool autoShowArmyInfo = false;
                var countryData = CountrySaveLoad.LoadCountry(_countryID);
                int force = countryData.GetArmyUpgradeLevels();
                forceText.text = $"Force: {force}";

                stars.SetProgress((float)force / BattleInGameService.ArmyMaxForce);

                var tutor = gui.TutorContainer;
                if (tutor.InTutorial)
                {
                    if (tutor.CurrentTutorialID == TutorContainer.ArmyTutorID && upgradeID == 0)
                    {
                        if (finalValue == 1)
                        {
                            OnOpenArmyInfoClick(0);
                            autoShowArmyInfo = true;
                        }
                        else if (finalValue == 3)
                        {
                            tutor.CompleteTutorial();
                        }
                    }
                    else if (tutor.CurrentTutorialID == TutorContainer.GetUnitInBattlePrepareTutorID && upgradeID == 1)
                    {
                        if (finalValue == 3) tutor.CompleteTutorial();
                    }
                }

                UpdateLockForCurrentUpgrade(out int newCurrentUpgradeIndex);

                bool changeCurrentUpgradeIndex = newCurrentUpgradeIndex != currentAvailableArmyIndex;
                if (changeCurrentUpgradeIndex) currentAvailableArmyIndex = newCurrentUpgradeIndex;

                if (!autoShowArmyInfo) TryShowTutorial();

                /*
                if (changeCurrentUpgradeIndex && !tutor.InTutorial && battleConfig.AutoShowArmyInfo)
                {
                    OnOpenArmyInfoClick(currentAvailableArmyIndex);
                }
                */
            }
        }

        public void OnBuyTrade(int countryID) { }

        public void OnRocketBuy(int totalRocketsCount)
        {
            rocketPanel.SetCurrentValue(totalRocketsCount);
            UpdateVisual();
        }

        #endregion

        public void GoToNextScreen() => _upgrades.ShowOtherArmyInfo(true);

        public void GoToPreviousScreen() => _upgrades.ShowOtherArmyInfo(false);

        public void SetData(int countryID, UpgradeType upgradeType, string titleKey, UpgradeData[] upgradeDatas, bool autoUpdateVisual = false)
        {
            _countryID = countryID;
            this.upgradeType = upgradeType;
            _titleKey = titleKey;
            _upgradeDatas = upgradeDatas;

            if (autoUpdateVisual) UpdateVisual();
        }

        private void OnBuyUpgradeClick(UpgradeArmyPanel panel, int count)
        {
            _upgrades.BuyUpgrade(_countryID, upgradeType, panel.ID, count);
        }

        private void OnBuyUpgradeForAdClick(UpgradeArmyPanel panel)
        {
            _upgrades.BuyForAds(_countryID, UpgradeType.Army, panel.ID, 1);
        }

        protected void UpdateVisual()
        {
            bool tutorialState = !gui.TutorContainer.IsComplete(TutorContainer.ArmyTutorID);

            float costMultiplier = 1;
            if (WorldEventService.Instance.CurrentWorldEventContains(UpgradeType.Army)) costMultiplier = WorldEventService.Instance.GetCurrentEventMultiplier();

            // amries
            var armyUpgrades = _upgrades.Upgrades.Get(upgradeType);
            for (int i = 0; i < armyUpgrades.UpgradeDataList.Length; i++)
            {
                _upgradeDatas[i].Description = armyUpgrades.UpgradeDataList[i].Description;
                _upgradeDatas[i].Cost = armyUpgrades.UpgradeDataList[i].Cost * costMultiplier;
                _upgradeDatas[i].AmountLevels = armyUpgrades.UpgradeDataList[i].AmountLevels;
                _upgradeDatas[i].EffectPerLevel = armyUpgrades.UpgradeDataList[i].EffectPerLevel;
                _upgradeDatas[i].Icon = armyUpgrades.UpgradeDataList[i].Icon;
            }

            SetDataToList(ArrayUtility.LimitArraySize(_upgradeDatas, 10), tutorialState, upgradeType == UpgradeType.Army);

            if (upgradeType == UpgradeType.Army)
            {
                rocketPanel.gameObject.SetActive(true);
                // rocket
                var rocketDataFromConfig = battleConfig.RocketData;
                UpgradeData rocketData = new();
                rocketData.Icon = rocketDataFromConfig.Icon;
                rocketData.Cost = rocketDataFromConfig.Cost * costMultiplier;
                rocketData.CurrentLevel = CommonData.RocketsCount;
                rocketData.Description = rocketDataFromConfig.Name;
                rocketPanel.Init(-1, false, rocketData, OnBuyRocketClick, OnBuyRocketForAdClick, OnInfoRocketClick, false, true, GetCanBuyForAd);
            }
            else rocketPanel.gameObject.SetActive(false);

            titleText.text = I2.Loc.LocalizationManager.GetTranslation(_titleKey);

            var countryData = CountrySaveLoad.LoadCountry(_countryID);
            int force = upgradeType == UpgradeType.Army ? countryData.GetArmyUpgradeLevels() : countryData.resistanceLevel;
            forceText.text = $"Force: {force}";

            stars.SetProgress((float)force / (upgradeType == UpgradeType.Army ? (BattleInGameService.ArmyMaxForce) : (BattleInGameService.ResistanceMaxForce)));
        }

        private int currentAvailableArmyIndex = -1;

        private void OnBuyRocketClick(UpgradeArmyPanel panel, int count)
        {
            _upgrades.BuyRocket(panel.FinalCost, count);
            OnBuyRocket();
        }

        private void OnBuyRocketForAdClick(UpgradeArmyPanel panel)
        {
            _upgrades.BuyRocketForAd(1);
            OnBuyRocket();
        }

        private void OnBuyRocket()
        {
            var tutor = gui.TutorContainer;

            if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.BuyRocketTutorID)
            {
                tutor.CompleteTutorial();
                MessageService.Instance.ShowRocketTutorMessage();
            }
        }

        private void OnInfoRocketClick(int id)
        {
            var infoScreen = gui.FindScreen<SoldierSpecificationsScreen>();
            var data = battleConfig.RocketData;
            infoScreen.SetData(data);
            gui.Show(infoScreen);
        }

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.ArmyTutorID))
            {
                focusTran = CreatedElements[0].BuyButton.transform;
                tutor.TryShowInUI(TutorContainer.ArmyTutorID, focusTran, true);
            }
            else if (!tutor.IsComplete(TutorContainer.IntelligenceTutorID))
            {
                focusTran = closeButton.transform;
                tutor.TryShowInUI(TutorContainer.IntelligenceTutorID, focusTran);
            }
            else if (currentAvailableArmyIndex > 0 && !tutor.IsComplete(TutorContainer.ArmyInfo_TutorIDs[currentAvailableArmyIndex]))
            {
                // для снайпера показываем пальцем
                if (currentAvailableArmyIndex == 1)
                {
                    focusTran = CreatedElements[currentAvailableArmyIndex].InfoButton.transform;
                    tutor.TryShowInUI(TutorContainer.ArmyInfo_TutorIDs[currentAvailableArmyIndex], focusTran, true);
                }
                // после снайпера в момент первого анлока форсированно открываем 
                else
                {
                    tutor.CompleteTutorial(TutorContainer.ArmyInfo_TutorIDs[currentAvailableArmyIndex]);
                    OnOpenArmyInfoClick(currentAvailableArmyIndex);
                }
            }
            else if (inBattle && !tutor.IsComplete(TutorContainer.GetUnitInBattlePrepareTutorID) && currentAvailableArmyIndex == 1 && !CreatedElements[1].IsFullUpgraded)
            {
                focusTran = CreatedElements[1].BuyButton.transform;
                tutor.TryShowInUI(TutorContainer.GetUnitInBattlePrepareTutorID, focusTran, true);
            }
            else if (inBattle && !tutor.IsComplete(TutorContainer.BuyRocketTutorID))
            {
                focusTran = rocketPanel.BuyButton.transform;
                tutor.TryShowInUI(TutorContainer.BuyRocketTutorID, focusTran, true);
            }
        }

        public void OnCloseButtonClick() => gui.Exit();

        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private UpgradeArmyPanel _listElement;

        [Space]
        [SerializeField] private UpgradeArmyPanel rocketPanel;

        private List<UpgradeArmyPanel> _createdElements = new();
        public List<UpgradeArmyPanel> CreatedElements => _createdElements;

        public RectTransform Container => _contentTransform;

        public void SetDataToList(
            UpgradeData[] list,
            bool tutorialState,
            bool useCostMultiplier)
        {
            if (_createdElements == null) _createdElements = new();

            int maxValue = Math.Max(list.Length, _createdElements.Count);
            bool currentUpgradeFound = false;

            for (int i = 0; i < maxValue; i++)
            {
                if (i < list.Length)
                {
                    var data = list[i];
                    bool lockForTutor = tutorialState && i != 0;
                    bool lockForCurrentUpgradeFound = this.upgradeType == UpgradeType.Army && currentUpgradeFound;
                    bool anyLock = lockForTutor || lockForCurrentUpgradeFound;

                    if (i >= _createdElements.Count)
                    {
                        var element = Instantiate(_listElement, Vector3.zero, Quaternion.identity, _contentTransform);
                        element.transform.localScale = Vector3.one;
                        element.transform.localPosition = Vector3.zero;
                        _createdElements.Add(element);
                    }

                    _createdElements[i].Init(i, useCostMultiplier, data, OnBuyUpgradeClick, OnBuyUpgradeForAdClick, OnOpenArmyInfoClick, anyLock, false, GetCanBuyForAd);
                    _createdElements[i].gameObject.SetActive(true);

                    if (!currentUpgradeFound)
                    {
                        currentUpgradeFound = !data.IsFullUpgraded;

                        if (currentUpgradeFound) currentAvailableArmyIndex = i;
                    }
                }
                else if (i < _createdElements.Count) _createdElements[i].gameObject.SetActive(false);
            }

            if (!currentUpgradeFound) currentAvailableArmyIndex = -1;
        }

        private void OnOpenArmyInfoClick(int id)
        {
            if (id == -1) return;

            var tutor = gui.TutorContainer;
            if (id < TutorContainer.ArmyInfo_TutorIDs.Length && !tutor.IsComplete(TutorContainer.ArmyInfo_TutorIDs[id]))
            {
                tutor.CompleteTutorial(TutorContainer.ArmyInfo_TutorIDs[id]);
            }

            var infoScreen = gui.FindScreen<SoldierSpecificationsScreen>();
            var data = battleConfig.GetUnitData((Battle.UnitType)id);
            infoScreen.SetData(data);
            gui.Show(infoScreen);
        }

        private void UpdateLockForCurrentUpgrade(out int newCurrentUpgradeIndex)
        {
            newCurrentUpgradeIndex = -1;
            bool currentUpgradeFound = false;

            for (int i = 0; i < _createdElements.Count; i++)
            {
                if (currentUpgradeFound) _createdElements[i].SetForceLock(true);
                else
                {
                    if (_createdElements[i].CurrentLevel == BattleInGameService.ArmyMaxUpgradeLevel) _createdElements[i].SetForceLock(true);
                    else
                    {
                        _createdElements[i].SetForceLock(false);
                        currentUpgradeFound = true;
                        newCurrentUpgradeIndex = i;
                    }
                }
            }
        }

        private bool GetCanBuyForAd()
        {
            return upgradeType == UpgradeType.Army ? _upgrades.CanBuyUpgradeForAd : false;
        }
    }
}