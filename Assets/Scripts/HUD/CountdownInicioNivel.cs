using System.Collections;
using UnityEngine;
using TMPro;
using Possession;
using Telekinesis;

public class CountdownInicioNivel : MonoBehaviour
{
    [Header("UI")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    [Header("Duracion")]
    public float tiempoEntreNumeros = 1f;
    public float tiempoTextoFinal = 0.5f;

    [Header("Jugador")]
    public MonoBehaviour movimientoJugador;

    [Header("Cooldowns de habilidades")]
    public HabilidadCooldown cooldownDash;
    public HabilidadCooldown cooldownPosesion;
    public HabilidadCooldown cooldownTelequinesis;

    [Header("Managers de habilidades")]
    public MonoBehaviour abilityManager;
    public MonoBehaviour possessionManager;
    public MonoBehaviour telekinesisManager;

    [Header("Inputs de habilidades")]
    public InputHandler possessionInputHandler;
    public TelekinesisInputHandler telekinesisInputHandler;

    [Header("Enemigos")]
    public MonoBehaviour[] movimientosEnemigos;

    private void Start()
    {
        StartCoroutine(IniciarCuentaAtras());
    }

    private IEnumerator IniciarCuentaAtras()
    {
        // Desactivar movimiento del jugador
        if (movimientoJugador != null)
            movimientoJugador.enabled = false;

        // Desactivar cooldowns de habilidades
        if (cooldownDash != null)
            cooldownDash.enabled = false;

        if (cooldownPosesion != null)
            cooldownPosesion.enabled = false;

        if (cooldownTelequinesis != null)
            cooldownTelequinesis.enabled = false;

        // Desactivar managers de habilidades
        if (abilityManager != null)
            abilityManager.enabled = false;

        if (possessionManager != null)
            possessionManager.enabled = false;

        if (telekinesisManager != null)
            telekinesisManager.enabled = false;

        // Desactivar inputs de habilidades
        if (possessionInputHandler != null)
            possessionInputHandler.enabled = false;

        if (telekinesisInputHandler != null)
            telekinesisInputHandler.enabled = false;

        // Desactivar enemigos
        if (movimientosEnemigos != null)
        {
            foreach (MonoBehaviour enemigo in movimientosEnemigos)
            {
                if (enemigo != null)
                    enemigo.enabled = false;
            }
        }

        // Esperar a que exista el temporizador
        while (TemporizadorGlobal.Instance == null)
            yield return null;

        // Parar y reiniciar temporizador
        TemporizadorGlobal.Instance.PararTemporizador();
        TemporizadorGlobal.Instance.ReiniciarTemporizador();

        // Mostrar countdown
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        if (countdownText != null)
            countdownText.text = "3";
        yield return new WaitForSeconds(tiempoEntreNumeros);

        if (countdownText != null)
            countdownText.text = "2";
        yield return new WaitForSeconds(tiempoEntreNumeros);

        if (countdownText != null)
            countdownText.text = "1";
        yield return new WaitForSeconds(tiempoEntreNumeros);

        if (countdownText != null)
            countdownText.text = "¡EMPIEZA!";
        yield return new WaitForSeconds(tiempoTextoFinal);

        // Ocultar panel
        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        // Activar movimiento del jugador
        if (movimientoJugador != null)
            movimientoJugador.enabled = true;

        // Activar cooldowns de habilidades
        if (cooldownDash != null)
            cooldownDash.enabled = true;

        if (cooldownPosesion != null)
            cooldownPosesion.enabled = true;

        if (cooldownTelequinesis != null)
            cooldownTelequinesis.enabled = true;

        // Activar managers de habilidades
        if (abilityManager != null)
            abilityManager.enabled = true;

        if (possessionManager != null)
            possessionManager.enabled = true;

        if (telekinesisManager != null)
            telekinesisManager.enabled = true;

        // Activar inputs de habilidades
        if (possessionInputHandler != null)
            possessionInputHandler.enabled = true;

        if (telekinesisInputHandler != null)
            telekinesisInputHandler.enabled = true;

        // Activar enemigos
        if (movimientosEnemigos != null)
        {
            foreach (MonoBehaviour enemigo in movimientosEnemigos)
            {
                if (enemigo != null)
                    enemigo.enabled = true;
            }
        }

        // Iniciar temporizador
        TemporizadorGlobal.Instance.IniciarTemporizador();
    }
}