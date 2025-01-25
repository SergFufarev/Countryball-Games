using UnityEngine;
using UnityEngine.UI;
using TheSTAR.Utility;

public class CountryBallAnim : MonoBehaviour
{
    [Space]
    [SerializeField] private Image emotionImg;
    [SerializeField] private Animator anim;

    public float GetCurrentAnimDurationSeconds
    {
        get
        {
            return (float)AnimationUtility.GetClipLength(anim, _currentEmotion.ToString()) / 1000;
        }
    }

    private CustomizationFaceType _currentFaceType;
    private CountryBallEmotionType _currentEmotion;

    public CountryBallEmotionType CurrentEmotion
    {
        get
        {
            return _currentEmotion;
        }
        set
        {
            _currentEmotion = value;
        }
    }

    public void SetFace(CustomizationFaceType face)
    {
        _currentFaceType = face;
        SetIdleSprite();
    }

    public void PlayPreIdleAnim()
    {
        _currentEmotion = CountryBallEmotionType.Idle;
        anim.Play("PreIdle");
    }

    public void PlayAnim(CountryBallEmotionType emotion)
    {
        _currentEmotion = emotion;

        if (anim != null) anim.Play(_currentEmotion.ToString());
    }

    // for anim
    public void SetSprite(int index)
    {
        if (CountryBallVisualService.Instance == null) return;

        emotionImg.sprite = CountryBallVisualService.Instance.GetBallSprite(_currentFaceType, _currentEmotion, index);
    }

    private void SetIdleSprite()
    {
        if (CountryBallVisualService.Instance == null) return;

        emotionImg.sprite = CountryBallVisualService.Instance.GetBallSprite(_currentFaceType, CountryBallEmotionType.Idle, 0);
    }
}