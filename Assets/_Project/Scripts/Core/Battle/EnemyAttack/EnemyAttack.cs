using System;
using System.Collections;
using UnityEngine;
using TheSTAR.Utility;
using FunnyBlox;
using Zenject;

public abstract class EnemyAttack : MonoBehaviour
{
    protected BattleInGameService battle;

    [SerializeField] protected bool use;
    [SerializeField] protected EnemyAttackState state;
    [SerializeField] protected float waitTime;

    public bool Use => use;
    public EnemyAttackState State => state;
    public float WaitTime => waitTime;

    [Inject] protected readonly VisualCountryController countries;

    protected bool inWait = false;

    private void Update()
    {
        if (!inWait) return;

        waitTime -= Time.deltaTime;

        if (waitTime <= 0)
        {
            waitTime = 0;
            inWait = false;

            if (state == EnemyAttackState.Pause)
            {
                state = EnemyAttackState.PauseComplete;
                TryGoFromCompletePauseToPreparation();
            }
            else if (state == EnemyAttackState.BattlePreparation)
            {
                state = EnemyAttackState.ReadyToAttack;
                Try();
            }
        }
    }

    public void Break()
    {
        state = EnemyAttackState.Idle;
        inWait = false;
        waitTime = 0;
    }

    public void Load(EnemyAttackState state, float waitTime)
    {
        this.state = state;

        if (state == EnemyAttackState.Pause || state == EnemyAttackState.BattlePreparation) StartWait(waitTime);
    }

    /// <summary>
    /// Останавливает текущие ожидания, подготовки, если такие есть. Начинает ожидание до следующего нападения, если это возможно
    /// </summary>
    public void Restart()
    {
        Break();
        Try();
    }

    public void Init(BattleInGameService battle)
    {
        this.battle = battle;
    }

    public abstract void Try();

    protected void StartWait(float waitTimeSeconds)
    {
        waitTime = waitTimeSeconds;
        inWait = true;
    }

    protected abstract void TryGoFromCompletePauseToPreparation();

    protected abstract void Attack();
}