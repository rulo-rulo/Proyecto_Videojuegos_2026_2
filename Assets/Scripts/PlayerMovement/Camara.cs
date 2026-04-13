using UnityEngine;

public class Camara : MonoBehaviour
{
    public Transform Player;
    public float Suavizado = 5f;

    private Vector3    offset;
    private Transform  currentTarget;

    void Start()
    {
        offset        = transform.position - Player.position;
        currentTarget = Player;
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 posicionObjetivo = currentTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Suavizado * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget != null ? newTarget : Player;
    }
}