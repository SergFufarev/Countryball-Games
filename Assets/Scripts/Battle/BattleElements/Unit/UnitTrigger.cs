using System;
using UnityEngine;

namespace Battle
{
    public class UnitTrigger : MonoBehaviour
    {
        [SerializeField] private SphereCollider col;

        private BattleSideType side;
        private Action<BattleElement> onFindEnemyElement;

        public void Init(BattleSideType side, Action<BattleElement> onFindEnemyElement)
        {
            this.side = side;
            this.onFindEnemyElement = onFindEnemyElement;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BattleBuilding") || other.CompareTag("Unit"))
            {
                var battleElement = other.GetComponent<BattleElement>();
                if (battleElement == null) return;

                if (side != battleElement.Side)
                {
                    onFindEnemyElement(battleElement);
                }
            }
        }

        public void SetRadius(float radius)
        {
            col.radius = radius;
        }
    }
}