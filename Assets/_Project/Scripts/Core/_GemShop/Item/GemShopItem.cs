using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SPSDigital.IAP
{
    public class GemShopItem : ShopItem
    {
        [SerializeField] private TextMeshProUGUI oldReward;

        private SGemShopItem _itemData;
        private bool _firstBuy = false;

        public string SKU => _itemData.SKU;

        public void Init(SGemShopItem itemData, Callback callbackMethod, bool firstBuy)
        {
            _itemData = itemData;
            _firstBuy = firstBuy;
            sku = itemData.SKU;

            callback = callbackMethod;

            UpdateUI();

            button.Init(OnButtonClick);
        }

        private void UpdateUI()
        {
            string rewardText = "";
            if (_itemData.Type == EGemShopItemType.Hard) rewardText = "stars";

            if (_firstBuy)
            {
                oldReward.text = $"{_itemData.GemAmount}";
                if (rewardValueText != null) rewardValueText.text = $"{_itemData.GemAmount * 2} {rewardText}";
                if (oldReward != null) oldReward.gameObject.SetActive(true);
            }
            else
            {
                if (rewardValueText != null) rewardValueText.text = $"{_itemData.GemAmount} {rewardText}";
                if (oldReward != null) oldReward.gameObject.SetActive(false);
            }

            priceTextMesh.text = string.Format("${0}", _itemData.Price);
        }

        public void SetIsFirst(bool first)
        {
            _firstBuy = first;
            UpdateUI();
        }
    }
}