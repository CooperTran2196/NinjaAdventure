// <summary>
// Teleports the player to a different scene and spawn point when they enter the trigger.
// </summary>

using UnityEngine;

public class SYS_SceneTeleport : MonoBehaviour
{
    [Header("References")]
    public string sceneToLoad = "The Name of the Scene to Load";
    public static string nextSpawnId;
    public string destinationSpawnId = "DoorA";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        nextSpawnId = destinationSpawnId;
        SYS_GameManager.Instance.sys_Fader.FadeToScene(sceneToLoad);
    }
}
