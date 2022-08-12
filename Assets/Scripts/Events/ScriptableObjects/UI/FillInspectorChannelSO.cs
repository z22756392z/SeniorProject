using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/UI/Fill Inspector Channel")]
public class FillInspectorChannelSO : DescriptionBaseSO
{
	public UnityAction<ItemSO, UIInventoryItem> OnEventRaised;

	public void FillInspector(ItemSO item, UIInventoryItem ui)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(item, ui);
	}
}
