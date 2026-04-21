using System.Collections;
using UnityEngine;

public class GhostFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2f;

    private Renderer[] rends;

    private void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
        Debug.Log("Renderers encontrados en fantasma: " + rends.Length);

        // Asegura que el fantasma empieza totalmente visible
        SetAlpha(1f);
    }

    public IEnumerator FadeOut()
    {
        Debug.Log("FadeOut del fantasma iniciado");

        float t = 0f;

        while (t < fadeDuration)
        {
            float alpha01 = Mathf.Lerp(1f, 0f, t / fadeDuration);
            SetAlpha(alpha01);

            Debug.Log("Alpha actual: " + alpha01);

            t += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer r in rends)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }
        }
    }
}