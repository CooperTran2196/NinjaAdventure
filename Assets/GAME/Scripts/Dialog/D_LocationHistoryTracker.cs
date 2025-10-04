// <summary>
// Tracks locations visited by the player in the dialogue system.
// </summary>

using UnityEngine;
using System.Collections.Generic;

public class D_LocationHistoryTracker : MonoBehaviour
{
    public static D_LocationHistoryTracker Instance;

    // Using HashSet to avoid duplicate entries/ Don't care about order
    public readonly HashSet<D_LocationSO> locationsVisited = new();

    // Singleton pattern
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Add the location to locationsVisited if not already present
    public void RecordLocation(D_LocationSO locationSO)
    {
        if (locationsVisited.Add(locationSO))
        {
            Debug.Log($"Visited location: {locationSO.displayName}");
        }
    }

    // Check if we've visited this location before
    public bool HasVisited(D_LocationSO locationSO)
    {
        return locationsVisited.Contains(locationSO);
    }
}
