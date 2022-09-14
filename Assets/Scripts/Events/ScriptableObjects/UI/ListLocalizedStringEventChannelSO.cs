using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/List Localized String Event Channel")]
public class ListLocalizedStringEventChannelSO : DescriptionBaseSO
{
	public event UnityAction<List<LocalizedString>> OnEventRaised;

	public void RaiseEvent(List<LocalizedString> value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
