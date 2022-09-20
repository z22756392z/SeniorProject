using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/UI/Fill Inspector Channel")]
public class FillInspectorChannelSO : DescriptionBaseSO
{
	public UnityAction<ItemSO> OnEventRaised;

	public void FillInspector(ItemSO item)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(item);
	}
}
