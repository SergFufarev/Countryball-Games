using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using Zenject;
using Battle;

public class BulletsContainer : MonoBehaviour, ISimulator
{
    [Inject] private BattleController battle;
    [SerializeField] private UnityDictionary<BulletType, Bullet> bullets;
    [SerializeField] private AnimationCurve balisticHeightCurve;

    private List<LinearBullet> _activeLinearBullets = new();
    private Dictionary<BulletType, List<Bullet>> _bulletsPool;

    private bool _isSimulate = false;

    public void Init()
    {
        var allBulletTypes = EnumUtility.GetValues<BulletType>();

        _bulletsPool = new();
        foreach (var bulletType in allBulletTypes) _bulletsPool.Add(bulletType, new());
    }

    private float BallisticFlyTime = 2;

    public void Shoot(
        BattleSideType side,
        BulletType bulletType,
        Shooter shooter,
        IDamageOwner goal,
        DamageReason shotReason,
        float force)
    {
        Bullet bullet = PoolUtility.GetPoolObject(_bulletsPool[bulletType], info => !info.gameObject.activeSelf, shooter.Transform.position, CreateNewBullet);

        if (bullet.FlyType == BulletFlyType.Linear)
        {
            var linearBullet = (LinearBullet)bullet;
            linearBullet.Init(side, shooter, force, goal, shotReason, OnBulletReachedGoal);
            _activeLinearBullets.Add(linearBullet);
        }
        else
        {
            var ballisticBullet = (BallisticBullet)bullet;
            ballisticBullet.Init(balisticHeightCurve, side, shooter, force, BallisticFlyTime, goal, shotReason, OnBulletReachedGoal);
            ballisticBullet.StartFly();
        }
        
        Bullet CreateNewBullet(Vector3 pos)
        {
            var bullet = Instantiate(bullets.Get(bulletType), shooter.Transform.position, Quaternion.identity, transform);
            _bulletsPool[bulletType].Add(bullet);
            return bullet;
        }
    }

    #region Simulation

    public void StartSimulate()
    {
        if (_isSimulate) return;

        _isSimulate = true;
    }

    public void StopSimulate()
    {
        _isSimulate = false;
        HideBullets();

        _activeLinearBullets.Clear();
    }

    public void Simulate()
    {
        if (!_isSimulate) return;

        for (int i = 0; i < _activeLinearBullets.Count; i++)
        {
            LinearBullet b = _activeLinearBullets[i];
            b.MoveToGoal();
        }
    }

    public void PauseSimulate()
    {
        _isSimulate = false;
    }

    public void ContinueSimulate()
    {
        _isSimulate = true;
    }

    #endregion

    private const float AoeRadius = 1;
    private const float AoeBigRadius = 1.5f;

    private void OnBulletReachedGoal(Bullet b)
    {
        if (b.FlyType == BulletFlyType.Linear) _activeLinearBullets.Remove((LinearBullet)b);

        if (b.DamageType == BulletDamageType.AoE)
        {
            battle.CreateAreaOfEffectDamage(b.Side, b.transform.position, (int)b.Force, AoeRadius);
        }
        else if (b.DamageType == BulletDamageType.Big_AoE)
        {
            battle.CreateAreaOfEffectDamage(b.Side, b.transform.position, (int)b.Force, AoeBigRadius);
        }
    }

    private void HideBullets()
    {
        foreach (var pool in _bulletsPool.Values)
        {
            foreach (var b in pool) b.gameObject.SetActive(false);
        }
    }
}

public enum BulletType
{
    MachineGunFire,
    Shot_1,
    UnitBallisticRocket,
    BlackBullet,
    UnitMiniBallisticRockets,
    UnitLinearRocket
}

public enum BulletDamageType
{
    Default,
    AoE,
    Big_AoE
}

public enum BulletFlyType
{
    Linear,
    Ballistic
}