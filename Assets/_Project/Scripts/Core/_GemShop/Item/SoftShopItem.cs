using System;
using UnityEngine;
using TMPro;
using FunnyBlox;

namespace SPSDigital.IAP
{
    public class SoftShopItem : ShopItem
    {
        [SerializeField] private TextMeshProUGUI coinsPlusText;

        public Action<int> _buySoftAction;
        private SoftShopItemData _itemData;

        public void Init(SoftShopItemData itemData, Action<int> buySoftAction)
        {
            _itemData = itemData;
            _buySoftAction = buySoftAction;


            rewardValueText.text = $"{itemData.RewardHours}h instant coins";
            priceTextMesh.text = $"{itemData.Price}";

            float incomePerSecond = (int)CurrencyService.Instance.FinalMoneyPerSecond;
            int incomePerHour = (int)(incomePerSecond * 60 * 60);

            coinsPlusText.text = $"+{itemData.RewardHours * incomePerHour}";

            button.Init(OnButtonClick);
        }

        public override void OnButtonClick()
        {
            _buySoftAction.Invoke(_itemData.Id);
        }
    }
}