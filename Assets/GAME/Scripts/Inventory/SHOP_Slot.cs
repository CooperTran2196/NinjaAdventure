using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SHOP_Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("References")]
    public SHOP_Manager shop_Manager;
    public INV_ItemInfo inv_ItemInfo;

    [Header("UI")]
    public TMP_Text itemNameText;
    public Image itemImage;
    public TMP_Text itemPriceText;

    [Header("Runtime Data")]
    public INV_ItemSO inv_ItemSO;
    public int price;

    void Awake()
    {
        shop_Manager     ??= GetComponentInParent<SHOP_Manager>();

        itemNameText    ??= transform.Find("itemNameText")?.GetComponent<TMP_Text>();
        itemImage       ??= transform.Find("itemImage")?.GetComponent<Image>();
        itemPriceText   ??= transform.Find("itemPriceText")?.GetComponent<TMP_Text>();

        if (!shop_Manager) Debug.LogError("SHOP_Slot: SHOP_Manager missing in parent.", this);

        if (!itemImage)    Debug.LogError("SHOP_Slot: itemImage missing.", this);
        if (!itemNameText) Debug.LogError("SHOP_Slot: itemNameText missing.", this);
        if (!itemPriceText)Debug.LogError("SHOP_Slot: itemPriceText missing.", this);
    }

    // called by SHOP_Manager at startup
    public void Initialize(INV_ItemSO newItemSO, int newPrice)
    {
        inv_ItemSO = newItemSO;
        price = newPrice;

        itemImage.enabled = true;
        itemImage.sprite = inv_ItemSO.image;
        itemNameText.text = inv_ItemSO.itemName;
        itemPriceText.text = price.ToString();
    }

    // called by Buy button
    public void OnBuyButtonClicked()
    {
        shop_Manager.TryBuyItem(inv_ItemSO, price);
    }
    
    // Show item info on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inv_ItemSO != null) inv_ItemInfo?.Show(inv_ItemSO);
    }

    // Hide item info when not hovering
    public void OnPointerExit(PointerEventData eventData)
    {
        inv_ItemInfo?.Hide();
    }

    // Follow mouse while hovering
    public void OnPointerMove(PointerEventData eventData)
    {
        if (inv_ItemSO != null) inv_ItemInfo?.FollowMouse(eventData.position);
    }
}
