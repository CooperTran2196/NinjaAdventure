using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SHOP_Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("References")]
    SHOP_Manager shop_Manager;
    INV_ItemInfo inv_ItemInfo;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text itemNameText;
    public Image    itemImage;
    public TMP_Text itemPriceText;

    [Header("Runtime Data")]
    public INV_ItemSO itemSO;
    public int        price;

    void Awake()
    {
        shop_Manager ??= GetComponentInParent<SHOP_Manager>();
        inv_ItemInfo ??= FindFirstObjectByType<INV_ItemInfo>();

        if (!shop_Manager)  { Debug.LogError($"{name}: SHOP_Manager is missing!", this); return; }
        if (!inv_ItemInfo)  { Debug.LogError($"{name}: INV_ItemInfo is missing!", this); return; }
        if (!itemImage)     { Debug.LogError($"{name}: itemImage is missing!", this); return; }
        if (!itemNameText)  { Debug.LogError($"{name}: itemNameText is missing!", this); return; }
        if (!itemPriceText) { Debug.LogError($"{name}: itemPriceText is missing!", this); return; }
    }

    // Called by SHOP_Manager at startup
    public void Initialize(INV_ItemSO itemSO)
    {
        this.itemSO = itemSO;
        price = itemSO.price;

        itemImage.enabled  = true;
        itemImage.sprite   = itemSO.image;
        itemNameText.text  = itemSO.itemName;
        itemPriceText.text = price.ToString();
    }

    // Called by Buy button
    public void OnBuyButtonClicked()
    {
        shop_Manager.TryBuyItem(itemSO, price);
    }

    // Show item info on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemSO) inv_ItemInfo.Show(itemSO);
    }

    // Hide item info when not hovering
    public void OnPointerExit(PointerEventData eventData)
    {
        inv_ItemInfo.Hide();
    }

    // Follow mouse while hovering
    public void OnPointerMove(PointerEventData eventData)
    {
        if (itemSO) inv_ItemInfo.FollowMouse(eventData.position);
    }
}
