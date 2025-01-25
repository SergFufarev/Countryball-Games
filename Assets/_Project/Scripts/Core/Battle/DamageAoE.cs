using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Battle
{
    public class DamageAoE : MonoBehaviour
    {
        [SerializeField] private SphereCollider col;

        private float maxRadius;
        private float force;
        private BattleSideType side;

        private const float Duration = 0.5f;

        public void Init(BattleSideType side, float force, float maxRadius)
        {
            this.side = side;
            this.maxRadius = maxRadius;
            this.force = force;
        }

        public void DoEffect()
        {
            DOVirtual.Float(0.01f, maxRadius, Duration, value =>
            {
                col.radius = value;
            }).OnComplete(End);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit"))
            {
                var unit = other.GetComponent<Unit>();
                if (unit.Side == side) return;

                unit.Damage(null, force, DamageReason.Rocket);
            }
            else if (other.CompareTag("BattleBuilding"))
            {
                var building = other.GetComponent<BattleBuilding>();
                if (building == null || building.Side == side) return;

                building.Damage(null, force, DamageReason.Rocket);
            }
        }

        private void End()
        {
            Destroy(gameObject);
        }
    }
}