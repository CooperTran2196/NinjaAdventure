using UnityEngine;

[DisallowMultipleComponent]
public class SYS_Checkpoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Checkpoint"; // unique per checkpoint

    // Assumes a 2D trigger collider is on this GameObject.
    // Tag your Player as "Player" (already in your project).
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Mark this as the latest checkpoint and quick autosave
        SYS_SaveSystem.Instance.NotifyCheckpointReached(spawnId, transform.position);
    }

    // Optional helper if you ever want to set it from code:
    public void SetSpawnId(string id) => spawnId = id;
}