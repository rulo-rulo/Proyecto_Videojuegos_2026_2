using System.Collections;
using UnityEngine;

public class Meta : MonoBehaviour
{
    [Header("Textos e Interfaz")]
    [Tooltip("El texto del Canvas que dice 'Pulsa E'")]
    public GameObject mensajeUI;

    [Tooltip("El texto 3D flotante sobre la puerta")]
    public GameObject textoSobrePuerta;

    [Tooltip("Arrastra aquí tu Canvas o panel de Victoria")]
    public GameObject panelVictoria; // Antes se llamaba pantallaCargaUI

    [Header("Animación de la Puerta")]
    [Tooltip("El objeto que tiene el componente Animator")]
    public Animator animatorPuerta;

    [Tooltip("El nombre exacto del estado en el Animator")]
    public string nombreAnimacion = "AbrirPuerta";

    [Header("Cinemática del Jugador")]
    [Tooltip("Arrastra a tu jugador aquí")]
    public Transform jugador;

    [Tooltip("Objeto vacío detrás de la puerta donde caminará el jugador")]
    public Transform puntoDestino;

    [Tooltip("Velocidad a la que el jugador camina hacia la puerta")]
    public float velocidadJugador = 2.5f;

    [Header("Audio Victoria")]
    [Tooltip("Arrastra aquí el AudioSource que reproducirá la música/sonido de victoria")]
    public AudioSource audioVictoria;

    private bool jugadorCerca = false;
    private bool metaAlcanzada = false; // Evita que se active varias veces

    private void Awake()
    {
        // Preparamos el audio de victoria para que no suene de golpe al arrancar
        if (audioVictoria != null)
        {
            audioVictoria.playOnAwake = false;
            audioVictoria.loop = false;
            audioVictoria.Stop();
        }
    }

    void Start()
    {
        // Nos aseguramos de que toda la interfaz esté oculta al empezar el nivel
        if (mensajeUI != null) mensajeUI.SetActive(false);
        if (textoSobrePuerta != null) textoSobrePuerta.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    void Update()
    {
        // Si el jugador está cerca, pulsa E, y AÚN NO ha llegado a la meta
        if (jugadorCerca && !metaAlcanzada && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(RutinaMeta());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !metaAlcanzada)
        {
            jugadorCerca = true;
            if (mensajeUI != null) mensajeUI.SetActive(true);
            if (textoSobrePuerta != null) textoSobrePuerta.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !metaAlcanzada)
        {
            jugadorCerca = false;
            if (mensajeUI != null) mensajeUI.SetActive(false);
            if (textoSobrePuerta != null) textoSobrePuerta.SetActive(false);
        }
    }

    // Corrutina que controla la secuencia completa
    private IEnumerator RutinaMeta()
    {
        metaAlcanzada = true; // Bloqueamos la meta

        // 1. Ocultamos los textos de interacción
        if (mensajeUI != null) mensajeUI.SetActive(false);
        if (textoSobrePuerta != null) textoSobrePuerta.SetActive(false);

        // 2. Reproducimos la animación de abrir la puerta
        if (animatorPuerta != null)
        {
            animatorPuerta.Play(nombreAnimacion);
        }

        yield return new WaitForSeconds(0.5f);

        // Guardamos las referencias del jugador
        CharacterController cc = null;
        Rigidbody rb = null;
        PlayerMovimiento scriptMovimiento = null;

        if (jugador != null && puntoDestino != null)
        {
            // Apagamos los controles y las físicas para que el jugador sea un "actor" en la cinemática
            scriptMovimiento = jugador.GetComponent<PlayerMovimiento>();
            if (scriptMovimiento != null) scriptMovimiento.enabled = false;

            cc = jugador.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            rb = jugador.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Vector3 destinoPlano = new Vector3(puntoDestino.position.x, jugador.position.y, puntoDestino.position.z);
            float tiempoMaximo = 4f;
            float tiempoActual = 0f;

            // 3. Caminamos hacia adentro de la puerta...
            while (Vector3.Distance(jugador.position, destinoPlano) > 0.1f && tiempoActual < tiempoMaximo)
            {
                jugador.position = Vector3.MoveTowards(jugador.position, destinoPlano, velocidadJugador * Time.deltaTime);
                tiempoActual += Time.deltaTime;
                yield return null;
            }
        }

        Debug.Log("Cinemática de caminar terminada.");

        // Esperamos 1 segundo de cortesía tras haber entrado
        yield return new WaitForSeconds(1f);

        // =========================================================
        // 4. LÓGICA DE VICTORIA (Tras terminar de caminar)
        // =========================================================

        // Avisamos al GameManager para que calcule puntuaciones, tiempo, etc.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinalizarNivel();
        }

        // Encendemos el Canvas de Victoria
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        // Reproducimos el sonido de victoria (ignora la pausa por si acaso)
        if (audioVictoria != null)
        {
            audioVictoria.ignoreListenerPause = true;
            audioVictoria.Stop();
            audioVictoria.Play();
        }

        // Liberamos y mostramos el ratón para poder pulsar los botones del menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pausamos el juego
        Time.timeScale = 0f;

        Debug.Log("Menú de victoria cargado y juego pausado.");
    }
}