using UnityEngine;

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
        UpdatePos();
    }

    public void HideHintPanel()
    {
        gameObject.SetActive(false);
    }

    public void ShowHintPanel()
    {
        gameObject.SetActive(true);
        UpdatePos();
    }

    private void UpdatePos()
    {
        _vec3WorkSpace = Input.mousePosition;
        _vec3WorkSpace.Set(_vec3WorkSpace.x + _rectTransform.rect.width / 2, _vec3WorkSpace.y + _rectTransform.rect.y / 2, _vec3WorkSpace.z);
        transform.position = _vec3WorkSpace;
    }
}
