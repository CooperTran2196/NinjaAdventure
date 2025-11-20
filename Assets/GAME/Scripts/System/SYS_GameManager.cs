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
    public AudioSource      musicSource;
    public P_Exp            p_Exp;
    public INV_Manager      inv_Manager;
    
    EndingUI endingUI;

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
    public bool  hasResurrection          = false;
    public float resurrectionDeathAnimDur = 1.5f;  // Wait for death anim before fadeout
    public float resurrectionClipLength   = 2.0f;  // Length of resurrection SFX (during blackout)

    [Header("Difficulty Settings")]
    public Difficulty   currentDifficulty    = Difficulty.Normal;
    public int          easyBonusSkillPoints = 10;
    public INV_ItemSO   easyBonusItem;  // Item to give player in easy mode

    [Header("MUST wire MANUALLY in Inspector")]
    public GameObject[] persistentObjects;

    Coroutine currentMusicFade;

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
        endingUI         ??= FindFirstObjectByType<EndingUI>();
        musicSource      ??= GetComponent<AudioSource>();

        if (!d_Manager)        { Debug.LogWarning($"{name}: D_Manager is missing!", this); }
        if (!d_HistoryTracker) { Debug.LogWarning($"{name}: D_HistoryTracker is missing!", this); }
        if (!shop_Manager)     { Debug.LogWarning($"{name}: SHOP_Manager is missing!", this); }
        if (!sys_Fader)        { Debug.LogWarning($"{name}: SYS_Fader is missing!", this); }
        if (!sys_SoundManager) { Debug.LogWarning($"{name}: SYS_SoundManager is missing!", this); }
        if (!inv_ItemInfo)     { Debug.LogWarning($"{name}: INV_ItemInfo is missing!", this); }
        
        inv_ItemInfo?.Hide();

        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.7f;
    }

    // Restart the game by clearing all DDOL objects and loading the initial scene
    public void FreshBoot()
    {
        Time.timeScale = 1f;

        // Reset IntroUI static flag so it shows again
        IntroUI.Reset();

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
            StartCoroutine(ResurrectionSequence(player));
        else if (player.isInTutorialZone)
            StartCoroutine(TutorialDeathSequence(player));
        else
            StartCoroutine(NormalDeathSequence());
    }
    
    IEnumerator ResurrectionSequence(P_Controller player)
    {
        hasResurrection = false;
        
        // Fade music out during death animation
        yield return StartCoroutine(FadeMusicVolume(0f, resurrectionDeathAnimDur));
        
        // Freeze game
        Time.timeScale = 0f;
        
        // Fade out to black
        sys_Fader.PlayFade("FadeOut");
        yield return new WaitForSecondsRealtime(sys_Fader.fadeTime);
        
        // Play resurrection SFX and wait for it to finish
        sys_SoundManager.PlayResurrectionSFX();
        yield return new WaitForSecondsRealtime(resurrectionClipLength);
        
        // Revive player
        player.Revive(player.transform.position);
        
        // Fade in from black
        sys_Fader.PlayFade("FadeIn");
        yield return new WaitForSecondsRealtime(sys_Fader.fadeTime);
        
        // Unfreeze game
        Time.timeScale = 1f;
        
        // Fade music volume back
        yield return StartCoroutine(FadeMusicVolume(0.7f, 1f));
    }
    
    IEnumerator NormalDeathSequence()
    {
        yield return new WaitForSeconds(gameOverDelay);
        endingUI.Show(false);
    }
    
    IEnumerator TutorialDeathSequence(P_Controller player)
    {
        // Let death animation play
        yield return new WaitForSeconds(DeathAnimDur);
        
        // Fade to black
        sys_Fader.FadeToScene(SceneAfterTuto);
        yield return new WaitForSeconds(sys_Fader.fadeTime);
        
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
        
        yield return new WaitForSeconds(sys_Fader.fadeTime);
        
        hasResurrection = true;
    }

    // MUSIC MANAGEMENT
    
    // Fade smoothly from current music to new clip
    public void PlayMusicWithFade(AudioClip newClip)
    {
        if (musicSource.clip == newClip && musicSource.isPlaying) return;

        if (currentMusicFade != null)
        {
            StopCoroutine(currentMusicFade);
        }
        currentMusicFade = StartCoroutine(FadeToNewMusic(newClip));
    }

    // Fade music volume to target over duration
    IEnumerator FadeMusicVolume(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    // Fade out current music, swap clip, fade in new music
    IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float startVolume  = musicSource.volume;
        float halfDuration = fadeDuration * 0.5f;

        // Fade out
        yield return StartCoroutine(FadeMusicVolume(0f, halfDuration));

        // Swap clips
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        yield return StartCoroutine(FadeMusicVolume(startVolume, halfDuration));
        
        currentMusicFade = null;
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
    }

    public void ApplyEasyModeBonuses()
    {
        if (currentDifficulty != Difficulty.Easy) return;

        // Apply skill points bonus
        p_Exp.AddSkillPoints(easyBonusSkillPoints);
        
        // Give easy mode item
        if (easyBonusItem)
            inv_Manager.AddItem(easyBonusItem, 1);

        Debug.Log($"Easy Mode bonuses applied: +{easyBonusSkillPoints} SP, item: {easyBonusItem?.itemName}");
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
