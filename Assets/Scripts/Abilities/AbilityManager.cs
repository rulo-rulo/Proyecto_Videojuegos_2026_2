using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    private MonoBehaviour currentAbility;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanUseAbility(MonoBehaviour ability)
    {
        return currentAbility == null || currentAbility == ability;
    }

    public void RegisterAbility(MonoBehaviour ability)
    {
        currentAbility = ability;
    }

    public void ClearAbility(MonoBehaviour ability)
    {
        if (currentAbility == ability)
            currentAbility = null;
    }
}