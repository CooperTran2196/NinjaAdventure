using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class INV_Slots : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    public INV_ItemSO item;
    public int quantity;

    [Header("UI")]
    public Image itemImage;
    public TMP_Text amountText;

    INV_Manager inv;
    static SHOP_Manager activeShop;

    void Awake()
    {

        itemImage  ??= transform.Find("Icon")?.GetComponent<Image>();
        amountText ??= transform.Find("QuantityText")?.GetComponent<TMP_Text>();
        inv        ??= GetComponentInParent<INV_Manager>();

        if (!itemImage)  Debug.LogError($"{name}: Item Image missing.", this);
        if (!amountText) Debug.LogError($"{name}: Amount Text missing.", this);
        if (!inv)        Debug.LogError($"{name}: INV_Manager missing in parent.", this);
    }

    void OnEnable()  => SHOP_Manager.OnShopStateChanged += HandleShopStateChanged;
    void OnDisable() => SHOP_Manager.OnShopStateChanged -= HandleShopStateChanged;

    void HandleShopStateChanged(SHOP_Manager shop, bool isOpen)
    {
        activeShop = isOpen ? shop : null;
    }

    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.enabled = true;
            itemImage.sprite = item.icon;
            amountText.text = quantity.ToString();
        }
        else
        {
            itemImage.enabled = false;
            amountText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (item == null || quantity <= 0) return;

        // Shop open: LEFT = sell 1; disable use/drop while open
        if (activeShop != null)
        {
            if (e.button == PointerEventData.InputButton.Left)
            {
                activeShop.SellItem(item);
                quantity -= 1;
                if (quantity <= 0) item = null;
                UpdateUI();
            }
            return;
        }

        // Shop closed: normal behavior
        if (e.button == PointerEventData.InputButton.Left)
            inv.UseItem(this);
        else if (e.button == PointerEventData.InputButton.Right)
            inv.DropItemFromSlot(this);
    }
}
