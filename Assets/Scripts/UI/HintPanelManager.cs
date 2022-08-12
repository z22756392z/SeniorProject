using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class HintPanelManager : MonoBehaviour
{
	[SerializeField] private HintPanelUI _hintPanel;
	[SerializeField] private LocalizeStringEvent _title;
	[SerializeField] private LocalizedStringEventChannelSO _localizedStringEventChannelSO = default;
    [SerializeField] private VoidEventChannelSO _hideHintPanel = default;
    private void Awake()
    {
        HideHintPanel();
    }

    private void OnEnable()
    {
		_localizedStringEventChannelSO.OnEventRaised += ShowItemInformation;
        _hideHintPanel.OnEventRaised += HideHintPanel;
    }

    private void OnDisable()
    {
		_localizedStringEventChannelSO.OnEventRaised -= ShowItemInformation;
        _hideHintPanel.OnEventRaised -= HideHintPanel;
    }

    void ShowItemInformation(LocalizedString value)
	{
		_title.StringReference = value;
        ShowHintPanel();
    }

	void ShowHintPanel()
    {
        _hintPanel.ShowHintPanel();
    }

    void HideHintPanel()
    {
        _hintPanel.HideHintPanel();
    }
}