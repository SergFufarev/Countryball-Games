using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunnyBlox
{
    public class CurrencyService : MonoSingleton<CurrencyService>, ISaver, IController
    {
        [SerializeField] private CommonConfig _commonSettings;
        [SerializeField] private bool simulateIncome;

        public float WorldEventBonusMultiplier
        {
            get
            {
                float value = 1;
                if (WorldEventService.Instance.CurrentWorldEvent == WorldEventType.WorldEconomicIncrease ||
                    WorldEventService.Instance.CurrentWorldEvent == WorldEventType.WorldEconomicReduce)
                    value = WorldEventService.Instance.GetCurrentEventMultiplier();

                return value;
            }
        }

        private float BoostMultiplier => isIncomeBoost ? 2 : 1;

        private bool isIncomeBoost = false;
        public bool IsIncomeBoost => isIncomeBoost;
        private float waitForSecondTime = 1;
        private DateTime endIncomeBoostTime;
        public DateTime EndIncomeBoostTime => endIncomeBoostTime;

        private List<ITransactionReactable> _trs = new();

        // заработок в секунду с учётом всех бонусов
        public float FinalMoneyPerSecond => finalMoneyPerSecond;
        private float finalMoneyPerSecond;

        [ContextMenu("RecalculateFinalIncome")]
        public void RecalculateFinalIncome()
        {
            finalMoneyPerSecond = CommonData.MoneyPerSecondBase * BoostMultiplier * WorldEventBonusMultiplier;
            ReactChangeMoneyPerSecond(finalMoneyPerSecond);
        }

        private void Start()
        {
            RecalculateFinalIncome();
        }

        public void Init(List<ITransactionReactable> trs)
        {
            _trs = trs;
            LoadData();
        }

        void FixedUpdate()
        {
            if (!simulateIncome) return;

            waitForSecondTime -= Time.deltaTime;
            if (waitForSecondTime <= 0)
            {
                AddCurrency(CurrencyType.Money, finalMoneyPerSecond, false);
                waitForSecondTime = 1;

                if (isIncomeBoost)
                {
                    if (DateTime.Now > EndIncomeBoostTime)
                    {
                        isIncomeBoost = false;
                        ReactIncomeBoostEnd();
                    }
                    else ReactIncomeBoostTick(EndIncomeBoostTime - DateTime.Now);
                }
            }
        }

        public void SetMoneyPerSecondValue(float baseIncome)
        {
            CommonData.MoneyPerSecondBase = baseIncome;
            RecalculateFinalIncome();
        }

        public float GetCurrencyValue(CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Money: return CommonData.Money;
                case CurrencyType.Stars: return CommonData.Stars;
            }

            return 0;
        }

        public void AddCurrency(CurrencyType currencyType, float value, bool autoSave = true)
        {
            switch (currencyType)
            {
                case CurrencyType.Money:
                    CommonData.Money += value;
                    ReactCurrency(currencyType, CommonData.Money);
                    break;

                case CurrencyType.Stars:
                    CommonData.Stars += value;
                    ReactCurrency(currencyType, CommonData.Stars);
                    break;
            }

            if (autoSave) SaveData();
        }

        public void ReduceCurrency(CurrencyType currencyType, float value, Action completeAction, Action failAction = null, bool autoSave = true)
        {
            switch (currencyType)
            {
                case CurrencyType.Money:
                    if (CommonData.Money >= value)
                    {
                        CommonData.Money -= value;
                        completeAction?.Invoke();
                        ReactCurrency(currencyType, CommonData.Money);
                    }
                    else failAction?.Invoke();
                    break;

                case CurrencyType.Stars:
                    if (CommonData.Stars >= value)
                    {
                        CommonData.Stars -= value;
                        completeAction?.Invoke();
                        ReactCurrency(currencyType, CommonData.Stars);
                    }
                    else failAction?.Invoke();
                    break;
            }

            if (autoSave) SaveData();
        }

        private void LoadIncomeBoost()
        {
            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME))
            {
                endIncomeBoostTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME);

                if (DateTime.Now < endIncomeBoostTime)
                {
                    //if (_incomeBoostCoroutine != null) StopCoroutine(_incomeBoostCoroutine);

                    //_incomeBoostCoroutine = StartCoroutine(IncomeBoostCor((int)(endIncomeBoostTime - DateTime.Now).TotalSeconds));
                    isIncomeBoost = true;
                }
            }
        }

        public void SetIncomeBoost(int timeSeconds)
        {
            DateTime finalTime;

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME))
            {
                DateTime currentEndTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME);

                if (DateTime.Now > currentEndTime)
                {
                    // буст уже заканчивался, просто начинаем новый
                    //_incomeBoostCoroutine = StartCoroutine(IncomeBoostCor(timeSeconds));

                    finalTime = DateTime.Now.AddSeconds(timeSeconds);
                }
                else
                {
                    // буст ещё не заканчивался, прибавляем к существующему
                    //if (_incomeBoostCoroutine != null) StopCoroutine(_incomeBoostCoroutine);
                    currentEndTime = currentEndTime.AddSeconds(timeSeconds);
                    //_incomeBoostCoroutine = StartCoroutine(IncomeBoostCor((int)(currentEndTime - DateTime.Now).TotalSeconds));

                    finalTime = DateTime.Now.AddSeconds((int)(currentEndTime - DateTime.Now).TotalSeconds);
                }
            }
            else
            {
                //_incomeBoostCoroutine = StartCoroutine(IncomeBoostCor(timeSeconds));
                finalTime = DateTime.Now.AddSeconds(timeSeconds);
            }

            isIncomeBoost = true;
            SaveManager.Save(CommonData.PREFSKEY_INCOME_BOOSTER_END_TIME, finalTime);
            endIncomeBoostTime = finalTime;

            RecalculateFinalIncome();
            ReactChangeMoneyPerSecond(finalMoneyPerSecond);
        }

        [ContextMenu("TestGiveMoney")]
        private void TestGiveMoney()
        {
            CommonData.Money += 1000000;
        }

        private void ReactChangeMoneyPerSecond(float value)
        {
            foreach (var tr in _trs) tr.ChangeMoneyPerSecondReact(value);
        }

        private void ReactCurrency(CurrencyType currency, float value)
        {
            foreach (var tr in _trs) tr.TransactionReact(currency, value);
        }

        private void ReactIncomeBoostTick(TimeSpan timeLeft)
        {
            foreach (var tr in _trs) tr.IncomeIncreaseTick(timeLeft);
        }

        private void ReactIncomeBoostEnd()
        {
            foreach (var tr in _trs) tr.EndIncomeIncrease();

            ReactChangeMoneyPerSecond(finalMoneyPerSecond);
        }

        #region SaveLoad

        public void SaveData()
        {
            if (CommonData.Money != 0) SaveManager.Save(CommonData.PREFSKEY_MONEY, CommonData.Money);
            if (CommonData.MoneyPerSecondBase != 0) SaveManager.Save(CommonData.PREFSKEY_MONEY_PER_SECOND, CommonData.MoneyPerSecondBase);

            SaveManager.Save(CommonData.PREFSKEY_STARS, CommonData.Stars);
        }

        public void LoadData()
        {
            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_MONEY))
            {
                CommonData.Money = SaveManager.Load<float>(CommonData.PREFSKEY_MONEY);
                CommonData.Stars = SaveManager.Load<float>(CommonData.PREFSKEY_STARS);

                CommonData.MoneyPerSecondBase = SaveManager.Load<float>(CommonData.PREFSKEY_MONEY_PER_SECOND);
            }
            else
            {
                CommonData.Money = _commonSettings.AmountMoneyOnStart;
                CommonData.Stars = _commonSettings.AmountStarsOnStart;

                CommonData.MoneyPerSecondBase = 0;

                SaveData();
            }

            LoadIncomeBoost();

            ReactChangeMoneyPerSecond(finalMoneyPerSecond);
            ReactCurrency(CurrencyType.Money, CommonData.Money);
            ReactCurrency(CurrencyType.Stars, CommonData.Stars);
        }

        #endregion

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }

    public enum CurrencyType
    {
        Money,
        Stars
    }

    public interface ITransactionReactable
    {
        // изменён заработок в секунду на valueFerSecond
        void ChangeMoneyPerSecondReact(float valuePerSecond);

        // произошла транзакция, в результате которой currency стало равно value
        void TransactionReact(CurrencyType currency, float value);

        // обновление кд для ускорения заработка
        void IncomeIncreaseTick(TimeSpan timeLeft);

        // бонус ускорения заработка закончен
        void EndIncomeIncrease();
    }
}