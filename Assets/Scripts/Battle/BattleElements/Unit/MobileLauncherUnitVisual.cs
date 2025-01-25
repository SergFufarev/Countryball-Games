using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileLauncherUnitVisual : UnitVisual
{
    [Header("Mobile Launcher")]
    [SerializeField] private GameObject[] rockets;

    public override void OnStartBattle()
    {
        base.OnStartBattle();

        for (int i = 0; i < rockets.Length; i++) rockets[i].SetActive(false);
    }
}