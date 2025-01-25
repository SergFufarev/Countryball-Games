using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility;
using System;

public class CountryCollectedData
{
    public CountryData CountryData;

    [Obsolete]
    public List<CountryBallData> CountryBallData; // todo старые данные для наката (не используется в версии с BigBattle)

    public Dictionary<int, int> factoryUpgrades;
    public Dictionary<int, int> armyUpgrades;
    public int resistanceLevel = 0;
    public bool wonderBuilded = false;

    public bool NeedUpgradeForBigBattle => CountryBallData != null;

    #region Give

    public int GiveArmy(int id, int count)
    {
        if (armyUpgrades == null) armyUpgrades = new();

        if (armyUpgrades.ContainsKey(id)) armyUpgrades[id] += count;
        else armyUpgrades.Add(id, count);

        if (armyUpgrades[id] > BattleInGameService.ArmyMaxUpgradeLevel) armyUpgrades[id] = BattleInGameService.ArmyMaxUpgradeLevel;

        return armyUpgrades[id];
    }

    public int GiveFactory(int id, int count)
    {
        if (factoryUpgrades == null) factoryUpgrades = new();

        if (factoryUpgrades.ContainsKey(id)) factoryUpgrades[id] += count;
        else factoryUpgrades.Add(id, count);

        if (factoryUpgrades[id] > UpgradeService.MaxFactoriesUpgradeLevel) factoryUpgrades[id] = UpgradeService.MaxFactoriesUpgradeLevel;

        return factoryUpgrades[id];
    }

    public int GiveResistance()
    {
        resistanceLevel++;
        if (resistanceLevel > BattleInGameService.ResistanceMaxForce) resistanceLevel = BattleInGameService.ResistanceMaxForce;

        return resistanceLevel;
    }

    public float GetFactoryUpgradesProgress()
    {
        float progress;
        float defaultBuildingsProgess = (float)GetFactoryUpgradeLevels() / (CountryData.Factories * 5);
        float wonderProgress = wonderBuilded ? 1 : 0;
        progress = ((defaultBuildingsProgess * CountryData.Factories) + wonderProgress) / (CountryData.Factories + 1);

        return progress;
    }

    #endregion

    public int ReduceArmy(int count)
    {
        DictionaryConvertUtility.ReduceFromEnd(armyUpgrades, 9, count);
        return GetArmyUpgradeLevels();
    }

    public int GetArmyUpgradeLevels() => DictionaryConvertUtility.GetTotalValue(armyUpgrades);

    public int GetFactoryUpgradeLevels() => DictionaryConvertUtility.GetTotalValue(factoryUpgrades);

    public void ParseDataForBigBattleVersion()
    {
        factoryUpgrades = ParseForBallType(CountryBallType.Factory, out _);

        // army
        ParseForBallType(CountryBallType.GroundArmy, out float progressGround);
        ParseForBallType(CountryBallType.AirArmy, out float progressAir);
        ParseForBallType(CountryBallType.NavalArmy, out float progressNaval);
        float totalArmyProgress = (progressGround + progressAir + progressNaval) / 3;
        int totalLevels = MathUtility.Limit((int)(totalArmyProgress * BattleInGameService.ArmyMaxForce), 0, BattleInGameService.ArmyMaxForce);
        armyUpgrades = DictionaryConvertUtility.ConvertTotalLevelsIntoDictionary(totalLevels, BattleInGameService.ArmyMaxUpgradeLevel);

        // resistance
        ParseForBallType(CountryBallType.Resistance, out float resistanceProgress);
        resistanceLevel = MathUtility.Limit((int)(resistanceProgress * BattleInGameService.ResistanceMaxForce), 0, BattleInGameService.ResistanceMaxForce);

        // wonder
        wonderBuilded = CountryData.isWonderBuilded;

        Dictionary<int, int> ParseForBallType(CountryBallType ballType, out float progress)
        {
            progress = 0;

            for (int i = 0; i < CountryBallData.Count; i++)
            {
                if (CountryBallData[i].CountryBallType == ballType)
                    return CountryBallData[i].GetConvertedUpgradeData(out progress);
            }

            return new();
        }

        CountryBallData = null;
        CountrySaveLoad.SaveCountry(CountryData.Id, this);
    }
}

// не используется в версии с BigBattle
[Obsolete]
public struct CountryBallData
{
    public CountryBallType CountryBallType;
    public UpgradeData[] CountryBallUpgrades;

    public CountryBallData(CountryBallType countryBallType)
    {
        CountryBallType = countryBallType;
        CountryBallUpgrades = new UpgradeData[0];
    }

    public Dictionary<int, int> GetConvertedUpgradeData(out float progress)
    {
        int totalCurrentLevels = 0;
        int totalNeededLevels = 0;

        Dictionary<int, int> result = new();

        for (int i = 0; i < CountryBallUpgrades.Length; i++)
        {
            totalCurrentLevels += CountryBallUpgrades[i].CurrentLevel;
            totalNeededLevels += CountryBallUpgrades[i].AmountLevels;
            result.Add(i, CountryBallUpgrades[i].CurrentLevel);
        }

        progress = (float)totalCurrentLevels / (float)totalNeededLevels;

        return result;
    }
}