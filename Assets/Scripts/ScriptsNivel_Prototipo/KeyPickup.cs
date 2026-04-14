using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KeyCounterUI.Instance.AddKey();
            Destroy(gameObject);
        }
    }
}