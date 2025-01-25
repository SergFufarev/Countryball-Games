using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;

public class EnemyMutual : EnemyAttack
{
    /// <summary>
    /// Вражеская територрия, с которой идёт атака
    /// </summary>
    private Country enemyAttackerCountry;

    /// <summary>
    /// Вражеская территория, на которую нападают
    /// </summary>
    private Country enemyDefenderCountry;

    public Country EnemyAttackerCountry => enemyAttackerCountry;
    public Country EnemyDefenderCountry => enemyDefenderCountry;

    private float currentRandomPeriod;

    public override void Try()
    {
        if (!use) return;

        if (battle.InBattle || countries._openEnemyCountries.Count < 2) return;

        if (battle.Revolt.State != EnemyAttackState.Idle || battle.Invasion.State != EnemyAttackState.Idle) return;

        if (state == EnemyAttackState.Idle)
        {
            // ожидание до предупреждения
            currentRandomPeriod = MathUtility.RandomRange(
                battle.BattleConfig.EnemyMutualAttacksPeriod.min,
                battle.BattleConfig.EnemyMutualAttacksPeriod.max).TotalSeconds;

            float waitTimeSeconds = currentRandomPeriod - battle.BattleConfig.BattleAlertTime.TotalSeconds;
            //cor = StartCoroutine(WaitForPreparationCor(waitTimeSeconds));
            state = EnemyAttackState.Pause;
            battle.currentEnemyAttackType = EnemyAttackType.Mutual;
            StartWait(waitTimeSeconds);
        }

        if (state == EnemyAttackState.PauseComplete) TryGoFromCompletePauseToPreparation();
        if (state == EnemyAttackState.ReadyToAttack) Attack();
    }

    protected override void TryGoFromCompletePauseToPreparation()
    {
        if (state != EnemyAttackState.PauseComplete) return;
        if (countries._openEnemyCountries.Count < 2) return;

        // переход к EnemyAttackState.BattlePreparation

        battle.GetRandomEnemiesForMutualAttack(out enemyAttackerCountry, out enemyDefenderCountry);

        if (enemyAttackerCountry == null || enemyDefenderCountry == null || enemyAttackerCountry.LocalCountryData.Owner == enemyDefenderCountry.LocalCountryData.Owner) return;

        state = EnemyAttackState.BattlePreparation;

        // message
        //MessageService.Instance.ShowBattleWarningMessage(enemyAttackerCountry.CountryData.Id, enemyDefenderCountry.CountryData.Id);

        // ожидание до боя
        float waitTime = battle.BattleConfig.BattleAlertTime.TotalSeconds;
        StartWait(waitTime);
    }

    protected override void Attack()
    {
        if (!use) return;

        if (enemyAttackerCountry == null ||
            enemyDefenderCountry == null ||
            enemyAttackerCountry.LocalCountryData.Owner == CommonData.PlayerID)
        {
            state = EnemyAttackState.Idle;
            return;
        }

        state = EnemyAttackState.InAttack;
        battle.Battle(enemyAttackerCountry, enemyDefenderCountry, () => state = EnemyAttackState.Idle);
    }

    public void LoadCountries(Country enemyAttackerCountry, Country enemyDefenderCountry)
    {
        this.enemyAttackerCountry = enemyAttackerCountry;
        this.enemyDefenderCountry = enemyDefenderCountry;
    }
}