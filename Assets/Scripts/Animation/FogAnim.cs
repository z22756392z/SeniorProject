using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogAnim : MonoBehaviour
{
    [Range(0.9f, 1.0011f)]public float Intensity = 1.0011f;
    public string Time = "_T";
    public GameObject tex;
    private Image image;
    private int id;
    void Start()
    {
        image = tex.GetComponent<Image>();
        image.material.shader = Shader.Find("Unlit/BloodStain");
        id = Shader.PropertyToID(Time);
        image.material.SetFloat(id, Intensity);
    }

    // Update is called once per frame
    void Update()
    {
        image.material.SetFloat(id, Intensity);
    }
}
