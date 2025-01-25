using UnityEngine;

namespace FunnyBlox.OfflineReward
{
    [CreateAssetMenu(fileName = "OfflineRewardData", menuName = "Data/Offline Reward Data", order = 51)]
    public class OfflineRewardData : ScriptableObject
    {
        //public int TimeoutMin = 10;
        public int TimeoutMax;
        public float Multiplier;
    }
}