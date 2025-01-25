using System;
using UnityEngine;
using TMPro;

public class TimeText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    public void SetValue(TimeSpan value)
    {
        // format 12h 30m 45s
        // todo use text utility

        bool useHours = value.Hours > 0;
        string hourPart = useHours ? $"{value.Hours}h" : "";

        bool useMinutes = useHours || value.Minutes > 0;
        string minutesPart = useMinutes ? $"{value.Minutes}m" : "";

        string secondPart = $"{value.Seconds}s";

        label.text = $"{hourPart} {minutesPart} {secondPart}";
    }
}