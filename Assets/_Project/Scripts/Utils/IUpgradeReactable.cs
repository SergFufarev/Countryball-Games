public interface IUpgradeReactable
{
    void OnBuyUpgrade(int countryID, UpgradeType upgradeType, int upgradeID, int finalValue);
    void OnBuyTrade(int countryID);
    void OnWonderBuilded(int countryID);
    void OnRocketBuy(int totalRocketsCount);
}