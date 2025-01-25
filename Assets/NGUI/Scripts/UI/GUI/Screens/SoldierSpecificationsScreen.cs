using UnityEngine;
using UnityEngine.UI;
using TheSTAR.Utility.Pointer;
using TMPro;

namespace TheSTAR.GUI.Screens
{
    public class SoldierSpecificationsScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image iconImg;
        [SerializeField] private TextMeshProUGUI hpCounter;
        [SerializeField] private TextMeshProUGUI attackSpeedCounter;
        [SerializeField] private TextMeshProUGUI damageCounter;
        [SerializeField] private TextMeshProUGUI smallDamageCounter;
        [SerializeField] private TextMeshProUGUI attackDistanceCounter;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private PointerButton closeButton;
        [SerializeField] private GameObject smallSpecifications;
        [SerializeField] private GameObject fullSpecifications;
        [SerializeField] private RectTransform windowRect;

        private float bigWindowHeight = 1880;
        private float smallWindowHeight = 1480;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            gui = cts.Get<GuiController>();

            closeButton.Init(gui.Exit);
        }

        protected override void OnShow()
        {
            base.OnShow();
            gui.TutorContainer.BreakTutorial();
        }

        public void SetData(UnitConfigData unitData) => SetDataFull(unitData.Icon, unitData.Name, unitData.Hp, unitData.AttackSpeed, unitData.Damage, unitData.AttackDistance, unitData.Description);

        public void SetData(RocketConfigData rocketData) => SetDataSmall(rocketData.Icon, rocketData.Name, rocketData.Damage, rocketData.Description);

        public void SetDataFull(Sprite icon, string unitName, int hp, float attackSpeed, int damage, float attackDistance, string description)
        {
            iconImg.sprite = icon;
            title.text = unitName;

            hpCounter.text = hp.ToString();
            attackSpeedCounter.text = attackSpeed.ToString();
            damageCounter.text = damage.ToString();
            attackDistanceCounter.text = attackDistance.ToString();

            smallSpecifications.SetActive(false);
            fullSpecifications.SetActive(true);

            this.description.text = description;

            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, bigWindowHeight);
        }

        public void SetDataSmall(Sprite icon, string unitName, int damage, string description)
        {
            iconImg.sprite = icon;
            title.text = unitName;

            smallDamageCounter.text = damage.ToString();
            smallSpecifications.SetActive(true);
            fullSpecifications.SetActive(false);

            this.description.text = description;

            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, smallWindowHeight);
        }
    }
}