using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TheSTAR.Utility;
using Random = UnityEngine.Random;

namespace TheSTAR.GUI.FlyUI
{
    public class HandfulFlyUI : MonoBehaviour
    {
        [SerializeField] private FlyUIObject flyUiPrefab;
        [SerializeField] private AnimationCurve heightCurve;

        private RectTransform endPos;
        private FlyUIObject[] flyUIObjects;
        private int flyObjectsCount;
        private Action endAction;

        private const float StartHandfulAnimDuration = 0.5f;
        private const float QueueDuration = 0.05f;
        private const float StartHandfulAnimWaitDuration = 0.5f;
        private const float FlyDuration = 0.7f;
        private const float MaxDistanceForGeneration = 200;
        private const float JumpHeight = 100;

        public void Fly(int count, RectTransform endPos, Action endAction)
        {
            flyObjectsCount = count;
            this.endPos = endPos;
            this.endAction = endAction;

            GenerateFlyUIObjects(flyObjectsCount);
            AnimateHandful();
        }

        public void GenerateFlyUIObjects(int count)
        {
            flyUIObjects = new FlyUIObject[count];
            for (int i = 0; i < count; i++) flyUIObjects[i] = GenerateFlyUIObject();
        }

        private FlyUIObject GenerateFlyUIObject()
        {
            return Instantiate(flyUiPrefab, transform.position, Quaternion.identity, transform);
        }

        private Vector2 GetRandomGenerationOffset()
        {
            Vector2 pos = new (Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            pos = pos.normalized;
            pos *= Random.Range(0, MaxDistanceForGeneration);
            return pos;
        }

        private void AnimateHandful()
        {
            foreach (var flyObject in flyUIObjects) AnimateStartForOneFlyObject(flyObject);

            DOVirtual.Float(0, 1, StartHandfulAnimWaitDuration, value => { }).OnComplete(() =>
            {
                for (int i = 0; i < flyUIObjects.Length; i++)
                {
                    Action end;

                    if (i == 0) end = endAction;
                    else if (i == flyUIObjects.Length - 1) end = TotalEnd;

                    AnimateToCounter(flyUIObjects[i], i * QueueDuration, i == 0 ? endAction : null);
                }
            }).SetEase(Ease.Linear);
        }

        private void AnimateStartForOneFlyObject(FlyUIObject flyObject)
        {
            var startPos = new Vector2(0, 0);
            var endPos = GetRandomGenerationOffset();
            Vector2 tempPos;

            DOVirtual.Float(0, 1, StartHandfulAnimDuration, value =>
            {
                tempPos = MathUtility.ProgressToValue(value, startPos, endPos) + new Vector3(0, heightCurve.Evaluate(value) * JumpHeight, 0);
                flyObject.transform.localPosition = tempPos;

            }).SetEase(Ease.Linear);
        }

        private void AnimateToCounter(FlyUIObject flyObject, float delay, Action completeAction)
        {
            var startPos = flyObject.transform.position;
            Vector2 tempPos;

            DOVirtual.Float(0, 1, delay, value => { }).OnComplete(() =>
            {
                DOVirtual.Float(0, 1, FlyDuration, value =>
                {
                    tempPos = MathUtility.ProgressToValue(value, startPos, endPos.position);
                    flyObject.transform.position = tempPos;

                }).OnComplete(() =>
                {
                    completeAction?.Invoke();
                    flyObject.gameObject.SetActive(false);
                }).SetEase(Ease.InQuart);
            }).SetEase(Ease.Linear);
        }

        private void TotalEnd()
        {
            Destroy(gameObject);
        }
    }
}