using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility;

public class BallPositionHelper : MonoBehaviour
{
    private Country[] countries;
    private CountriesData config;

#if UNITY_EDITOR

    public void ParseToConfig(Country[] countries, CountriesData config)
    {
        this.config = config;
        this.countries = countries;
        ParseToConfig();
    }

    private void ParseToConfig()
    {
        var ballTypes = EnumUtility.GetValues<CountryBallType>();

        foreach (var ballType in ballTypes) ParseToConfigForType(ballType);
    }

    private void ParseToConfigForType(CountryBallType ballType)
    {
        foreach (var country in countries)
        {
            var countryInConfig = config.CountryData.Find(info => info.Name == country.LocalCountryData.Name);
            var ballsInCountry = country.GetComponentsInChildren<CountryBall>();
            CountryBall ball = Array.Find(ballsInCountry, info => info.CountryBallType == ballType);

            for (int i = 0; i < ballsInCountry.Length; i++)
            {
                if (ballsInCountry[i].CountryBallType == ballType)
                {
                    var localPos = ball.transform.localPosition;
                    var x = localPos.x;
                    var y = localPos.y;
                    var z = localPos.z;

                    countryInConfig.SetSmartPositionForBall(ballType, x, y, z);
                    break;
                }
            }
        }

        config.Save();
    }

#endif
}