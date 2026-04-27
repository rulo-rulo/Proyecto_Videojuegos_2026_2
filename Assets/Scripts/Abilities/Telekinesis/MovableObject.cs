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

        [Header("Sistema de Sonido (Ruido de Arrastre)")]
        [Tooltip("Velocidad mínima deslizando para que haga ruido.")]
        [SerializeField] private float umbralArrastre = 1f;
        [Tooltip("Cada cuántos segundos suelta una onda mientras raspa el suelo.")]
        [SerializeField] private float intervaloRuidoArrastre = 0.4f;
        [Tooltip("Multiplicador para el arrastre (suele ser más bajito que el golpe seco).")]
        [SerializeField] private float multiplicadorRuidoArrastre = 0.8f;

        [Header("Fricción por Script")]
        [SerializeField] private float fuerzaFrenado = 5f;
        [SerializeField] private float umbralParadaTotal = 0.1f;

        private bool tocandoSuperficie = false;
        private float timerArrastre = 0f; // Cronómetro para las ondas de arrastre

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ApplyForce(Vector3 direction, float force)
        {
            rb.isKinematic = false;
            tocandoSuperficie = false;
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
            Debug.Log($"[Telekinesis] Fuerza aplicada a {gameObject.name} | Dirección: {direction}");
        }

        // -------------------------------------------------- Freno por Script

        private void FixedUpdate()
        {
            if (tocandoSuperficie && !rb.isKinematic)
            {
                Vector3 velocidadHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                velocidadHorizontal = Vector3.Lerp(velocidadHorizontal, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);

                rb.linearVelocity = new Vector3(velocidadHorizontal.x, rb.linearVelocity.y, velocidadHorizontal.z);

                if (velocidadHorizontal.magnitude < umbralParadaTotal)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        // -------------------------------------------------- Físicas de Impacto, Suelo y Arrastre

        private void OnCollisionEnter(Collision collision)
        {
            tocandoSuperficie = true;

            float velocidadImpacto = collision.relativeVelocity.magnitude;

            if (velocidadImpacto >= umbralImpacto)
            {
                Vector3 puntoDeImpacto = collision.contacts[0].point;
                // Onda grande por el golpe inicial (usamos el multiplicador normal)
                GenerarOndaDeSonido(velocidadImpacto, puntoDeImpacto, multiplicadorRuido);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            tocandoSuperficie = true;

            // Medimos la velocidad a la que se está deslizando por el suelo
            float velocidadActual = rb.linearVelocity.magnitude;

            if (velocidadActual >= umbralArrastre)
            {
                // Sumamos tiempo al cronómetro
                timerArrastre += Time.deltaTime;

                if (timerArrastre >= intervaloRuidoArrastre)
                {
                    timerArrastre = 0f; // Reseteamos cronómetro
                    Vector3 puntoDeFriccion = collision.contacts[0].point;

                    // Onda pequeña por el arrastre (usamos el multiplicador de arrastre)
                    GenerarOndaDeSonido(velocidadActual, puntoDeFriccion, multiplicadorRuidoArrastre);
                }
            }
            else
            {
                // Si va muy lento, detenemos el cronómetro
                timerArrastre = 0f;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            tocandoSuperficie = false;
            timerArrastre = 0f; // Si salta por los aires, cancelamos el arrastre
        }

        // -------------------------------------------------- Generador de Onda

        // Le he añadido el parámetro 'multiplicador' para diferenciar entre golpe y arrastre
        private void GenerarOndaDeSonido(float velocidad, Vector3 origen, float multiplicador)
        {
            float fuerzaGolpe = rb.mass * velocidad;
            float radioCalculado = fuerzaGolpe * multiplicador;
            float radioFinal = Mathf.Clamp(radioCalculado, 0f, radioMaximoSonido);

            // --- 1. ALERTA VISUAL ---
            GameObject ondaVisual = new GameObject("OndaSonidoVisual");
            ondaVisual.transform.position = new Vector3(origen.x, origen.y + 0.1f, origen.z);

            EfectoOnda efecto = ondaVisual.AddComponent<EfectoOnda>();
            Color grisOscuro = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            efecto.Iniciar(radioFinal, grisOscuro, 1.5f);

            // --- 2. ALERTA LÓGICA ---
            MovimientoRutaPatrullero[] enemigos = FindObjectsByType<MovimientoRutaPatrullero>(FindObjectsSortMode.None);
            foreach (MovimientoRutaPatrullero enemigo in enemigos)
            {
                float distanciaAlRuido = Vector3.Distance(origen, enemigo.transform.position);
                if (distanciaAlRuido <= radioFinal)
                {
                    enemigo.ReportarInteraccion(origen);
                }
            }
        }
    }
}