using Zenject;
using MAXHelper;
using FunnyBlox;

namespace TheSTAR.GUI
{
    public class GuiControllerInGame : GuiController
    {
        //[Inject] private readonly Sound.SoundController sound;
        [Inject] private readonly GameController game;
        [Inject] private readonly RateUsController rateUs;
        //[Inject] private readonly IARManager inappReview;
        //[Inject] private readonly AdsManager ads;
        [Inject] private readonly UpgradeService upgrades;
        [Inject] private readonly CurrencyService currency;
        [Inject] private readonly VisualCountryController countries;
        [Inject] private readonly BattleInGameService battle;

        protected override IController[] PackControllers()
        {
            return new IController[]
            {
                this,
                game,
                //sound,
                rateUs,
                //inappReview,
                upgrades,
                currency,
                countries,
                battle,
                time
            };
        }
    }
}