using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias (Sombra de Recarga)")]
    public Dash scriptDash;
    public Image imagenSombra;
    public TextMeshProUGUI textoContador;

    [Header("Ajustes de Input")]
    // 1. Aquí definimos el Left Shift
    public KeyCode teclaTeclado = KeyCode.LeftShift;

    // 2. Aquí definimos el nombre del Eje para el L2 / LT
    [Tooltip("Debe llamarse igual que en el Input Manager de Unity")]
    public string ejeGatilloMando = "GatilloIzquierdo";

    [Header("Ajustes de Tiempo")]
    public float tiempoEnfriamiento = 5f;

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorActivo = new Color(0.4f, 0.4f, 0.4f, 1f);

    private Image imagenBase;
    private float timer = 0f;
    private bool estaEnEnfriamiento = false;
    private bool gatilloYaPulsado = false;

    public bool EstaEnEnfriamiento => estaEnEnfriamiento;

    void Awake()
    {
        imagenBase = GetComponent<Image>();
    }

    void Update()
    {
        if (estaEnEnfriamiento)
        {
            ActualizarCooldown();
        }
        else
        {
            // DETECCIÓN DE LEFT SHIFT
            bool pulsoTeclado = Input.GetKeyDown(teclaTeclado);

            // DETECCIÓN DE L2 / LT (Gatillo)
            bool pulsoMando = false;
            float valorGatillo = Input.GetAxisRaw(ejeGatilloMando);

            if (valorGatillo > 0.5f && !gatilloYaPulsado)
            {
                pulsoMando = true;
                gatilloYaPulsado = true;
            }
            else if (valorGatillo <= 0.1f)
            {
                gatilloYaPulsado = false;
            }

            // SI PULSAS CUALQUIERA DE LOS DOS, HACE EL DASH
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
        EstablecerUsoActivo(false);
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

    public void EstablecerUsoActivo(bool enUso)
    {
        if (imagenBase != null) imagenBase.color = enUso ? colorActivo : colorNormal;
    }
}