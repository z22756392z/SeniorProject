using UnityEngine;
using UnityEngine.Localization.Components;
public class UITitleItem : ItemCoreComponent
{
    [SerializeField] private LocalizeStringEvent _title;
    [SerializeField] private BoolEventChannelSO _acupunturePointUITitleEvent = default;

    private void OnEnable()
    {
        _acupunturePointUITitleEvent.OnEventRaised += SetTitleDisplay;
    }

    private void OnDisable()
    {
        _acupunturePointUITitleEvent.OnEventRaised -= SetTitleDisplay;
    }

    public override void Init(ItemCore core)
    {
        base.Init(core);
        SetTitle();
    }

    private void SetTitle()
    {
        _title.StringReference = core._itemStack.Item.Name;
    }

    private void SetTitleDisplay(bool value)
    {
        _title.gameObject.SetActive(value);
    }
}
