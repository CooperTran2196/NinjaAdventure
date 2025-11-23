using UnityEngine;

public class SYS_SpawnPoint : MonoBehaviour
{
    [Header("Must match the teleporter's Destination Spawn Id")]
    public string spawnId = "DoorA";

    // Moves the player to this spawn point if spawn ID matches
    void Start()
    {
        // Only move the player if this has the correct spawn ID
        if (SYS_SceneTeleport.nextSpawnId != spawnId) return;

        // Find the player and move them here
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            playerObj.transform.position = transform.position;
        }
        else
        {
            Debug.LogWarning($"{name}: Player GameObject not found!", this);
        }
    }
}
