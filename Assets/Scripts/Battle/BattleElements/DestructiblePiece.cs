using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestructiblePiece : MonoBehaviour
{
    [SerializeField] private Rigidbody body;

    private Vector3 partBaseLocalPosition;
    private Vector3 partBaseLocalEulerAngles;
    private float partBaseLocalScale;

    private Tweener animTweener;

    public void SaveDefaultData()
    {
        partBaseLocalPosition = transform.localPosition;
        partBaseLocalScale = transform.localScale.x;
        partBaseLocalEulerAngles = transform.localEulerAngles;
    }

    public void Reset()
    {
        if (animTweener != null)
        {
            animTweener.Kill();
            animTweener = null;
        }

        body.isKinematic = true;
        transform.localPosition = partBaseLocalPosition;
        transform.localEulerAngles = partBaseLocalEulerAngles;
        transform.localScale = new Vector3(
            partBaseLocalScale,
            partBaseLocalScale,
            partBaseLocalScale);
    }

    public void Destruct(float force, Vector3 explosionPos, float radius)
    {
        body.isKinematic = false;
        body.AddExplosionForce(force, explosionPos, radius);
    }

    public void Disappearance(float time)
    {
        body.isKinematic = true;
        animTweener = transform.DOScale(Vector3.zero, time);
    }
}