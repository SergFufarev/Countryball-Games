using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeanBlockContainer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private LeanBlockElement[] elements = new LeanBlockElement[0];

    private const float DefaultFOV = 50;

    [ContextMenu("UpdateBlockers")]
    public void UpdateBlockers()
    {
        float size = mainCamera.fieldOfView / DefaultFOV;

        foreach (var element in elements) element.UpdateByPair(uiCamera, size);
    }
}