using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class INV_Slots : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotType { Empty, Item, Weapon }

    [Header("References")]
    INV_Manager  inv_Manager;
    Canvas       canvas;
    INV_ItemInfo itemInfoPopup;
    
    [Header("MUST wire MANUALLY in Inspector")]
    public Image      itemImage;
    public TMP_Text   amountText;
    public GameObject draggingIconPrefab;

    [Header("Data")]
                    public SlotType    type     = SlotType.Empty;
                    public INV_ItemSO  itemSO;
                    public W_SO        weaponSO;
                    public int         quantity;

    [Header("Hover Settings")]
                    public float hoverDelay = 1f;

    // Runtime state
    static SHOP_Manager shop_Manager;
    GameObject currentDragIcon;
    Coroutine hoverCoroutine;

    void Awake()
    {
        // GetComponent same-GameObject refs
        itemImage   ??= transform.Find("Icon")?.GetComponent<Image>();
        amountText  ??= transform.Find("QuantityText")?.GetComponent<TMP_Text>();
        inv_Manager ??= GetComponentInParent<INV_Manager>();
        canvas      = GetComponentInParent<Canvas>();

        // Validate required components
        if (!itemImage)  { Debug.LogError($"{name}: itemImage is missing!", this); return; }
        if (!amountText) { Debug.LogError($"{name}: amountText is missing!", this); return; }
        if (!inv_Manager) { Debug.LogError($"{name}: INV_Manager is missing!", this); return; }
        if (!canvas)     { Debug.LogError($"{name}: Canvas parent is missing!", this); return; }
        
        // Validate "MUST wire MANUALLY" refs
        if (!draggingIconPrefab) { Debug.LogError($"{name}: draggingIconPrefab is not assigned!", this); return; }
    }

    void Start()
    {
        // Get shared popup from GameManager (may be null if not ready yet)
        if (SYS_GameManager.Instance != null)
        {
            itemInfoPopup = SYS_GameManager.Instance.inv_ItemInfo;
        }
        
        if (!itemInfoPopup) Debug.LogWarning($"{name}: itemInfoPopup not found - will retry on hover!", this);
    }

    void OnEnable()  => SHOP_Keeper.OnShopStateChanged += HandleShopStateChanged;
    void OnDisable() => SHOP_Keeper.OnShopStateChanged -= HandleShopStateChanged;

    void HandleShopStateChanged(SHOP_Manager shop, bool isOpen)
    {
        shop_Manager = isOpen ? shop : null;
    }

    // Update icon and amount text based on slot contents
    public void UpdateUI()
    {
        if (type == SlotType.Item && itemSO)
        {
            itemImage.enabled = true;
            itemImage.sprite = itemSO.image;
            itemImage.preserveAspect = false; // Items fill slot
            amountText.text = quantity.ToString();
        }
        else if (type == SlotType.Weapon && weaponSO)
        {
            itemImage.enabled = true;
            itemImage.sprite = weaponSO.image;
            itemImage.preserveAspect = true; // Weapons preserve aspect ratio
            amountText.text = ""; // Weapons don't stack
        }
        else
        {
            itemImage.enabled = false;
            amountText.text = "";
            type = SlotType.Empty;
        }
    }

    // Handle left/right click: use/equip, sell, drop
    public void OnPointerClick(PointerEventData eventData)
    {
        if (type == SlotType.Empty) return;

        // Shop interactions (items only)
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
                inv_Manager.UseItem(this);
            else if (type == SlotType.Weapon)
                inv_Manager.EquipWeapon(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (type == SlotType.Item)
            {
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
                inv_Manager.DropWeapon(weaponSO);
                weaponSO = null;
                type = SlotType.Empty;
            }
            
            UpdateUI();
        }
    }

    // ===== DRAG AND DROP SYSTEM =====
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (type == SlotType.Empty) return;
        if (draggingIconPrefab == null) return;

        // Cancel pending hover if dragging starts
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        itemInfoPopup?.Hide();

        // Create visual icon that follows mouse
        currentDragIcon = Instantiate(draggingIconPrefab, canvas.transform);
        currentDragIcon.transform.SetAsLastSibling();
        
        // Copy item/weapon sprite to drag icon
        Image iconImage = currentDragIcon.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = itemImage.sprite;
        }
        
        // Fade original slot icon (looks like item lifted off)
        Color originalColor = itemImage.color;
        itemImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentDragIcon != null)
        {
            currentDragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy floating icon
        if (currentDragIcon != null)
        {
            Destroy(currentDragIcon);
        }
        
        // Restore original slot icon to full opacity
        Color originalColor = itemImage.color;
        itemImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        INV_Slots draggedSlot = eventData.pointerDrag?.GetComponent<INV_Slots>();
        
        if (draggedSlot == null || draggedSlot == this) return;

        SwapSlots(draggedSlot);
    }

    void SwapSlots(INV_Slots otherSlot)
    {
        // Store this slot's data
        SlotType   tempType     = type;
        INV_ItemSO tempItemSO   = itemSO;
        W_SO       tempWeaponSO = weaponSO;
        int        tempQuantity = quantity;

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

    // ===== HOVER SYSTEM =====

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (type == SlotType.Empty || !itemInfoPopup) return;

        hoverCoroutine = StartCoroutine(ShowPopupWithDelay(eventData.position));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancel pending hover if still waiting
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        itemInfoPopup?.Hide();
    }

    IEnumerator ShowPopupWithDelay(Vector2 mousePosition)
    {
        yield return new WaitForSeconds(hoverDelay);

        // Lazy initialization if not available in Awake
        if (!itemInfoPopup && SYS_GameManager.Instance != null)
        {
            itemInfoPopup = SYS_GameManager.Instance.inv_ItemInfo;
        }

        if (!itemInfoPopup)
        {
            hoverCoroutine = null;
            yield break;
        }

        itemInfoPopup.FollowMouse(mousePosition);

        if (type == SlotType.Item && itemSO)
        {
            itemInfoPopup.Show(itemSO);
        }
        else if (type == SlotType.Weapon && weaponSO)
        {
            itemInfoPopup.Show(weaponSO);
        }

        hoverCoroutine = null;
    }
}
