using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class INV_Slots : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public enum SlotType { Empty, Item, Weapon }

    [Header("Slot Type")]
    public SlotType type = SlotType.Empty;

    [Header("Data")]
    public INV_ItemSO itemSO;
    public W_SO weaponSO;
    public int quantity;

    [Header("UI References")]
    public Image itemImage;
    public TMP_Text amountText;

    [Header("Drag & Drop Prefab")]
    public GameObject draggingIconPrefab; // Visual icon that follows mouse during drag

    // Internal references
    INV_Manager inv_Manager;
    Canvas canvas;
    static SHOP_Manager shop_Manager;

    // Runtime drag state
    GameObject currentDragIcon;

    void Awake()
    {
        // Auto-find components
        itemImage   ??= transform.Find("Icon")?.GetComponent<Image>();
        amountText  ??= transform.Find("QuantityText")?.GetComponent<TMP_Text>();
        inv_Manager ??= GetComponentInParent<INV_Manager>();
        canvas      = GetComponentInParent<Canvas>();

        // Validate required references
        if (!itemImage)            Debug.LogError($"INV_Slots ({name}): itemImage is missing.");
        if (!amountText)           Debug.LogError($"INV_Slots ({name}): amountText is missing.");
        if (!inv_Manager)          Debug.LogError($"INV_Slots ({name}): INV_Manager is missing.");
        if (!canvas)               Debug.LogError($"INV_Slots ({name}): Canvas parent is missing.");
        if (!draggingIconPrefab)   Debug.LogError($"INV_Slots ({name}): Dragging Icon Prefab is not assigned!");
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
        if (type == SlotType.Item && itemSO)
        {
            // Existing item display
            itemImage.enabled = true;
            itemImage.sprite = itemSO.image;
            amountText.text = quantity.ToString();
        }
        else if (type == SlotType.Weapon && weaponSO)
        {
            // NEW: Weapon display
            itemImage.enabled = true;
            itemImage.sprite = weaponSO.image; // uses new image field
            amountText.text = ""; // weapons don't stack
        }
        else // Empty
        {
            itemImage.enabled = false;
            amountText.text = "";
            type = SlotType.Empty;
        }
    }

    // LEFT = use or sell; RIGHT = drop (ONLY IF shop closed)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (type == SlotType.Empty) return;

        // Shop interactions (items only, weapons can't be sold in shop)
        if (shop_Manager && type == SlotType.Item)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                shop_Manager.SellItem(itemSO);
                quantity -= 1;
                if (quantity <= 0)
                {
                    itemSO = null;
                    type = SlotType.Empty;
                }
                UpdateUI();
            }
            return;
        }

        // Normal interactions (shop closed)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (type == SlotType.Item)
                inv_Manager.UseItem(this); // existing
            else if (type == SlotType.Weapon)
                inv_Manager.EquipWeapon(this); // NEW method
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Drop to world
            if (type == SlotType.Item)
            {
                // Existing drop item code
                inv_Manager.DropItem(itemSO, 1);
                quantity -= 1;
                if (quantity <= 0)
                {
                    itemSO = null;
                    type = SlotType.Empty;
                }
            }
            else if (type == SlotType.Weapon)
            {
                // NEW: Drop weapon (placeholder for now)
                inv_Manager.DropWeapon(weaponSO);
                weaponSO = null;
                type = SlotType.Empty;
            }
            UpdateUI();
        }
    }

    // ===== DRAG AND DROP SYSTEM =====
    
    // Called ONCE when you start dragging (click + hold)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (type == SlotType.Empty) return;
        if (draggingIconPrefab == null) return;

        // Create visual icon that follows mouse
        currentDragIcon = Instantiate(draggingIconPrefab, canvas.transform);
        currentDragIcon.transform.SetAsLastSibling(); // Render on top of everything
        
        // Copy item/weapon sprite to the drag icon
        Image iconImage = currentDragIcon.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = itemImage.sprite;
        }
        
        // Fade original slot icon (so it looks like item "lifted off")
        Color originalColor = itemImage.color;
        itemImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
    }

    // Called EVERY FRAME while dragging (moves icon with mouse)
    public void OnDrag(PointerEventData eventData)
    {
        if (currentDragIcon != null)
        {
            currentDragIcon.transform.position = eventData.position;
        }
    }

    // Called ONCE when you release mouse button (cleanup)
    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy the floating icon
        if (currentDragIcon != null)
        {
            Destroy(currentDragIcon);
        }
        
        // Restore original slot icon to full opacity
        Color originalColor = itemImage.color;
        itemImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    // Called when you drop onto another slot
    public void OnDrop(PointerEventData eventData)
    {
        INV_Slots draggedSlot = eventData.pointerDrag?.GetComponent<INV_Slots>();
        
        if (draggedSlot == null || draggedSlot == this) return;

        SwapSlots(draggedSlot);
    }

    void SwapSlots(INV_Slots otherSlot)
    {
        // Store this slot's data
        SlotType    tempType     = type;
        INV_ItemSO  tempItemSO   = itemSO;
        W_SO        tempWeaponSO = weaponSO;
        int         tempQuantity = quantity;

        // Copy other slot's data to this slot
        type     = otherSlot.type;
        itemSO   = otherSlot.itemSO;
        weaponSO = otherSlot.weaponSO;
        quantity = otherSlot.quantity;

        // Copy this slot's original data to other slot
        otherSlot.type     = tempType;
        otherSlot.itemSO   = tempItemSO;
        otherSlot.weaponSO = tempWeaponSO;
        otherSlot.quantity = tempQuantity;

        // Update both UIs
        UpdateUI();
        otherSlot.UpdateUI();
    }
}
