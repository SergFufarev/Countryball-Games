using UnityEngine;

namespace Battle
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Squad currentSquad;

        public bool IsEmpty => CurrentSquadType == null;

        public UnitType? CurrentSquadType
        {
            get
            {
                if (currentSquad == null) return null;
                else return currentSquad.SquadType;
            }
        }

        public void SetSquad(Squad squad)
        {
            currentSquad = squad;
        }

        public Squad GetSquad()
        {
            return currentSquad;
        }
    }
}