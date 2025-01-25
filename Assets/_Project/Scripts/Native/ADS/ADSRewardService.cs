using Sirenix.OdinInspector;
using SPSDigital.Metrica;
using UnityEngine;

namespace SPSDigital.ADS
{
    public enum ERewardType
    {
        Type1 = 0,
        Speedup = 1
    }

    public class ADSRewardService : MonoBehaviour
    {
        public void OnGetReward(int rewardType)
        {
            switch ((ERewardType)rewardType)
            {
                case ERewardType.Type1:
                    AdsService.Instance.FinishRewarded += OnRewarded;
                    break;
            }

            AdsService.Instance.ShowRewarded("in_game");
        }

        private void OnRewarded(bool state)
        {
            AdsService.Instance.FinishRewarded -= OnRewarded;

#if SPSDIGITAL_METRICA
            AppMetricaBridge.ReportEvent(AppMetricaVariables.REWARD_CHEST);

            AppMetricaBridge.ReportEvent("video_ads_watch"
                , AppMetricaBridge.SetParametrs(
                    ad_type: "rewarded"
                    , placement: "chest"
                    , result: state ? "obligatory" : "canceled"
                    , connection: "1"
                    , level_number: ""
                    , level_name: ""
                    , level_count: ""
                    , level_diff: ""
                    , level_loop: ""
                    , level_random: ""
                    , level_type: ""
                )
            );
#endif

            if (!state) return;
            //TODO: rewarded effect
        }
    }
}