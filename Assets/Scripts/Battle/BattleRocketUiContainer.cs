using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace Battle
{
    public class BattleRocketUiContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rocketsCounter;
        [SerializeField] private GameObject progressbarContainer;
        [SerializeField] private Image progressImg;
        [SerializeField] private PointerButton rocketBtn;

        public Transform buttonTran => rocketBtn.transform;

        public void Init(Action clickAction)
        {
            rocketBtn.Init(clickAction);
        }

        public void SetValue(int value)
        {
            rocketsCounter.text = value.ToString();
        }

        public void SetProgress(float progress)
        {
            progressImg.fillAmount = progress;
        }

        /*
        public void SetProgressbarActivity(bool activity)
        {
            progressbarContainer.SetActive(activity);
        }
        */
    }
}