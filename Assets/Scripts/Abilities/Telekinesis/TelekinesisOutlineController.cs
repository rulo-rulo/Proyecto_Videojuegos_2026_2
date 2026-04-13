using UnityEngine;
using System.Collections.Generic;

namespace Telekinesis
{
    public class TelekinesisOutlineController : MonoBehaviour
    {
        [SerializeField] private Color normalColor  = Color.red;
        [SerializeField] private Color nearestColor = Color.green;
        [SerializeField] private float outlineWidth = 5f;

        private List<Outline> activeOutlines = new List<Outline>();

        public void ShowOutlines(List<MovableObject> objects, MovableObject nearest)
        {
            HideOutlines();

            foreach (MovableObject obj in objects)
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline == null)
                    outline = obj.gameObject.AddComponent<Outline>();

                outline.OutlineMode  = Outline.Mode.OutlineAll;
                outline.OutlineWidth = outlineWidth;
                outline.OutlineColor = obj == nearest ? nearestColor : normalColor;
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
