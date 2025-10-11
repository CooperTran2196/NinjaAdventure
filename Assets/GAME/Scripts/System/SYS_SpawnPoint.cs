// <summary>
// Spawns the player at the correct spawn point based on the last teleporter used.
// </summary>

using UnityEngine;

public class SYS_SpawnPoint : MonoBehaviour
{
    [Header("Must match the teleporter's Destination Spawn Id")]
    public string spawnId = "DoorA";
    

    void Start()
    {
        // Only move the player if this has the correct spawn ID
        if (SYS_SceneTeleport.nextSpawnId != spawnId) return;

        // Find the player and move them here
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        player.position = transform.position;
        SYS_SaveSystem.Instance.NotifySpawnCommitted(spawnId, transform.position);

    }
}
