using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Events/Localized String Event Channel")]
public class LocalizedStringEventChannelSO : DescriptionBaseSO
{
	public event UnityAction<LocalizedString> OnEventRaised;

	public void RaiseEvent(LocalizedString value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
