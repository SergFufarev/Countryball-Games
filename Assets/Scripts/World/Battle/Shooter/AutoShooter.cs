using System;
using System.Collections.Generic;
using DG.Tweening;

[Serializable]
public class AutoShooter : Shooter
{
    protected float _period = 1;
    protected List<IDamageOwner> _goals;
    protected bool _isSimulate;
    protected ShootStatus _status;

    public event Action ShootingEntry; // вызывается когда добавлена первая цель
    public event Action ShootingExit; // вызывается когда удалена последняя цель

    public bool ContainsAnyGoal => _goals.Count > 0;

    public AutoShooter(float force, float period, GetShootingPosDelegate getCurrentShootingPos) : base (getCurrentShootingPos)
    {
        this._force = force;
        this._period = period;
        _status = ShootStatus.ReadyToShoot;
        _goals = new();
    }

    public bool ContainsGoal(IDamageOwner goal) => _goals.Contains(goal);

    public void StartSimulateShooting()
    {
        if (_isSimulate) return;

        _isSimulate = true;

        TryShoot();
    }

    public void StopSimulateShooting()
    {
        _isSimulate = false;
    }

    public virtual void AddGoal(IDamageOwner goal)
    {
        if (ContainsGoal(goal)) return;

        _goals.Add(goal);
        TryShoot();

        if (_goals.Count == 1) ShootingEntry?.Invoke();
    }

    public virtual void RemoveGoal(IDamageOwner goal)
    {
        if (!_goals.Contains(goal)) return;
        _goals.Remove(goal);

        if (_goals.Count == 0) ShootingExit?.Invoke();
    }

    public void ClearGoals()
    {
        _goals.Clear();
    }

    protected virtual void TryShoot()
    {
        if (_isSimulate && _status == ShootStatus.ReadyToShoot && _goals.Count > 0) Shoot(_goals[0]);
    }

    public override void Shoot(IDamageOwner goal, float? customForce = null)
    {
        base.Shoot(goal, customForce);
        StartRecharging();
    }

    public void ShootWithoutRecharging(IDamageOwner goal, float? customForce = null) => base.Shoot(goal, customForce);

    protected virtual void StartRecharging()
    {
        _status = ShootStatus.Recharging;

        ResetLeanTween();

        rechargingTween = DOVirtual.Float(0, 1, _period, value => { }).OnComplete(EndRecharging);
    }

    private Tweener rechargingTween;

    private void ResetLeanTween()
    {
        if (rechargingTween != null)
        {
            rechargingTween.Kill();
            //LeanTween.cancel((int)leanTweenId);
            //leanTweenId = null;
        }
    }

    protected void EndRecharging()
    {
        _status = ShootStatus.ReadyToShoot;
        TryShoot();
    }
}

public enum ShootStatus
{
    ReadyToShoot,
    Recharging
}