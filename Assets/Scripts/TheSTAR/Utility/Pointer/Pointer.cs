using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheSTAR.Utility.Pointer
{
    public class Pointer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Action<PointerEventData> _downAction, _dragAction, _upAction, _enterAction, _exitAction;

        public void InitPointer(
            Action<PointerEventData> downAction, 
            Action<PointerEventData> dragAction = null, 
            Action<PointerEventData> upAction = null, 
            Action<PointerEventData> enterAction = null, 
            Action<PointerEventData> exitAction = null)
        {
            _downAction = downAction;
            _dragAction = dragAction;
            _upAction = upAction;
            _enterAction = enterAction;
            _exitAction = exitAction;
        }

        #region Pointer

        public void OnPointerDown(PointerEventData eventData) => _downAction?.Invoke(eventData);

        public void OnDrag(PointerEventData eventData) => _dragAction?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => _upAction?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) => _enterAction?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => _exitAction?.Invoke(eventData);

        #endregion // Pointer
    }
}