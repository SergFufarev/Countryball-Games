using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;

namespace FunnyBlox
{
    [CreateAssetMenu(fileName = "CommonConfig", menuName = "Data/CommonConfig", order = 51)]
    public class CommonConfig : ScriptableObject
    {
        public float AmountMoneyOnStart = 30f;
        public float AmountStarsOnStart = 10f;

        [Space]
        public List<UpgradeData> UpgradesForCapturedRegions = new ();

        public int[] CoinsForAdsRewardValues = new int[0];
        public GameTimeSpan CoinsForAdWaitTime;

        // Version B only


        public int TradeEffectMultiplier = 2;

        [Space]
        public int GamePlusIncomeBonus = 10;

        public int GamePlusCostBonus = 20;

        [Space]
        public int[] IntelligenceTimes = new int[0];

        [Header("Loading")]
        public Sprite firstSessionFon;
        public Sprite[] loadFons;

        [Header("WorldEvents")]
        public GameTimeSpan WorldEventPeriod;
        public UnityDictionary<WorldEventType, string> WorldEventMessages = new();
        public UnityDictionary<WorldEventType, int> WorldEventValues = new();

        public string GetWorldEventMessage(WorldEventType worldEventType)
        {
            return WorldEventMessages.Get(worldEventType);
        }

        public int GetWorldEventValue(WorldEventType worldEventType)
        {
            return WorldEventValues.Get(worldEventType);
        }
    }
}