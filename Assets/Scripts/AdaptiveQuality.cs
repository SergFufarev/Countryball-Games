using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Sirenix.OdinInspector;

public class AdaptiveQuality : MonoBehaviour
{
    [SerializeField] private int neededMemory; // требуемое количество оперативной памяти
    [SerializeField] private PostProcessVolume postProcessingVolume;
    [SerializeField] private PostProcessProfile lowProfile;
    [SerializeField] private PostProcessProfile hightProfile;

    [Header("Force")]
    [SerializeField] private bool setForceQuality;
    [ShowIf("setForceQuality")] [SerializeField] private AdaptiveQualityType forceQuality;

    private AdaptiveQualityType currentQuality;
    public AdaptiveQualityType CurrentQuality => currentQuality;

    public AdaptiveQualityType AutoUpdateQuality()
    {
        if (setForceQuality) SetQuality(forceQuality);
        else
        {
            int ram = SystemInfo.systemMemorySize;

            AdaptiveQualityType quality = ram > neededMemory ? AdaptiveQualityType.Height : AdaptiveQualityType.Low;
            SetQuality(quality);
        }

        return currentQuality;
    }

    public void SetQuality(AdaptiveQualityType quality)
    {
        if (postProcessingVolume != null)
        {
            switch (quality)
            {
                case AdaptiveQualityType.Height:
                    postProcessingVolume.profile = hightProfile;
                    break;

                case AdaptiveQualityType.Low:
                    postProcessingVolume.profile = lowProfile;
                    break;
            }
        }

        currentQuality = quality;
    }
}

public enum AdaptiveQualityType
{
    Height,
    Low
}