using System;
using System.Collections;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility.Pointer;
using TMPro;

namespace TheSTAR.GUI.Screens
{
    // todo add stars counter (universal element)

    public class IntelligenceScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI _countryName;

        [Space]
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private PointerButton okButton;
        [SerializeField] private PointerButton skipWaitForHardButton;
        [SerializeField] private GameObject waitContainer;
        [SerializeField] private TimeText waitTimer;
        [SerializeField] private TextMeshProUGUI hardCostLabel;
        
        private DateTime unlockTime;
        private int hardCost;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            closeButton.Init(OnCloseClick);
            okButton.Init(OnConfirmIntelligence);
            skipWaitForHardButton.Init(OnBuyForHardClick);
        }

        public void Init(CountryData countryData)
        {
            _countryName.text = countryData.Name;
        }

        protected override void OnShow()
        {
            base.OnShow();

            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME))
                unlockTime = SaveManager.Load<DateTime>(CommonData.PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME);
            else
                unlockTime = DateTime.Now.AddMinutes(-1);

            StartCoroutine(UpdateLockedVisual());
        }

        protected override void OnHide()
        {
            base.OnHide();

            StopAllCoroutines();
        }

        private IEnumerator UpdateLockedVisual()
        {
            bool locked;

            do
            {
                locked = DateTime.Now < unlockTime;

                okButton.gameObject.SetActive(!locked);
                waitContainer.SetActive(locked);

                if (locked)
                {
                    TimeSpan timeWait = unlockTime - DateTime.Now;
                    waitTimer.SetValue(timeWait);

                    hardCost = (int)timeWait.TotalMinutes + 1;
                    hardCostLabel.text = $"{hardCost}";

                    yield return new WaitForSeconds(1);
                }
            }
            while (locked && gameObject.activeSelf);

            yield return null;
        }

        public void OnBuyForHardClick()
        {
            CurrencyService.Instance.ReduceCurrency(CurrencyType.Stars, hardCost, () =>
            {
                Intelligence.Instance.RunIntelligence(true);
            });
        }

        public void OnConfirmIntelligence() => Intelligence.Instance.RunIntelligence(false);

        public void OnCloseClick() => gui.Exit();
    }
}