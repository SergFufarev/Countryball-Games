using UnityEngine;
using TMPro;
using TheSTAR.Utility;

public class AddCurrencyMessage : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI message;

    public void Message(float value)
    {
        message.text = $"+{TextUtility.NumericValueToText(value, NumericTextFormatType.CompactFromK)}";
        anim.SetTrigger("Add");
    }
}