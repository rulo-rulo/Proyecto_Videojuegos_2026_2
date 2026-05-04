using UnityEngine;

namespace Telekinesis
{
    public class DirectionArrow : MonoBehaviour
    {
        [SerializeField] private Color  arrowColor  = Color.red;
        [SerializeField] private float  arrowLength = 1.5f;
        [SerializeField] private float  arrowWidth  = 0.08f;
        [SerializeField] private float  headLength  = 0.4f;
        [SerializeField] private float  heightOffset = 1.2f;

        private LineRenderer body;
        private LineRenderer head;
        private GameObject   arrowRoot;

        private void Awake()
        {
            arrowRoot = new GameObject("ArrowRoot");
            arrowRoot.transform.localPosition = Vector3.up * heightOffset;

            body = CreateLine("ArrowBody", arrowWidth);
            head = CreateLine("ArrowHead", arrowWidth * 2.5f, arrowWidth * 0.01f);

            SetVisible(false);
        }

        private LineRenderer CreateLine(string name, float startWidth, float endWidth = -1f)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(arrowRoot.transform);
            go.transform.localPosition = Vector3.zero;

            LineRenderer lr        = go.AddComponent<LineRenderer>();
            lr.useWorldSpace       = true;
            lr.positionCount       = 2;
            lr.startWidth          = startWidth;
            lr.endWidth            = endWidth < 0 ? startWidth : endWidth;
            lr.material            = new Material(Shader.Find("Sprites/Default"));
            lr.startColor          = arrowColor;
            lr.endColor            = arrowColor;
            lr.numCornerVertices   = 4;
            lr.numCapVertices      = 4;

            return lr;
        }

        public void Show(Vector3 direction)
        {
            SetVisible(true);
            UpdateDirection(direction);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void UpdateDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            Vector3 dir        = direction.normalized;
            Vector3 origin = transform.position + Vector3.up * heightOffset;
            Vector3 bodyStart = origin;
            Vector3 bodyEnd   = origin + dir * (arrowLength - headLength);
            Vector3 headEnd   = origin + dir * arrowLength;

            body.SetPosition(0, bodyStart);
            body.SetPosition(1, bodyEnd);

            head.SetPosition(0, bodyEnd);
            head.SetPosition(1, headEnd);
        }

        private void SetVisible(bool visible)
        {
            if (body != null) body.enabled = visible;
            if (head != null) head.enabled = visible;
        }
    }
}
