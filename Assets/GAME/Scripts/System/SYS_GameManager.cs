using System.Collections;
using UnityEngine;

public enum Difficulty { Normal, Easy }

public class SYS_GameManager : MonoBehaviour
{
    public static SYS_GameManager Instance;

    [Header("Central game manager - handles singletons, scene, audio")]
    [Header("References")]
    public SYS_Fader        sys_Fader;
    public SYS_SoundManager sys_SoundManager;
    public D_Manager        d_Manager;
    public D_HistoryTracker d_HistoryTracker;
    public SHOP_Manager     shop_Manager;
    public INV_ItemInfo     inv_ItemInfo;
    public AudioSource      audioSource;
    public Animator         fadeAnimator;

    [Header("Restart Settings")]
    public string initialSceneName = "Level0";
    public string initialSpawnId   = "Start";

    [Header("Tutorial Defeat Settings")]
    public string SceneAfterTuto  = "Level2";
    public string PosAfterTuto    = "PosAfterTuto";
    public float  DeathAnimDur    = 1.5f;  // Time to show death animation
    public float  gameOverDelay   = 2f;    // Delay before showing Game Over UI

    [Header("Audio Settings")]
    public float fadeDuration = 1.5f;

    [Header("Resurrection System")]
    public bool      hasResurrection     = false;
    public float     resurrectionDelay   = 1.5f;
    public float     resurrectionPause   = 2.0f;
    public AudioClip resurrectionSFX;

    [Header("Difficulty Settings")]
    public Difficulty currentDifficulty    = Difficulty.Normal;
    public int        easyBonusSkillPoints = 10;
    public int        easyBonusMaxHP       = 100;
    public int        easyBonusAD          = 5;
    public int        easyBonusMaxMP       = 5;

    [Header("MUST wire MANUALLY in Inspector")]
    public GameObject[] persistentObjects;

    Coroutine currentFade;

    void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null)
        {
            CleanUpAndDestroy();
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        MarkPersistentObjects();

        d_Manager        ??= FindFirstObjectByType<D_Manager>();
        d_HistoryTracker ??= FindFirstObjectByType<D_HistoryTracker>();
        shop_Manager     ??= FindFirstObjectByType<SHOP_Manager>();
        sys_Fader        ??= FindFirstObjectByType<SYS_Fader>();
        sys_SoundManager ??= FindFirstObjectByType<SYS_SoundManager>();
        inv_ItemInfo     ??= FindFirstObjectByType<INV_ItemInfo>();
        audioSource      ??= GetComponent<AudioSource>();

        if (!d_Manager)        { Debug.LogWarning($"{name}: D_Manager is missing!", this); }
        if (!d_HistoryTracker) { Debug.LogWarning($"{name}: D_HistoryTracker is missing!", this); }
        if (!shop_Manager)     { Debug.LogWarning($"{name}: SHOP_Manager is missing!", this); }
        if (!sys_Fader)        { Debug.LogWarning($"{name}: SYS_Fader is missing!", this); }
        if (!sys_SoundManager) { Debug.LogWarning($"{name}: SYS_SoundManager is missing!", this); }
        if (!inv_ItemInfo)     { Debug.LogWarning($"{name}: INV_ItemInfo is missing!", this); }
        if (!audioSource)      { Debug.LogWarning($"{name}: AudioSource is missing!", this); }
        
        inv_ItemInfo?.Hide();

        if (audioSource)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
    }

    // Restart the game by clearing all DDOL objects and loading the initial scene
    public void FreshBoot()
    {
        Time.timeScale = 1f;

        var temp = new GameObject("DDOL_Cleaner");
        DontDestroyOnLoad(temp);
        var ddol = temp.scene;
        Object.Destroy(temp);
        foreach (var root in ddol.GetRootGameObjects())
            Object.Destroy(root);

        UnityEngine.SceneManagement.SceneManager.LoadScene(initialSceneName);
    }

    // PLAYER DEATH HANDLING
    
    public void HandlePlayerDeath(P_Controller player)
    {
        if (hasResurrection)
        {
            StartCoroutine(ResurrectionSequence(player));
        }
        else if (player.isInTutorialZone)
        {
            StartCoroutine(TutorialDeathSequence(player));
        }
        else
        {
            StartCoroutine(NormalDeathSequence());
        }
    }
    
    IEnumerator ResurrectionSequence(P_Controller player)
    {
        hasResurrection = false;
        
        yield return new WaitForSeconds(resurrectionDelay);
        
        Time.timeScale = 0f;
        
        fadeAnimator.Play("FadeOut");
        yield return new WaitForSecondsRealtime(sys_Fader.fadeTime);
        
        yield return new WaitForSecondsRealtime(resurrectionPause);
        
        if (resurrectionSFX)
        {
            audioSource.PlayOneShot(resurrectionSFX);
        }
        
        fadeAnimator.Play("FadeIn");
        yield return new WaitForSecondsRealtime(sys_Fader.fadeTime);
        
        player.Revive(player.transform.position);
        
        Time.timeScale = 1f;
    }
    
    IEnumerator NormalDeathSequence()
    {
        yield return new WaitForSeconds(gameOverDelay);
        
        var endingUI = FindFirstObjectByType<EndingUI>();
        if (endingUI) endingUI.Show(false);
        else Debug.LogError("SYS_GameManager: EndingUI not found!");
    }
    
    IEnumerator TutorialDeathSequence(P_Controller player)
    {
        // Let death animation play
        yield return new WaitForSeconds(DeathAnimDur);
        
        // Fade to black
        if (sys_Fader)
        {
            sys_Fader.FadeToScene(SceneAfterTuto);
            yield return new WaitForSeconds(sys_Fader.fadeTime);
        }
        
        // Load Level2 (all DDOL objects come along automatically)
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneAfterTuto);
        yield return new WaitForSeconds(0.3f);
        
        // Find spawn point in new scene
        var spawnPoint = GameObject.Find(PosAfterTuto);
        if (!spawnPoint)
        {
            Debug.LogError($"SYS_GameManager: Spawn point '{PosAfterTuto}' not found in {SceneAfterTuto}!");
            yield break;
        }
        
        // Revive player at spawn point
        player.Revive(spawnPoint.transform.position);
        
        if (sys_Fader)
        {
            yield return new WaitForSeconds(sys_Fader.fadeTime);
        }
        
        hasResurrection = true;
    }

    // Play music clip instantly without fading
    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Fade smoothly from current music to new clip, stops any existing fade
    public void PlayMusicWithFade(AudioClip newClip)
    {
        // Don't fade if already playing this clip
        if (audioSource.clip == newClip && audioSource.isPlaying)
            return;

        // Stop current fade to prevent conflicts
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
            currentFade = null;
        }

        currentFade = StartCoroutine(FadeToNewMusic(newClip));
    }

    IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float elapsed      = 0f;
        float halfDuration = fadeDuration * 0.5f;
        float startVolume  = audioSource.volume;

        // Fade out current music
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, smoothT);
            yield return null;
        }

        // Swap clips
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(0f, startVolume, smoothT);
            yield return null;
        }

        audioSource.volume = startVolume;
        currentFade        = null;
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
    }

    public void ApplyEasyModeBonuses()
    {
        if (currentDifficulty != Difficulty.Easy) return;

        var player = FindFirstObjectByType<P_Exp>();
        if (!player)
        {
            Debug.LogWarning("SYS_GameManager: P_Exp not found for easy mode bonuses!");
            return;
        }

        var c_Stats = player.GetComponent<C_Stats>();
        if (!c_Stats)
        {
            Debug.LogWarning("SYS_GameManager: C_Stats not found on player!");
            return;
        }

        player.AddSkillPoints(easyBonusSkillPoints);
        c_Stats.maxHP += easyBonusMaxHP;
        c_Stats.currentHP = c_Stats.maxHP;
        c_Stats.AD += easyBonusAD;
        c_Stats.maxMP += easyBonusMaxMP;
        c_Stats.currentMP = c_Stats.maxMP;

        Debug.Log($"Easy Mode bonuses applied: +{easyBonusSkillPoints} SP, +{easyBonusMaxHP} HP, +{easyBonusAD} AD, +{easyBonusMaxMP} MP");
    }

    void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
            if (obj)
                DontDestroyOnLoad(obj);
    }

    void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
            Destroy(obj);
        Destroy(gameObject);
    }
}
