using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SPSDigital.IAP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using FunnyBlox;

namespace WP
{
    public class WPReceipt
    {
        public string SKU;
        public Product Product;
        public string Signature;
        public string Data;
    }

    public class MobileInAppPurchaser : MonoBehaviour, IStoreListener
    {
        #region Fields

        [SerializeField] private GemShopData gemShopData;

        private IStoreController StoreController;
        private IExtensionProvider StoreExstensionProvider;

        private bool isWaitingToInit;
        public bool IsWaitingToInit => isWaitingToInit;
        public CultureInfo CurrentCultureInfo;

        private static MobileInAppPurchaser instance;
        public static MobileInAppPurchaser Instance => instance;

        #endregion

        #region Events

        public event UnityAction<bool> OnInitializedEvent;
        public UnityAction<bool, WPReceipt> OnPurchaseResult;
        public UnityAction<Product> OnPurchaseProductResult = null;

        #endregion

        private void Start()
        {
            if (instance == null)
            {
                Init(OnMobileInAppPurchaserInit);
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #region Initialization

        public void Init(UnityAction<bool> OnInitializedEvent)
        {
            this.OnInitializedEvent = OnInitializedEvent;
            isWaitingToInit = true;
            InitUnityPurchasing();
        }

        private void InitUnityPurchasing()
        {
            ConfigurationBuilder Builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());


            foreach (SGemShopItem item in gemShopData.GemShopItemsList) Builder.AddProduct(item.SKU, item.ProductType);

            Builder.AddProduct(gemShopData.RemoveAdsData.SKU, gemShopData.RemoveAdsData.ProductType);

            UnityPurchasing.Initialize(this, Builder);
        }

        private void OnMobileInAppPurchaserInit(bool success)
        {
            if (success)
            {
                Debug.Log("Магазин успешно инициализирован");
            }
            else
            {
                //TODO: Show error popup

                Debug.Log("Магазин не удалось инициализировать");
            }
        }

        #endregion

        #region Public

        public bool IsInitialized()
        {
            return (StoreController != null && StoreExstensionProvider != null);
        }

        // ???????? ??? ???????
        public bool BuyProduct(string SKU)
        {
            if (IsInitialized())
            {
                Product Product = StoreController.products.WithID(SKU);
                if (Product != null && Product.availableToPurchase)
                {
                    Debug.LogWarning(string.Format("[MobileInAppPurchaser] Purchasing product asychronously: '{0}'",
                        Product.definition.id));

                    StoreController.InitiatePurchase(Product);
                    return true;
                }
                else
                {
                    Debug.LogError(
                        "[MobileInAppPurchaser] BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.LogError("[MobileInAppPurchaser] BuyProductID FAIL. Not initialized.");
            }

            return false;
        }

        // TODO: https://docs.unity3d.com/Manual/UnityIAPRestoringTransactions.html  (FOR Non-Consumable or renewable Subscription)
        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            Debug.Log("RestorePurchases");

            if (!IsInitialized())
            {
                Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");

                var google = StoreExstensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                google.RestoreTransactions(OnTransactionsRestored);
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");

                var apple = StoreExstensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions(OnTransactionsRestored);
            }
            //else {
            //    // We are not running on an Apple device. No work is necessary to restore purchases.
            //    Logger.Log(string.Format("[MobileInAppPurchaser] RestorePurchases FAIL. Not supported " + 
            //        "on this platform. Current = {0}", Application.platform));
            //}
        }

        #endregion

        #region Helpers

        private void OnTransactionsRestored(bool result)
        {
            if (result)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.

                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.LogWarning(string.Format("[MobileInAppPurchaser] RestorePurchases continuing: {0} . " +
                                               "If no further messages, no purchases available to restore.", result));
            }
            else
            {
                Debug.LogError(string.Format("[MobileInAppPurchaser] RestorePurchases Restoration failed."));
            }
        }


        // ?????? ?????????? ?? JSON, ?? ???????????? SimpleJSON, ????? ??? ?????? ??????????
        private string[] StoreNamePurcahseInfoSignature(Product product)
        {
            SimpleJSON.JSONNode jsNode = SimpleJSON.JSON.Parse(product.receipt);
            string storeName = jsNode["Store"].ToString();
            string signature = "empty";
            string purchaseInfo = "empty";

#if UNITY_IOS
            purchaseInfo = jsNode["Payload"];
#elif UNITY_ANDROID
            SimpleJSON.JSONNode payloadNode = SimpleJSON.JSON.Parse(jsNode["Payload"]);
            signature = payloadNode["signature"];
            purchaseInfo = payloadNode["json"];
#endif

            if (signature != "empty")
            {
                signature = RemoveQuotes(signature);
            }

            if (purchaseInfo != "empty")
            {
                purchaseInfo = RemoveQuotes(purchaseInfo);
            }

            storeName = RemoveQuotes(storeName);

            return new string[] { storeName, purchaseInfo, signature };
        }

        private string RemoveQuotes(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                Debug.Log("RemoveQuotes: string is null or empty!");
                return "empty";
            }

            string newStr = str;

            if (str[0] == '"')
                newStr = newStr.Remove(0, 1);
            if (str[str.Length - 1] == '"')
                newStr = newStr.Remove(newStr.Length - 1, 1);

            return newStr;
        }


        // ?????????? ????????? ???????? ???????, ????? ????? ??????? ?????? ??? ?????????? ?????????
        private void ProcessPurchaseInner(Product Prod)
        {
            string[] data = StoreNamePurcahseInfoSignature(Prod);
            string storeNameTemp = data[0];
            string purchaseInfoTemp = data[1];
            string signatureTemp = data[2];

            string ProductSKU = Prod.definition.id;

            WPReceipt WPReceipt = new WPReceipt()
            {
                SKU = ProductSKU,
                Data = purchaseInfoTemp,
                Signature = signatureTemp,
                Product = Prod
            };

            OnPurchaseResult?.Invoke(true, WPReceipt);
        }


        private static IEnumerable<CultureInfo> GetCultureInfosByCurrencySymbol(string currencySymbol)
        {
            if (currencySymbol == null) return null;

            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Where(x => new RegionInfo(x.LCID).ISOCurrencySymbol == currencySymbol);
        }

        private void InitCurrentCultureInfo()
        {
            if (StoreController != null && StoreController.products.all.Length > 0)
            {
                IEnumerable<CultureInfo> CultureInfoList = GetCultureInfosByCurrencySymbol(
                    StoreController.products.all[0].metadata.isoCurrencyCode);

                CultureInfo TempCurrentCulture = CultureInfo.CurrentCulture;

                foreach (CultureInfo CI in CultureInfoList)
                {
                    if (CI == TempCurrentCulture)
                    {
                        CurrentCultureInfo = CI;
                        break;
                    }
                }

                if (CurrentCultureInfo == null)
                {
                    CurrentCultureInfo = CultureInfoList.First();
                }
            }
            else
            {
                CurrentCultureInfo = CultureInfo.CurrentCulture;
            }
        }

        #endregion


        #region IStoreListener

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.LogWarning("[MobileInAppPurchaser] INIT DONE!");

            StoreController = controller;
            StoreExstensionProvider = extensions;

            InitCurrentCultureInfo();

            isWaitingToInit = false;
            OnInitializedEvent?.Invoke(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning("[MobileInAppPurchaser] INIT FAILED! Reason: " + error);

            isWaitingToInit = false;

            OnInitializedEvent?.Invoke(false);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning("[MobileInAppPurchaser] INIT FAILED! Reason: " + error + " Error:" + message);

            isWaitingToInit = false;

            OnInitializedEvent?.Invoke(false);
        }

        public void OnPurchaseFailed(Product Prod, PurchaseFailureReason FailureReason)
        {
            string ProductSKU = Prod.definition.id;

            Debug.LogWarning(string.Format("[MobileInAppPurchaser] OnPurchaseFailed: FAIL. Product: '{0}' Receipt: {1}",
                ProductSKU, Prod.receipt));
            Debug.LogWarning("PurchaseFailureReason: " + FailureReason);

            OnPurchaseResult?.Invoke(false, null);
        }


        
        
        // ????? ?? ???????? ???????, ???????? ?????????, ??? ??? ???????
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs E)
        {
            Debug.LogWarning(string.Format("[MobileInAppPurchaser] ProcessPurchase: PASS. Product: '{0}' Receipt: {1}",
                E.purchasedProduct.definition.id, E.purchasedProduct.receipt));

            // UNITY VALIDATION

            List<string> ReceitsIDs = new List<string>();
            bool validPurchase = true; // Presume valid for platforms with no R.V.
            // Unity IAP's validation logic is only included on these platforms.
#if UNITY_EDITOR
            
#elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.


            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(), Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(E.purchasedProduct.receipt);
                // For informational purposes, we list the receipt(s)
                Debug.Log("Valid!");
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    ReceitsIDs.Add(productReceipt.productID);
                }
            }
            catch (IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }

#endif

            // It is important you check not just that the receipt is valid, but also what information it contains.
            // A common technique by users attempting to access content without purchase is to supply receipts from other products or applications.
            // These receipts are genuine and do pass validation, so you should make decisions based on the product IDs parsed by the CrossPlatformValidator.

            bool bValidID = false;
            if (ReceitsIDs != null && ReceitsIDs.Count > 0)
            {
                foreach (string ProductID in ReceitsIDs)
                {
                    //Debug.Log($"{E.purchasedProduct.definition.storeSpecificId}, and {ProductID}");
                    if (E.purchasedProduct.definition.storeSpecificId.Equals(ProductID))
                    {
                        bValidID = true;
                        break;
                    }
                }
            }

#if UNITY_EDITOR
            validPurchase = bValidID = true;
#endif

            // ????????? ???????? ???????
            if (validPurchase && bValidID)
            {
                if (IsInitialized())
                {
                    ProcessPurchaseInner(E.purchasedProduct);
                    OnPurchaseProductResult?.Invoke(E.purchasedProduct);
                }
            }
            else
            {
                // Show some UI that there was an error
            }

            Debug.Log("Complete!");

            return PurchaseProcessingResult.Complete;
        }

        #endregion
    }
}