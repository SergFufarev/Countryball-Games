using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SPSDigital.IAP
{
    [CreateAssetMenu(fileName = "GemShopData", menuName = "Gem Shop Data")]
    public class GemShopData : ScriptableObject
    {
        /// <summary> Товары за реальные деньги </summary>
        [LabelText("Список инапов")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<SGemShopItem> GemShopItemsList;

        /// <summary> Товары с покупкай софты за харду </summary>
        public List<SoftShopItemData> SoftItemsList;

        public SGemShopItem RemoveAdsData;
    }
}