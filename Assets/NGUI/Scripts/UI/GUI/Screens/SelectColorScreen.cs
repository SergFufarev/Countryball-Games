using UnityEngine;
using FunnyBlox;

namespace TheSTAR.GUI.Screens
{
    public class SelectColorScreen : GuiScreen
    {
        [SerializeField] private ColorButton[] colorButtons;

        private GuiController gui;
        private VisualCountryController countries;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();
            countries = cts.Get<VisualCountryController>();
            for (int i = 0; i < colorButtons.Length; i++) colorButtons[i].Init(i, SelectColor);
        }

        private void SelectColor(int colorIndex)
        {
            SaveManager.Save(CommonData.PREFSKEY_USE_CUSTOM_COUNTRY_COLOR, true);
            SaveManager.Save(CommonData.PREFSKEY_CUSTOM_COUNTRY_COLOR_INDEX, colorIndex);

            foreach (var country in countries._playerCountries)
            {
                if (country != null) country.UpdateTerritoryMaterial();
            }

            gui.ShowMainScreen();
        }
    }
}