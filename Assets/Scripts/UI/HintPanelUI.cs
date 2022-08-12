using UnityEngine;
using UnityEngine.InputSystem;

public class HintPanelUI : MonoBehaviour
{
    private Vector3 _vec3WorkSpace;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

    }
    private void Update()
    {
        if (Camera.main == null) return;
        _vec3WorkSpace = Input.mousePosition;
        _vec3WorkSpace.Set(_vec3WorkSpace.x + _rectTransform.rect.width / 2, _vec3WorkSpace.y + _rectTransform.rect.y / 2, _vec3WorkSpace.z);
        transform.position = _vec3WorkSpace;
        /*
        _vec3WorkSpace = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position.Set(_vec3WorkSpace.x, _vec3WorkSpace.y, transform.position.z); 
        */
    }

    public void HideHintPanel()
    {
        gameObject.SetActive(false);
    }

    public void ShowHintPanel()
    {
        gameObject.SetActive(true);
    }
}
