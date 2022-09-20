using UnityEngine;
using TMPro;
using UnityEngine.Localization.Components;

public class UIInspectorDescription : MonoBehaviour
{
	[SerializeField] private LocalizeStringEvent _textDescription = default;
	[SerializeField] private LocalizeStringEvent _textName = default;

	public void FillDescription(ItemSO itemToInspect)
	{
		
		_textName.StringReference = itemToInspect.Name;
		_textName.StringReference.Arguments = new[] { new { Purpose = 0, Amount = 1 } };
		_textDescription.StringReference = itemToInspect.Description;
		
		_textName.gameObject.SetActive(true);
		_textDescription.gameObject.SetActive(true);
		
	}

	public void HideDescription()
    {
		_textName.gameObject.SetActive(false);
		_textDescription.gameObject.SetActive(false);
	}
}