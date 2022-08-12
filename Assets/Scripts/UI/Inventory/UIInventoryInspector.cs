using UnityEngine;

public class UIInventoryInspector : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _hideInspector = default;
    [SerializeField] private UIInspectorDescription _inspectorDescription = default;
    [SerializeField] private FillInspectorChannelSO _fillInspectorChannelSO = default;
    [SerializeField] private GameObject _inspector = default;
    private UIInventoryItem _preUIInventoryInspector;

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

    public void FillInspector(ItemSO itemToInspect, UIInventoryItem ui)
    {
        _inspectorDescription.FillDescription(itemToInspect);
        if (ui != _preUIInventoryInspector)
        {
            _preUIInventoryInspector?.UnClicked();
            _preUIInventoryInspector = ui;
        }
        ShowInspector();
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
