using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility.Pointer;

public class SettingsUiOption : MonoBehaviour
{
    [SerializeField] private GameObject[] onObjects = new GameObject[0];
    [SerializeField] private GameObject[] offObjects = new GameObject[0];
    [SerializeField] private PointerButton[] toggleButtons;

    public void Init(Action action)
    {
        foreach (var element in toggleButtons) element.Init(action);
    }

    public void SetValue(bool value)
    {
        foreach (var onObject in onObjects) onObject.SetActive(value);

        foreach (var offObject in offObjects) offObject.SetActive(!value);
    }
}