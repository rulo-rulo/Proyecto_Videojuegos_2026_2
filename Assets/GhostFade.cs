using System.Collections;
using UnityEngine;

public class GhostFade : MonoBehaviour
{
    [SerializeField] float fadeDuration = 1f;

    Renderer[] rends;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
        Debug.Log("Renderers encontrados en fantasma: " + rends.Length);
    }

    public IEnumerator FadeOut()
    {
        Debug.Log("FadeOut del fantasma iniciado");

        float t = 0f;

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            foreach (Renderer r in rends)
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = alpha;
                        m.SetColor("_BaseColor", c);
                    }
                    else if (m.HasProperty("_Color"))
                    {
                        Color c = m.color;
                        c.a = alpha;
                        m.color = c;
                    }
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        foreach (Renderer r in rends)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = 0f;
                    m.SetColor("_BaseColor", c);
                }
                else if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = 0f;
                    m.color = c;
                }
            }
        }

        gameObject.SetActive(false);
    }
}