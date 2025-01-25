using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;

public class CountryBallMaxSizeHelper : MonoBehaviour
{
    [SerializeField] private Country[] countries;
    [SerializeField] private CountriesData countriesConfig;

    private List<CountryBall> mainCountryBalls;

    private const float TestMinSize = 0.1f;
    private const float TestMaxSize = 5f;

#if UNITY_EDITOR
    private void GetAllMainCountryBalls()
    {
        mainCountryBalls = new();

        foreach (var country in countries)
        {
            mainCountryBalls.Add(country.GetComponentInChildren<CountryBall>());
        }
    }

    private void PrepareBalls()
    {
        foreach (var ball in mainCountryBalls) ball.PrepareColliderToTestMaxSize();
    }

    private void Test()
    {
        GetAllMainCountryBalls();
        PrepareBalls();
    }

#endif
}