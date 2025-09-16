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

    void Awake()
    {

        itemImage  ??= transform.Find("Icon")?.GetComponent<Image>();
        amountText ??= transform.Find("QuantityText")?.GetComponent<TMP_Text>();
        inv        ??= GetComponentInParent<INV_Manager>();

        if (!itemImage)  Debug.LogError($"{name}: Item Image missing.", this);
        if (!amountText) Debug.LogError($"{name}: Amount Text missing.", this);
        if (!inv)        Debug.LogError($"{name}: INV_Manager missing in parent.", this);
    }

    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.enabled = true;
            itemImage.sprite  = item.icon;
            amountText.text   = quantity.ToString();
        }
        else
        {
            itemImage.enabled = false;
            amountText.text   = "";
        }
    }

    // Left click to use, Right click to drop 1
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (item == null || quantity <= 0) return;
            inv.UseItem(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item == null || quantity <= 0) return;
            inv.DropItemFromSlot(this);
        }
    }
}
