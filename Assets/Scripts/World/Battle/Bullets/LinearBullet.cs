using System;
using Battle;

public class LinearBullet : Bullet
{
    private MovableToGoal _movableToGoal;
    
    public void Init(BattleSideType side, Shooter owner, float force, IDamageOwner goal, DamageReason reason, Action<Bullet> endAction)
    {
        this.owner = owner;

        this.side = side;
        _force = force;
        _goal = goal;
        shotReason = reason;
        _movableToGoal = new(MoveToGoalMode.Full3D, transform, Speed, 0.1f, OnReachedGoal);
        this.endAction = endAction;

        visual.LookAt(goal.DamageTransform);
    }

    public void MoveToGoal() => _movableToGoal.MoveTo(_goal.DamageTransform);
}
