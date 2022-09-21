using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Components;
using UnityEngine.Events;
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
	[SerializeField] private BoolEventChannelSO _isItemButtonClickable = default;
	[HideInInspector] public ItemStack currentItem;

	public bool isClickable = true;
	bool _isSelected = false;
	bool _isClicked = false;
	// 正面左手: 3, 背面左手:4 , 正面右手: 5 , 背面右手 6
    public override void Init(ItemCore core)
    {
        base.Init(core);
		SetItem(core._itemStack, false);
    }

	public void ApplyColorItemImage(Color color)
    {
		_itemPreviewImage.color = color;

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
		
		_bgLocalizedImage.enabled = true;
		_bgLocalizedImage.AssetReference = itemStack.Item.LocalizePreviewImage;
		if (itemStack.Item.IsLocalized)
		{
			
		}
		else
		{
			//_bgLocalizedImage.enabled = false;
			//_itemPreviewImage.sprite = itemStack.Item.PreviewImage;
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
		_isItemButtonClickable.OnEventRaised += SetClickable;
	}
    private void OnDisable()
    {
		ItemClicked -= ShowItemInformation;
		_isItemButtonClickable.OnEventRaised -= SetClickable;
	}

	private void SetClickable(bool val)
    {
		isClickable = val;
    }

    public void HoverItem()
	{
		_imgHover.gameObject.SetActive(true);
		StartCoroutine("ShowHint");
	}

	public void UnhoverItem()
	{
		_imgHover.gameObject.SetActive(false);
		StopCoroutine("ShowHint");
		_hideHintPanel.RaiseEvent();
	}

	public void ClickItem()
	{
		if (ItemClicked != null && currentItem != null && currentItem.Item != null)
		{
			ItemClicked.Invoke(currentItem.Item);
			_imgSelected.gameObject.SetActive(true);
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
		if (isClickable)
			_fillInspectorChannelSO.FillInspector(item);
	}

	private IEnumerator ShowHint()
    {
		yield return new WaitForSeconds(0.5f);
		FillHintInformation();
	}

	void FillHintInformation()
    {
		_fillHintPanel.RaiseEvent(currentItem.Item.Name);
    }
}