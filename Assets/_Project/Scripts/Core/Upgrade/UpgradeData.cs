using UnityEngine;
using System;
using Newtonsoft.Json;

namespace FunnyBlox
{
    [Serializable]
    public struct UpgradeData
    {
        [JsonIgnore] public string Description;
        [JsonIgnore] public float Cost;
        public int AmountLevels;
        public int CurrentLevel;
        [JsonIgnore] public float EffectPerLevel;

        [JsonIgnore] public Sprite Icon;
        
        public UpgradeData(UpgradeData data)
        {
            Description = data.Description;
            Icon = data.Icon;
            Cost = data.Cost;
            AmountLevels = data.AmountLevels;
            CurrentLevel = data.CurrentLevel;
            EffectPerLevel = data.EffectPerLevel;
        }

        [JsonIgnore]
        public bool IsFullUpgraded => CurrentLevel >= AmountLevels;

        [JsonIgnore]
        public float Progress => (float)CurrentLevel / AmountLevels;
    }
}