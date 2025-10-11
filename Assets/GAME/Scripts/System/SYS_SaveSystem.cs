// ===============================
// File: SYS_SaveSystem.cs
// Place on a persistent (DontDestroyOnLoad) object.
// ===============================
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central JSON Saving/Loading orchestrator.
/// - Reconstructive Loading (Inventory → Skills → Progress → Recalc → exact HP with safe clamp)
/// - Autosave after spawn commit + after checkpoints
/// - Simple API for EndingUI and future Save/Load buttons
/// - Uses manager-provided "Saving/Loading" snapshots; this class stays dumb about math.
/// </summary>
[DisallowMultipleComponent]
public class SYS_SaveSystem : MonoBehaviour
{
    public static SYS_SaveSystem Instance;

    [Header("Files")]
    [SerializeField] private string autoSaveFile   = "autosave.json";
    [SerializeField] private string initialSaveFile= "initial.json";

    [Header("Runtime (view)")]
    [SerializeField] private string lastSavedScene = "";
    [SerializeField] private string lastSpawnId    = "";
    [SerializeField] private Vector3 lastSpawnPos  = Vector3.zero;
    [SerializeField] private string bossDeathReturnSpawnId = ""; // non-empty during boss fights

    private GameObject playerRef;
    private bool initialWritten;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ItemRegistry.Init();
        SkillRegistry.Init();
    }

    // --- Public API: called by spawners/checkpoints (we will add one-liners later) ---

    /// <summary>Call this AFTER a SYS_SpawnPoint actually moved the player.</summary>
    public void NotifySpawnCommitted(string spawnId, Vector3 position)
    {
        lastSavedScene = SceneManager.GetActiveScene().name;
        lastSpawnId    = spawnId;
        lastSpawnPos   = position;

        if (!playerRef) playerRef = GameObject.FindGameObjectWithTag("Player");

        // Write autosave and initial (once).
        AutoSaveNow();
        if (!initialWritten)
        {
            SaveGame(initialSaveFile);
            initialWritten = true;
        }
    }

    /// <summary>Call this from a checkpoint trigger whenever the player crosses it.</summary>
    public void NotifyCheckpointReached(string spawnId, Vector3 position)
    {
        lastSavedScene = SceneManager.GetActiveScene().name;
        lastSpawnId    = spawnId;
        lastSpawnPos   = position;

        if (!playerRef) playerRef = GameObject.FindGameObjectWithTag("Player");

        // Quick autosave so Replay returns here.
        AutoSaveNow();
    }

    // --- Boss respawn selection (set when fight starts; clear on victory) ---
    public void SetBossDeathReturnSpawn(string spawnId) => bossDeathReturnSpawnId = spawnId;
    public void ClearBossDeathReturnSpawn()             => bossDeathReturnSpawnId = "";

    // --- Buttons / UI (manual saves) ---

    /// <summary>Manual save to a named slot (e.g., "slot1.json").</summary>
    public void SaveGame(string fileName)
    {
        var data = BuildSaveData();
        WriteJsonAtomic(GetPath(fileName), JsonUtility.ToJson(data, prettyPrint: true));
    }

    /// <summary>Manual load from a named slot and travel there (EndingUI can call this later).</summary>
    public bool LoadGameAndTravel(string fileName, string overrideSpawnId = null)
    {
        var json = ReadJsonOrNull(GetPath(fileName));
        if (string.IsNullOrEmpty(json)) return false;

        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return false;

        // Rebuild game state in current scene so UI etc. stay consistent pre-travel
        ApplyLoadedData(data, preferExactHP: true);

        // Decide spawn
        var spawnToUse = !string.IsNullOrEmpty(overrideSpawnId) ? overrideSpawnId : data.lastSpawnId;
        SYS_SceneTeleport.nextSpawnId = spawnToUse;

        // Travel
        var gm = SYS_GameManager.Instance;
        if (gm && gm.sys_Fader) gm.sys_Fader.FadeToScene(data.sceneName);
        else SceneManager.LoadScene(data.sceneName);

        return true;
    }

    // --- EndingUI helpers (hook now; UI buttons can reuse) ---

    /// <summary>Replay from autosave (Game Over). Uses bossDeathReturnSpawnId if set, else last autosave spawn.</summary>
    public bool ReplayFromAutosave()
    {
        var json = ReadJsonOrNull(GetPath(autoSaveFile));
        if (string.IsNullOrEmpty(json)) return false;

        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return false;

        ApplyLoadedData(data, preferExactHP: true);

        // Spawn selection: boss-return takes priority
        var spawnToUse = !string.IsNullOrEmpty(bossDeathReturnSpawnId) ? bossDeathReturnSpawnId : data.lastSpawnId;
        SYS_SceneTeleport.nextSpawnId = spawnToUse;

        var gm = SYS_GameManager.Instance;
        if (gm && gm.sys_Fader) gm.sys_Fader.FadeToScene(data.sceneName);
        else SceneManager.LoadScene(data.sceneName);

        return true;
    }

    /// <summary>Advance to next scene, keep progress. Next scene will autosave after spawn.</summary>
    public void AdvanceToNextScene(string nextSceneName, string nextSpawnId = "Start")
    {
        SYS_SceneTeleport.nextSpawnId = nextSpawnId;

        var gm = SYS_GameManager.Instance;
        if (gm && gm.sys_Fader) gm.sys_Fader.FadeToScene(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>Restart from very beginning (first initial save written this session).</summary>
    public bool RestartFromInitial()
    {
        return LoadGameAndTravel(initialSaveFile);
    }

    /// <summary>Write the current world to autosave.json without traveling.</summary>
    public void AutoSaveNow()
    {
        var data = BuildSaveData();
        WriteJsonAtomic(GetPath(autoSaveFile), JsonUtility.ToJson(data, prettyPrint: true));
    }

    // ================== Core build/apply ==================

    private SaveData BuildSaveData()
    {
        if (!playerRef) playerRef = GameObject.FindGameObjectWithTag("Player");

        var scene = SceneManager.GetActiveScene().name;

        // Managers we orchestrate (they will implement these methods next)
        var inv = INV_Manager.Instance;
        var st  = FindFirstObjectByType<ST_Manager>();
        var pxp = playerRef ? playerRef.GetComponent<P_Exp>() : null;
        var cs  = playerRef ? playerRef.GetComponent<C_Stats>() : null;

        // Gather snapshots from managers (Saving)
        var invSnap  = inv  ? inv.SavingInventory()  : new InventorySave();
        var skillSnap= st   ? st.SavingSkills()      : new SkillsSave();
        var progSnap = pxp  ? pxp.SavingProgress()   : new ProgressSave();

        // Health exact + percent
        int hpAbs = cs != null ? Mathf.Clamp(cs.currentHP, 0, cs.maxHP) : 0;
        float hpPct = (cs != null && cs.maxHP > 0) ? Mathf.Clamp01(cs.currentHP / (float)cs.maxHP) : 0f;

        var data = new SaveData
        {
            schema      = 1,
            timestamp   = DateTime.UtcNow.ToString("o"),
            sceneName   = lastSavedScene == "" ? scene : lastSavedScene,
            lastSpawnId = lastSpawnId,
            lastSpawnPos= lastSpawnPos,
            bossReturnSpawnId = bossDeathReturnSpawnId,

            player = new PlayerSave
            {
                position   = playerRef ? playerRef.transform.position : Vector3.zero, // informational
                hpAbsolute = hpAbs,
                hpPercent  = hpPct,
                progress   = progSnap,
                inventory  = invSnap,
                skills     = skillSnap
            }
        };

        return data;
    }

    private void ApplyLoadedData(SaveData data, bool preferExactHP)
    {
        if (!playerRef) playerRef = GameObject.FindGameObjectWithTag("Player");

        var inv = INV_Manager.Instance;
        var st  = FindFirstObjectByType<ST_Manager>();
        var pxp = playerRef ? playerRef.GetComponent<P_Exp>()     : null;
        var cs  = playerRef ? playerRef.GetComponent<C_Stats>()   : null;
        var sm  = FindFirstObjectByType<P_StatsManager>();

        // Loading: Inventory → Skills → Progress
        if (inv != null) inv.LoadingInventory(data.player.inventory);
        if (st  != null) st.LoadingSkills(data.player.skills);
        if (pxp != null) pxp.LoadingProgress(data.player.progress);

        // Rebuild derived stats once
        if (sm != null) sm.RecalculateAllStats();

        // Restore currentHP exactly when possible, with safe clamp
        if (cs != null)
        {
            if (preferExactHP)
            {
                int target = data.player.hpAbsolute;
                if (target > cs.maxHP) target = cs.maxHP;
                if (target < 0)        target = 0;
                cs.currentHP = target;
            }
            else
            {
                cs.currentHP = Mathf.RoundToInt(data.player.hpPercent * cs.maxHP);
            }
        }

        // Update runtime mirrors
        lastSavedScene = data.sceneName;
        lastSpawnId    = string.IsNullOrEmpty(bossDeathReturnSpawnId) ? data.lastSpawnId : bossDeathReturnSpawnId;
        lastSpawnPos   = data.lastSpawnPos;
    }

    // ================== JSON utils ==================

    private string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);

    private void WriteJsonAtomic(string path, string json)
    {
        try
        {
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SYS_SaveSystem] Failed to write save '{path}': {e.Message}");
        }
    }

    private string ReadJsonOrNull(string path)
    {
        try { return File.Exists(path) ? File.ReadAllText(path) : null; }
        catch (Exception e)
        {
            Debug.LogError($"[SYS_SaveSystem] Failed to read save '{path}': {e.Message}");
            return null;
        }
    }

    // ================== Serializable DTOs ==================
    // NOTE: Managers will use these types in their Saving/Loading methods.

    [Serializable]
    public class SaveData
    {
        public int schema;
        public string timestamp;

        public string sceneName;
        public string lastSpawnId;
        public Vector3 lastSpawnPos;
        public string bossReturnSpawnId;

        public PlayerSave player;
    }

    [Serializable]
    public class PlayerSave
    {
        public Vector3 position;     // informational; spawn uses spawnId
        public int     hpAbsolute;   // prefer exact on load
        public float   hpPercent;    // fallback

        public ProgressSave  progress;
        public InventorySave inventory;
        public SkillsSave    skills;
    }

    // --- Provided by INV_Manager.Saving/Loading ---
    [Serializable]
    public class InventorySave
    {
        public int gold;
        public List<InvItem> items = new List<InvItem>();
        [Serializable] public class InvItem { public string id; public int qty; }
    }

    // --- Provided by ST_Manager.Saving/Loading ---
    [Serializable]
    public class SkillsSave
    {
        public List<SkillEntry> skills = new List<SkillEntry>();
        [Serializable] public class SkillEntry { public string id; public int level; }
    }

    // --- Provided by P_Exp.Saving/Loading ---
    [Serializable]
    public class ProgressSave
    {
        public int level;
        public int currentExp;
        public int skillPoints;
        public int totalKills;
        public float playTime;
        public int goldMirror; // optional: if you keep gold here too (inv is canonical)
    }
}
