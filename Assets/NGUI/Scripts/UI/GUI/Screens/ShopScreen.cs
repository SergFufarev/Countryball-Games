using UnityEngine;
using SPSDigital.IAP;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using System;

namespace TheSTAR.GUI.Screens
{
    public class ShopScreen : GuiScreen, ITransactionReactable, ITutorialStarter
    {
        [SerializeField] private PointerButton backButton;
        [SerializeField] private PointerButton pageMainButton;
        [SerializeField] private PointerButton pagePackButton;
        [SerializeField] private PointerButton pageCustomizationButton;
        [SerializeField] private CountriesData _countriesDataConfig;

        [Space]
        [SerializeField] private GameObject[] _groups = new GameObject[0];

        [Space]
        [SerializeField] private GemShop gemShop;
        [SerializeField] private BallCustomization customization;

        [Space]
        [SerializeField] private CurrencyCounter softCounter;
        [SerializeField] private CurrencyCounter hardCounter;

        public GemShop GemShop => gemShop;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            backButton.Init(OnClickCloseScreen);
            pageMainButton.Init(OnMainGroupClick);
            pagePackButton.Init(OnPackGroupClick);
            pageCustomizationButton.Init(OnSkinGroupClick);

            gemShop.Init(cts);
            customization.Init(cts);
        }

        protected override void OnShow()
        {
            base.OnShow();

            TryShowTutorial();
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (gui.TutorContainer.InTutorial) gui.TutorContainer.BreakTutorial();
        }

        private void Start()
        {
            ShowGroup(0);
            customization.Init(GetPlayerCountryMaterial());
        }

        private Material GetPlayerCountryMaterial()
        {
            return _countriesDataConfig.FlagMaterials[CountrySaveLoad.LoadPlayerBaseCountryID];
        }

        public void PrepareForBuyCurrency() => ShowGroup(0);

        #region Click

        public void OnMainGroupClick() => ShowGroup(0);
        public void OnPackGroupClick() => ShowGroup(1);
        public void OnSkinGroupClick() => ShowGroup(2);
        public void OnClickCloseScreen() => gui.Exit();

        #endregion

        int currentGroupIndex;

        private void ShowGroup(int index)
        {
            for (int i = 0; i < _groups.Length; i++) _groups[i].SetActive(i == index);
            currentGroupIndex = index;

            TryShowTutorial();
        }

        #region React

        public void ChangeMoneyPerSecondReact(float valueFerSecond) {}

        public void TransactionReact(CurrencyType currency, float value)
        {
            switch (currency)
            {
                case CurrencyType.Money:
                    softCounter.SetValue(value);
                    break;

                case CurrencyType.Stars:
                    hardCounter.SetValue(value);
                    break;
            }
        }

        public void IncomeIncreaseTick(TimeSpan timeLeft) {}

        public void EndIncomeIncrease() {}

        #endregion

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (CommonData.PlayerCountriesCount >= 5 && !tutor.IsComplete(TutorContainer.BuyCustomHatTutorID))
            {
                if (currentGroupIndex != 2)
                {
                    focusTran = pageCustomizationButton.transform;
                    tutor.TryShowInUI(TutorContainer.BuyCustomHatTutorID, focusTran);
                }
            }
        }
    }
}