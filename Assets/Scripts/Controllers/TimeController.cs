using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour, IController
{
    private float _currentTimeScale = 1;
    public float CurrentTimeScale => _currentTimeScale;

    public void Stop()
    {
        Time.timeScale = 0;
    }

    public void Play()
    {
        Time.timeScale = _currentTimeScale;
    }

    public void SetTimeScale(float timeScale)
    {
        _currentTimeScale = timeScale;
        Time.timeScale = timeScale;
    }

    public void ResetTimeScale() => SetTimeScale(1);

    #region Comparable

    public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
    public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

    #endregion
}