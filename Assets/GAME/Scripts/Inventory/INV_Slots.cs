using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class INV_Slots : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    INV_Manager inv_Manager;

    [Header("MUST have for each slot")]
    [Header("Data")]
    public INV_ItemSO itemSO;
    public int quantity;

    [Header("UI")]
    public Image itemImage;
    public TMP_Text amountText;

    static SHOP_Manager shop_Manager;

    void Awake()
    {
        itemImage ??= transform.Find("Icon")?.GetComponent<Image>();
        amountText ??= transform.Find("QuantityText")?.GetComponent<TMP_Text>();
        inv_Manager ??= GetComponentInParent<INV_Manager>();

        if (!itemImage)     Debug.LogError($"INV_Slots: itemImage missing.", this);
        if (!amountText)    Debug.LogError($"INV_Slots: amountText missing.", this);
        if (!inv_Manager)   Debug.LogError($"INV_Slots: INV_Manager missing in parent.", this);
    }

    void OnEnable()  => SHOP_Keeper.OnShopStateChanged += HandleShopStateChanged;
    void OnDisable() => SHOP_Keeper.OnShopStateChanged -= HandleShopStateChanged;

    void HandleShopStateChanged(SHOP_Manager shop, bool isOpen)
    {
        shop_Manager = isOpen ? shop : null;
    }

    // update icon and amount text
    public void UpdateUI()
    {
        if (itemSO)
        {
            itemImage.enabled = true;
            itemImage.sprite = itemSO.image;
            amountText.text = quantity.ToString();
        }
        else
        {
            itemImage.enabled = false;
            amountText.text = "";
        }
    }

    // LEFT = use or sell; RIGHT = drop (ONLY IF shop closed)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemSO == null) return;

        // Shop open: Right = sell 1; disable use/drop while open
        if (shop_Manager)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                shop_Manager.SellItem(itemSO);
                quantity -= 1;
                if (quantity <= 0) itemSO = null;
                UpdateUI();
            }
            return;
        }
        // Shop closed: normal behavior
        else
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                inv_Manager.UseItem(this);
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                inv_Manager.DropItem(itemSO, 1);
                quantity -= 1;
                if (quantity <= 0) itemSO = null;
                UpdateUI();
            }
        }
    }
}
