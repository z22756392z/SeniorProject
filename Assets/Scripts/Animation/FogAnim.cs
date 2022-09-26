using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogAnim : MonoBehaviour
{
    public float Intensity = 0;
    public string fog = "_Health";
    public GameObject tex;
    private MeshRenderer renderer;
    private int id;
    void Start()
    {
        renderer = tex.GetComponent<MeshRenderer>();
        id = Shader.PropertyToID(fog);
    }

    // Update is called once per frame
    void Update()
    {
        renderer.material.SetFloat(fog, Intensity);
    }
}
