using FunnyBlox;

public class EnemyRevolt : EnemyAttack
{
    /// <summary> наш регион, в котором планируется восстание </summary>
    private Country revoltRegion;
    public Country RevoltRegion => revoltRegion;

    public override void Try()
    {
        if (!use) return;

        if (battle.InBattle || countries._playerCountries.Count <= 1) return;

        if (battle.Invasion.State != EnemyAttackState.Idle || battle.Mutual.State != EnemyAttackState.Idle) return;

        if (state == EnemyAttackState.Idle)
        {
            // ожидание до предупреждения
            float waitTime = battle.BattleConfig.RevoltPeriod.TotalSeconds - battle.BattleConfig.BattleAlertTime.TotalSeconds;
            //cor = StartCoroutine(WaitForPreparationCor(waitTime));
            state = EnemyAttackState.Pause;
            battle.currentEnemyAttackType = EnemyAttackType.Revolt;
            StartWait(waitTime);
        }

        if (state == EnemyAttackState.PauseComplete) TryGoFromCompletePauseToPreparation();
        if (state == EnemyAttackState.ReadyToAttack) Attack();
    }

    protected override void TryGoFromCompletePauseToPreparation()
    {
        if (state != EnemyAttackState.PauseComplete) return;

        // переход к EnemyAttackState.BattlePreparation

        state = EnemyAttackState.BattlePreparation;
        revoltRegion = battle.GetRandomPlayersRegion();
        
        if (revoltRegion != null)
        {
            // улучшение сопротивления
            var revoltRegionData = CountrySaveLoad.LoadCountry(revoltRegion.ID);
            int newResistance = revoltRegionData.GiveResistance();
            CountrySaveLoad.SaveCountry(revoltRegion.ID, revoltRegionData);

            revoltRegion.SetResistanceData(newResistance);
            revoltRegion.UpdateBallProgress(CountryBallType.Resistance);

            // revolt message
            MessageService.Instance.ShowBattleWarningMessage(revoltRegion.ID, revoltRegion.ID);

            // ожидание до восстания
            float waitTime = battle.BattleConfig.BattleAlertTime.TotalSeconds;
            StartWait(waitTime);
        }
        else
        {
            // сейчас невозможно нигде устроить революцию
            state = EnemyAttackState.Idle;
        }
    }

    protected override void Attack()
    {
        if (!use) return;

        state = EnemyAttackState.InAttack;

        if (revoltRegion == null && revoltRegion.LocalCountryData.Owner != CommonData.PlayerID)
        {
            Try();
            return;
        }

        revoltRegion.Revolt(() => state = EnemyAttackState.Idle);
    }

    public void LoadCountries(Country revoltRegion)
    {
        this.revoltRegion = revoltRegion;
    }
}