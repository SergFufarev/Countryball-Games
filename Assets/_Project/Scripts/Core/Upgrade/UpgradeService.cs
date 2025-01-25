using UnityEngine;
using System.Collections.Generic;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using FunnyBlox;
using TheSTAR.Utility;
using MAXHelper;

public class UpgradeService : MonoBehaviour, ISaver, IController
{
    [Inject] private readonly GuiController gui;

    private List<IUpgradeReactable> _urs = new();
    public List<int> tradeRegions = new();

    [SerializeField] private CommonConfig _commonSettings;
    [SerializeField] private UnityDictionary<UpgradeType, UpgradeConfig> _upgrades;

    public UnityDictionary<UpgradeType, UpgradeConfig> Upgrades => _upgrades;

    public CommonConfig CommonSettings => _commonSettings;

    private int fullUpgradedCountries = 0;

    public float GetDefaultEffect => _upgrades.Get(UpgradeType.Economics).UpgradeDataList[0].EffectPerLevel;
    public float GetWonderEffect => _upgrades.Get(UpgradeType.Economics).UpgradeDataList[^1].EffectPerLevel;

    public const int MaxFactoriesUpgradeLevel = 5;

    public float TradeEffectMultiplier => _commonSettings.TradeEffectMultiplier;

    private bool canBuyUpgradeForAd = true;
    public bool CanBuyUpgradeForAd => canBuyUpgradeForAd;

    private int selectedCountryID;
    private UpgradeType currentUpgradeType;


    public void Init(List<IUpgradeReactable> urs)
    {
        _urs = urs;
        LoadData();
    }

    public void ShowUpgradeScreen(int countryID, UpgradeType upgradeType)
    {
        selectedCountryID = countryID;
        var countryData = CountrySaveLoad.LoadCountry(countryID);
        var data = Upgrades.Get(upgradeType);
        string key = (data != null ? data.LocalisationKey : "");

        switch (upgradeType)
        {
            case UpgradeType.Economics:
                ShowUpgradeForEconomics();
                currentUpgradeType = UpgradeType.Economics;
                break;

            case UpgradeType.Army:
                ShowUpgradeForArmy();
                currentUpgradeType = UpgradeType.Army;
                break;

            case UpgradeType.Resistance:
                ShowUpgradeForResistance();
                currentUpgradeType = UpgradeType.Resistance;
                break;
        }

        void ShowUpgradeForEconomics()
        {
            var factoryScreen = gui.FindScreen<FactoryScreen>();
            factoryScreen.SetData(countryID, ConvertToUpgradeDatas(countryData.factoryUpgrades, 4, 5));
            gui.Show(factoryScreen);
        }

        void ShowUpgradeForArmy()
        {
            var upgradeArmyScreen = gui.FindScreen<UpgradeArmyScreen>();
            var loadedArmies = countryData.armyUpgrades;
            var armyUpgrades = ConvertToUpgradeDatas(loadedArmies, 10, 3);
            upgradeArmyScreen.SetData(countryID, UpgradeType.Army, key, armyUpgrades);
            gui.Show(upgradeArmyScreen);
        }

        void ShowUpgradeForResistance()
        {
            var upgradeArmyScreen = gui.FindScreen<UpgradeArmyScreen>();
            var loadedResistance = countryData.resistanceLevel;
            var resistanceUpgrades = ConvertToResistanceUpgradeDatas(loadedResistance);
            upgradeArmyScreen.SetData(countryID, UpgradeType.Resistance, key, resistanceUpgrades);
            gui.Show(upgradeArmyScreen);
        }
    }

    public void ShowOtherArmyInfo(bool next)
    {
        var data = CountrySaveLoad.LoadCountry(selectedCountryID);

        UpgradeType otherUpgradeType = currentUpgradeType;

        if (next) otherUpgradeType = GetNextCountryBallType();
        else otherUpgradeType = GetPreviousCountryBallType();

        gui.Exit();
        ShowUpgradeScreen(selectedCountryID, otherUpgradeType);

        UpgradeType GetNextCountryBallType()
        {
            do otherUpgradeType = EnumUtility.GetNextValue(otherUpgradeType);
            while (!CanShowUpgrade(otherUpgradeType));
            return otherUpgradeType;
        }

        UpgradeType GetPreviousCountryBallType()
        {
            do otherUpgradeType = EnumUtility.GetPreviousValue(otherUpgradeType);
            while (!CanShowUpgrade(otherUpgradeType));
            return otherUpgradeType;
        }

        bool CanShowUpgrade(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.Economics => true,
                UpgradeType.Army => true,
                UpgradeType.Resistance => !data.CountryData.IsBaseCountry,
                _ => false,
            };
        }
    }

    #region Buy

    public void BuyForAds(int countryID, UpgradeType upgradeType, int upgradeID, int count)
    {
        AdsManager.ShowRewarded(gameObject, (successful) =>
        {
            if (successful)
            {
                OnAnyBuyForAd();
                GiveUpgrade(countryID, upgradeType, upgradeID, count);
            }
        });
    }

    public void BuyTradeForAds(int countryId, int amount)
    {
        AdsManager.ShowRewarded(gameObject, (successful) =>
        {
            if (successful)
            {
                OnAnyBuyForAd();
                GiveTrade(countryId);
            }
        });
    }

    public void BuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int count)
    {
        float cost;
        var upgradeData = _upgrades.Get(upgradeType);
        var countryData = CountrySaveLoad.LoadCountry(countryID);

        if (countryData.CountryData.IsPlayerOwner) cost = upgradeData.UpgradeDataList[upgradeID].Cost * count;
        else
        {
            cost = upgradeData.UpgradeDataListRegionVariant[upgradeID].Cost * count;
        }

        if (upgradeType == UpgradeType.Army)
        {
            int currentLevel;

            if (countryData.armyUpgrades == null) countryData.armyUpgrades = new();
            currentLevel = countryData.armyUpgrades.ContainsKey(upgradeID) ? countryData.armyUpgrades[upgradeID] : 0;
            cost *= BattleInGameService.ArmyUpgradeCostMutliplier(currentLevel);

            if (WorldEventService.Instance.CurrentWorldEventContains(UpgradeType.Army)) cost *= WorldEventService.Instance.GetCurrentEventMultiplier();
        }

        CurrencyService.Instance.ReduceCurrency(CurrencyType.Money, cost, () =>
        {
            OnAnyBuyForCurrency();
            GiveUpgrade(countryID, upgradeType, upgradeID, count);
        });
    }

    private void GiveUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int count)
    {
        var countryData = CountrySaveLoad.LoadCountry(countryID);

        int finalValue = -1;

        switch (upgradeType)
        {
            case UpgradeType.Economics:
                finalValue = GiveFactoryUpgrade();

                if (countryData.GetFactoryUpgradesProgress() == 1)
                {
                    fullUpgradedCountries++;

                    switch (fullUpgradedCountries)
                    {
                        case 1:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "1_full_economic_upgrade 1");
                            break;

                        case 5:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "2_full_economic_upgrade 5");
                            break;

                        case 10:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "3_full_economic_upgrade 10");
                            break;

                        case 20:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "4_full_economic_upgrade 20");
                            break;

                        case 30:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "5_full_economic_upgrade 30");
                            break;

                        case 40:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "6_full_economic_upgrade 40");
                            break;

                        case 49:
                            AnalyticsManager.Instance.Log(AnalyticSectionType.Economics, "7_full_economic_upgrade 49");
                            break;
                    }
                }

                break;

            case UpgradeType.Army:
                finalValue = GiveArmyUpgrade();
                break;

            case UpgradeType.Resistance:
                finalValue = GiveResistanceUpgrade();
                break;
        }

        CountrySaveLoad.SaveCountry(countryID, countryData);
        OnBuyUpgradeReact(countryID, upgradeType, upgradeID, finalValue);

        int GiveFactoryUpgrade()
        {
            return countryData.GiveFactory(upgradeID, count);
        }

        int GiveArmyUpgrade()
        {
            return countryData.GiveArmy(upgradeID, count);
        }

        int GiveResistanceUpgrade()
        {
            countryData.GiveResistance();
            return 1;
        }
    }

    public void BuyWonder(int countryID)
    {
        var countryData = CountrySaveLoad.LoadCountry(countryID);

        float cost = _upgrades.Get(UpgradeType.Economics).UpgradeDataList[^1].Cost;
        CurrencyService.Instance.ReduceCurrency(CurrencyType.Money, cost, () =>
        {
            // data
            countryData.wonderBuilded = true;
            CountrySaveLoad.SaveCountry(countryID, countryData);

            // visual

            OnAnyBuyForCurrency();
            OnBuyWonderReact(countryID);
        });
    }

    public void BuyForRegionTrade(int countryID, float cost)
    {
        if (tradeRegions.Contains(countryID)) return;

        CurrencyService.Instance.ReduceCurrency(CurrencyType.Money, cost, () =>
        {
            OnAnyBuyForCurrency();
            GiveTrade(countryID);
        });
    }

    public void BuyRocket(float cost, int count)
    {
        CurrencyService.Instance.ReduceCurrency(CurrencyType.Money, cost * count, () =>
        {
            OnAnyBuyForCurrency();
            GiveRockets(count);
        });
    }

    public void BuyRocketForAd(int count)
    {
        AdsManager.ShowRewarded(gameObject, (successful) =>
        {
            if (successful)
            {
                GiveRockets(count);
                OnAnyBuyForAd();
            }
        });
    }

    private void OnAnyBuyForCurrency()
    {
        canBuyUpgradeForAd = true;
    }

    private void OnAnyBuyForAd()
    {
        canBuyUpgradeForAd = false;
    }

    #endregion

    #region Give
    
    private void GiveTrade(int countryID)
    {
        tradeRegions.Add(countryID);

        SaveManager.Save(CommonData.PREFSKEY_TRADE_REGIONS, tradeRegions);

        OnBuyTradeReact(countryID);
    }

    private void GiveRockets(int count)
    {
        CommonData.RocketsCount += count;
        SaveManager.Save(CommonData.PREFSKEY_BIG_BATTLE_ROCKETS_COUNT, CommonData.RocketsCount);

        OnBuyRockets(CommonData.RocketsCount);
    }

    #endregion

    #region Convert

    public UpgradeType ConvertToUpgradeType(CountryBallType ballType)
    {
        return StaticConvertToUpgradeType(ballType);
    }

    public static UpgradeType StaticConvertToUpgradeType(CountryBallType ballType)
    {
        return ballType switch
        {
            CountryBallType.GroundArmy or CountryBallType.AirArmy or CountryBallType.NavalArmy => UpgradeType.Army,
            CountryBallType.Resistance => UpgradeType.Resistance,
            _ => UpgradeType.Economics,
        };
    }

    public CountryBallType ConvertToBallType(UpgradeType upgradeType)
    {
        return StaticConvertToBallType(upgradeType);
    }

    public static CountryBallType StaticConvertToBallType(UpgradeType upgradeType)
    {
        return upgradeType switch
        {
            UpgradeType.Army => CountryBallType.GroundArmy,
            UpgradeType.Resistance => CountryBallType.Resistance,
            _ => CountryBallType.Factory,
        };
    }

    public UpgradeData[] ConvertToUpgradeDatas(Dictionary<int, int> dictionaryDatas, int totalUpgrades, int totalLevelsInUpgrade)
    {
        if (dictionaryDatas == null) dictionaryDatas = new();

        UpgradeData[] result = new UpgradeData[totalUpgrades];
        int currentLevel;

        for (int i = 0; i < totalUpgrades; i++)
        {
            result[i].AmountLevels = totalLevelsInUpgrade;

            if (dictionaryDatas.ContainsKey(i)) currentLevel = dictionaryDatas[i];
            else currentLevel = 0;

            result[i].CurrentLevel = currentLevel;
        }

        return result;
    }

    public UpgradeData[] ConvertToResistanceUpgradeDatas(int level)
    {
        int totalUpgrades = 3;
        UpgradeData[] result = new UpgradeData[totalUpgrades];

        for (int i = 0; i < totalUpgrades; i++)
        {
            result[i].AmountLevels = 1;
            result[i].CurrentLevel = (level > i ? 1 : 0);
        }

        return result;
    }

    #endregion

    #region Reactables

    private void OnBuyWonderReact(int countryID)
    {
        foreach (var ur in _urs) ur.OnWonderBuilded(countryID); // OnBuyUpgrade(countryID, upgradeType, upgradeID, finalValue);
    }

    private void OnBuyUpgradeReact(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue)
    {
        foreach (var ur in _urs) ur.OnBuyUpgrade(countryID, upgradeType, upgradeID, finalValue);
    }

    private void OnBuyTradeReact(int countryID)
    {
        foreach (var ur in _urs) ur.OnBuyTrade(countryID);
    }

    private void OnBuyRockets(int totalRocketsCount)
    {
        foreach (var ur in _urs) ur.OnRocketBuy(totalRocketsCount);
    }

    #endregion

    #region SaveLoad

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_FULL_UPGRADED_FACTORY, fullUpgradedCountries);
    }

    public void LoadData()
    {
        fullUpgradedCountries = SaveManager.Load<int>(CommonData.PREFSKEY_FULL_UPGRADED_FACTORY);
        tradeRegions = SaveManager.Load<List<int>>(CommonData.PREFSKEY_TRADE_REGIONS);

        if (tradeRegions == null) tradeRegions = new();
    }

    #endregion

    #region Comparable

    public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
    public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

    #endregion
}

public enum UpgradeType
{
    Economics,
    Army,
    Resistance
}