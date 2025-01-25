using UnityEngine;
using FunnyBlox;
using TheSTAR.GUI.Screens;
using TheSTAR.Utility.Pointer;
using TMPro;
using System;
using Sirenix.OdinInspector;

namespace TheSTAR.GUI.UniversalElements
{
    public class IncomeContainer : GuiUniversalElement, ITransactionReactable
    {
        [SerializeField] private GameObject fullUIContainer;

        [Space]
        [SerializeField] private CurrencyCounter softCounter;
        [SerializeField] private CurrencyCounter hardCounter;
        [SerializeField] private GameObject moneyPerSecondContainer;
        [SerializeField] private TextMeshProUGUI moneyPerSecondCounterText;
        [SerializeField] private GameObject incomeBoostIndicator;
        [SerializeField] private GameObject incomeIncreaseContainer;
        [SerializeField] private PointerButton incomeIncreaseButton;
        [SerializeField] private GameObject incomeIncreaseLockContainer;
        [SerializeField] private TimeText incomeIncreaseLockCountdown;

        [Space]
        [SerializeField] private bool useSoftMessage;
        [SerializeField] [ShowIf("useSoftMessage")] private AddCurrencyMessage addSoftMessage;

        private GuiController gui;
        private CurrencyService currency;

        public RectTransform EndFlyCoinPos => softCounter.EndFlyPos;

        private bool isIncomeBoostActive;

        private DateTime incomeBoosterEndTime;

        private bool CanGoToShopScreen => gui.CurrentScreen is not BattleScreen;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            currency = cts.Get<CurrencyService>();

            softCounter.Init(() =>
            {
                if (CanGoToShopScreen) gui.Show<ShopScreen>();
            });

            hardCounter.Init(() =>
            {
                if (CanGoToShopScreen) gui.Show<ShopScreen>();
            });

            incomeIncreaseButton.Init(OnIncomeSpeedUpClick);
        }

        public void InitVisual(bool useSoft, bool useHard, bool useSoftPerSecond, bool useIncomeIncrease)
        {
            softCounter.gameObject.SetActive(useSoft);
            hardCounter.gameObject.SetActive(useHard);
            moneyPerSecondContainer.SetActive(useSoftPerSecond);
            incomeIncreaseContainer.SetActive(useIncomeIncrease);

            isIncomeBoostActive = currency.IsIncomeBoost;
            incomeBoosterEndTime = currency.EndIncomeBoostTime;

            incomeIncreaseButton.gameObject.SetActive(!isIncomeBoostActive);
            incomeIncreaseLockContainer.SetActive(isIncomeBoostActive);
            incomeBoostIndicator.SetActive(isIncomeBoostActive);

            IncomeIncreaseTick(incomeBoosterEndTime - DateTime.Now);
        }

        public void OnIncomeSpeedUpClick() => gui.Show<IncomeAccelerationScreen>();

        #region Reacts

        public void ChangeMoneyPerSecondReact(float valuePerSecond)
        {
            valuePerSecond = ((int)(valuePerSecond * 10)) / 10f;
            moneyPerSecondCounterText.text = valuePerSecond.ToString();
        }

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

        public void IncomeIncreaseTick(TimeSpan timeLeft)
        {
            incomeIncreaseLockCountdown.SetValue(timeLeft);
        }

        public void EndIncomeIncrease()
        {
            incomeIncreaseButton.gameObject.SetActive(true);
            incomeIncreaseLockContainer.SetActive(false);
            incomeBoostIndicator.SetActive(false);
        }

        #endregion Reacts

        public void ShowFullUI(bool full)
        {
            fullUIContainer.SetActive(full);
        }

        [ContextMenu("TestSoftMessage")]
        private void TestSoftMessage() => MessageAddSoft(580);

        private void MessageAddSoft(float value)
        {
            if (useSoftMessage) addSoftMessage.Message(value);
        }

        public void OnCompleteFlyCurrency(CurrencyType currencyType, float value)
        {
            if (currencyType == CurrencyType.Money)
            {
                MessageAddSoft(value);
                softCounter.AnimateIncome();
            }
        }
    }
}