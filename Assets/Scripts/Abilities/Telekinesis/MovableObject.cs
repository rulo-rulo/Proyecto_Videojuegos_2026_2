using UnityEngine;
using Possession;

namespace Telekinesis
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovableObject : MonoBehaviour
    {
        private Rigidbody rb;

        [Header("Peso")]
        [SerializeField] private WeightClass weightClass = WeightClass.Medium;

        [Header("Multiplicadores por peso")]
        [SerializeField] private float fuerzaLigero = 1.5f;
        [SerializeField] private float fuerzaMedio   = 1.0f;
        [SerializeField] private float fuerzaPesado  = 0.5f;

        [SerializeField] private float ruidoLigero = 0.6f;
        [SerializeField] private float ruidoMedio   = 1.0f;
        [SerializeField] private float ruidoPesado  = 1.8f;

        [Header("Sistema de Sonido (Ruido de Impacto)")]
        [SerializeField] private float umbralImpacto      = 2f;
        [SerializeField] private float multiplicadorRuido = 1.5f;
        [SerializeField] private float radioMaximoSonido  = 30f;

        [Header("Sistema de Sonido (Ruido de Arrastre)")]
        [SerializeField] private float umbralArrastre             = 1f;
        [SerializeField] private float intervaloRuidoArrastre     = 0.4f;
        [SerializeField] private float multiplicadorRuidoArrastre = 0.8f;

        [Header("Friccion por Script")]
        [SerializeField] private float fuerzaFrenado    = 5f;
        [SerializeField] private float umbralParadaTotal = 0.1f;

        private bool  tocandoSuperficie = false;
        private float timerArrastre     = 0f;

        public WeightClass WeightClass => weightClass;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ApplyForce(Vector3 direction, float force)
        {
            rb.isKinematic    = false;
            tocandoSuperficie = false;

            float multiplicador = weightClass switch
            {
                WeightClass.Light  => fuerzaLigero,
                WeightClass.Medium => fuerzaMedio,
                WeightClass.Heavy  => fuerzaPesado,
                _                  => fuerzaMedio
            };

            float fuerzaFinal = force * multiplicador;
            rb.AddForce(direction.normalized * fuerzaFinal, ForceMode.Impulse);
            Debug.Log($"[Telekinesis] Fuerza aplicada a {gameObject.name} | Peso: {weightClass} | Fuerza: {fuerzaFinal}");
        }

        private float GetMultiplicadorRuido()
        {
            return weightClass switch
            {
                WeightClass.Light  => ruidoLigero,
                WeightClass.Medium => ruidoMedio,
                WeightClass.Heavy  => ruidoPesado,
                _                  => ruidoMedio
            };
        }

        private void FixedUpdate()
        {
            if (tocandoSuperficie && !rb.isKinematic)
            {
                Vector3 velocidadHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                velocidadHorizontal    = Vector3.Lerp(velocidadHorizontal, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);
                rb.angularVelocity     = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * fuerzaFrenado);
                rb.linearVelocity      = new Vector3(velocidadHorizontal.x, rb.linearVelocity.y, velocidadHorizontal.z);

                if (velocidadHorizontal.magnitude < umbralParadaTotal)
                {
                    rb.linearVelocity  = new Vector3(0, rb.linearVelocity.y, 0);
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            tocandoSuperficie = true;

            float velocidadImpacto = collision.relativeVelocity.magnitude;

            if (velocidadImpacto >= umbralImpacto)
            {
                Vector3 puntoDeImpacto = collision.contacts[0].point;
                GenerarOndaDeSonido(velocidadImpacto, puntoDeImpacto, multiplicadorRuido * GetMultiplicadorRuido());
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            tocandoSuperficie = true;

            float velocidadActual = rb.linearVelocity.magnitude;

            if (velocidadActual >= umbralArrastre)
            {
                timerArrastre += Time.deltaTime;

                if (timerArrastre >= intervaloRuidoArrastre)
                {
                    timerArrastre = 0f;
                    Vector3 puntoDeFriccion = collision.contacts[0].point;
                    GenerarOndaDeSonido(velocidadActual, puntoDeFriccion, multiplicadorRuidoArrastre * GetMultiplicadorRuido());
                }
            }
            else
            {
                timerArrastre = 0f;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            tocandoSuperficie = false;
            timerArrastre     = 0f;
        }

        private void GenerarOndaDeSonido(float velocidad, Vector3 origen, float multiplicador)
        {
            float fuerzaGolpe    = rb.mass * velocidad;
            float radioCalculado = fuerzaGolpe * multiplicador;
            float radioFinal     = Mathf.Clamp(radioCalculado, 0f, radioMaximoSonido);

            GameObject ondaVisual = new GameObject("OndaSonidoVisual");
            ondaVisual.transform.position = new Vector3(origen.x, origen.y + 0.1f, origen.z);

            EfectoOnda efecto = ondaVisual.AddComponent<EfectoOnda>();
            Color grisOscuro  = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            efecto.Iniciar(radioFinal, grisOscuro, 1.5f);

            MovimientoRutaPatrullero[] enemigos = FindObjectsByType<MovimientoRutaPatrullero>(FindObjectsSortMode.None);
            foreach (MovimientoRutaPatrullero enemigo in enemigos)
            {
                float distanciaAlRuido = Vector3.Distance(origen, enemigo.transform.position);
                if (distanciaAlRuido <= radioFinal)
                    enemigo.ReportarInteraccion(origen);
            }
        }
    }
}