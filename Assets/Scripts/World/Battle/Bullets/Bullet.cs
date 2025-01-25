using System;
using UnityEngine;
using Battle;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] protected BulletType bulletType;
    [SerializeField] protected BulletDamageType damageType;
    [SerializeField] protected BulletFlyType flyType;
    [SerializeField] protected Transform visual;
    [SerializeField] protected float Speed = 8;
    [SerializeField] protected TrailRenderer trail;

    protected float _force;
    protected IDamageOwner _goal;
    protected Shooter owner;
    protected DamageReason shotReason;
    protected BattleSideType side;

    public float Force => _force;
    public BulletType BulletType => bulletType;
    public BulletDamageType DamageType => damageType;
    public BulletFlyType FlyType => flyType;
    public IDamageOwner Goal => _goal;
    public Shooter Owner => owner;

    protected Action<Bullet> endAction;

    public BattleSideType Side => side;

    protected void OnReachedGoal()
    {
        if (damageType == BulletDamageType.Default) _goal.Damage(owner, _force, shotReason);

        gameObject.SetActive(false);
        endAction?.Invoke(this);
        if (trail != null) trail.Clear();
    }
}