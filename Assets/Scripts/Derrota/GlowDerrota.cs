using UnityEngine;

public class GlowDerrota : MonoBehaviour
{
    public float velocidad = 2f;
    public float escalaExtra = 0.05f;
    public float alphaMin = 0.4f;
    public float alphaMax = 0.9f;

    private Vector3 escalaInicial;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        escalaInicial = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * velocidad) + 1f) * 0.5f;

        transform.localScale = escalaInicial * (1f + escalaExtra * t);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(alphaMin, alphaMax, t);
        }
    }
}