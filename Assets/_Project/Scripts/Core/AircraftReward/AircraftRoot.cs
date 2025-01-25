using UnityEngine;
using TheSTAR.GUI.FlyUI;

public class AircraftRoot : MonoBehaviour, IUiFlySender
{
    [SerializeField] private Collider[] cols;
    [SerializeField] private bool inFly;
    public bool InFly => inFly;

    public Transform startSendPos => transform;

    private void SetColliderActivity(bool value)
    {
        foreach (var col in cols)
            col.enabled = value;
    }

    public void OnStartFly()
    {
        inFly = true;
        SetColliderActivity(true);
    }

    public void OnEndFly()
    {
        inFly = false;
        SetColliderActivity(false);
    }
}
