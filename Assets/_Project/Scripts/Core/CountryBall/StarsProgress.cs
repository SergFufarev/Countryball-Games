using UnityEngine;
using UnityEngine.UI;

public class StarsProgress : MonoBehaviour
{
    [SerializeField] private Image fill;

    public void SetProgress(float progress) => fill.fillAmount = progress;
}