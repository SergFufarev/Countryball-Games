using System;
using Battle;
using DG.Tweening;
using UnityEngine;
using TheSTAR.Utility;

public class BallisticBullet : Bullet
{
    private float flyDuration;
    private AnimationCurve heightCurve;
    private const float Height = 2;

    private float minAngle = -45;
    private float maxAngle = 45;

    private Vector3 baseEulerAngles;

    public void Init(AnimationCurve heightCurve, BattleSideType side, Shooter owner, float force, float flyDuration, IDamageOwner goal, DamageReason reason, Action<Bullet> endAction)
    {
        this.owner = owner;

        this.side = side;
        _force = force;
        this.flyDuration = flyDuration;
        _goal = goal;
        shotReason = reason;
        this.endAction = endAction;
        this.heightCurve = heightCurve;

        visual.LookAt(goal.DamageTransform);

        baseEulerAngles = visual.localEulerAngles;
    }

    public void StartFly()
    {
        DOVirtual.Float(0, 1, flyDuration, value =>
        {
            Vector3 tempPos = MathUtility.ProgressToValue(value, owner.Transform.position, _goal.DamageTransform.position) + new Vector3(0, heightCurve.Evaluate(value) * Height, 0);
            transform.position = tempPos;
            visual.localEulerAngles = new Vector3(MathUtility.ProgressToValue(value, minAngle, maxAngle), baseEulerAngles.y, baseEulerAngles.z);
        }).OnComplete(OnReachedGoal).SetEase(Ease.Linear);
    }
}