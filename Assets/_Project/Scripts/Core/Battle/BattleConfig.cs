using System;
using UnityEngine;
using TheSTAR.Utility;
using Battle;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Data/BattleConfig")]
public class BattleConfig : ScriptableObject
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float battleDuration = 5f;
    [SerializeField] private int _bombForce = 150;
    [SerializeField] private float _bombSpeed = 5f;
    [SerializeField] private int _bombSkipRecoverCost = 100; // скип восстановления за харду

    [Header("Periods")]
    [SerializeField] private GameTimeSpan enemyAttackPeriod;
    [SerializeField] private GameTimeSpan revoltPeriod;
    [SerializeField] private GameTimeSpanRange enemyMutualAttacksPeriod;
    [SerializeField] private GameTimeSpan battleAlertTime;
    [SerializeField] private GameTimeSpan bombRecoverTime;

    [Header("Big Battle")]
    [SerializeField] private UnityDictionary<UnitType, UnitConfigData> unitDatas = new();
    [SerializeField] private RocketConfigData rocketData;
    [SerializeField] private UnityDictionary<BuildingType, BuildingConfigData> buildingDatas = new();

    //[Space]
    //[SerializeField] private bool autoShowArmyInfo = false;
    //public bool AutoShowArmyInfo => autoShowArmyInfo;

    public float MoveSpeed => _moveSpeed;
    public float BattleDuration => battleDuration;
    public int BombForce => _bombForce;
    public float BombSpeed => _bombSpeed;
    public UnitConfigData GetUnitData(UnitType unitType) => unitDatas.Get(unitType);
    public RocketConfigData RocketData => rocketData;
    public BuildingConfigData GetBuildingData(BuildingType buildingType) => buildingDatas.Get(buildingType);

    public GameTimeSpan EnemyAttackPeriod => enemyAttackPeriod;
    public GameTimeSpan RevoltPeriod => revoltPeriod;
    public GameTimeSpanRange EnemyMutualAttacksPeriod => enemyMutualAttacksPeriod;
    public GameTimeSpan BattleAlertTime => battleAlertTime;
    public GameTimeSpan BombRecoverTime => bombRecoverTime;
}

[Serializable]
public struct UnitConfigData
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string name;
    [SerializeField] private int hp;
    [SerializeField] private float attackSpeed;
    [SerializeField] private int damage;
    [SerializeField] private float attentionDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private int squadSize;
    [SerializeField] private float moveSpeed;
    [SerializeField] private BulletType bulletType;
    [SerializeField] private string description;

    public Sprite Icon => icon;
    public string Name => name;
    public int Hp => hp;
    public float AttackSpeed => attackSpeed;
    public int Damage => damage;
    public float AttentionDistance => attentionDistance;
    public float AttackDistance
    {
        get
        {
            return attackDistance;
        }
        set
        {
            attackDistance = value;
        }
    }
    public int SquadSize => squadSize;
    public float MoveSpeed => moveSpeed;
    public BulletType BulletType => bulletType;
    public string Description => description;
}

[Serializable]
public struct RocketConfigData
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string name;
    [SerializeField] private int cost;
    [SerializeField] private int damage;
    [SerializeField] private string description;
    [SerializeField] private int radius;

    public Sprite Icon => icon;
    public string Name => name;
    public int Cost => cost;
    public int Damage => damage;
    public string Description => description;
    public int Radius => radius;
}

[Serializable]
public struct BuildingConfigData
{
    [SerializeField] private int hp;
    [SerializeField] private float shootDistanceMultiplier;

    public int Hp => hp;
    public float ShootDistanceMultiplier => shootDistanceMultiplier;

}