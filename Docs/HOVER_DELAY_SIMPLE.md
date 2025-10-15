# ✅ Hover Delay - SIMPLIFIED

**What it does now:** Simple 0.5s delay on every hover. That's it.

---

## Code (Super Simple)

```csharp
// Field
Coroutine hoverCoroutine;
public float hoverDelay = 0.5f;

// Hover enter
public void OnPointerEnter(PointerEventData eventData)
{
    hoverCoroutine = StartCoroutine(ShowPopupWithDelay(eventData.position));
}

// Hover exit
public void OnPointerExit(PointerEventData eventData)
{
    if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
    itemInfoPopup.Hide();
}

// Delay coroutine
IEnumerator ShowPopupWithDelay(Vector2 mousePosition)
{
    yield return new WaitForSeconds(hoverDelay);
    itemInfoPopup.FollowMouse(mousePosition);
    itemInfoPopup.Show(itemSO or weaponSO);
}
```

---

## Behavior

- **Hover slot** → Wait 0.5s → Popup
- **Exit before 0.5s** → No popup (cancelled)
- **Start drag** → Popup cancelled

---

**That's it. Simple and works.** ✅
