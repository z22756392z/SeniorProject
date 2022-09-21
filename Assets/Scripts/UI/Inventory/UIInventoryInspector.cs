using UnityEngine;

public class UIInventoryInspector : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _hideInspector = default;
    [SerializeField] private UIInspectorDescription _inspectorDescription = default;
    [SerializeField] private UIInspectorPreview _inspectorPreview = default;
    [SerializeField] private UIInspectorForAnimation _inspectorAnimation = default;
    [SerializeField] private FillInspectorChannelSO _fillInspectorChannelSO = default;
    [SerializeField] private GameObject _inspector = default;

    private bool clickable = true;
    private ItemSO _curItem = default;
    private void OnEnable()
    {
        _fillInspectorChannelSO.OnEventRaised += FireAnim;
        _inspectorAnimation.AnimationEnded += AnimEnd;
        _inspectorAnimation.ContentChanged += FillInspector;
    }

    private void OnDisable()
    {
        _fillInspectorChannelSO.OnEventRaised -= FireAnim;
        _inspectorAnimation.AnimationEnded -= AnimEnd;
        _inspectorAnimation.ContentChanged -= FillInspector;
    }

    public void FireAnim(ItemSO itemToInspect)
    {
        if (!clickable) return;
        clickable = false;
        ShowInspector();
       
        
        if (_curItem != default && _curItem == itemToInspect)
        {
            _inspectorAnimation.AnimationEnded -= AnimEnd;
            _inspectorAnimation.AnimationEnded += HideInspector;
        }

        _curItem = itemToInspect;
        _inspectorAnimation.SetAnim(itemToInspect);
    }
    public void FillInspector()
    {
        _inspectorDescription.FillDescription(_curItem);
        _inspectorPreview.FillPreview(_curItem);
    }

    public void HideItemInformation()
    {
        if (!clickable) return;
        FireAnim(_curItem);
    }

    public void ShowInspector()
    {
        _inspector.SetActive(true);
    }

    void AnimEnd()
    {
        clickable = true;
    }
    public void HideInspector()
    {
        clickable = true;
        _curItem = default;
        _inspectorAnimation.AnimationEnded -= HideInspector;
        _inspectorAnimation.AnimationEnded += AnimEnd;
        _inspector.SetActive(false);
    }

}
