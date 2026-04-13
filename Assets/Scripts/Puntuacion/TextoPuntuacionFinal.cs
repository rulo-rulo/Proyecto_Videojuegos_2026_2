using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextoPuntuacionFinal : MonoBehaviour
{
    private TextMeshProUGUI textoPuntuacion;

    private void Awake()
    {
        textoPuntuacion = GetComponent<TextMeshProUGUI>();
    }

    // Recuerda: OnEnable salta justo cuando se enciende la pantalla de victoria
    private void OnEnable()
    {
        // Comprobamos que ambos gestores existan por seguridad
        if (TemporizadorGlobal.Instance != null && GestorPuntuacion.Instance != null)
        {
            // 1. Le decimos al gestor que calcule los puntos pasándole el tiempo que hemos hecho
            GestorPuntuacion.Instance.CalcularPuntuacionPorTiempo(TemporizadorGlobal.Instance.tiempoTranscurrido);

            // 2. Escribimos el resultado en el texto
            textoPuntuacion.text = $"Puntuación: {GestorPuntuacion.Instance.PuntuacionFinal}";
        }
    }
}