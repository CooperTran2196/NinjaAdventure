using UnityEngine;

public class SYS_MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SYS_GameManager.Instance.PlayMusic(audioClip);
        }
    }
}
