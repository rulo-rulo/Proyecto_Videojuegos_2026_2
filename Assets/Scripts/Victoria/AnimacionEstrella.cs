using UnityEngine;
using System.Collections;

public class AnimacionEstrella : MonoBehaviour
{
    public float duracion = 0.25f;
    public float escalaInicial = 0.2f;
    public float escalaMaxima = 1.2f;
    public float retraso = 0f;

    private Vector3 escalaFinal;

    private void Awake()
    {
        escalaFinal = Vector3.one;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        yield return new WaitForSecondsRealtime(retraso);

        float tiempo = 0f;
        transform.localScale = Vector3.one * escalaInicial;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;

            float escala = Mathf.Lerp(escalaInicial, escalaMaxima, t);
            transform.localScale = Vector3.one * escala;

            yield return null;
        }

        tiempo = 0f;
        Vector3 escalaActual = transform.localScale;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;

            transform.localScale = Vector3.Lerp(escalaActual, escalaFinal, t);

            yield return null;
        }

        transform.localScale = escalaFinal;
    }
}
