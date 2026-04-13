using UnityEngine;
using TMPro;

// Esto asegura que no puedas poner el script en un objeto que no tenga texto
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextoTiempoFinal : MonoBehaviour
{
    private TextMeshProUGUI textoVictoria;

    private void Awake()
    {
        // Cogemos el componente de texto automáticamente
        textoVictoria = GetComponent<TextMeshProUGUI>();
    }

    // OnEnable se ejecuta justo en el frame en que este objeto o su padre se activan (gameObject.SetActive(true))
    private void OnEnable()
    {
        if (TemporizadorGlobal.Instance != null)
        {
            // Ponemos el texto final
            textoVictoria.text = TemporizadorGlobal.Instance.ObtenerTiempoFormateado();
        }
    }
}