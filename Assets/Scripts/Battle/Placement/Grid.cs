using System;
using UnityEngine;

namespace Battle
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private BattleSideType sideType;
        [SerializeField] private CellLine[] cellLines;
        [SerializeField] private Cell[] resistanceCells;
        [SerializeField] private Collider col;

        public const int TotalSize = 30;

        public CellLine[] CellLines => cellLines;
        public Cell[] RevoltCells => resistanceCells;
        public BattleSideType SideType => sideType;

        public int GridWidth => cellLines[0].Cells.Length;

        public Cell GetCellByTotalIndex(int totalIndex)
        {
            return CellLines[totalIndex / GridWidth].Cells[totalIndex % GridWidth];
        }

        public void SetColliderActivity(bool active)
        {
            col.enabled = active;
        }

        [Serializable]
        public struct CellLine
        {
            [SerializeField] private Cell[] cells;
            public Cell[] Cells => cells;
        }

        public Cell FindNearCell(Vector3 target)
        {
            // find near line

            int bestLineIndex = -1;
            float bestLineDistance = -1;

            for (int lineIndex = 0; lineIndex < cellLines.Length; lineIndex++)
            {
                if (lineIndex == 0)
                {
                    bestLineIndex = lineIndex;
                    bestLineDistance = Vector3.Distance(target, cellLines[lineIndex].Cells[0].transform.position);
                }
                else
                {
                    float distance = Vector3.Distance(target, cellLines[lineIndex].Cells[0].transform.position);
                    if (distance < bestLineDistance)
                    {
                        bestLineIndex = lineIndex;
                        bestLineDistance = distance;
                    }
                    else break;
                }
            }

            // find near cell in line

            int bestCellIndex = -1;
            float bestCellDistance = -1;

            for (int cellIndex = 0; cellIndex < GridWidth; cellIndex++)
            {
                if (cellIndex == 0)
                {
                    bestCellIndex = cellIndex;
                    bestCellDistance = Vector3.Distance(target, cellLines[bestLineIndex].Cells[cellIndex].transform.position);
                }
                else
                {
                    float distance = Vector3.Distance(target, cellLines[bestLineIndex].Cells[cellIndex].transform.position);
                    if (distance < bestCellDistance)
                    {
                        bestCellIndex = cellIndex;
                        bestCellDistance = distance;
                    }
                    else break;
                }
            }

            return cellLines[bestLineIndex].Cells[bestCellIndex];
        }
    }
}