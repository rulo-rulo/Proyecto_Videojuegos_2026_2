using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias")]
    public Dash scriptDash; // Solo se llena para el DASH
    public Image imagenSombra;
    public TextMeshProUGUI textoContador;

    [Header("Ajustes")]
    public KeyCode teclaHabilidad = KeyCode.E;
    public float tiempoEnfriamiento = 5f;

    private float timer = 0f;
    private bool estaEnEnfriamiento = false;

    public bool EstaEnEnfriamiento => estaEnEnfriamiento;

    void Start()
    {
        if (imagenSombra != null) imagenSombra.fillAmount = 0;
    }

    void Update()
    {
        if (estaEnEnfriamiento)
        {
            ActualizarCooldown();
        }
        // Si hay un script de Dash asignado, este script maneja la tecla
        else if (scriptDash != null && Input.GetKeyDown(teclaHabilidad))
        {
            scriptDash.ExecuteDash();
            IniciarCooldown();
        }
    }

    public void IniciarCooldown()
    {
        estaEnEnfriamiento = true;
        timer = tiempoEnfriamiento;
    }

    void ActualizarCooldown()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            estaEnEnfriamiento = false;
            if (imagenSombra != null) imagenSombra.fillAmount = 0;
            if (textoContador != null) textoContador.text = "";
        }
        else
        {
            if (imagenSombra != null) imagenSombra.fillAmount = timer / tiempoEnfriamiento;
            if (textoContador != null) textoContador.text = Mathf.Ceil(timer).ToString();
        }
    }
}