using FunnyBlox;

public class EnemyInvasion : EnemyAttack
{
    /// <summary>
    /// Вражеская територрия, с которой идёт атака
    /// </summary>
    private Country enemyAttackerCountry;

    /// <summary>
    /// Територрия игрока, на которую нападает враг
    /// </summary>
    private Country countryToEnemyAttack;

    public Country EnemyAttackerCountry => enemyAttackerCountry;
    public Country CountryToEnemyAttack => countryToEnemyAttack;

    public override void Try()
    {
        if (!use) return;

        if (battle.InBattle || countries._playerCountries.Count <= 1) return;

        if (battle.Revolt.State != EnemyAttackState.Idle || battle.Mutual.State != EnemyAttackState.Idle) return;

        if (state == EnemyAttackState.Idle)
        {
            // ожидание до предупреждения
            float waitTime = battle.BattleConfig.EnemyAttackPeriod.TotalSeconds - battle.BattleConfig.BattleAlertTime.TotalSeconds;
            state = EnemyAttackState.Pause;
            battle.currentEnemyAttackType = EnemyAttackType.Invasion;
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
        countryToEnemyAttack = battle.GetRandomPlayersRegion();

        if (countryToEnemyAttack != null)
        {
            enemyAttackerCountry = countries.FindEnemyForCountry(countryToEnemyAttack);

            // message
            MessageService.Instance.ShowBattleWarningMessage(enemyAttackerCountry.ID, countryToEnemyAttack.ID);

            // ожидание до боя
            float waitTime = battle.BattleConfig.BattleAlertTime.TotalSeconds;
            StartWait(waitTime);
        }
        else
        {
            // сейчас невозможно напасть на какой-либо регион игрока
            state = EnemyAttackState.Idle;
        }
    }

    protected override void Attack()
    {
        if (!use) return;

        state = EnemyAttackState.InAttack;

        if (enemyAttackerCountry == null ||
            countryToEnemyAttack == null ||
            enemyAttackerCountry.LocalCountryData.Owner == CommonData.PlayerID ||
            countryToEnemyAttack.LocalCountryData.Owner != CommonData.PlayerID) return;

        battle.Battle(enemyAttackerCountry, countryToEnemyAttack, () => state = EnemyAttackState.Idle);
    }

    public void LoadCountries(Country enemyAttackerCountry, Country countryToEnemyAttack)
    {
        this.enemyAttackerCountry = enemyAttackerCountry;
        this.countryToEnemyAttack = countryToEnemyAttack;
    }
}