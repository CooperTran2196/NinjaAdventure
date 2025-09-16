// Assets/GAME/Scripts/INV/SHOP_Slot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SHOP_Slot : MonoBehaviour
{
    [Header("Runtime Data")]
    public INV_ItemSO itemSO { get; private set; }
    public int price { get; private set; }

    [Header("UI")]
    public Image icon;         // child "Icon"
    public TMP_Text nameText;  // child "NameText"
    public TMP_Text priceText; // child "PriceText"

    [Header("Refs")]
    public SHOP_Manager shop;  // parent manager

    void Awake()
    {
        shop      ??= GetComponentInParent<SHOP_Manager>();
        icon      ??= transform.Find("Icon")?.GetComponent<Image>();
        nameText  ??= transform.Find("NameText")?.GetComponent<TMP_Text>();
        priceText ??= transform.Find("PriceText")?.GetComponent<TMP_Text>();

        if (!shop)      Debug.LogError($"{name}: SHOP_Manager missing in parent.", this);
        if (!icon)      Debug.LogError($"{name}: Icon Image missing.", this);
        if (!nameText)  Debug.LogError($"{name}: NameText TMP missing.", this);
        if (!priceText) Debug.LogError($"{name}: PriceText TMP missing.", this);
    }

    // called by SHOP_Manager at startup
    public void Initialize(INV_ItemSO newItemSO, int newPrice)
    {
        itemSO = newItemSO;
        price  = newPrice;

        icon.enabled = true;
        icon.sprite  = itemSO.icon;
        nameText.text  = itemSO.itemName;
        priceText.text = price.ToString();
    }

    // keep this so the next video (buy) just works
    public void OnBuyButtonClicked()
    {
        shop.TryBuyItem(itemSO, price);
    }
}
