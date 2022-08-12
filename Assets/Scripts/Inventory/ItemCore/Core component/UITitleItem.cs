using UnityEngine;
using UnityEngine.Localization.Components;
public class UITitleItem : ItemCoreComponent
{
    [SerializeField] private LocalizeStringEvent _title;
    public override void Init(ItemCore core)
    {
        base.Init(core);
        SetTitle();
    }

    private void SetTitle()
    {
        _title.StringReference = core._itemStack.Item.Name;
    }
}
