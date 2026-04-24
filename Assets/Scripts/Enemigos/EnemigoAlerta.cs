using UnityEngine;

public class EnemigoAlerta : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 3f;
    public float distanciaMinima = 0.2f;

    [Header("Estado")]
    public bool enBusqueda = false;
    public Vector3 ultimoPuntoDetectado;

    void OnEnable()
    {
        AlertaGlobal.OnAlertaGlobal += RecibirAlerta;
    }

    void OnDisable()
    {
        AlertaGlobal.OnAlertaGlobal -= RecibirAlerta;
    }

    void Update()
    {
        if (enBusqueda)
        {
            Vector3 destinoPlano = new Vector3(
                ultimoPuntoDetectado.x,
                transform.position.y,
                ultimoPuntoDetectado.z
            );

            transform.position = Vector3.MoveTowards(
                transform.position,
                destinoPlano,
                velocidad * Time.deltaTime
            );

            Vector3 direccion = destinoPlano - transform.position;
            direccion.y = 0f;

            if (direccion.sqrMagnitude > 0.001f)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rotacionObjetivo,
                    5f * Time.deltaTime
                );
            }

            if (Vector3.Distance(transform.position, destinoPlano) <= distanciaMinima)
            {
                enBusqueda = false;
                Debug.Log(gameObject.name + " llegó al último punto de detección.");
            }
        }
    }

    void RecibirAlerta(Vector3 punto)
    {
        ultimoPuntoDetectado = punto;
        enBusqueda = true;

        Debug.Log(gameObject.name + " recibió alerta y cambia a estado de búsqueda.");
    }
}