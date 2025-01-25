using UnityEngine;
using FunnyBlox;

public static class CountrySaveLoad
{
    // key
    public static string CountryPrefsKey(int id) => $"Country_{id}";

    // has
    public static bool HasPlayerBaseCountyID => PlayerPrefs.HasKey(CommonData.PREFSKEY_BASE_COINTRY_ID);
    public static bool HasCountriesCount => PlayerPrefs.HasKey(CommonData.PREFSKEY_COUNTRIES_DATA);
    public static bool HasCountry(int id) => PlayerPrefs.HasKey(CountryPrefsKey(id)) && LoadCountry(id) != null;

    // save
    public static void SavePlayerBaseCountryID(int id) => SaveManager.Save(CommonData.PREFSKEY_BASE_COINTRY_ID, id);
    public static void SaveCountriesCount(int count) => SaveManager.Save(CommonData.PREFSKEY_COUNTRIES_DATA, count);
    public static void SaveCountry(int id, CountryCollectedData data)
    {
        SaveManager.Save(CountryPrefsKey(id), data);
    }

    // load
    public static int LoadPlayerBaseCountryID => SaveManager.Load<int>(CommonData.PREFSKEY_BASE_COINTRY_ID);
    public static int LoadCountriesCount => SaveManager.Load<int>(CommonData.PREFSKEY_COUNTRIES_DATA);
    public static CountryCollectedData LoadCountry(int id) => SaveManager.Load<CountryCollectedData>(CountryPrefsKey(id));

    // delete
    public static void DeleteBaseCountryID() => PlayerPrefs.DeleteKey(CommonData.PREFSKEY_BASE_COINTRY_ID);
    public static void DeleteCountriesCount() => PlayerPrefs.DeleteKey(CommonData.PREFSKEY_COUNTRIES_DATA);
    public static void DeleteCountry(int id) => PlayerPrefs.DeleteKey(CountryPrefsKey(id));
}