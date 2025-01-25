using System;
using UnityEngine;

[Serializable]
public class Shooter
{
    protected float _force = 1;
    public float Force => _force;

    private GetShootingPosDelegate getCurrentShootingPos;

    public event Action<Shooter, IDamageOwner, float> ShootEvent;

    public Transform Transform => getCurrentShootingPos();

    public void SetShootingPos(GetShootingPosDelegate getCurrentShootingPos) => this.getCurrentShootingPos = getCurrentShootingPos;

    public Shooter(GetShootingPosDelegate getCurrentShootingPos) => SetShootingPos(getCurrentShootingPos);

    public virtual void Shoot(IDamageOwner goal, float? customForce = null)
    {
        ShootEvent?.Invoke(this, goal, customForce == null ? _force : (float)customForce);
    }
}

public delegate Transform GetShootingPosDelegate();