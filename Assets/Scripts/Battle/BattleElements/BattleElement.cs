using System;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Представляет элемент боя, имеющий сторону (зелёную или красную), ищеющий здоровье и способный быть уничтоженным
    /// </summary>
    public abstract class BattleElement : MonoBehaviour, IDamageOwner
    {
        protected float maxHp;
        [SerializeField] protected BattleSideType side;

        public event Action<IDamageOwner> onDieEvent;

        private HpOwner hp;

        public BattleSideType Side => side;
        public HpOwner HpOwner => hp;
        public virtual Transform DamageTransform => transform;

        public void InitStats()
        {
            hp = new HpOwner(transform, OnChangeHP, OnDie);
            hp.SetMaxHp(maxHp);
        }

        public virtual void Damage(Shooter shooter, float value, DamageReason reason)
        {
            hp.Damage(value);
        }

        private void OnChangeHP(float value)
        {
            // todo update hp bar
        }

        public virtual void OnDie()
        {
            gameObject.SetActive(false);
            onDieEvent?.Invoke(this);
        }

        public virtual void OnStartBattle()
        {
        }

        public virtual void OnEndBattle(bool win)
        {
        }
    }
}