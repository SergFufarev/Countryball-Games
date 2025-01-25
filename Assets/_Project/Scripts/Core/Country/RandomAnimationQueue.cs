using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility;
using Zenject;

public class RandomAnimationQueue : MonoBehaviour
{
    [SerializeField] private List<WaitToRandomAnimationData> waitDatas = new();

    [Inject] private readonly BattleInGameService battle;

    private WaitStopper currentStopper;

    public void Add(CountryBall ball)
    {
        var animateTime = DateTime.Now.AddSeconds(ball.RandomAnimPeriod);
        if (!ball.IsEmotionIdle)
        {
            float dopSeconds = ball.Visual.FaceAnim.GetCurrentAnimDurationSeconds;
            animateTime = animateTime.AddSeconds(dopSeconds);
        }

        WaitToRandomAnimationData newWaitData = new (ball, animateTime);

        if (waitDatas.Count > 0)
        {
            for (int i = 0; i < waitDatas.Count; i++)
            {
                var data = waitDatas[i];
                if (animateTime < data.AnimateTime)
                {
                    waitDatas.Insert(i, newWaitData);
                    if (i == 0) StartWait();
                    return;
                }
            }
        }

        // add to end
        waitDatas.Add(newWaitData);
        if (waitDatas.Count == 1) StartWait();
    }

    public void Remove(CountryBall ball)
    {
        for (int i = 0; i < waitDatas.Count; i++)
        {
            if (waitDatas[i].Ball == ball)
            {
                waitDatas.RemoveAt(i);
                return;
            }
        }
    }

    private void StartWait() => StartWait(waitDatas[0]);
    private void StartWait(WaitToRandomAnimationData data)
    {
        if (currentStopper != null) currentStopper.Stop();
        float waitSeconds = (float)(data.AnimateTime - DateTime.Now).TotalSeconds;

        if (waitSeconds > 0) currentStopper = TimeUtility.WaitAsync(waitSeconds, () => CompleteWait(data));
        else CompleteWait(data);
    }

    private void CompleteWait(WaitToRandomAnimationData data)
    {
        var ball = data.Ball;
        if (ball.VisualIsActive)
        {
            //if (data.Ball.IsEmotionIdle)
            if (!battle.IsCountryInBattle(data.Ball.Country))
                data.Ball.PlayRandomAnim();

            // re add
            waitDatas.Remove(data);
            Add(ball);
        }
        else waitDatas.Remove(data);

        if (waitDatas.Count > 0) StartWait();
    }

    [Serializable]
    public class WaitToRandomAnimationData : IComparable<WaitToRandomAnimationData>
    {
        [SerializeField] private CountryBall ball;
        [SerializeField] private DateTime animateTime;

        public CountryBall Ball => ball;
        public DateTime AnimateTime => animateTime;

        public WaitToRandomAnimationData(CountryBall ball, DateTime animateTime)
        {
            this.ball = ball;
            this.animateTime = animateTime;
        }

        public int CompareTo(WaitToRandomAnimationData other) => animateTime.CompareTo(other.animateTime);
    }
}