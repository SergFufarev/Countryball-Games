using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyUIElement : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI dayTitle;
    [SerializeField] private TextMeshProUGUI rewardValueTitle;

    private const float InvisibleAlpha = 0.7f;
    private const float VisibleAlpha = 1f;
    
    public void Init(int index, bool visible, Sprite icon, int rewardValue, DailyBonusConfig.DailyRewardType rewardType)
    {
        dayTitle.text = $"DAY {index + 1}";
        rewardValueTitle.text = rewardType == DailyBonusConfig.DailyRewardType.IncomeBoost ? $"{rewardValue}m" : $"{rewardValue}";
        SetVisible(visible);

        this.icon.sprite = icon;
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? VisibleAlpha : InvisibleAlpha;
    }
}
