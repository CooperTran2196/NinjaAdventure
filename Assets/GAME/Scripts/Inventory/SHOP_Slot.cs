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

        if (shop_Manager == null) Debug.LogError($"{name}: SHOP_Manager is missing in SHOP_Slot");
        if (inv_ItemInfo == null) Debug.LogError($"{name}: INV_ItemInfo is missing in SHOP_Slot");
        if (itemImage == null)    Debug.LogError($"{name}: itemImage is missing in SHOP_Slot");
        if (itemNameText == null) Debug.LogError($"{name}: itemNameText is missing in SHOP_Slot");
        if (itemPriceText == null)Debug.LogError($"{name}: itemPriceText is missing in SHOP_Slot");
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
