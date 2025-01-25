using System;
using UnityEngine;
using UnityEngine.UI;
using FunnyBlox;
using TheSTAR.Utility;
using TheSTAR.GUI;
using TheSTAR.Utility.Pointer;
using TMPro;

public class CountryUIPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countryNameLabel;
    [SerializeField] private PointerButton attackButton;
    [SerializeField] private PointerButton exploreButton;
    [SerializeField] private GameObject linesContainer;

    [Header("Elements")]
    [SerializeField] private CountryGroupElement armyGroupElement;
    [SerializeField] private CountryGroupElement fleetGroupElement;
    [SerializeField] private CountryGroupElement airGroupElement;
    [SerializeField] private CountryGroupElement economicsGroupElement;
    [SerializeField] private CountryGroupElement revoltGroupElement;

    [Space]
    [SerializeField] private Image topSprite;
    [SerializeField] private Image bgSprite;

    [Space]
    [SerializeField] private Color playerMainColor;
    [SerializeField] private Color playerColor;
    [SerializeField] private Color enemyColor;
    [SerializeField] private Color unexploreColor;
    [SerializeField] private Color defaultBgColor;

    public Transform ArmyGroupButton => armyGroupElement.GoToButtonTran;

    private Action attackAction;
    private Action exploreAction;

    public void Init(GuiController gui, Country country)
    {
        CountryUiPanelVisualType visualType;

        if (country.LocalCountryData.IsPlayerOwner) visualType = CountryUiPanelVisualType.Player;
        else if (country.LocalCountryData.OpenState) visualType = CountryUiPanelVisualType.ToAttack;
        else visualType = CountryUiPanelVisualType.ToExplore;

        countryNameLabel.text = country.LocalCountryData.Name;
        attackAction = () =>
        {
            country.CountryBalls.Get(CountryBallType.GroundArmy).OnSelect(true);
        };
        exploreAction = () =>
        {
            gui.CurrentScreen.Hide();
            CameraService.Instance.MoveTo(country, () => country.CountryBalls.Get(CountryBallType.Main).OnSelect(true));
        };

        // army
        if (visualType != CountryUiPanelVisualType.ToExplore)
        {
            armyGroupElement.SetProgress(country.CountryBalls.Get(CountryBallType.GroundArmy).Progress);
            armyGroupElement.Init(() =>
            {
                var tutor = gui.TutorContainer;
                if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.CountriesScreenTutorID) tutor.CompleteTutorial();

                country.CountryBalls.Get(CountryBallType.GroundArmy).OnSelect(true);
            }, visualType == CountryUiPanelVisualType.Player);
            Activate(armyGroupElement);
        }
        else Deactivate(armyGroupElement);

        // fleet
        if (country.LocalCountryData.UseNavalArmy && visualType != CountryUiPanelVisualType.ToExplore)
        {
            fleetGroupElement.SetProgress(country.CountryBalls.Get(CountryBallType.NavalArmy).Progress);
            fleetGroupElement.Init(() => country.CountryBalls.Get(CountryBallType.NavalArmy).OnSelect(true), visualType == CountryUiPanelVisualType.Player);
            Activate(fleetGroupElement);
        }
        else Deactivate(fleetGroupElement);

        // air
        if (country.LocalCountryData.UseAirArmy && visualType != CountryUiPanelVisualType.ToExplore)
        {
            airGroupElement.SetProgress(country.CountryBalls.Get(CountryBallType.AirArmy).Progress);
            airGroupElement.Init(() => country.CountryBalls.Get(CountryBallType.AirArmy).OnSelect(true), visualType == CountryUiPanelVisualType.Player);
            Activate(airGroupElement);
        }
        else Deactivate(airGroupElement);

        // economics
        if (country.LocalCountryData.IsPlayerOwner && visualType != CountryUiPanelVisualType.ToExplore)
        {
            economicsGroupElement.SetProgress(country.CountryBalls.Get(CountryBallType.Factory).Progress);
            economicsGroupElement.Init(() => country.CountryBalls.Get(CountryBallType.Factory).OnSelect(true), true);
            Activate(economicsGroupElement);
        }
        else Deactivate(economicsGroupElement);

        // revolt
        if (!country.LocalCountryData.IsBaseCountry && visualType != CountryUiPanelVisualType.ToExplore)
        {
            revoltGroupElement.SetProgress(country.CountryBalls.Get(CountryBallType.Resistance).Progress);
            revoltGroupElement.Init(() => country.CountryBalls.Get(CountryBallType.Resistance).OnSelect(true), true);
            Activate(revoltGroupElement);
        }
        else Deactivate(revoltGroupElement);

        revoltGroupElement.transform.SetAsLastSibling();

        linesContainer.SetActive(visualType != CountryUiPanelVisualType.ToExplore);

        switch (visualType)
        {
            case CountryUiPanelVisualType.Player:
                topSprite.color = country.LocalCountryData.IsBaseCountry ? playerMainColor : playerColor;
                bgSprite.color = defaultBgColor;
                attackButton.gameObject.SetActive(false);
                exploreButton.gameObject.SetActive(false);
                break;

            case CountryUiPanelVisualType.ToAttack:
                topSprite.color = enemyColor;
                bgSprite.color = defaultBgColor;
                exploreButton.gameObject.SetActive(false);

                // attackButton
                attackButton.gameObject.SetActive(true);

                Transform firstArmyGoToButtonTran = armyGroupElement.GoToButtonTran;
                Transform lastArmyGoToButtonTran =
                    country.LocalCountryData.UseAirArmy ? airGroupElement.GoToButtonTran :
                        (country.LocalCountryData.UseNavalArmy ? fleetGroupElement.GoToButtonTran : armyGroupElement.GoToButtonTran);

                attackButton.transform.position = MathUtility.MiddlePosition(firstArmyGoToButtonTran.position, lastArmyGoToButtonTran.position);

                break;

            case CountryUiPanelVisualType.ToExplore:
                topSprite.color = unexploreColor;
                bgSprite.color = unexploreColor;

                attackButton.gameObject.SetActive(false);

                // explore button
                exploreButton.gameObject.SetActive(true);
                break;
        }

        attackButton.Init(OnAttackClick);
        exploreButton.Init(OnExploreClick);

        void Activate(CountryGroupElement element) => element.gameObject.SetActive(true);

        void Deactivate(CountryGroupElement element) => element.gameObject.SetActive(false);
    }

    public void OnAttackClick() => attackAction?.Invoke();

    public void OnExploreClick() => exploreAction?.Invoke();
}

public enum CountryUiPanelVisualType
{
    Player,
    ToAttack,
    ToExplore
}