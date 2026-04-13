using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias")]
    public Dash scriptDash;      // Arrastra al Jugador aquí
    public Image imagenSombra;      // La imagen con Fill Method: Radial 360
    public TextMeshProUGUI textoContador;

    [Header("Ajustes")]
    public KeyCode teclaHabilidad = KeyCode.E;
    public float tiempoEnfriamiento = 5f;

    private float timer = 0f;
    private bool estaEnEnfriamiento = false;

    void Update()
    {
        // El Cooldown funciona aunque el juego esté pausado si no usas Time.timeScale
        if (estaEnEnfriamiento)
        {
            ActualizarCooldown();
        }
        else if (Input.GetKeyDown(teclaHabilidad))
        {
            UsarHabilidad();
        }
    }

    void UsarHabilidad()
    {
        // 1. Ejecutar Dash
        if (scriptDash != null) scriptDash.ExecuteDash();

        // 2. Bloquear y empezar timer inmediatamente
        estaEnEnfriamiento = true;
        timer = tiempoEnfriamiento;
    }

    void ActualizarCooldown()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // Reset al llegar a 0 exacto
            timer = 0f;
            estaEnEnfriamiento = false;
            if (imagenSombra != null) imagenSombra.fillAmount = 0;
            if (textoContador != null) textoContador.text = "";
        }
        else
        {
            // Actualizar visuales (proporción 1 a 0)
            if (imagenSombra != null) imagenSombra.fillAmount = timer / tiempoEnfriamiento;
            if (textoContador != null) textoContador.text = Mathf.Ceil(timer).ToString();
        }
    }
}