using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FunnyBlox;
using System;
using TMPro;
using TheSTAR.Utility;

public class FactoryUpgradeUIElement : BaseUpgradeUIElement
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountReadyLabel;
    [SerializeField] private TextMeshProUGUI _captionLabel;
    [SerializeField] private TextMeshProUGUI _costOnceLabel;
    [SerializeField] private TextMeshProUGUI _effectLabel;
    [SerializeField] private Image _progressBar;
    [SerializeField] private Button _buyOnceButton;

    public Button BuyOnceButton => _buyOnceButton;
    public override Transform BuyOnceTran => _buyOnceButton.transform;

    private int _id;
    private int _totalLevels;
    private int _currentLevel;
    private int _amountLeft;
    private float _cost;
    private int _amountAvailableForBuy;
    private Action<FactoryUpgradeUIElement> _buyClickAction;

    public int ID => _id;

    private bool _forceLock = false;

    private UpgradeData _data;

    public void Init(int id, UpgradeData data, Action<FactoryUpgradeUIElement> buyAction, bool forceLock)
    {
        _data = data;

        _id = id;

        _cost = data.Cost;
        _forceLock = forceLock;

        _buyClickAction = buyAction;

        _totalLevels = data.AmountLevels;
        _currentLevel = data.CurrentLevel;

        if (_icon != null) _icon.sprite = data.Icon;

        _captionLabel.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation(data.Description)) ? data.Description : I2.Loc.LocalizationManager.GetTranslation(data.Description);

        if (_effectLabel != null) _effectLabel.text = $"Add gold/sec: {data.EffectPerLevel}";

        UpdateUI();

        if (_buyOnceButton != null)
        {
            _buyOnceButton.onClick.RemoveAllListeners();
            _buyOnceButton.onClick.AddListener(BuyOnceClick);
        }
    }

    public void SetCurrentValue(int value)
    {
        _currentLevel = value;
        UpdateUI();
    }

    private void UpdateUI()
    {
        _cost = _data.Cost;
        _amountReadyLabel.text = $"{_currentLevel}/{_totalLevels}";
        _costOnceLabel.text = TextUtility.NumericValueToText(_cost, NumericTextFormatType.CompactFromK);
        _progressBar.fillAmount = (float)_currentLevel / _totalLevels;
        UpdateAmountAvailableForBuy(CommonData.Money);
    }

    private float _floatAmount;

    public override void UpdateAmountAvailableForBuy(float money)
    {
        bool fullUpgraded = _currentLevel >= _totalLevels;

        ButtonActivityState state;

        if (fullUpgraded)
        {
            state = ButtonActivityState.Unavailable;
            _amountAvailableForBuy = 0;
            _amountLeft = 0;
        }
        else
        {
            state = money > _cost ? ButtonActivityState.AvailableToBuyForGold : ButtonActivityState.AvailableToBuyForAds;

            _amountLeft = _totalLevels - _currentLevel;
            _floatAmount = CommonData.Money / _cost;
            _amountAvailableForBuy = _floatAmount < 1f ? 0 : (int)_floatAmount;
        }

        if (_amountAvailableForBuy > _amountLeft) _amountAvailableForBuy = _amountLeft;

        SetButtonsActivity(state);
    }

    public void SetButtonsActivity(ButtonActivityState state)
    {
        if (_forceLock) state = ButtonActivityState.Unavailable;
        if (state == ButtonActivityState.AvailableToBuyForAds) state = ButtonActivityState.Unavailable;

        switch (state)
        {
            case ButtonActivityState.AvailableToBuyForGold:
                _buyOnceButton.interactable = true;
                _buyOnceButton.gameObject.SetActive(true);
                break;

            case ButtonActivityState.AvailableToBuyForAds:
                _buyOnceButton.gameObject.SetActive(false);

                break;

            case ButtonActivityState.Unavailable:
                _buyOnceButton.interactable = false;
                break;
        }
    }

    public void BuyOnceClick()
    {
        _buyClickAction?.Invoke(this);
    }

    public void BreakForceLock()
    {
        _forceLock = false;
    }
}