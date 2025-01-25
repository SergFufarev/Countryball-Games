using System;
using UnityEngine;
using UnityEngine.Purchasing;
using WP;

namespace SPSDigital.Metrica
{
    public class AppMetricaIAP : MonoBehaviour
    {
        
        [SerializeField] private MobileInAppPurchaser mobileInAppPurchaser;

        public static string ProductIDConsumable;
        
        [System.Serializable]
        public struct Receipt
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }

        [System.Serializable]
        public struct PayloadAndroid
        {
            public string Json;
            public string Signature;
        }

        private void Start()
        {
            mobileInAppPurchaser.OnPurchaseProductResult += PurchaseCompletedHandler;
        }

        private void PurchaseCompletedHandler(Product result)
        {
            var product = result;
            if(string.Equals(product.definition.id, ProductIDConsumable, StringComparison.Ordinal)) {
                string currency = product.metadata.isoCurrencyCode;
                decimal price = product.metadata.localizedPrice;

                // Creating the instance of the YandexAppMetricaRevenue class.
                YandexAppMetricaRevenue revenue = new YandexAppMetricaRevenue(price, currency);
                if(product.receipt != null)
                {
                    // Creating the instance of the YandexAppMetricaReceipt class.
                    YandexAppMetricaReceipt yaReceipt = new YandexAppMetricaReceipt();
                    Receipt receipt = JsonUtility.FromJson<Receipt>(product.receipt);
#if UNITY_ANDROID
                    PayloadAndroid payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);
                    yaReceipt.Signature = payloadAndroid.Signature;
                    yaReceipt.Data = payloadAndroid.Json;
#elif UNITY_IPHONE
            yaReceipt.TransactionID = receipt.TransactionID;
            yaReceipt.Data = receipt.Payload;
#endif
                    revenue.Receipt = yaReceipt;
                }
                // Sending data to the AppMetrica server.
                AppMetrica.Instance.ReportRevenue(revenue);
            }

            ProductIDConsumable = "";
        }
    }
}