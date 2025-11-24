using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class TutorialDeathZone : MonoBehaviour
{
    [Header("Trigger zone - notifies GameManager when player enters/exits")]
    
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's the player
        if (other.GetComponent<P_Controller>() != null)
        {
            SYS_GameManager.Instance.SetPlayerInTutorialZone(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if it's the player
        if (other.GetComponent<P_Controller>() != null)
        {
            SYS_GameManager.Instance.SetPlayerInTutorialZone(false);
        }
    }
}
