using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAXHelper {
    
    public class UITermsPanel : MonoBehaviour {
        
        public event UnityAction EventOnAcceptClick;

        #region Fields
        [SerializeField] protected Button AcceptButton;
        [SerializeField] protected Button PrivacyButton;
        [SerializeField] protected string PrivacyLink;
        #endregion

        #region Unity Event Functions

        private void Awake() {
            AcceptButton.onClick.AddListener(OnAcceptClick);
            PrivacyButton.onClick.AddListener(OnPrivacyClick);
        }
        private void Start() {
            transform.SetAsLastSibling();
        }

        private void OnDestroy() {
            AcceptButton.onClick.RemoveAllListeners();
            PrivacyButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region Helpers
        protected virtual void OnAcceptClick() {
            EventOnAcceptClick?.Invoke();
            Hide();
        }

        protected virtual void OnPrivacyClick() {
            if (!string.IsNullOrEmpty(PrivacyLink)) {
                Application.OpenURL(PrivacyLink);
            }
        }

        protected virtual void Hide() {
            Destroy(gameObject);
        }
        #endregion

    }
    
}


