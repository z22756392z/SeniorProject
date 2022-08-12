using UnityEngine;
using UnityEngine.Localization;


[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class ItemSO : SerializableScriptableObject
{
	[Tooltip("The name of the item")]
	[SerializeField] protected LocalizedString _name = default;

	[Tooltip("A preview image for the item")]
	[SerializeField]
	protected Sprite _previewImage = default;

	[Tooltip("A description of the item")]
	[SerializeField]
	protected LocalizedString _description = default;


	[Tooltip("The type of item")]
	[SerializeField]
	protected ItemTypeSO _itemType = default;

	[Tooltip("A prefab reference for the model of the item")]
	[SerializeField]
	protected GameObject _prefab = default;


	public LocalizedString Name => _name;
	public Sprite PreviewImage => _previewImage;
	public LocalizedString Description => _description;
	public ItemTypeSO ItemType => _itemType;
	public GameObject Prefab => _prefab;
	public virtual bool IsLocalized { get; }
	public virtual LocalizedSprite LocalizePreviewImage { get; }
	public virtual string Disease { get; }
	public virtual int LandMark { get; }
	public virtual Vector2 Offest { get; }
	public virtual float Customize { get; }

}
