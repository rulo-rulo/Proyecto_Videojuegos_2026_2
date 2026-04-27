using UnityEngine;

namespace Telekinesis
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovableObject : MonoBehaviour
    {
        private Rigidbody rb;

        [Header("Sistema de Sonido (Ruido de Impacto)")]
        [SerializeField] private float umbralImpacto = 2f;
        [SerializeField] private float multiplicadorRuido = 1.5f;
        [SerializeField] private float radioMaximoSonido = 30f;

        [Header("Fricción por Script")]
        [Tooltip("Qué tan rápido frena el objeto al tocar el suelo.")]
        [SerializeField] private float fuerzaFrenado = 2f;
        [Tooltip("Si la velocidad baja de este número, el objeto se detiene por completo.")]
        [SerializeField] private float umbralParadaTotal = 0.1f;

        // Variable para saber si estamos tocando el suelo
        private bool tocandoSuperficie = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ApplyForce(Vector3 direction, float force)
        {
            rb.isKinematic = false;
            tocandoSuperficie = false; // Al lanzarlo, vuela, no toca el suelo
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
            Debug.Log($"[Telekinesis] Fuerza aplicada a {gameObject.name} | Dirección: {direction}");
        }

        // -------------------------------------------------- Freno por Script

        private void FixedUpdate()
        {
            // Solo aplicamos fricción si está tocando algo y no está siendo agarrado
            if (tocandoSuperficie && !rb.isKinematic)
            {
                // Extraemos solo la velocidad horizontal (X, Z). Dejamos la Y en paz para que caiga bien.
                Vector3 velocidadHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                // 1. Frenado progresivo (deslizamiento controlado)
                velocidadHorizontal = Vector3.Lerp(velocidadHorizontal, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);

                // 2. Freno progresivo de la rotación (para que deje de rodar)
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);

                // Reaplicamos la velocidad modificada
                rb.linearVelocity = new Vector3(velocidadHorizontal.x, rb.linearVelocity.y, velocidadHorizontal.z);

                // 3. Parada en seco (cuando ya casi no se mueve, lo forzamos a 0)
                if (velocidadHorizontal.magnitude < umbralParadaTotal)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        // -------------------------------------------------- Físicas de Impacto y Suelo

        private void OnCollisionEnter(Collision collision)
        {
            tocandoSuperficie = true; // Acabamos de tocar el suelo/pared

            float velocidadImpacto = collision.relativeVelocity.magnitude;

            if (velocidadImpacto >= umbralImpacto)
            {
                Vector3 puntoDeImpacto = collision.contacts[0].point;
                GenerarOndaDeSonido(velocidadImpacto, puntoDeImpacto);
            }
        }

        // Mientras siga tocando, mantenemos la variable en true
        private void OnCollisionStay(Collision collision)
        {
            tocandoSuperficie = true;
        }

        // Si rebota o lo lanzamos, ya no toca la superficie
        private void OnCollisionExit(Collision collision)
        {
            tocandoSuperficie = false;
        }

        private void GenerarOndaDeSonido(float velocidad, Vector3 puntoDeImpacto)
        {
            float fuerzaGolpe = rb.mass * velocidad;
            float radioCalculado = fuerzaGolpe * multiplicadorRuido;
            float radioFinal = Mathf.Clamp(radioCalculado, 0f, radioMaximoSonido);

            // --- 1. ALERTA VISUAL ---
            GameObject ondaVisual = new GameObject("OndaSonidoVisual");
            ondaVisual.transform.position = new Vector3(puntoDeImpacto.x, puntoDeImpacto.y + 0.1f, puntoDeImpacto.z);

            EfectoOnda efecto = ondaVisual.AddComponent<EfectoOnda>();
            Color grisOscuro = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            efecto.Iniciar(radioFinal, grisOscuro, 1.5f);

            // --- 2. ALERTA LÓGICA ---
            MovimientoRutaPatrullero[] enemigos = FindObjectsByType<MovimientoRutaPatrullero>(FindObjectsSortMode.None);
            foreach (MovimientoRutaPatrullero enemigo in enemigos)
            {
                float distanciaAlRuido = Vector3.Distance(puntoDeImpacto, enemigo.transform.position);
                if (distanciaAlRuido <= radioFinal)
                {
                    enemigo.ReportarInteraccion(puntoDeImpacto);
                }
            }
        }
    }
}