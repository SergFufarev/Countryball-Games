using System;
using UnityEngine;
using TheSTAR.Utility.Pointer;
using TheSTAR.Sound;
using TMPro;
using TheSTAR.Utility;

public class CurrencyCounter : MonoBehaviour
{
    [SerializeField] private PointerButton _btn;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private RectTransform endFlyPos;
    [SerializeField] private Animator anim;

    public RectTransform EndFlyPos => endFlyPos;

    public void Init(Action clickAction) => _btn.Init(clickAction);

    public void SetValue(float value)
    {
        _valueText.text = TextUtility.NumericValueToText(value, NumericTextFormatType.CompactFromM);
    }

    [ContextMenu("AnimateIncome")]
    public void AnimateIncome()
    {
        anim.SetTrigger("Income");
    }
}