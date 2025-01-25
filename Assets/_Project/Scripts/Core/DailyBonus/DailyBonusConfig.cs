using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyBonusConfig", menuName = "Data/DailyBonusConfig")]
public class DailyBonusConfig : ScriptableObject
{
    public DailyBonusData[] dailyBonuses = new DailyBonusData[0];

    [SerializeField]
    public UnityDictionary<DailyRewardType, Sprite> Icons = new ();

    [Serializable]
    public class DailyBonusData
    {
        public DailyReward[] rewards;
    }

    [Serializable]
    public class DailyReward
    {
        public DailyRewardType rewardType;
        public int rewardValue;
    }

    public enum DailyRewardType
    {
        Money,
        Stars,
        IncomeBoost
    }
}