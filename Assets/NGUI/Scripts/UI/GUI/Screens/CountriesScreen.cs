using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class CountriesScreen : GuiScreen, ITutorialStarter
    {
        [SerializeField] private Transform elementsContainer;
        [SerializeField] private CountryUIPanel panelPrefab;
        [SerializeField] private List<CountryUIPanel> createdPanels = new ();
        [SerializeField] private bool showOnlyOneUnexplore;
        [SerializeField] private PointerButton closeButton;

        private GuiController gui;
        private VisualCountryController countries;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();

            closeButton.Init(OnCloseClick);
        }

        protected override void OnShow()
        {
            base.OnShow();

            Invoke(nameof(UpdateUI), 0.1f);
        }

        private void UpdateUI()
        {
            // data
            var allCountries = new List<Country>(countries._countries);

            // 1
            var playerCountries = new List<Country>(countries._playerCountries);
            playerCountries.Remove(countries.PlayerBaseCountry);
            playerCountries.Insert(0, countries.PlayerBaseCountry);

            // 2
            var countriesToAttack = new List<Country>(countries._openCountries);
            countriesToAttack = ArrayUtility.Exclude(countriesToAttack, playerCountries);

            // 3
            List<Country> countriesToExplore;
            if (showOnlyOneUnexplore)
            {
                countriesToExplore = new();
                countriesToExplore.Add(countries.FindWeekCountry());
            }
            else
            {
                countriesToExplore = ArrayUtility.Exclude(allCountries, playerCountries);
                countriesToExplore = ArrayUtility.Exclude(countriesToExplore, countriesToAttack);
            }

            // generate list

            Country country;
            for (int i = 0; i < playerCountries.Count + countriesToAttack.Count + countriesToExplore.Count; i++)
            {
                if (i < playerCountries.Count) country = playerCountries[i];
                else if (i < playerCountries.Count + countriesToAttack.Count) country = countriesToAttack[i - playerCountries.Count];
                else country = countriesToExplore[i - playerCountries.Count - countriesToAttack.Count];

                if (createdPanels.Count > i) createdPanels[i].Init(gui, country);
                else
                {
                    var panel = Instantiate(panelPrefab, elementsContainer);
                    createdPanels.Add(panel);

                    panel.Init(gui, country);
                }
            }

            TryShowTutorial();
        }

        public void TryShowTutorial()
        {
            var tutor = gui.TutorContainer;
            Transform focusTran;

            if (!tutor.IsComplete(TutorContainer.CountriesScreenTutorID))
            {
                focusTran = createdPanels[0].ArmyGroupButton;
                tutor.TryShowInUI(TutorContainer.CountriesScreenTutorID, focusTran);
            }
        }

        public void OnCloseClick() => gui.Exit();
    }
}