// <summary>
// Tracks locations visited by the player in the dialogue system.
// </summary>

using UnityEngine;
using System.Collections.Generic;

public class D_LocationHistoryTracker : MonoBehaviour
{

    // Using HashSet to avoid duplicate entries/ Don't care about order
    public readonly HashSet<D_LocationSO> locationsVisited = new();


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
