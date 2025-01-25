using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeanBlockElement : MonoBehaviour
{
    [SerializeField] private Transform uiPair;
    [SerializeField] private Vector3 defaultSize;

    public void UpdateByPair(Camera uiCamera, float sizeMultiplier)
    {
        if (uiPair == null) return;
        gameObject.SetActive(uiPair.gameObject.activeInHierarchy);

        var pairScreenPos = uiCamera.WorldToScreenPoint(uiPair.position);
        pairScreenPos += new Vector3(0, 0, 5);
        var lockerPos = Camera.main.ScreenToWorldPoint(pairScreenPos);
        transform.position = lockerPos;

        transform.localScale = defaultSize * sizeMultiplier;
    }

    [ContextMenu("UpdateDefaultSize")]
    private void UpdateDefaultSize()
    {
        defaultSize = transform.localScale;
    }
}