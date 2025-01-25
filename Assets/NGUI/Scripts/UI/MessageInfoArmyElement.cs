using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FunnyBlox;

public class MessageInfoArmyElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI forceTitle;

    public void SetValue(int force)
    {
        forceTitle.text = $"Force: {force}/{BattleInGameService.ArmyMaxForce}";
    }
}