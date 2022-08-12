using System.Collections.Generic;
using UnityEngine;

public class SolutionController : MonoBehaviour
{
    [Header("Listening to")]
    [SerializeField] private VoidEventChannelSO onSceneReady = default;

    public List<GameObject> elements = default;

    private void Awake()
    {
        foreach (var item in elements)
        {
            item.SetActive(false);
        }
    }

    private void OnEnable()
    {
        onSceneReady.OnEventRaised += _Start;
    }

    private void OnDisable()
    {
        onSceneReady.OnEventRaised -= _Start;
    }

    private void _Start()
    {
        foreach (var item in elements)
        {
            item.SetActive(true);
        }
    }
}
