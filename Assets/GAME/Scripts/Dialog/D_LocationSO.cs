// <summary>
// Used to define a location in the dialogue system.
// </summary>

using UnityEngine;

[CreateAssetMenu(fileName = "LocationSO", menuName = "Dialogue/LocationSO")]
public class D_LocationSO : ScriptableObject
{
    [Header("Unique ID for this location")]
    public string locationID;
    [Header("The name shown in dialogue/UI")]
    public string displayName = "Auto Filled by OnValidate";

    void OnValidate()
    {
        displayName = name;
    }
}
