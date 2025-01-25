using UnityEngine;
using TheSTAR.Utility;

public class BattleWinEffect : MonoBehaviour
{
    [SerializeField] private GameObject[] fireworkPrefabs;
    [SerializeField] private Transform[] fireworkPositions;

    [SerializeField] private float delayMin = 0.1f;
    [SerializeField] private float delayMax = 0.3f;

    private Transform previosPos;

    [ContextMenu("Play")]
    public void Play()
    {
        GenerateRandomFirework();
    }

    private void GenerateRandomFirework()
    {
        var fireworkPrefab = ArrayUtility.GetRandomValue(fireworkPrefabs);
        var fireworkPosition = ArrayUtility.GetRandomValue(fireworkPositions, new Transform[] { previosPos });

        Instantiate(fireworkPrefab, fireworkPosition.position, Quaternion.identity, fireworkPosition);
        previosPos = fireworkPosition;

        Invoke(nameof(GenerateRandomFirework), Random.Range(delayMin, delayMax));
    }
}
