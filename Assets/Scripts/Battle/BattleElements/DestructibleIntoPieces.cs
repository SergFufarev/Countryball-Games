using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestructibleIntoPieces : MonoBehaviour
{
    [SerializeField] private List<DestructiblePiece> parts; // разлетающиеся части
    [SerializeField] private Transform physicForcePos; // в какую точку прикладывается физическая сила
    [SerializeField] private float explosionForce = 150;
    [SerializeField] private float explosionRadius = 5;
    [SerializeField] private bool autoHidePieces = true;

    [ShowIf("autoHidePieces")]
    [SerializeField] private float disappearancePartsDelay = 2;

    private const float PartsDisappearanceTime = 0.5f;

    private bool initialized = false;
    private bool inIdle = true;

    Tweener waitTweener;

    public void Init()
    {
        if (initialized) return;

        SavePartPositions();

        initialized = true;
    }

    [ContextMenu("SavePartPositions")]
    private void SavePartPositions()
    {
        for (int i = 0; i < parts.Count; i++) parts[i].SaveDefaultData();
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        for (int i = 0; i < parts.Count; i++) parts[i].Reset();

        if (waitTweener != null)
        {
            waitTweener.Kill();
            waitTweener = null;
        }

        inIdle = true;
    }

    [ContextMenu("Destruct")]
    public void Destruct() => Destruct(null);
    public void Destruct(Transform pos)
    {
        gameObject.SetActive(true);

        if (pos != null)
        {
            //transform.SetPositionAndRotation(pos.position, pos.rotation);
            transform.position = pos.position;
        }
        if (!inIdle) Reset();
        inIdle = false;

        waitTweener = DOVirtual.Float(0, 1, 0.1f, value => { }).OnComplete(() =>
        {
            for (int i = 0; i < parts.Count; i++) parts[i].Destruct(explosionForce, physicForcePos.position, explosionRadius);

            if (!autoHidePieces) return;
            waitTweener = DOVirtual.Float(0, 1, disappearancePartsDelay, value => { }).OnComplete(DisappearanceParts);
        });
    }

    void DisappearanceParts()
    {
        for (int i = 0; i < parts.Count; i++) parts[i].Disappearance(PartsDisappearanceTime);
    }
}