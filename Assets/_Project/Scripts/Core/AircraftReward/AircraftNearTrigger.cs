using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftNearTrigger : MonoBehaviour
{
    [SerializeField] private AircraftRoot root;

    public bool AircraftInFly => root.InFly;
}
