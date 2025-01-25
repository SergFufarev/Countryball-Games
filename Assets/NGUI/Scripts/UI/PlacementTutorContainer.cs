using UnityEngine;
using DG.Tweening;
using FunnyBlox;

public class PlacementTutorContainer : MonoBehaviour
{
    [SerializeField] private Transform cursor;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Animator cursorAnim;

    Vector2 uiFromPos;
    Vector2 uiToPos;

    private Tweener animTweener;

    public void Init(Transform from, Transform to)
    {
        uiFromPos = ConvertToUiPos(from);
        uiToPos = ConvertToUiPos(to);
    }

    private Vector2 ConvertToUiPos(Transform worldTran)
    {
        var screenPos = Camera.main.WorldToScreenPoint(worldTran.position);
        return uiCamera.ScreenToWorldPoint(screenPos);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        AnimateCursor();
    }

    private void AnimateCursor()
    {
        cursor.transform.position = uiFromPos;

        cursorAnim.Play("Tap");
        animTweener = DOVirtual.Float(0, 1, 0.5f, value => { }).OnComplete(() =>
        {
            animTweener = cursor.DOMove(uiToPos, 0.5f).OnComplete(() =>
            {
                cursorAnim.Play("Up");
                animTweener = DOVirtual.Float(0, 1, 1, value => { }).OnComplete(() =>
                {
                    animTweener = cursor.DOMove(uiFromPos, 0.5f).OnComplete(AnimateCursor);
                });
            });
        });
    }

    public void Hide()
    {
        if (animTweener != null)
        {
            animTweener.Kill();
            animTweener = null;
        }

        gameObject.SetActive(false);
    }

    [ContextMenu("SetFrom")]
    private void SetFrom()
    {
        cursor.transform.position = uiFromPos;
    }

    [ContextMenu("SetTo")]
    private void SetTo()
    {
        cursor.transform.position = uiToPos;
    }
}