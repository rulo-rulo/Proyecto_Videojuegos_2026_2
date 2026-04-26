using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadCooldown : MonoBehaviour
{
    [Header("Referencias (Sombra de Recarga)")]
    public Dash scriptDash;
    public Image imagenSombra;
    public TextMeshProUGUI textoContador;

    [Header("Ajustes de Teclas")]
    public KeyCode teclaTeclado = KeyCode.LeftShift;
    public KeyCode teclaMando = KeyCode.JoystickButton4;

    [Header("Ajustes de Tiempo")]
    public float tiempoEnfriamiento = 5f;

    [Header("Colores cuando la habilidad se está usando")]
    public Color colorNormal = Color.white;
    public Color colorActivo = new Color(0.4f, 0.4f, 0.4f, 1f);

    private Image imagenBase;
    private float timer = 0f;
    private bool estaEnEnfriamiento = false;

    public bool EstaEnEnfriamiento => estaEnEnfriamiento;

    void Awake()
    {
        imagenBase = GetComponent<Image>();
    }

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
            if (Input.GetKeyDown(teclaTeclado) || Input.GetKeyDown(teclaMando))
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
            if (imagenSombra != null) imagenSombra.fillAmount = timer / tiempoEnfriamiento;
            if (textoContador != null) textoContador.text = Mathf.Ceil(timer).ToString();
        }
    }

    public void EstablecerUsoActivo(bool enUso)
    {
        if (imagenBase != null)
        {
            imagenBase.color = enUso ? colorActivo : colorNormal;
        }
    }
}