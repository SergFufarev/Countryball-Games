using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimulator
{
    /// <summary>
    /// Начать симуляцию сначала
    /// </summary>
    void StartSimulate();

    /// <summary>
    /// Полностью остановить симуляцию
    /// </summary>
    void StopSimulate();

    /// <summary>
    /// Симулировать кадр
    /// </summary>
    void Simulate();

    /// <summary>
    /// Приостановить симуляцию
    /// </summary>
    void PauseSimulate();

    /// <summary>
    /// Возобновить симуляцию
    /// </summary>
    void ContinueSimulate();
}