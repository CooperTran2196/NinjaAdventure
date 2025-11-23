using UnityEngine;

[CreateAssetMenu(fileName = "LocationSO", menuName = "Dialog/LocationSO")]
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
