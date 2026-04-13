using UnityEngine;
using System.Collections;

public class DerrotaAnimacion : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float duracion = 0.5f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float tiempo = 0f;
        canvasGroup.alpha = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, tiempo / duracion);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
