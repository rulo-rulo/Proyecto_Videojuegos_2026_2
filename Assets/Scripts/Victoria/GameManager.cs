using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Llaves")]
    public int llavesRecogidas = 0;
    public int llavesTotales = 5;

    [Header("UI Victoria")]
    public GameObject panelVictoria;
    public TMP_Text textoLlavesVictoria;

    [Header("UI Derrota")]
    public GameObject panelDerrota;
    public TMP_Text textoLlavesDerrota;

    [Header("Pause")]
    public GameObject pauseButton;

    private bool nivelTerminado = false;

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
        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        ActualizarTextoLlaves();
    }

    public void RecogerLlave()
    {
        llavesRecogidas++;

        if (llavesRecogidas > llavesTotales)
            llavesRecogidas = llavesTotales;

        ActualizarTextoLlaves();
    }

    public void FinalizarNivel()
    {
        if (nivelTerminado) return;
        nivelTerminado = true;

        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        ActualizarTextoLlaves();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void FinalizarDerrota()
    {
        if (nivelTerminado) return;
        nivelTerminado = true;

        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (panelDerrota != null)
            panelDerrota.SetActive(true);

        ActualizarTextoLlaves();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ActualizarTextoLlaves()
    {
        string texto = "Llaves: " + llavesRecogidas + "/" + llavesTotales;

        if (textoLlavesVictoria != null)
            textoLlavesVictoria.text = texto;

        if (textoLlavesDerrota != null)
            textoLlavesDerrota.text = texto;
    }
}