using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class Squad : MonoBehaviour
    {
        [SerializeField] private Collider col;
        [SerializeField] private Cell currentCell;

        public Cell GetCurrentCell => currentCell;

        public void SetCurrentCell(Cell cell) => currentCell = cell;

        private Unit[] currentUnits;

        private BattleSideType battleSideType;
        private UnitType squadType;
        private CustomizationFaceType unitFaceType;
        private bool inSmoothMove = false;

        public Unit[] Units => currentUnits;
        public BattleSideType BattleSideType => battleSideType;
        public UnitType SquadType => squadType;
        public bool InSmoothMove => inSmoothMove;

        public void Init(BattleSideType battleSideType, UnitType squadType, CustomizationFaceType unitFaceType)
        {
            this.battleSideType = battleSideType;
            this.squadType = squadType;
            this.unitFaceType = unitFaceType;
        }

        public void GenerateUnits(
            ArmiesController armies,
            BulletsContainer bullets,
            UnitConfigData data,
            Transform finalMoveGoal,
            Unit unitPrefab,
            Material flagMaterial,
            UnitVisual visualPrefab)
        {
            if (currentUnits != null && currentUnits.Length > 0)
            {
                for (int i = 0; i < currentUnits.Length; i++) Destroy(currentUnits[i].gameObject);
            }

            currentUnits = new Unit[data.SquadSize];
            for (int i = 0; i < data.SquadSize; i++) GenerateUnit(i);

            void GenerateUnit(int index)
            {
                var unit = Instantiate(unitPrefab, transform.position, Quaternion.identity, transform);
                unit.transform.localPosition = GetPositionForUnit(data.SquadSize, index);
                unit.GenerateVisual(visualPrefab);
                unit.Init(armies, bullets, data, finalMoveGoal, battleSideType, unitFaceType, flagMaterial);
                currentUnits[index] = unit;
                unit.name = squadType.ToString();

                //bool isCountryBallUnit = squadType == UnitType.Infantry || squadType == UnitType.Snipers || squadType == UnitType.Mortar;
                //if (isCountryBallUnit)
                //if (Random.Range(0f, 1f) > 0.5f) return;

                unit.InitPlayStaticAnimation(Random.Range(0f, 2f));
            }
        }

        public Vector3 GetPositionForUnit(int squadSize, int unitIndex)
        {
            return unitPositions[squadSize - 1][unitIndex];
        }

        private readonly List<List<Vector3>> unitPositions = new List<List<Vector3>>()
        {
            {
                new List<Vector3>()
                {
                    new Vector3(0, 0, 0)
                }
            },
            {
                new List<Vector3>()
                {
                    new Vector3(-0.15f, 0, 0),
                    new Vector3(0.15f, 0, 0)
                }
            },
            {
                new List<Vector3>()
                {
                    new Vector3(-0.15f, 0, -0.15f),
                    new Vector3(0.15f, 0, -0.15f),
                    new Vector3(0, 0, 0.15f)
                }
            },
            {
                new List<Vector3>()
                {
                    new Vector3(-0.15f, 0, -0.15f),
                    new Vector3(-0.15f, 0, 0.15f),
                    new Vector3(0.15f, 0, -0.15f),
                    new Vector3(0.15f, 0, 0.15f)
                }
            },
            {
                new List<Vector3>()
                {
                    new Vector3(0, 0, 0),
                    new Vector3(-0.18f, 0, -0.18f),
                    new Vector3(-0.18f, 0, 0.18f),
                    new Vector3(0.18f, 0, -0.18f),
                    new Vector3(0.18f, 0, 0.18f)
                }
            }
        };

        public void SetColliderActivity(bool activity)
        {
            col.enabled = activity;
        }

        #region Drag

        public void StartDrag()
        {
            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        public void Drag()
        {

        }

        public void EndDrag()
        {
            transform.localScale = Vector3.one;
        }

        public void StartSmoothMove()
        {
            inSmoothMove = true;
        }

        public void EndSmoothMove()
        {
            inSmoothMove = false;
        }

        #endregion
    }
}