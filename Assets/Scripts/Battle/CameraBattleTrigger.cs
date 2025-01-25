using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Cinemachine;

namespace Battle
{
    public class CameraBattleTrigger : MonoBehaviour
    {
        private bool inMove = false;
        private const float moveSpeed = 1;
        private List<Unit> connectedUnits = new ();

        [Inject] private ArmiesController armies;

        private void Start()
        {
            armies.OnUnitDieEvent += OnUnitDie;
        }

        private void Update()
        {
            if (!inMove) return;

            transform.Translate(moveSpeed * Time.deltaTime * Vector3.forward);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit"))
            {
                var unit = other.GetComponent<Unit>();
                if (unit.Side == BattleSideType.Red) return;

                connectedUnits.Add(unit);
                if (connectedUnits.Count > 0) inMove = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Unit"))
            {
                var unit = other.GetComponent<Unit>();
                if (unit.Side == BattleSideType.Red) return;

                connectedUnits.Remove(unit);
                if (connectedUnits.Count == 0) inMove = false;
            }
        }

        private void OnUnitDie(Unit unit)
        {
            if (connectedUnits.Contains(unit))
            {
                connectedUnits.Remove(unit);
                if (connectedUnits.Count == 0) inMove = false;
            }
        }
    }
}