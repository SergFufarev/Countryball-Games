using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CountryBallVisualConfig", menuName = "Data/CountryBallVisualConfig")]
public class CountryBallVisualConfig : ScriptableObject
{
    [Header("Emotions")]
    public EmotionsKit[] Kits;

    [Header("Prefabs")]
    public CountryBallRoleElement[] Roles;
    public CountryBallCustomElement[] Customs;

    [Serializable]
    public struct EmotionsKit
    {
        public CustomizationFaceType faceType;
        public EmotionData[] emotions;

        public CountryBallEmotionType[] GetAvailableEmotions
        {
            get
            {
                List<CountryBallEmotionType> availableEmotions = new();

                for (int i = 0; i < emotions.Length; i++)
                {
                    if (emotions[i].Use) availableEmotions.Add(emotions[i].EmotionType);
                }

                return availableEmotions.ToArray();
            }
        }
    }

    [Serializable]
    public struct EmotionData
    {
        [SerializeField] private bool use;
        public CountryBallEmotionType EmotionType;
        public Sprite[] Sprites;

        public bool Use => use;
    }

    public CountryBallEmotionType[] GetAvailableEmotionsForFace(CustomizationFaceType faceType)
    {
        return Kits[(int)faceType].GetAvailableEmotions;
    }
}

public enum CountryBallEmotionType
{
    Idle,
    Angry,
    Crying,
    Happy,
    Nervous,
    Shocked,
    Sleeping,
    Yawning
}