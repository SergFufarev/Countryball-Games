using UnityEngine;

namespace FunnyBlox
{
    [CreateAssetMenu(fileName = "ArmyUpgradeDatas", menuName = "Data/Army Upgrade Datas", order = 51)]
    public class UpgradeConfig : ScriptableObject
    {        
        [SerializeField] private string localisationKey;
        public string LocalisationKey => localisationKey;

        public CountryBallType UpgradeType;
        
        public UpgradeData[] UpgradeDataList; // данные для столицы
        public UpgradeData[] UpgradeDataListRegionVariant; // данные для регионов
    }
}