using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveObject : MonoBehaviour
{
    public GameObject target, self;
    
    public void SetTargetActive()
    {
        target.SetActive(true);
        self.SetActive(false);
        Debug.Log("####");
    }
}
