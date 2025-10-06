// <summary>
// Attach this to a trigger collider to mark a location as visited when the player enters it.
// </summary>
 
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class D_LocationVisitedTrigger : MonoBehaviour
{
    [SerializeField] D_LocationSO location;
    [Header("SET FALSE on teleporters")]
    [SerializeField] bool destroyOnTouch = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SYS_GameManager.Instance.d_HistoryTracker.RecordLocation(location);
        if (destroyOnTouch)
            Destroy(gameObject);
    }
}
