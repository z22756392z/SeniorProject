using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class UITitleItem : ItemCoreComponent
{
    [SerializeField] private TextMeshProUGUI _titleText = default;
    [SerializeField] private LocalizeStringEvent _titleStringEvent = default;
    [SerializeField] private BoolEventChannelSO _acupunturePointUITitleEvent = default;

    [SerializeField] private float baseLength = 0.085f;
    private float _initalFontSize;
    private bool _LogOnce = true;

    protected override void Awake()
    {
        base.Awake();
        _initalFontSize = _titleText.fontSize;
    }

    private void OnEnable()
    {
       
        _acupunturePointUITitleEvent.OnEventRaised += SetTitleDisplay;
        _titleStringEvent.gameObject.SetActive(false);
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

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        float difference;
        if (core.HandnessColor == core.LeftHandColor)
        {
            difference = Mediapipe.Unity.HandTracking.AcupuncturePointHandSolution.LeftBaseLength * 10 - baseLength * 10 + 1;
            
        }
        else
        {
            difference = Mediapipe.Unity.HandTracking.AcupuncturePointHandSolution.RightBaseLength * 10 - baseLength * 10 + 1;
        }
        _titleText.fontSizeMin = _initalFontSize * difference;
    }

    private void SetTitle()
    {
        
        if (core._itemStack.Item.Name.GetLocalizedString()[0] == 'N')
        {
            if (_LogOnce)
            {
                
                Mediapipe.Unity.Logger.LogDebug("Localize String: " + core._itemStack.Item.Name.TableEntryReference.Key + " not setup");
            }
            _LogOnce = false;
        }
        else
        {
            _titleStringEvent.gameObject.SetActive(true);
            _titleStringEvent.StringReference = core._itemStack.Item.Name;
        }
    }

    private void SetTitleDisplay(bool value)
    {
        _titleStringEvent.gameObject.SetActive(value);
    }
}
