using UnityEngine;
using UnityEngine.UI;

public class ScrollScript : MonoBehaviour
{
    public float speed_x = 0.1f, speed_y = 0f;
    LineRenderer LineRenderer;
    SpriteRenderer SpriteRenderer;
    TrailRenderer TrailRenderer;
    Image image;
    private void Start()
    {
        LineRenderer = GetComponent<LineRenderer>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        TrailRenderer = GetComponent<TrailRenderer>();
        image = GetComponent<Image>();
    }
    void Update()
    {
        if (SpriteRenderer != null)
        {
            SpriteRenderer.material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (LineRenderer != null)
        {
            LineRenderer.material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (TrailRenderer != null)
        {
            TrailRenderer.material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (image != null)
        {
            image.material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
    }
}
