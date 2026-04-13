using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Tiempo")]
    public float tiempoTranscurrido = 0f;
    public bool nivelTerminado = false;

    [Header("Llaves")]
    public int llavesRecogidas = 0;
    public int llavesTotales = 7;

    [Header("Puntuacion")]
    public int puntuacionFinal = 0;

    [Header("UI Victoria")]
    public TMP_Text textoTiempo;
    public TMP_Text textoPuntuacion;
    public TMP_Text textoLlaves;

    [Header("Estrellas")]
    public GameObject estrella1;
    public GameObject estrella2;
    public GameObject estrella3;

    [Header("UI Derrota")]
    [Tooltip("Arrastra aquí el Canvas o Panel que contiene tu script DerrotaMenu")]
    public GameObject panelDerrota; // <--- NUEVA VARIABLE

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
        // Nos aseguramos de que las estrellas y la pantalla de derrota estén ocultas al empezar
        if (estrella1 != null) estrella1.SetActive(false);
        if (estrella2 != null) estrella2.SetActive(false);
        if (estrella3 != null) estrella3.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false); // <--- NUEVO
    }

    private void Update()
    {
        if (!nivelTerminado)
        {
            tiempoTranscurrido += Time.deltaTime;
        }
    }

    public void RecogerLlave()
    {
        llavesRecogidas++;
    }

    public void FinalizarNivel()
    {
        nivelTerminado = true;

        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60f);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60f);

        if (textoTiempo != null)
            textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");

        puntuacionFinal = CalcularPuntuacion();

        if (textoPuntuacion != null)
            textoPuntuacion.text = "Puntuación: " + puntuacionFinal;

        if (textoLlaves != null)
            textoLlaves.text = "Llaves recogidas: " + llavesRecogidas + "/" + llavesTotales;

        int estrellas = CalcularEstrellas();
        MostrarEstrellas(estrellas);
    }

    // =========================================================
    // NUEVA FUNCIÓN: Esto es lo que busca el VisionCone del enemigo
    // =========================================================
    public void FinalizarDerrota()
    {
        nivelTerminado = true; // Pausamos el contador de tiempo

        // Encendemos la pantalla de derrota
        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }

        // Pausamos el juego para que el enemigo no siga atacando
        Time.timeScale = 0f;

        // Desbloqueamos el ratón para que puedas hacer clic en "Reiniciar" o "Menú Principal"
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private int CalcularPuntuacion()
    {
        int puntosPorLlaves = llavesRecogidas * 100;
        int bonusTiempo = Mathf.Max(0, 3000 - Mathf.FloorToInt(tiempoTranscurrido * 20f));
        return puntosPorLlaves + bonusTiempo;
    }

    private int CalcularEstrellas()
    {
        if (tiempoTranscurrido <= 60f)
            return 3;
        else if (tiempoTranscurrido <= 90f)
            return 2;
        else
            return 1;
    }

    private void MostrarEstrellas(int cantidad)
    {
        if (estrella1 != null) estrella1.SetActive(cantidad >= 1);
        if (estrella2 != null) estrella2.SetActive(cantidad >= 2);
        if (estrella3 != null) estrella3.SetActive(cantidad >= 3);
    }
}