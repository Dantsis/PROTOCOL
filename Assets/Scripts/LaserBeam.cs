using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public LineRenderer line;
    public float width = 0.1f;

    void Awake()
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.startWidth = width;
        line.endWidth = width;
        line.enabled = false;
    }

    public void Draw(Vector2 start, Vector2 end)
    {
        line.enabled = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public void Hide()
    {
        line.enabled = false;
    }
}
