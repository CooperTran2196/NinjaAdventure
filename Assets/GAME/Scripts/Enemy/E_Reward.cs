using UnityEngine;

public class E_Reward : MonoBehaviour
{
    [Header("Rewards")]
    public int expReward = 10; // tweak per enemy prefab

    C_Health c_Health;
    static P_Exp p_Exp;

    void Awake()
    {
        c_Health ??= GetComponentInParent<C_Health>();
        if (!p_Exp) p_Exp = FindFirstObjectByType<P_Exp>();

        if (!c_Health) Debug.LogError($"{name}: C_Health is missing in E_Reward");
        if (!p_Exp) Debug.LogError($"{name}: P_Exp is missing in E_Reward");
    }

    void OnEnable()
    {
        c_Health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        c_Health.OnDied -= HandleDied;
    }

    void HandleDied()
    {
        p_Exp?.AddXP(expReward);
    }
}
