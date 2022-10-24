using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseBook : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _hideInspector = default;

    public void Close()
    {
        _hideInspector.RaiseEvent();
    }
}
