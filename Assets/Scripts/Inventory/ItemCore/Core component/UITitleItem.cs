using UnityEngine;
using UnityEngine.Localization.Components;

public class UITitleItem : ItemCoreComponent
{
    [SerializeField] private LocalizeStringEvent _title;
    [SerializeField] private BoolEventChannelSO _acupunturePointUITitleEvent = default;
    
    private bool _LogOnce = true;
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
        
        if (core._itemStack.Item.Name.GetLocalizedString()[0] == 'N')
        {
            if (_LogOnce)
                Mediapipe.Unity.Logger.LogDebug("Localize String: " + core._itemStack.Item.Name.TableEntryReference.Key + " not setup");
            _LogOnce = false;
        }
        else
        {
            _title.StringReference = core._itemStack.Item.Name;
        }
    }

    private void SetTitleDisplay(bool value)
    {
        _title.gameObject.SetActive(value);
    }
}
