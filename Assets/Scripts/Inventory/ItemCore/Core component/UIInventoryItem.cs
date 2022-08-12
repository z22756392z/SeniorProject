using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Components;
using UnityEngine.Events;
using UnityEngine.Localization;
using System.Collections;

public class UIInventoryItem : ItemCoreComponent
{
	[SerializeField] private TextMeshProUGUI _itemCount = default;
	[SerializeField] private Image _itemPreviewImage = default;
	[SerializeField] private Image _bgImage = default;
	[SerializeField] private Image _imgHover = default;
	[SerializeField] private Image _imgSelected = default;
	[SerializeField] private Image _bgInactiveImage = default;
	[SerializeField] private Button _itemButton = default;
	[SerializeField] private LocalizeSpriteEvent _bgLocalizedImage = default;

	public UnityAction<ItemSO> ItemClicked;
	[SerializeField] private FillInspectorChannelSO _fillInspectorChannelSO = default;
	[SerializeField] private LocalizedStringEventChannelSO _fillHintPanel = default;
	[SerializeField] private VoidEventChannelSO _hideHintPanel = default;
	[SerializeField] private VoidEventChannelSO _hideInspector = default;
	[HideInInspector] public ItemStack currentItem;
	
	bool _isSelected = false;
	bool _isClicked = false;
    public override void Init(ItemCore core)
    {
        base.Init(core);
		SetItem(core._itemStack, false);
    }

	public void SetItem(ItemStack itemStack, bool isSelected)
	{
		_isSelected = isSelected;
		_itemPreviewImage.gameObject.SetActive(true);
		_itemCount.gameObject.SetActive(true);
		_bgImage.gameObject.SetActive(true);
		_imgHover.gameObject.SetActive(true);
		_imgSelected.gameObject.SetActive(true);
		_itemButton.gameObject.SetActive(true);
		_bgInactiveImage.gameObject.SetActive(false);

		UnhoverItem();
		currentItem = itemStack;

		_imgSelected.gameObject.SetActive(isSelected);

		if (itemStack.Item.IsLocalized)
		{
			_bgLocalizedImage.enabled = true;
			_bgLocalizedImage.AssetReference = itemStack.Item.LocalizePreviewImage;
		}
		else
		{
			_bgLocalizedImage.enabled = false;
			_itemPreviewImage.sprite = itemStack.Item.PreviewImage;
		}
		_itemCount.text = itemStack.Amount.ToString();
		_bgImage.color = itemStack.Item.ItemType.TypeColor;
	}

	public void SetInactiveItem()
	{
		UnhoverItem();
		currentItem = null;
		_itemPreviewImage.gameObject.SetActive(false);
		_itemCount.gameObject.SetActive(false);
		_bgImage.gameObject.SetActive(false);
		_imgHover.gameObject.SetActive(false);
		_imgSelected.gameObject.SetActive(false);
		_itemButton.gameObject.SetActive(false);
		_bgInactiveImage.gameObject.SetActive(true);
	}

	public void SelectFirstElement()
	{
		_isSelected = true;
		_itemButton.Select();
		ClickItem();
	}

	private void OnEnable()
	{
		if (_isSelected)
		{ ClickItem(); }
		ItemClicked += ShowItemInformation;
	}
    private void OnDisable()
    {
		ItemClicked -= ShowItemInformation;
	}

    public void HoverItem()
	{
		_imgHover.gameObject.SetActive(true);
		StartCoroutine(ShowHint());
	}

	public void UnhoverItem()
	{
		_imgHover.gameObject.SetActive(false);
		StopCoroutine(ShowHint());
		_hideHintPanel.RaiseEvent();
	}

	public void ClickItem()
	{
		_isClicked = !_isClicked;
		if (ItemClicked != null && currentItem != null && currentItem.Item != null)
		{
			if (_isClicked) {
				ItemClicked.Invoke(currentItem.Item);
				_imgSelected.gameObject.SetActive(true);
			}
				
			else
            {
				_hideInspector.RaiseEvent();
				_imgSelected.gameObject.SetActive(false);
			}
		}
	}

	public void UnClicked()
    {
		_imgSelected.gameObject.SetActive(false);
		_isClicked = false;
	}

	public void SelectItem()
	{
		_isSelected = true;
		if (currentItem != null && currentItem.Item != null)
		{
			
		}
		else
		{
			
		}
	}

	public void UnselectItem()
	{
		_isSelected = false;
	}

	public void InspectItem(ItemSO itemToInspect)
	{
		//show Information
		ShowItemInformation(itemToInspect);
	}
	void ShowItemInformation(ItemSO item)
	{
		_fillInspectorChannelSO.FillInspector(item,this);
	}

	private IEnumerator ShowHint()
    {
		Debug.LogWarning("Start");
		yield return new WaitForSeconds(0.5f);
		Debug.LogWarning("End");
		FillHintInformation();
	}

	void FillHintInformation()
    {
		_fillHintPanel.RaiseEvent(currentItem.Item.Name);
    }
}