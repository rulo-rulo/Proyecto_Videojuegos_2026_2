using UnityEngine;

public class TemporizadorGlobal : MonoBehaviour
{
    // Singleton para acceder desde cualquier script (Ej: TemporizadorGlobal.Instance.tiempo)
    public static TemporizadorGlobal Instance;

    [Header("Estado")]
    public float tiempoTranscurrido = 0f;
    private bool estaContando = false;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            // Opcional: Descomenta la siguiente línea si quieres que el tiempo persista entre escenas
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // El tiempo empieza a contar en cuanto arranca el nivel
        IniciarTemporizador();
    }

    private void Update()
    {
        if (estaContando)
        {
            // Sumamos el tiempo real que pasa entre frames
            tiempoTranscurrido += Time.deltaTime;
        }
    }

    // --- MÉTODOS DE CONTROL ---

    public void IniciarTemporizador()
    {
        estaContando = true;
    }

    public void PararTemporizador()
    {
        estaContando = false;
    }

    public void ReiniciarTemporizador()
    {
        tiempoTranscurrido = 0f;
    }

    // Función útil para obtener el tiempo en formato bonito (Tiempo: 00:00)
    public string ObtenerTiempoFormateado()
    {
        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60);

        // Añadimos "Tiempo: " justo antes del formato de los números
        return $"Tiempo: {minutos:00}:{segundos:00}";
    }
}