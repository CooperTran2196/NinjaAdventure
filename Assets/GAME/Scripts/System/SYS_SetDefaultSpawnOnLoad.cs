using UnityEngine;

[DisallowMultipleComponent]
public class SYS_SetDefaultSpawnOnLoad : MonoBehaviour
{
    [SerializeField] private string defaultSpawnId = "Start";
    [SerializeField] private bool onlyIfEmpty = true;

    void Awake()
    {
        if (!onlyIfEmpty || string.IsNullOrEmpty(SYS_SceneTeleport.nextSpawnId))
            SYS_SceneTeleport.nextSpawnId = defaultSpawnId;
    }
}
