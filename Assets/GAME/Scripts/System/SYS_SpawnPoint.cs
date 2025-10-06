using UnityEngine;

public class SYS_SpawnPoint : MonoBehaviour
{
    [Header("Must match the teleporter's Destination Spawn Id")]
    public string spawnId = "DoorA";

    [Header("Optional: use this area's center as the arrival point")]
    public BoxCollider2D arrivalArea; // leave null to use transform.position

    void Start()
    {
        if (SYS_SceneTeleport.nextSpawnId != spawnId) return;

        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player) return; // assume Player is present; no other guards

        var pos = arrivalArea ? (Vector3)arrivalArea.bounds.center : transform.position;
        player.position = pos;
    }
}
