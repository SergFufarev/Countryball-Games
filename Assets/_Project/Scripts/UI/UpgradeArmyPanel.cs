using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FunnyBlox;
using TheSTAR.Utility;
using TheSTAR.GUI.Screens;

public class UpgradeArmyPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI currentUpgradeValueText;
    [SerializeField] private TextMeshProUGUI maxUpgradeValueText;
    [SerializeField] private TextMeshProUGUI currentAdUpgradeValueText;
    [SerializeField] private TextMeshProUGUI maxAdUpgradeValueText;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI captionLabel;
    [SerializeField] private Button buyOneButton;
    [SerializeField] private Button buyForAdButton;
    [SerializeField] private TextMeshProUGUI costOneLabel;
    [SerializeField] private bool unlimiteMaxLevel;

    [Space]
    [SerializeField] private Button infoButton;

    public Button BuyButton => buyOneButton;
    public Button InfoButton => infoButton;

    private int _id;
    private float _baseCost;
    private bool _forceLock = false;

    private int _totalLevel;
    private int _currentLevel;
    private int _amountLeft;
    private int _amountAvailableForBuy;
    private float _floatAmount;

    private bool isRocket = false;

    public int ID => _id;

    public float FinalCost => _baseCost * (isRocket ? (CurrentLevel + 1) : 1);

    public int CurrentLevel => _currentLevel;

    private Action<UpgradeArmyPanel, int> _buyAction;
    private Action<UpgradeArmyPanel> _buyForAdAction;

    private GetCanBuyForAdDelegate getCanBuyForAdDelegate;

    [ContextMenu("UpdateAmountAvailableForBuy")]
    public void UpdateAmountAvailableForBuy()
    {
        UpdateAmountAvailableForBuy(CommonData.Money);
    }

    public bool IsFullUpgraded => !unlimiteMaxLevel && _currentLevel >= _totalLevel;

    public void UpdateAmountAvailableForBuy(float money)
    {
        bool fullUpgraded = IsFullUpgraded;

        ButtonActivityState state;

        if (fullUpgraded)
        {
            state = ButtonActivityState.FullCompleted;
            _amountAvailableForBuy = 0;
            _amountLeft = 0;
        }
        else
        {
            state = money >= FinalCost ? ButtonActivityState.AvailableToBuyForGold : ButtonActivityState.AvailableToBuyForAds;

            _amountLeft = _totalLevel - _currentLevel;
            _floatAmount = CommonData.Money / _baseCost;
            _amountAvailableForBuy = _floatAmount < 1f ? 0 : (int)_floatAmount;
        }

        if (_amountAvailableForBuy > _amountLeft) _amountAvailableForBuy = _amountLeft;

        SetButtonsActivity(state);
    }


    public void SetButtonsActivity(ButtonActivityState state)
    {
        if (_forceLock && state != ButtonActivityState.FullCompleted) state = ButtonActivityState.Unavailable;

        if (state == ButtonActivityState.AvailableToBuyForAds && !getCanBuyForAdDelegate())
        {
            state = ButtonActivityState.Unavailable;
        }

        switch (state)
        {
            case ButtonActivityState.AvailableToBuyForAds:
                buyOneButton.gameObject.SetActive(false);
                buyForAdButton.gameObject.SetActive(true);
                break;

            case ButtonActivityState.AvailableToBuyForGold:
                buyOneButton.interactable = true;
                buyOneButton.gameObject.SetActive(true);
                buyForAdButton.gameObject.SetActive(false);
                break;

            case ButtonActivityState.Unavailable:
                buyOneButton.interactable = false;
                buyOneButton.gameObject.SetActive(true);
                buyForAdButton.gameObject.SetActive(false);
                break;

            case ButtonActivityState.FullCompleted:
                buyOneButton.gameObject.SetActive(false);
                buyForAdButton.gameObject.SetActive(false);
                break;
        }
    }

    private UpgradeData _data;
    private bool _useCostMultiplier;

    public void Init(
        int id,
        bool useCostMultiplier,
        UpgradeData data,
        Action<UpgradeArmyPanel, int> buyAction,
        Action<UpgradeArmyPanel> buyForAdAction,
        Action<int> onInfoClickAction,
        bool forceLock,
        bool isRocket,
        GetCanBuyForAdDelegate getCanBuyForAdDelegate)
    {
        this.isRocket = isRocket;
        _data = data;
        _useCostMultiplier = useCostMultiplier;
        _id = id;
        _forceLock = forceLock;
         icon.sprite = data.Icon;
        _buyAction = buyAction;
        _buyForAdAction = buyForAdAction;
        this.getCanBuyForAdDelegate = getCanBuyForAdDelegate;
        _totalLevel = data.AmountLevels;
        _currentLevel = data.CurrentLevel;

        captionLabel.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation(data.Description)) ? data.Description : I2.Loc.LocalizationManager.GetTranslation(data.Description);

        UpdateUI();

        buyOneButton.onClick.RemoveAllListeners();
        buyOneButton.onClick.AddListener(BuyOnce);

        buyForAdButton.onClick.RemoveAllListeners();
        buyForAdButton.onClick.AddListener(BuyForAds);

        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => onInfoClickAction.Invoke(id));
    }

    public void SetForceLock(bool forceLock)
    {
        _forceLock = forceLock;
        UpdateUI();
    }

    public void SetCurrentValue(int currentValue)
    {
        _currentLevel = currentValue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        _baseCost = _data.Cost * (_useCostMultiplier ? BattleInGameService.ArmyUpgradeCostMutliplier(_currentLevel) : 1);
        
        if (maxUpgradeValueText)
        {
            currentUpgradeValueText.text = _currentLevel.ToString();
            currentAdUpgradeValueText.text = _currentLevel.ToString();
            maxUpgradeValueText.text = _totalLevel.ToString();
            maxAdUpgradeValueText.text = _totalLevel.ToString();
        }
        else
        {
            currentUpgradeValueText.text = $"x{_currentLevel}";
        }

        costOneLabel.text = TextUtility.NumericValueToText(FinalCost, NumericTextFormatType.CompactFromK);
        if (progressBar != null) progressBar.fillAmount = (float)_currentLevel / _totalLevel;
        UpdateAmountAvailableForBuy(CommonData.Money);
    }

    #region Buy

    public void BuyOnce()
    {
        _buyAction?.Invoke(this, 1);
    }

    public void BuyForAds()
    {
        _buyForAdAction?.Invoke(this);
    }

    public void BuyMaximal()
    {
        _buyAction?.Invoke(this, _amountAvailableForBuy);
    }

    #endregion
}