using UnityEngine;

public class GestorPuntuacion : MonoBehaviour
{
    public static GestorPuntuacion Instance;

    [Header("Ajustes de Puntuación")]
    [Tooltip("Puntos máximos que consigues si te pasas el nivel en 0 segundos.")]
    public int puntuacionBase = 3000;
    [Tooltip("Puntos que se restan por cada segundo que tardes.")]
    public int penalizacionPorSegundo = 30;

    // Variable donde guardaremos el resultado final
    public int PuntuacionFinal { get; private set; }

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

    // Esta función cogerá el tiempo del temporizador y hará la magia
    public void CalcularPuntuacionPorTiempo(float tiempoTranscurrido)
    {
        int segundosTotales = Mathf.FloorToInt(tiempoTranscurrido);

        // Calculamos los puntos: Base - (Segundos * Penalización)
        PuntuacionFinal = puntuacionBase - (segundosTotales * penalizacionPorSegundo);

        // Nos aseguramos de que la puntuación nunca sea negativa
        if (PuntuacionFinal < 0)
        {
            PuntuacionFinal = 0;
        }
    }
}