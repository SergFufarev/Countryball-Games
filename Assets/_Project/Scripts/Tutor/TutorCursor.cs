using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorCursor : MonoBehaviour
{
    [SerializeField] private Image cursorImg;

    public void SetTransformData(CursorTransformData data)
    {
        transform.rotation = Quaternion.Euler(data.Rotation);
        transform.localScale = data.Scale;
    }

    public void SetVisibility(bool visibility)
    {
        cursorImg.gameObject.SetActive(visibility);
    }
}