using UnityEngine;
using TheSTAR.Utility;
using Zenject;
using DG.Tweening;

namespace Battle
{
    public class ArmyDragHelper : MonoBehaviour
    {
        [Inject] private ArmiesController armies;

        private bool drag;
        private Squad currentDragSquad;

        private const float SmoothDragDuration = 0.1f;
        private Vector3 startDragPos;
        private float smoothMoveProgress;

        public void OnDown()
        {
            if (drag) return;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            if (Physics.Raycast(ray, out RaycastHit hit, 13))
            {
                if (hit.collider.CompareTag("Squad"))
                {
                    var squad = hit.collider.GetComponent<Squad>();
                    if (squad.InSmoothMove || squad.BattleSideType == BattleSideType.Red) return;

                    currentDragSquad = squad;
                    currentDragSquad.StartDrag();
                    armies.SetSquadsColliderActivity(currentDragSquad.BattleSideType, false);
                    smoothMoveProgress = 0;
                    startDragPos = currentDragSquad.transform.position;
                    drag = true;
                }
            }
        }

        public void OnDrag()
        {
            if (!drag) return;

            if (smoothMoveProgress < 1)
            {
                smoothMoveProgress += Time.deltaTime / SmoothDragDuration;
                if (smoothMoveProgress > 1) smoothMoveProgress = 1;
            }

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                    if (smoothMoveProgress < 1)
                    {
                        currentDragSquad.transform.position = MathUtility.ProgressToValue(smoothMoveProgress, startDragPos, hit.point);
                    }
                    else currentDragSquad.transform.position = hit.point;
                }
            }
        }

        public void OnUp()
        {
            if (!drag) return;

            armies.SetSquadsColliderActivity(currentDragSquad.BattleSideType, true);
            currentDragSquad.EndDrag();
            PlaceSquadIntoNearCell(currentDragSquad);
            currentDragSquad = null;
            smoothMoveProgress = 0;
            startDragPos = Vector3.zero;
            drag = false;
        }

        public void PlaceSquadIntoNearCell(Squad squad)
        {
            // find near cell
            var nearCell = armies.GetGrid(squad.BattleSideType).FindNearCell(squad.transform.position);
            Vector3 endPos = nearCell.transform.position;

            // smooth move to endPos

            squad.StartSmoothMove();

            currentDragSquad.transform.DOMove(endPos, SmoothDragDuration).OnComplete(() =>
            {
                squad.EndSmoothMove();
            });

            // check for replace

            if (squad.GetCurrentCell == nearCell)
            {
                // do nothing
            }
            else if (nearCell.GetSquad() == null)
            {
                squad.GetCurrentCell.SetSquad(null);
                nearCell.SetSquad(squad);
                squad.SetCurrentCell(nearCell);
            }
            else
            {
                var fromCell = squad.GetCurrentCell;
                var toCell = nearCell;
                var squadToReplace = nearCell.GetSquad();

                squadToReplace.transform.position = fromCell.transform.position;
                squadToReplace.SetCurrentCell(fromCell);
                fromCell.SetSquad(squadToReplace);

                toCell.SetSquad(squad);
                squad.SetCurrentCell(toCell);
            }
        }
    }
}