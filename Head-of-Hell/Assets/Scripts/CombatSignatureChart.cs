using UnityEngine;
using UnityEngine.UI;

public class CombatSignatureChart : Graphic
{
    [Header("Values")]
    [Range(0f, 1f)] public float defense = 0.5f;    // top
    [Range(0f, 1f)] public float aggression = 0.5f; // right
    [Range(0f, 1f)] public float risk = 0.5f;       // bottom
    [Range(0f, 1f)] public float mobility = 0.5f;   // left

    [Header("Layout")]
    [SerializeField] private float chartRadius = 100f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private int gridLevels = 3;

    [Header("Colors")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private Color axisColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color fillColor = new Color(1f, 0.35f, 0.55f, 0.22f);
    [SerializeField] private Color outlineColor = new Color(1f, 0.45f, 0.65f, 0.95f);

    public void SetValues(float agg, float def, float mob, float rsk)
    {
        aggression = Mathf.Clamp01(agg);
        defense = Mathf.Clamp01(def);
        mobility = Mathf.Clamp01(mob);
        risk = Mathf.Clamp01(rsk);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        Vector2 center = rect.center;

        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        radius = Mathf.Min(radius, chartRadius);

        DrawGrid(vh, center, radius);
        DrawAxes(vh, center, radius);
        DrawProfileShape(vh, center, radius);
        DrawProfileOutline(vh, center, radius);
    }

    private void DrawGrid(VertexHelper vh, Vector2 center, float radius)
    {
        if (gridLevels < 1) return;

        for (int i = 1; i <= gridLevels; i++)
        {
            float t = i / (float)gridLevels;
            Vector2 top = center + Vector2.up * (radius * t);
            Vector2 right = center + Vector2.right * (radius * t);
            Vector2 bottom = center + Vector2.down * (radius * t);
            Vector2 left = center + Vector2.left * (radius * t);

            AddLine(vh, top, right, lineThickness, gridColor);
            AddLine(vh, right, bottom, lineThickness, gridColor);
            AddLine(vh, bottom, left, lineThickness, gridColor);
            AddLine(vh, left, top, lineThickness, gridColor);
        }
    }

    private void DrawAxes(VertexHelper vh, Vector2 center, float radius)
    {
        AddLine(vh, center, center + Vector2.up * radius, lineThickness, axisColor);
        AddLine(vh, center, center + Vector2.right * radius, lineThickness, axisColor);
        AddLine(vh, center, center + Vector2.down * radius, lineThickness, axisColor);
        AddLine(vh, center, center + Vector2.left * radius, lineThickness, axisColor);
    }

    private void DrawProfileShape(VertexHelper vh, Vector2 center, float radius)
    {
        Vector2 top = center + Vector2.up * (radius * defense);
        Vector2 right = center + Vector2.right * (radius * aggression);
        Vector2 bottom = center + Vector2.down * (radius * risk);
        Vector2 left = center + Vector2.left * (radius * mobility);

        int startIndex = vh.currentVertCount;

        AddVert(vh, top, fillColor);
        AddVert(vh, right, fillColor);
        AddVert(vh, bottom, fillColor);
        AddVert(vh, left, fillColor);

        vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 0, startIndex + 2, startIndex + 3);
    }

    private void DrawProfileOutline(VertexHelper vh, Vector2 center, float radius)
    {
        Vector2 top = center + Vector2.up * (radius * defense);
        Vector2 right = center + Vector2.right * (radius * aggression);
        Vector2 bottom = center + Vector2.down * (radius * risk);
        Vector2 left = center + Vector2.left * (radius * mobility);

        AddLine(vh, top, right, lineThickness, outlineColor);
        AddLine(vh, right, bottom, lineThickness, outlineColor);
        AddLine(vh, bottom, left, lineThickness, outlineColor);
        AddLine(vh, left, top, lineThickness, outlineColor);
    }

    private void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color color)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int startIndex = vh.currentVertCount;

        AddVert(vh, a - normal, color);
        AddVert(vh, a + normal, color);
        AddVert(vh, b + normal, color);
        AddVert(vh, b - normal, color);

        vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 0, startIndex + 2, startIndex + 3);
    }

    private void AddVert(VertexHelper vh, Vector2 position, Color color)
    {
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        v.position = position;
        vh.AddVert(v);
    }
}