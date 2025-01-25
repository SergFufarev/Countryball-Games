using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using TheSTAR.GUI;

namespace FunnyBlox.Saver
{
    public class RestartServise : MonoSingleton<RestartServise>
    {
        [SerializeField] private CommonConfig _commonSettings;

        [Inject] private readonly GuiController gui;

        public void Restart()
        {
            var gameVersion = CommonData.CurrentGameVersionType;

            VisualCountryController.ClearData();

            CommonData.MoneyPerSecondBase = 0;
            SaveManager.Save(CommonData.PREFSKEY_MONEY_PER_SECOND, CommonData.MoneyPerSecondBase);

            CommonData.Money = _commonSettings.AmountMoneyOnStart;
            SaveManager.Save(CommonData.PREFSKEY_MONEY, CommonData.Money);

            SaveManager.Save(CommonData.PREFSKEY_BOMB_ATTACK_TIME, new DateTime());

            // сбрасываем тутор только если он не был пройден до конца
            if (!gui.TutorContainer.IsLastTutorialComplete) gui.TutorContainer.ClearCompletedTutorials();

            NotificationManager.Instance.CancelAllNotificatinos();

            if (gameVersion == GameVersionType.VersionB) PlayerPrefs.DeleteKey(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME);

            Time.timeScale = 1;

            SceneManager.LoadScene("Game");
        }

#if UNITY_EDITOR
        [ContextMenu("ClearData")]
        private void ClearData()
        {
            SaveManager.ClearData();
        }
#endif
    }
}