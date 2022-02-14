using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class MyStoreClass : MonoBehaviour, IStoreListener
{
    
    // Loading the sku of the parent subscription so we can get the receipt later. Unity IAP won't return receipts
    // for the "subcription term" skus. The receipt is always null for subscription terms.
    static string kProductSubscription = "premium.subscription.notrial";
    
    IStoreController m_StoreController;
    private static IExtensionProvider storeExtensionProvider; // The store-specific Purchasing subsystems.
    void Awake()
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(kProductSubscription, ProductType.Subscription);
        // If wanting to offer just the "weekly" product, load it too and offer that for purchase
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        storeExtensionProvider = extensions;
        var purchases = GetComponent<Purchases>();
        purchases.SetDebugLogsEnabled(true);
        foreach (Product product in controller.products.all)
        {
            if (product.hasReceipt) {
                var amazonExtensions = storeExtensionProvider.GetExtension<IAmazonExtensions>();
                var userId = amazonExtensions.amazonUserId;
                purchases.SyncObserverModeAmazonPurchase(
                    product.definition.id,
                    product.transactionID,
                    userId
                );
            }
        }
    }

    public void PurchaseCurrency()
    {
        if (m_StoreController != null)
        {
            // Fetch the currency Product reference from Unity Purchasing
            Product product = m_StoreController.products.WithID(kProductSubscription);
            if (product != null && product.availableToPurchase)
            {
                m_StoreController.InitiatePurchase(product);
            }
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error) {}
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var purchases = GetComponent<Purchases>();
        
        var amazonExtensions = storeExtensionProvider.GetExtension<IAmazonExtensions>();
        var userId = amazonExtensions.amazonUserId;
        purchases.SyncObserverModeAmazonPurchase(
            e.purchasedProduct.definition.id,
            e.purchasedProduct.transactionID,
            userId
        );
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r) {}
}