using System;
using UnityEngine;
using UnityEngine.UI;
using FunnyBlox;
using TMPro;

public class TradeUpgradeUiElement : BaseUpgradeUIElement
{
    [SerializeField] private bool zeroCostToFreeTitle; // отображать ли текст Free если цена равна 0
    [SerializeField] private TextMeshProUGUI _amountReadyLabel;
    [SerializeField] private TextMeshProUGUI _captionLabel;
    [SerializeField] private TextMeshProUGUI _costOnceLabel;

    [SerializeField] private Image _progressBar;
    [SerializeField] private Button _buyOnceButton;

    [Space]
    [SerializeField] private GameObject buyContainer;
    [SerializeField] private GameObject completeContainer;
    [SerializeField] private TextMeshProUGUI tradeCompleteTitle;

    public Button BuyOnceButton => _buyOnceButton;
    public override Transform BuyOnceTran => _buyOnceButton.transform;

    private int _id;
    [SerializeField] private int _countryID;
    private int _amountTotal;
    private int _amountReady;
    private int _amountLeft;
    private float _cost;
    private int _amountAvailableForBuy;
    private Action<int, int, float> _buyAction;
    private Action<int, int> _buyForAdsAction;

    private bool _forceLock = false;

    public void Init(int id, int countryID, UpgradeData data, Action<int, int, float> buyAction, Action<int, int> buyForAdAction, bool forceLock)
    {
        _id = id;
        _countryID = countryID;

        _cost = data.Cost;
        _forceLock = forceLock;

        _buyAction = buyAction;
        _buyForAdsAction = buyForAdAction;

        _amountTotal = data.AmountLevels;
        _amountReady = data.CurrentLevel;

        _amountReadyLabel.text = string.Format("{0}/{1}", _amountReady, _amountTotal);

        string countryName = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation(data.Description)) ? data.Description : I2.Loc.LocalizationManager.GetTranslation(data.Description);
        _captionLabel.text = countryName;
        tradeCompleteTitle.text = $"Trade with\n{countryName}";

        if (_cost == 0 && zeroCostToFreeTitle) _costOnceLabel.text = "Free";
        else _costOnceLabel.text = _cost.ToString();

        _progressBar.fillAmount = (float)_amountReady / _amountTotal;

        UpdateAmountAvailableForBuy(CommonData.Money);

        if (_buyOnceButton != null)
        {
            _buyOnceButton.onClick.RemoveAllListeners();
            _buyOnceButton.onClick.AddListener(BuyOnce);
        }
    }

    private float _floatAmount;

    public override void UpdateAmountAvailableForBuy(float money)
    {
        bool fullUpgraded = _amountReady >= _amountTotal;

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

            _amountLeft = _amountTotal - _amountReady;
            _floatAmount = CommonData.Money / _cost;
            _amountAvailableForBuy = _floatAmount < 1f ? 0 : (int)_floatAmount;
        }

        if (_amountAvailableForBuy > _amountLeft) _amountAvailableForBuy = _amountLeft;

        SetButtonsActivity(state);

        buyContainer.SetActive(!fullUpgraded);
        completeContainer.SetActive(fullUpgraded);
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

    public void BuyOnce()
    {
        _buyAction?.Invoke(_countryID, 1, _cost);
    }

    public void BuyMaximal()
    {
        _buyAction?.Invoke(_countryID, _amountAvailableForBuy, _cost * _amountAvailableForBuy);
    }

    public void BuyForAds()
    {
        _buyForAdsAction?.Invoke(_countryID, _amountTotal - _amountReady);
    }
}