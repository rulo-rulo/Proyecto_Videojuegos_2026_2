using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias")]
    public Dash scriptDash; // Referencia corregida a la clase 'Dash'
    public Image imagenSombra;
    public TextMeshProUGUI textoContador;

    [Header("Ajustes de Input")]
    public KeyCode teclaTeclado = KeyCode.LeftShift;
    public string ejeGatilloMando = "GatilloIzquierdo";

    [Header("Ajustes de Tiempo")]
    public float tiempoEnfriamiento = 5f;

    [Header("Colores de Icono")]
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
            if (DetectarInput())
            {
                // Si este cooldown es para el DASH
                if (scriptDash != null)
                {
                    scriptDash.ExecuteDash(); // Ejecutamos la acción
                    IniciarCooldown();        // Iniciamos el enfriamiento
                }
            }
        }
    }

    private bool DetectarInput()
    {
        // Teclado
        if (Input.GetKeyDown(teclaTeclado)) return true;

        // Mando (L2/LT)
        float valorGatillo = Input.GetAxisRaw(ejeGatilloMando);
        if (valorGatillo > 0.5f && !gatilloYaPulsado)
        {
            gatilloYaPulsado = true;
            return true;
        }
        else if (valorGatillo <= 0.1f)
        {
            gatilloYaPulsado = false;
        }

        return false;
    }

    public void IniciarCooldown()
    {
        estaEnEnfriamiento = true;
        timer = tiempoEnfriamiento;
        if (imagenBase != null) imagenBase.color = colorActivo;
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
            if (imagenBase != null) imagenBase.color = colorNormal;
        }
        else
        {
            if (imagenSombra != null) imagenSombra.fillAmount = timer / tiempoEnfriamiento;
            if (textoContador != null) textoContador.text = Mathf.Ceil(timer).ToString();
        }
    }

    // Añade esto dentro de la clase HabilidadCooldown
    public void EstablecerUsoActivo(bool enUso)
    {
        // Si la habilidad no está en enfriamiento, cambiamos el color
        // para indicar que está seleccionada o en uso.
        if (!estaEnEnfriamiento && imagenBase != null)
        {
            imagenBase.color = enUso ? colorActivo : colorNormal;
        }
    }
}