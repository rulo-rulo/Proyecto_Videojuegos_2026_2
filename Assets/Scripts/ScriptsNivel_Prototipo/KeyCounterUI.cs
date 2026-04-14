using TMPro;
using UnityEngine;

public class KeyCounterUI : MonoBehaviour
{
    public static KeyCounterUI Instance;

    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private int totalKeys = 5;

    private int collectedKeys = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddKey()
    {
        collectedKeys++;

        if (collectedKeys > totalKeys)
        {
            collectedKeys = totalKeys;
        }

        UpdateUI();
    }

    public void SetTotalKeys(int amount)
    {
        totalKeys = amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (keyText == null)
        {
            Debug.LogError("KeyCounterUI: keyText no está asignado en el Inspector.");
            return;
        }

        keyText.text = "x " + collectedKeys + "/" + totalKeys;
    }

    public bool HasAllKeys()
    {
        return collectedKeys >= totalKeys;
    }
}