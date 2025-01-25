using UnityEngine;
using TheSTAR.Sound;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class DailyBonusScreen : GuiScreen, ITutorialStarter
    {
        [SerializeField] private DailyUIElement[] dailyElements = new DailyUIElement[0];
        [SerializeField] private Transform lightTran;
        [SerializeField] private DailyBonusConfig dailyBonusConfig;
        [SerializeField] private PointerButton claimButton;

        private DailyBonusConfig.DailyBonusData _currentDailyBonusData;

        private GuiController gui;
        //private SoundController sounds;
        private TutorContainer tutor;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            tutor = gui.TutorContainer;

            claimButton.Init(OnClaimClick);
        }

        private int currentBonusIndex;

        protected override void OnShow()
        {
            base.OnShow();

            currentBonusIndex = DailyBonusService.Instance.GetCurrentBonusIndex();
            _currentDailyBonusData = dailyBonusConfig.dailyBonuses[currentBonusIndex];

            DailyUIElement element;
            for (int i = 0; i < dailyElements.Length; i ++)
            {
                element = dailyElements[i];
                var reward = dailyBonusConfig.dailyBonuses[i].rewards[0];

                element.Init(i, i <= currentBonusIndex, dailyBonusConfig.Icons.Get(reward.rewardType), reward.rewardValue, reward.rewardType);
            }

            Invoke(nameof(DelayActivateLight), 0.01f);

            TryShowTutorial();
        }

        private void DelayActivateLight()
        {
            lightTran.position = dailyElements[currentBonusIndex].transform.position;
            lightTran.gameObject.SetActive(true);
        }

        public void OnClaimClick()
        {
            if (_currentDailyBonusData != null)
            {
                foreach (var reward in _currentDailyBonusData.rewards)
                {
                    switch (reward.rewardType)
                    {
                        case DailyBonusConfig.DailyRewardType.Money:
                            CurrencyService.Instance.AddCurrency(CurrencyType.Money, reward.rewardValue);
                            break;

                        case DailyBonusConfig.DailyRewardType.Stars:
                            CurrencyService.Instance.AddCurrency(CurrencyType.Stars, reward.rewardValue);
                            break;

                        case DailyBonusConfig.DailyRewardType.IncomeBoost:
                            CurrencyService.Instance.SetIncomeBoost(reward.rewardValue * 60);
                            break;
                    }
                }
            }

            if (tutor.InTutorial) tutor.CompleteTutorial();

            DailyBonusService.Instance.OnGetDailyReward();
            gui.Exit();
            SoundController.Instance.PlaySound(SoundType.Purchase);
        }

        public void TryShowTutorial()
        {
            Transform focusTran;
            if (!tutor.InTutorial && !tutor.IsComplete(TutorContainer.DailyBonusTutorID))
            {
                focusTran = claimButton.transform;
                tutor.TryShowInUI(TutorContainer.DailyBonusTutorID, focusTran);
            }
        }
    }
}