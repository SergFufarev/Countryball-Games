using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;

public abstract class BaseUpgradeUIElement : MonoBehaviour
{
    public abstract Transform BuyOnceTran { get; }

    //public abstract void Init(int id, UpgradeData data, Action<int, int, float> buyAction, Action<int, int> buyForAdAction, bool forceLock);

    public abstract void UpdateAmountAvailableForBuy(float money);
}

public enum ButtonActivityState
{
    /// <summary> Доступна покупка за голду </summary>
    AvailableToBuyForGold,

    /// <summary> Доступна покупка за рекламу </summary>
    AvailableToBuyForAds,

    /// <summary> Покупка недоступна из-за нехватки ресурсов </summary>
    Unavailable,

    /// <summary> Апгрейд полностью завершен </summary>
    FullCompleted
}