using UnityEngine;  

public class MusicTrigger : MonoBehaviour  
{  
    [SerializeField] private AudioClip musicClip;  

    private void OnTriggerEnter2D(Collider2D other)  
    {  
        if (other.CompareTag("Player"))  
        {  
            AudioManager.Instance.PlayMusic(musicClip);  
        }  
    }  
}  