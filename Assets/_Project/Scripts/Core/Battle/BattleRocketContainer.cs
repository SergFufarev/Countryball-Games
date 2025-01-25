using System;
using UnityEngine;
using DG.Tweening;

public class BattleRocketContainer : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private Action completeAction;

    private bool inAttack = false;
    public bool InAttack => inAttack;

    public void Attack(Vector3 pos, Action completeAction)
    {
        if (inAttack) return;

        inAttack = true;

        this.completeAction = completeAction;
        transform.position = pos;

        anim.Play("Fly");

        DOVirtual.Float(0, 1, 1, value => { }).OnComplete(EndFly);
    }

    public void EndFly()
    {
        inAttack = false;
        completeAction?.Invoke();
    }
}