using UnityEngine;

public class UIInventoryInspector : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _hideInspector = default;
    [SerializeField] private UIInspectorDescription _inspectorDescription = default;
    [SerializeField] private UIInspectorPreview _inspectorPreview = default;
    [SerializeField] private UIInspectorForAnimation _inspectorAnimation = default;
    [SerializeField] private FillInspectorChannelSO _fillInspectorChannelSO = default;
    [SerializeField] private GameObject _inspector = default;


    private void OnEnable()
    {
        _fillInspectorChannelSO.OnEventRaised += FillInspector;
        _hideInspector.OnEventRaised += HideItemInformation;
    }

    private void OnDisable()
    {
        _fillInspectorChannelSO.OnEventRaised -= FillInspector;
        _hideInspector.OnEventRaised -= HideItemInformation;
    }

    public void FillInspector(ItemSO itemToInspect)
    {
        _inspectorAnimation.SetAnim(itemToInspect);

        _inspectorDescription.FillDescription(itemToInspect);
        _inspectorPreview.FillPreview(itemToInspect);
        
        ShowInspector();
    }

    public void HideInspector()
    {
        _inspectorDescription.HideDescription();
        _inspectorPreview.HidePreview();
       
        HideItemInformation();
    }

    public void ShowInspector()
    {
        _inspector.SetActive(true);
    }

    public void HideItemInformation()
    {
        _inspector.SetActive(false);
    }

}
