using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias")]
    public Dash scriptDash;
    public Image imagenSombra;
    public TextMeshProUGUI textoContador;

    [Header("Ajustes de Input")]
    public KeyCode teclaTeclado = KeyCode.LeftShift;
    [Tooltip("El nombre que le pusiste en el Input Manager")]
    public string ejeGatilloMando = "GatilloIzquierdo";

    [Header("Ajustes de Tiempo")]
    public float tiempoEnfriamiento = 5f;

    private float timer = 0f;
    private bool estaEnEnfriamiento = false;

    // Esta variable evita que el gatillo se dispare varias veces seguidas si lo mantienes apretado
    private bool gatilloYaPulsado = false;

    public bool EstaEnEnfriamiento => estaEnEnfriamiento;

    void Start()
    {
        if (imagenSombra != null) imagenSombra.fillAmount = 0;
        if (textoContador != null) textoContador.text = "";
    }

    void Update()
    {
        if (estaEnEnfriamiento)
        {
            ActualizarCooldown();
        }
        else
        {
            // 1. Comprobamos el TECLADO (Funciona igual)
            bool pulsoTeclado = Input.GetKeyDown(teclaTeclado);

            // 2. Comprobamos el MANDO (Gatillo)
            bool pulsoMando = false;
            float valorGatillo = Input.GetAxisRaw(ejeGatilloMando);

            // Si apretamos el gatillo a más de la mitad y NO estaba ya apretado...
            if (valorGatillo > 0.5f && !gatilloYaPulsado)
            {
                pulsoMando = true;
                gatilloYaPulsado = true; // Bloqueamos para que no se dispare en bucle
            }
            // Si soltamos el gatillo (vuelve casi a 0), reseteamos el seguro
            else if (valorGatillo <= 0.1f)
            {
                gatilloYaPulsado = false;
            }

            // 3. Ejecutamos si se pulsó cualquiera de los dos
            if (pulsoTeclado || pulsoMando)
            {
                if (scriptDash != null && scriptDash.ExecuteDash())
                {
                    IniciarCooldown();
                }
            }
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
            if (imagenSombra != null)
                imagenSombra.fillAmount = timer / tiempoEnfriamiento;

            if (textoContador != null)
                textoContador.text = Mathf.Ceil(timer).ToString();
        }
    }
}