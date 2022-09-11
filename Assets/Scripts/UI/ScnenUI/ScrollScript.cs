using System.Collections;
using System.Collections.Generic;
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
            GetComponent<SpriteRenderer>().material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (LineRenderer != null)
        {
            GetComponent<LineRenderer>().material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (TrailRenderer != null)
        {
            GetComponent<TrailRenderer>().material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
        if (image != null)
        {
            GetComponent<Image>().material.mainTextureOffset = new Vector2(Time.time * speed_x, Time.time * speed_y);
        }
    }
}
