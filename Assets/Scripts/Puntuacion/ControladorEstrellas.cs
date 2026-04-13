using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ¡NUEVO! Necesario para saber en qué nivel estamos

public class ControladorEstrellas : MonoBehaviour
{
    [Header("Imágenes de las Estrellas Doradas")]
    [SerializeField] private Image imagenEstrella1;
    [SerializeField] private Image imagenEstrella2;
    [SerializeField] private Image imagenEstrella3;

    [Header("Ajustes de Color")]
    [SerializeField] private Color colorEncendida = Color.white;
    [SerializeField] private Color colorApagada = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Requisitos de Puntuación")]
    [SerializeField] private int puntosPara1Estrella = 1000;
    [SerializeField] private int puntosPara2Estrellas = 1500;
    [SerializeField] private int puntosPara3Estrellas = 2000;

    private void OnEnable()
    {
        // 1. Apagamos todas las estrellas por defecto
        EstablecerColorEstrella(imagenEstrella1, colorApagada);
        EstablecerColorEstrella(imagenEstrella2, colorApagada);
        EstablecerColorEstrella(imagenEstrella3, colorApagada);

        if (GestorPuntuacion.Instance != null)
        {
            int puntos = GestorPuntuacion.Instance.PuntuacionFinal;
            int estrellasConseguidasEnEstaPartida = 0;

            // 2. Encendemos visualmente y contamos cuántas hemos ganado
            if (puntos >= puntosPara1Estrella)
            {
                EstablecerColorEstrella(imagenEstrella1, colorEncendida);
                estrellasConseguidasEnEstaPartida = 1;
            }
            if (puntos >= puntosPara2Estrellas)
            {
                EstablecerColorEstrella(imagenEstrella2, colorEncendida);
                estrellasConseguidasEnEstaPartida = 2;
            }
            if (puntos >= puntosPara3Estrellas)
            {
                EstablecerColorEstrella(imagenEstrella3, colorEncendida);
                estrellasConseguidasEnEstaPartida = 3;
            }

            // 3. Guardamos el progreso de forma global
            GuardarProgreso(estrellasConseguidasEnEstaPartida);
        }
    }

    private void EstablecerColorEstrella(Image estrella, Color color)
    {
        if (estrella != null)
        {
            estrella.color = color;
        }
    }

    // --- SISTEMA DE GUARDADO GLOBAL ---
    private void GuardarProgreso(int nuevasEstrellas)
    {
        // Cogemos el nombre de la escena actual (Ej: "Nivel_1")
        string nombreNivelActual = SceneManager.GetActiveScene().name;

        // Miramos cuántas estrellas teníamos guardadas en este nivel (0 si es la primera vez)
        int estrellasAnteriores = PlayerPrefs.GetInt(nombreNivelActual + "_Estrellas", 0);

        // Si hemos superado nuestro récord en este nivel...
        if (nuevasEstrellas > estrellasAnteriores)
        {
            // 1. Guardamos el nuevo récord del nivel
            PlayerPrefs.SetInt(nombreNivelActual + "_Estrellas", nuevasEstrellas);

            // 2. Calculamos la diferencia (Ej: Si teníamos 1 y ahora tenemos 3, la diferencia es 2)
            int diferencia = nuevasEstrellas - estrellasAnteriores;

            // 3. Sumamos esa diferencia al gran total global
            int totalGlobal = PlayerPrefs.GetInt("TotalEstrellas_Global", 0);
            PlayerPrefs.SetInt("TotalEstrellas_Global", totalGlobal + diferencia);

            // 4. Forzamos el guardado en el disco duro
            PlayerPrefs.Save();

            Debug.Log($"¡Nuevo récord! Has sumado {diferencia} estrella/s. Tu total global ahora es: {PlayerPrefs.GetInt("TotalEstrellas_Global")}");
        }
    }
}