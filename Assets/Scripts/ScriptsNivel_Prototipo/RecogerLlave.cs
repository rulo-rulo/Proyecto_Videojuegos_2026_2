using UnityEngine;

public class RecogerLlave : MonoBehaviour
{
    private bool recogida = false;

    private void OnTriggerEnter(Collider other)
    {
        if (recogida) return;

        if (other.CompareTag("Player"))
        {
            recogida = true;

            if (KeyCounterUI.Instance != null)
            {
                KeyCounterUI.Instance.AddKey();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecogerLlave();
            }

            Destroy(gameObject);
        }
    }
}