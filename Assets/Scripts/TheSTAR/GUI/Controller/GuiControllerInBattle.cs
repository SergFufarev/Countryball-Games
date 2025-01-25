using Zenject;
using FunnyBlox;
using TheSTAR.Sound;
using Battle;

namespace TheSTAR.GUI
{
    public class GuiControllerInBattle : GuiController
    {
        [Inject] private readonly BattleController battle;
        [Inject] private readonly CurrencyService currency;
        [Inject] private readonly UpgradeService upgrades;
        //[Inject] private readonly SoundController sounds;
        [Inject] private readonly BattleCameraController battleCameraController;
        [Inject] private readonly RateUsController rateUs;
        //[Inject] private readonly IARManager iarManager;

        protected override IController[] PackControllers()
        {
            return new IController[]
            {
                this,
                battle,
                time,
                currency,
                upgrades,
                //sounds,
                battleCameraController,
                rateUs,
                //iarManager
            };
        }
    }
}