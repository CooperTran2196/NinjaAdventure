using UnityEngine;

public class SYS_SceneTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public string sceneToLoad        = "The Name of the Scene to Load";
    public string destinationSpawnId = "DoorA";
    
    public static string nextSpawnId;

    // When the player enters the teleporter, set the next spawn ID and load the new scene
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        nextSpawnId = destinationSpawnId;
        SYS_GameManager.Instance.sys_Fader.FadeToScene(sceneToLoad);
    }
}
