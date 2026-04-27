using System.Collections;
using UnityEngine;

namespace Telekinesis
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovableObject : MonoBehaviour
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ApplyForce(Vector3 direction, float force)
        {
            rb.isKinematic = false;
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);

            StartCoroutine(NotificarCuandoCaiga());

            Debug.Log($"[Telekinesis] Fuerza aplicada a {gameObject.name} | Dirección: {direction}");
        }

        private IEnumerator NotificarCuandoCaiga()
        {
            yield return new WaitForSeconds(0.7f);

            MovimientoRutaPatrullero[] enemigos =
                FindObjectsByType<MovimientoRutaPatrullero>(FindObjectsSortMode.None);

            foreach (MovimientoRutaPatrullero enemigo in enemigos)
            {
                enemigo.ReportarInteraccion(transform);
            }
        }
    }
}