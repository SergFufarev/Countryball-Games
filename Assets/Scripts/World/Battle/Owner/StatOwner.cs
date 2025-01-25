using System;
using UnityEngine;

[Serializable]
public class StatOwner
{
    private float _currentStatValue;
    public float CurrentStatValue => _currentStatValue;

    private float _maxStatValue;
    public float MaxStatValue => _maxStatValue;

    private readonly Action<float> onChangeStatValueAction; // значение изменено
    private readonly Action onEmptyStatValueAction; // значение опустошено (например, закончилось HP или Energy)

    private readonly Transform transform;
    public Transform Transform => transform;

    public StatOwner(Transform transform, Action<float> onChangeStatValueAction, Action onEmptyStatValueAction)
    {
        this.transform = transform;

        this.onChangeStatValueAction = onChangeStatValueAction;
        this.onEmptyStatValueAction = onEmptyStatValueAction;
    }

    public StatOwner(Transform transform, float maxValue, Action<float> onChangeStatValueAction, Action onEmptyStatValueAction) : this(transform, onChangeStatValueAction, onEmptyStatValueAction)
    {
        this._maxStatValue = maxValue;
        _currentStatValue = maxValue;
    }

    public void SetMaxStatValue(float value)
    {
        _maxStatValue = value;
    }

    public void SetCurrentStatValue(float value)
    {
        float difference = value - _currentStatValue;
        _currentStatValue = value;
        onChangeStatValueAction?.Invoke(difference);
    }

    public void Reset()
    {
        float difference = _maxStatValue - _currentStatValue;
        _currentStatValue = _maxStatValue;
        onChangeStatValueAction?.Invoke(difference);
    }

    public void AddStatValue(float value)
    {
        _currentStatValue += value;

        onChangeStatValueAction?.Invoke(value);
    }

    public void ReduceStatValue(float force)
    {
        if (_currentStatValue <= 0) return;

        _currentStatValue -= force;

        bool empty = false;

        if (_currentStatValue <= 0)
        {
            _currentStatValue = 0;
            empty = true;
        }

        if (empty) Empty();
        else onChangeStatValueAction?.Invoke(-force);
    }

    private void Empty() => onEmptyStatValueAction?.Invoke();
}