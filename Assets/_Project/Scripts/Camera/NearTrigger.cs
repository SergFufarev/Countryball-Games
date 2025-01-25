using System;
using UnityEngine;
using FunnyBlox;
using System.Collections.Generic;

public class NearTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider col;

    public event Action OnNearAircraft;
    public event Action OnEndNearAircraft;
    public event Action<CountryBall> OnNearCountryBall;
    public event Action<CountryBall> OnEndNearCountryBall;

    private List<CountryBall> nearBalls = new();

    public List<CountryBall> NearBalls => nearBalls;

    [SerializeField] private int nearBallsCount;

    private bool showHideNearBalls;

    public void Init(bool showHideNearBalls)
    {
        this.showHideNearBalls = showHideNearBalls;
    }

    public void Set(float size, Vector3 center)
    {
        col.size = new Vector3(size / 2, col.size.y, size);
        col.center = center;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CountryBall"))
        {
            var ball = other.GetComponent<CountryBall>();
            if (showHideNearBalls) ball.SetVisualActivity(true);
            nearBalls.Add(ball);
            nearBallsCount = nearBalls.Count;
            OnNearCountryBall?.Invoke(ball);
        }
        else if (other.CompareTag("AircraftNearTrigger"))
        {
            var aircraftNearTrigger = other.GetComponent<AircraftNearTrigger>();
            if (aircraftNearTrigger.AircraftInFly) OnNearAircraft?.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CountryBall"))
        {
            var ball = other.GetComponent<CountryBall>();
            if (showHideNearBalls) ball.SetVisualActivity(false);
            nearBalls.Remove(ball);
            nearBallsCount = nearBalls.Count;
            OnEndNearCountryBall?.Invoke(ball);
        }
        else if (other.CompareTag("AircraftNearTrigger"))
        {
            OnEndNearAircraft?.Invoke();
        }
    }
}