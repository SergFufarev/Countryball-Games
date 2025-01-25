using Sirenix.OdinInspector;
using UnityEngine.Purchasing;

namespace SPSDigital.IAP
{
    [System.Serializable]
    public struct SGemShopItem
    {
        [FoldoutGroup("Данные товара")] [LabelText("Идентификатор")]
        public int Id;

        [FoldoutGroup("Данные товара")] [LabelText("Категория")]
        public EGemShopItemType Type;

        [FoldoutGroup("Данные товара")] [LabelText("Название")]
        public string Name;
        
        [FoldoutGroup("Данные товара")] [LabelText("Название при покупке")]
        public string IAPName;
        
        [FoldoutGroup("Данные товара")] public string SKU;

        [FoldoutGroup("Данные товара")] [LabelText("Тип (расходник, постоянный)")]
        public ProductType ProductType;

        [FoldoutGroup("Данные товара")] [LabelText("Цена ($)")]
        public float Price;

        [FoldoutGroup("Данные товара")] [LabelText("Количество харды")]
        public int GemAmount;

        [FoldoutGroup("Данные товара")] [LabelText("Количество софты")]
        public int CoinAmount;
    }

    [System.Serializable]
    public struct SoftShopItemData
    {
        [FoldoutGroup("Данные товара")]
        [LabelText("Идентификатор")]
        public int Id;

        [FoldoutGroup("Данные товара")]
        [LabelText("Цена (в харде)")]
        public float Price;

        [FoldoutGroup("Данные товара")]
        [LabelText("Награда (в часах)")]
        public int RewardHours;
    }
}