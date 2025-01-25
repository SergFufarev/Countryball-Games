using System;
using UnityEngine;

[Serializable]
public class HpOwner : StatOwner
{
    public float CurrentHp => CurrentStatValue;
    public float MaxHp => MaxStatValue;

    public HpOwner(Transform transform, Action<float> onChangeHpAction, Action onDeadAction) : base(transform, onChangeHpAction, onDeadAction)
    {
    }

    public HpOwner(Transform transform, float maxHp, Action<float> onChangeHpAction, Action onDeadAction) : base(transform, maxHp, onChangeHpAction, onDeadAction)
    {
    }

    public void AddHP(float value) => AddStatValue(value);

    public void Damage(float value) => ReduceStatValue(value);

    public void SetMaxHp(float value, bool autoSetCurrentHp = true)
    {
        float difference = value - MaxStatValue;
        SetMaxStatValue(value);

        if (autoSetCurrentHp) SetCurrentStatValue(CurrentStatValue + difference);
    }
}

public enum HpOwnerStatus
{
    Alive,
    Dead
}

public interface IHpReactable
{
    void OnChangeHpReact(HpOwner hpOwner);
}

public interface IDamageOwner
{
    public HpOwner HpOwner { get; }
    void Damage(Shooter shooter, float value, DamageReason reason);

    public Transform DamageTransform { get; }
}

public enum DamageReason
{
    Unit,
    Tower,
    Rocket
}