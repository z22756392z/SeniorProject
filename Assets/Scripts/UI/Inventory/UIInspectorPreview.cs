using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UIInspectorPreview : MonoBehaviour
{
	[SerializeField] private LocalizeSpriteEvent _previewImage = default;

	public void FillPreview(ItemSO ItemToInspect)
	{
		_previewImage.gameObject.SetActive(true);
		_previewImage.AssetReference = ItemToInspect.LocalizePreviewImage;
	}

	public void HidePreview()
    {
		_previewImage.gameObject.SetActive(false);
	}
}
