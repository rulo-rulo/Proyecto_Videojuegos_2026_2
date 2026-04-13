using UnityEngine;
using TMPro; // Importante para usar TextMeshPro

public class InterfazTiempo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoTiempo;

    void Update()
    {
        // Accedemos al Singleton y usamos la función de formateo que creamos antes
        if (TemporizadorGlobal.Instance != null && textoTiempo != null)
        {
            textoTiempo.text = TemporizadorGlobal.Instance.ObtenerTiempoFormateado();
        }
    }
}