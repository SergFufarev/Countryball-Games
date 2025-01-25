using UnityEngine;
using FunnyBlox;
using System;
using DG.Tweening;

public class AtomBomb : MonoBehaviour
{
    private const float jumpValue = 3;

    public void Fly(Country fromCountry, Country toCountry, float speed, Action completeAction)
    {
        gameObject.SetActive(true);

        CountryBall fromBall = fromCountry.CountryBalls.Get(CountryBallType.Main);
        CountryBall toBall = toCountry.CountryBalls.Get(CountryBallType.Main);

        float distance = Vector3.Distance(fromBall.transform.position, toBall.transform.position);
        float time = distance / speed;
        float jumpAngle = 90 / distance * jumpValue;

        transform.position = fromBall.transform.position;
        transform.LookAt(toBall.transform);

        var defaultEulers = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(defaultEulers.x - jumpAngle, defaultEulers.y, defaultEulers.z);

        transform.DOKill();

        transform.DOMoveX(toBall.transform.position.x, time).OnComplete(() =>
        {
            gameObject.SetActive(false);
            completeAction?.Invoke();
        }).SetEase(Ease.Linear);

        transform.DOMoveZ(toBall.transform.position.z, time).SetEase(Ease.Linear);

        // jump
        transform.DOMoveY(toBall.transform.position.y + jumpValue, time / 2).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(toBall.transform.position.y, time / 2).SetEase(Ease.InQuad);
        });

        transform.DORotate(new Vector3(defaultEulers.x + jumpAngle, defaultEulers.y, defaultEulers.z), time).SetEase(Ease.OutQuad);
    }
}