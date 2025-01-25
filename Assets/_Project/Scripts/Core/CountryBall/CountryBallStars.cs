using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountryBallStars : MonoBehaviour
{
    [SerializeField] private Image fill;

    public void SetProgress(float progress)
    {
        fill.fillAmount = progress;
    }
}