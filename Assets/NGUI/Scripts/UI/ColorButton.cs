using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private int index;

    public void Init(int index, Action<int> clickAction)
    {
        this.index = index;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => clickAction(this.index));
    }
}