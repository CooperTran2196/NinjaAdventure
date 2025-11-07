using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class TutorialDeathZone : MonoBehaviour
{
    [Header("Simple trigger zone - dying here transitions to Level2")]
    
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (!col) Debug.LogError($"{name}: Collider2D is missing!", this);
    }
}
