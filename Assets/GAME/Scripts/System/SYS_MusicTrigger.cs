using UnityEngine;

public class SYS_MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    // When the player enters the trigger, play the specified music with fade
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SYS_GameManager.Instance.PlayMusicWithFade(audioClip);
    }
}
