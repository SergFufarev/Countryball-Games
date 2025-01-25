using System;

namespace SPSDigital.IAP
{
    public class AdsShopItem : ShopItem
    {
        private SGemShopItem _itemData;
        Action callbackAction;

        public void Init(SGemShopItem itemData, Action callbackMethod)
        {
            _itemData = itemData;
            sku = itemData.SKU;

            callbackAction = callbackMethod;

            UpdateUI();

            button.Init(OnButtonClick);
        }

        private void UpdateUI()
        {
            priceTextMesh.text = string.Format("${0}", _itemData.Price);
        }

        public override void OnButtonClick()
        {
            callbackAction?.Invoke();
        }
    }
}