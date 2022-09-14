using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class is used for Events that have one int argument.
/// Example: An Achievement unlock event, where the int is the Achievement ID.
/// </summary>

[CreateAssetMenu(menuName = "Events/List String Event Channel")]
public class ListStringEventChannelSO : DescriptionBaseSO
{
	public UnityAction<List<string>>OnEventRaised;

	public void RaiseEvent(List<string> value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
