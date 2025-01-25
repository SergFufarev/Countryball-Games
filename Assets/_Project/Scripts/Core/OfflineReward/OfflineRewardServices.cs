using System;
using System.Globalization;
using UnityEngine;
using Zenject;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;

namespace FunnyBlox.OfflineReward
{
    public class OfflineRewardServices : MonoSingleton<OfflineRewardServices>
    {
        [SerializeField] private OfflineRewardData _rewardData;
        private int _rewardAmount;

        [Inject] private GuiController gui;

        public void GetAvailableReward(out bool success)
        {
            success = false;

            _rewardAmount = 0;

            DateTime lastTime = GetDateTime(CommonData.PREFSKEY_OFFLINE_REWARD_LAST_TIME);
            TimeSpan timePassed = DateTime.Now - lastTime;
            int totalMinutes = (int)timePassed.TotalMinutes;

            if (totalMinutes >= _rewardData.TimeoutMax)
            {
                // give reward
                _rewardAmount = Mathf.RoundToInt(_rewardData.TimeoutMax * 60f * _rewardData.Multiplier);

                var rewardScreen = gui.FindScreen<RewardScreen>();
                rewardScreen.SetData(_rewardAmount);
                gui.Show(rewardScreen);

                SetDateTime(CommonData.PREFSKEY_OFFLINE_REWARD_LAST_TIME, DateTime.Now);

                success = true;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            //if (hasFocus) Invoke(nameof(GetAvailableReward), 0.5f);
        }

        private void SetDateTime(string key, DateTime value)
        {
            string result = value.ToString("u", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(key, result);
            PlayerPrefs.Save();
        }

        private DateTime GetDateTime(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string prefsResult = PlayerPrefs.GetString(key);

                DateTime result = DateTime.ParseExact(prefsResult, "u", CultureInfo.InvariantCulture);
                return result;
            }
            else
            {
                SetDateTime(key, DateTime.Now);
                return DateTime.Now;
            }
        }
    }
}