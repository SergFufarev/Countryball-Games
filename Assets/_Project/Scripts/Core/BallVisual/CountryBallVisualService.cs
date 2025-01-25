using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using SPSDigital.IAP;

public class CountryBallVisualService : MonoSingleton<CountryBallVisualService>, ISaver
{
    [SerializeField] private CountryBallVisualConfig visualConfig;
    [SerializeField] public CountriesData _countriesDataConfig;

    public Sprite GetBallSprite(CustomizationFaceType faceType, CountryBallEmotionType emotionType, int index)
    {
        try
        {
            return visualConfig.Kits[(int)faceType].emotions[(int)emotionType].Sprites[index];
        }
        catch
        {
            Debug.LogError($"Error: try get sprite for kit {(int)faceType}, emotion {(int)emotionType}, sprite {index}");
            return null;
        }
    }

    public int FacesCount => visualConfig.Kits.Length;

    public CustomizationFaceType GetPlayerFaceType => _currentFaceType;
    public CustomizationHatType GetPlayerHatType => _currentHatType;

    private CustomizationHatType _currentHatType = DefautlHat;
    private CustomizationFaceType _currentFaceType = DefautlFace;

    private List<CustomizationHatType> purchasedHats;
    private List<CustomizationFaceType> purchasedFaces;

    public const CustomizationHatType DefautlHat = CustomizationHatType.None;
    public const CustomizationFaceType DefautlFace = CustomizationFaceType.Cute;

    public bool HatPurchased(CustomizationHatType hatType) => purchasedHats.Contains(hatType);
    public bool FacePurchased(CustomizationFaceType faceType) => purchasedFaces.Contains(faceType);

    public void InitService()
    {
        LoadData();
    }

    public CountryBallRoleElement GetCountryBallRolePrefab(CountryBallType countryBallType) => visualConfig.Roles[(int)countryBallType];
    public CountryBallCustomElement GetCountryBallCustomizationPrefab(CustomizationHatType customizationHatType) => visualConfig.Customs[(int)customizationHatType];

    public void SetCustomisation(CustomizationHatType hat, CustomizationFaceType face)
    {
        _currentHatType = hat;
        _currentFaceType = face;

        OnSetCustomisationEvent?.Invoke();

        SaveData();
    }

    public event Action OnSetCustomisationEvent;

    public void OnBuyCustomisation(CustomizationHatType hat, bool useAutoSave = true)
    {
        if (purchasedHats.Contains(hat)) return;

        purchasedHats.Add(hat);
        if (useAutoSave) SaveData();
    }

    public void OnBuyCustomisation(CustomizationFaceType face, bool useAutoSave = true)
    {
        if (purchasedFaces.Contains(face)) return;

        purchasedFaces.Add(face);
        if (useAutoSave) SaveData();
    }

    public void GetCustomisationByOwner(int owner, out CustomizationHatType hatType, out CustomizationFaceType faceType)
    {
        if (owner == CommonData.PlayerID)
        {
            hatType = GetPlayerHatType;
            faceType = GetPlayerFaceType;
        }
        else
        {
            hatType = _countriesDataConfig.CountryData[owner].hatType;
            faceType = _countriesDataConfig.CountryData[owner].faceType;
        }
    }

    public CountryBallEmotionType[] GetAvailableEmotionsForFaceType(CustomizationFaceType faceType)
    {
        return visualConfig.GetAvailableEmotionsForFace(faceType);
    }

    #region Save/Load

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_CURRENT_HAT_TYPE, _currentHatType);
        SaveManager.Save(CommonData.PREFSKEY_CURRENT_FACE_TYPE, _currentFaceType);
        SaveManager.Save(CommonData.PREFSKEY_PURCHASED_HATS, purchasedHats);
        SaveManager.Save(CommonData.PREFSKEY_PURCHASED_FACES, purchasedFaces);
    }

    public void LoadData()
    {
        Debug.Log("Load CountryBallVisualService");

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_CURRENT_HAT_TYPE))
        {
            _currentHatType = SaveManager.Load<CustomizationHatType>(CommonData.PREFSKEY_CURRENT_HAT_TYPE);
            _currentFaceType = SaveManager.Load<CustomizationFaceType>(CommonData.PREFSKEY_CURRENT_FACE_TYPE);
        }
        else
        {
            _currentHatType = DefautlHat;
            _currentFaceType = DefautlFace;
        }

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_PURCHASED_HATS)) purchasedHats = SaveManager.Load<List<CustomizationHatType>>(CommonData.PREFSKEY_PURCHASED_HATS);
        else purchasedHats = new List<CustomizationHatType>();

        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_PURCHASED_FACES)) purchasedFaces = SaveManager.Load<List<CustomizationFaceType>>(CommonData.PREFSKEY_PURCHASED_FACES);
        else purchasedFaces = new List<CustomizationFaceType>();

        // Проверка:
        // После наката могли произойти изменения по кастомкам, после чего могла стать недоступна надетая на игрока кастомка
        // В таком случае надеваем дефолтную

        if (GemShop.IsPremiumActive) return;
        if (!HatPurchased(_currentHatType)) _currentHatType = DefautlHat;
        if (!FacePurchased(_currentFaceType)) _currentFaceType = DefautlFace;
    }

    #endregion
}

public enum CustomizationHatType
{
    None,
    Beerhat,
    Bowlerhat,
    Cap,
    Capcolonel,
    Catears,
    Cowboy,
    Crown,
    Devil,
    Egyptian,
    Fez,
    Flower,
    Hood,
    Hunterhat,
    Japanhat,
    Mexicohat,
    Mohawk,
    Moustache,
    Pharaoh,
    Pirate,
    Santa,
    Ushanka,
    Viking,
    Winterhat = 23,
    Witch
}

public enum CustomizationFaceType
{
    Basic,
    Cute,
    Crazy,
    Glasses,
    New
}