using System;
using UnityEngine;
using UnityEngine.UI;
using FunnyBlox;
using SPSDigital.IAP;
using TheSTAR.Sound;
using TheSTAR.GUI;
using TheSTAR.Utility.Pointer;
using TheSTAR.Utility;
using TMPro;

public class BallCustomization : MonoBehaviour, ITutorialStarter
{
    [SerializeField] private Renderer _skinRenderer;
    [SerializeField] private CountryBallCustomElement[] _customElements;
    [SerializeField] private Image emotionImg;
    [SerializeField] private PointerButton okButton;
    [SerializeField] private PointerButton buyButton;
    [SerializeField] private PointerButton previousHatButton;
    [SerializeField] private PointerButton nextHatButton;
    [SerializeField] private PointerButton previousFaceButton;
    [SerializeField] private PointerButton nextFaceButton;
    [SerializeField] private TextMeshProUGUI costLabel;

    private CustomizationHatType _selectedHatType = CountryBallVisualService.DefautlHat;
    private CustomizationFaceType _selectedFaceType = CountryBallVisualService.DefautlFace;

    private CustomizationHatType tutorHatType = CustomizationHatType.Winterhat;

    private readonly CustomizationHatType _gamePlusHat = CustomizationHatType.Beerhat; // шапка, доступная только в New Game + в версии Б

    private Material _countryMaterial;

    private int _currentCost;

    private CountryBallCustomElement _currentHat = null;

    private GuiController gui;

    public void Init(ControllerStorage cts)
    {
        gui = cts.Get<GuiController>();

        nextHatButton.Init(OnNextHatClick);
        previousHatButton.Init(OnPreviousHatClick);
        nextFaceButton.Init(OnNextFaceClick);
        previousFaceButton.Init(OnPreviousFaceClick);
        okButton.Init(OnOkClick);
        buyButton.Init(OnBuyClick);
    }

    public void Init(Material ballMaterial)
    {
        _countryMaterial = ballMaterial;

        _selectedFaceType = CountryBallVisualService.Instance.GetPlayerFaceType;
        _selectedHatType = CountryBallVisualService.Instance.GetPlayerHatType;

        UpdateVisual();
    }

    public void OnEnable()
    {
        // иначе может отобразиться некорректная информация о покупке
        UpdateBuyButton();
        TryShowTutorial();
    }

    #region Click

    public void OnNextHatClick()
    {
        int tempIndex = (((int)_selectedHatType) + 1);
        if (tempIndex > _customElements.Length) tempIndex = 0;

        if (CommonData.CurrentGameVersionType == GameVersionType.VersionB)
        {
            // если у нас не GamePlus, то пропускаем шапку для GamePlus
            if (tempIndex == (int)_gamePlusHat && !SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_PLUS)) tempIndex++;
        }

        _selectedHatType = (CustomizationHatType)tempIndex;

        UpdateVisual();
    }

    public void OnPreviousHatClick()
    {
        int tempIndex = (((int)_selectedHatType) - 1);
        if (tempIndex < 0) tempIndex = _customElements.Length;

        if (CommonData.CurrentGameVersionType == GameVersionType.VersionB)
        {
            // если у нас не GamePlus, то пропускаем шапку для GamePlus
            if (tempIndex == (int)_gamePlusHat && !SaveManager.Load<bool>(CommonData.PREFSKEY_GAME_PLUS)) tempIndex--;
        }

        _selectedHatType = (CustomizationHatType)tempIndex;

        UpdateVisual();
    }

    public void OnNextFaceClick()
    {
        int tempIndex = (((int)_selectedFaceType) + 1);
        if (tempIndex >= CountryBallVisualService.Instance.FacesCount) tempIndex = 0;
        _selectedFaceType = (CustomizationFaceType)tempIndex;

        UpdateVisual();
    }

    public void OnPreviousFaceClick()
    {
        int tempIndex = (((int)_selectedFaceType) - 1);
        if (tempIndex < 0) tempIndex = CountryBallVisualService.Instance.FacesCount - 1;
        _selectedFaceType = (CustomizationFaceType)tempIndex;

        UpdateVisual();
    }

    public void OnOkClick()
    {
        UseSetting();
    }

    public void OnBuyClick()
    {
        CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, _currentCost, () =>
        {
            CountryBallVisualService.Instance.OnBuyCustomisation(_selectedFaceType, false);
            CountryBallVisualService.Instance.OnBuyCustomisation(_selectedHatType, false);

            SoundController.Instance.PlaySound(SoundType.Purchase);

            UseSetting();

            if (gui.TutorContainer.InTutorial && _selectedHatType == tutorHatType) gui.TutorContainer.CompleteTutorial();
        });
    }

    #endregion

    // использовать созданный образ
    private void UseSetting()
    {
        CountryBallVisualService.Instance.SetCustomisation(_selectedHatType, _selectedFaceType);
        UpdateBuyButton();
    }

    private void UpdateVisual()
    {
        UpdateBuyButton();
        UpdateBallVisual();
        TryShowTutorial();
    }

    private void UpdateBallVisual()
    {
        // material

        _skinRenderer.material = _countryMaterial;

        // hat

        if (_currentHat != null) _currentHat.gameObject.SetActive(false);

        if (_selectedHatType != CountryBallVisualService.DefautlHat)
        {
            _currentHat = Array.Find(_customElements, info => info.CustomizationType == _selectedHatType);
            _currentHat.gameObject.SetActive(true);
        }

        // face

        emotionImg.sprite = CountryBallVisualService.Instance.GetBallSprite(_selectedFaceType, CountryBallEmotionType.Idle, 0);
    }

    private void UpdateBuyButton()
    {
        _currentCost = 0;

        if (!GemShop.IsPremiumActive)
        {
            // hat

            bool needBuyHat;

            if (CommonData.CurrentGameVersionType == GameVersionType.VersionB && _selectedHatType == _gamePlusHat) needBuyHat = false;
            else needBuyHat = !IsHatAvailable(_selectedHatType);

            if (needBuyHat) _currentCost += CommonData.ItemCost;

            // face

            bool needBuyFace = !IsFaceAvailable(_selectedFaceType);
            if (needBuyFace) _currentCost += CommonData.ItemCost;
        }

        if (_currentCost == 0)
        {
            okButton.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(false);
        }
        else
        {
            costLabel.text = _currentCost.ToString();
            okButton.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(true);
        }
    }

    private bool IsHatAvailable(CustomizationHatType hatType)
    {
        return hatType == CountryBallVisualService.DefautlHat || CountryBallVisualService.Instance.HatPurchased(hatType);
    }

    private bool IsFaceAvailable(CustomizationFaceType faceType)
    {
        return faceType == CountryBallVisualService.DefautlFace || CountryBallVisualService.Instance.FacePurchased(faceType);
    }

    public void TryShowTutorial()
    {
        if (!gameObject.activeSelf) return;

        var tutor = gui.TutorContainer;
        Transform focusTran;

        if (CommonData.PlayerCountriesCount >= 5 && !tutor.IsComplete(TutorContainer.BuyCustomHatTutorID))
        {
            bool compareHat = _selectedHatType == tutorHatType;
            bool compareFace = IsFaceAvailable(_selectedFaceType);

            if (compareHat && compareFace)
            {
                if (CurrencyService.Instance.GetCurrencyValue(CurrencyType.Stars) < CommonData.ItemCost)
                {
                    CurrencyService.Instance.AddCurrency(CurrencyType.Stars, CommonData.ItemCost);
                }

                focusTran = buyButton.transform;
                tutor.TryShowInUI(TutorContainer.BuyCustomHatTutorID, focusTran);
            }
            else if (compareHat)
            {
                focusTran = previousFaceButton.transform;
                tutor.TryShowInUI(TutorContainer.BuyCustomHatTutorID, focusTran);
            }
            else
            {
                int allHatsCount = EnumUtility.GetValues<CustomizationHatType>().Length;
                int toNextDistance = (int)tutorHatType - (int)_selectedHatType;
                int toPreviousDistance = allHatsCount - (int)tutorHatType + (int)_selectedHatType;

                if (toNextDistance < toPreviousDistance && ((int)_selectedHatType < (int)tutorHatType)) focusTran = nextHatButton.transform;
                else focusTran = previousHatButton.transform;

                tutor.TryShowInUI(TutorContainer.BuyCustomHatTutorID, focusTran);
            }
        }
    }
}
