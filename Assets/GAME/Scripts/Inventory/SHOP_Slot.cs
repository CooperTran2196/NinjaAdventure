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
    public INV_ItemSO itemSO;
    public int price;

    void Awake()
    {
        shop_Manager     ??= GetComponentInParent<SHOP_Manager>();
        
        itemNameText    ??= transform.Find("itemNameText")?.GetComponent<TMP_Text>();
        itemImage       ??= transform.Find("itemImage")?.GetComponent<Image>();
        itemPriceText   ??= transform.Find("itemPriceText")?.GetComponent<TMP_Text>();

        if (!shop_Manager) Debug.LogError("SHOP_Slot: SHOP_Manager missing in parent.", this);
        if (!inv_ItemInfo) Debug.LogError("SHOP_Slot: INV_ItemInfo reference missing.", this);
        if (!itemImage)    Debug.LogError("SHOP_Slot: itemImage missing.", this);
        if (!itemNameText) Debug.LogError("SHOP_Slot: itemNameText missing.", this);
        if (!itemPriceText)Debug.LogError("SHOP_Slot: itemPriceText missing.", this);
    }

    // called by SHOP_Manager at startup
    public void Initialize(INV_ItemSO itemSO)
    {
        this.itemSO = itemSO;
        price = itemSO.price;

        itemImage.enabled = true;
        itemImage.sprite = itemSO.image;
        itemNameText.text = itemSO.itemName;
        itemPriceText.text = price.ToString();
    }

    // called by Buy button
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
