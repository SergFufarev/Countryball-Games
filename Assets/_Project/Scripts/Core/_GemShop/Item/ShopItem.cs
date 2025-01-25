using UnityEngine;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace SPSDigital.IAP
{
    public abstract class ShopItem : MonoBehaviour
    {
        [SerializeField] protected PointerButton button;

        //protected int id;
        protected string sku;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected TextMeshProUGUI priceTextMesh;
        [SerializeField] protected TextMeshProUGUI rewardValueText;

        public delegate void Callback(string sku);
        protected Callback callback;

        public virtual void OnButtonClick()
        {
            callback?.Invoke(sku);
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0.2f;
        }
    }
}