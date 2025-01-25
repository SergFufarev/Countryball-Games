using UnityEngine;
using Sirenix.OdinInspector;

public class UnitVisual : MonoBehaviour
{
    [SerializeField] private Transform shootingPos;
    [SerializeField] private bool useFaceAnim;

    [ShowIf("useFaceAnim")]
    [SerializeField] private CountryBallAnim faceAnim;
    [SerializeField] private Animator[] animators;
    [SerializeField] private bool randomizeStaticAnim;

    [Space]
    [SerializeField] private bool useCountryBallRend;
    [ShowIf("useCountryBallRend")]
    [SerializeField] private Renderer countryBallRend;

    public Transform ShootingPos => shootingPos;

    public void SetFace(CustomizationFaceType face)
    {
        if (!useFaceAnim) return;
        faceAnim.SetFace(face);
    }

    public void PlayFaceAnim(CountryBallEmotionType emotionType, float delay)
    {
        if (!useFaceAnim) return;
        faceAnim.CurrentEmotion = emotionType;

        if (delay == 0) DelayPlayAnim();
        else
        {
            faceAnim.PlayPreIdleAnim();
            Invoke(nameof(DelayPlayAnim), delay);
        }
    }

    public void PlayBodyAnim(BodyAnimType bodyAnim)
    {
        for (int i = 0; i < animators.Length; i++)
        {
            if (bodyAnim == BodyAnimType.Shoot) animators[i].SetTrigger("Shoot");
            else if (bodyAnim == BodyAnimType.Static && randomizeStaticAnim)
            {
                if (Random.Range(0f, 1f) < 0.5f) animators[i].Play("Static");
                else animators[i].Play("Static_2");
            }
            else
            {
                animators[i].Play(bodyAnim.ToString());
            }
        }
    }

    private void DelayPlayAnim()
    {
        if (!useFaceAnim) return;
        faceAnim.PlayAnim(faceAnim.CurrentEmotion);
    }

    public void TrySetCountryBallMaterial(Material mat)
    {
        if (!useCountryBallRend) return;

        return; // пока не используем, так как материалы флага встают неверно

        countryBallRend.material = mat;
    }

    public virtual void OnStartBattle()
    {
    }
}

public enum BodyAnimType
{
    Static,
    Shoot,
    Move,
    Happy
}