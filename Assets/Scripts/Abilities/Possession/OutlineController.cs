using UnityEngine;
using System.Collections.Generic;

namespace Possession
{
    public class OutlineController : MonoBehaviour
    {
        [SerializeField] private Color normalColor  = Color.white;
        [SerializeField] private Color nearestColor = Color.yellow;
        [SerializeField] private float outlineWidth = 5f;

        private List<Outline> activeOutlines = new List<Outline>();

        // -------------------------------------------------- API pública

        public void ShowOutlines(List<IPossessable> possessables, IPossessable nearest)
        {
            HideOutlines();

            foreach (IPossessable possessable in possessables)
            {
                GameObject go = ((MonoBehaviour)possessable).gameObject;

                Outline outline = go.GetComponent<Outline>();
                if (outline == null)
                    outline = go.AddComponent<Outline>();

                outline.OutlineMode  = Outline.Mode.OutlineAll;
                outline.OutlineWidth = outlineWidth;
                outline.OutlineColor = possessable == nearest ? nearestColor : normalColor;
                outline.enabled      = true;

                activeOutlines.Add(outline);
            }
        }

        public void HideOutlines()
        {
            foreach (Outline outline in activeOutlines)
            {
                if (outline != null)
                    outline.enabled = false;
            }

            activeOutlines.Clear();
        }
    }
}
