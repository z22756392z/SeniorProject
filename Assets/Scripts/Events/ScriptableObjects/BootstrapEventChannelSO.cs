using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// This class is used for Events to toggle the interaction UI.
/// Example: Dispaly or hide the interaction UI via a bool and the interaction type from the enum via int
/// </summary>

[CreateAssetMenu(menuName = "Events/Bootstrap Event Channel")]
public class BootstrapEventChannelSO : DescriptionBaseSO
{
	public BootstrapGet OnEventRaised;

	public Mediapipe.Unity.Bootstrap GetBootstrap()
	{
		if (OnEventRaised != null)
			return OnEventRaised.Invoke();
		return null;
	}
}

public delegate Mediapipe.Unity.Bootstrap BootstrapGet();

