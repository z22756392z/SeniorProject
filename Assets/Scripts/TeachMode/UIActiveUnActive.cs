using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActiveUnActive : MonoBehaviour
{
    public GameObject Object;

    public void SetActiveObject()
    {
        Object.SetActive(true);
    }
    public void SetUnActiveObject()
    {
        Object.SetActive(false);
    }
}
