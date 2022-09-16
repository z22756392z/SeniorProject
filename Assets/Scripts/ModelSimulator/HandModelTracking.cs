using UnityEngine;

public class HandModelTracking : MonoBehaviour
{
  [SerializeField] private GameObject _controllingPart = default;
  private void Update()
  {
    if(_controllingPart != null)
    {
      _controllingPart.transform.position = transform.position;
    }
  }
}
