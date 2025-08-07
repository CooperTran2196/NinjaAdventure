using UnityEngine;  

public class AudioManager : MonoBehaviour  
{  
    public static AudioManager Instance;  
    private AudioSource bgmSource;  

    private void Awake()  
    {  
        if (Instance == null)  
        {  
            Instance = this;  
        }  
        else  
        {  
            Destroy(gameObject);  
            return;  
        }  
        bgmSource = gameObject.AddComponent<AudioSource>();  
        bgmSource.loop = true;  
    }  

    public void PlayMusic(AudioClip clip)  
    {  
        if (bgmSource.clip != clip)  
        {  
            bgmSource.Stop();  
            bgmSource.clip = clip;  
            bgmSource.Play();  
        }  
    }  
}  