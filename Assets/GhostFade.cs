using UnityEngine;
using System.Collections;

public class GhostFade : MonoBehaviour
{
    [SerializeField] float fadeDuration = 1f;

    Renderer[] rends;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
    }

    public IEnumerator FadeOut()
    {
        float t = 0;

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            foreach (var r in rends)
            {
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_Color"))
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

        gameObject.SetActive(false);
    }
}