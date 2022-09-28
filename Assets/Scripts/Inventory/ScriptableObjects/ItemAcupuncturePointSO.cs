using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ItemAcupuncturePoint", menuName = "Inventory/Acupuncture Point")]
public class ItemAcupuncturePointSO : ItemSO
{
	[Tooltip("A description of the item")]
	[SerializeField]
	protected LocalizedString _disease = default;
	[SerializeField] private int _landMark = default;
	[SerializeField] private Vector2 _offset = default;
	[SerializeField] private float _customize = default;
	[SerializeField] private bool _isLocalized = false;
	[SerializeField] private LocalizedSprite _localizePreviewImage = default;

	public override bool IsLocalized => _isLocalized;
	public override LocalizedSprite LocalizePreviewImage => _localizePreviewImage;

	public override LocalizedString Disease => _disease;
	public override int LandMark => _landMark;
	public override Vector2 Offest => _offset;
	public override float Customize => _customize;

	public void Setup(string titleKeyID, string descritpionKeyID, ItemTypeSO itemType , string diseaseID, float offset_x, float offset_y, int rel_position, float customize,GameObject AcupuncturePointPrefab, string localizedSpriteID)
    {
		_localizePreviewImage = new LocalizedSprite();
		_localizePreviewImage.TableReference = "AcpunturePointSprite";
		_localizePreviewImage.TableEntryReference = localizedSpriteID;

		_disease = new LocalizedString("AcupuncturePoint", diseaseID);
		_name = new LocalizedString("AcupuncturePoint", titleKeyID);
		_description = new LocalizedString("AcupuncturePoint", descritpionKeyID);
		_itemType = itemType;
		_prefab = AcupuncturePointPrefab;
		_offset = new Vector2(offset_x, offset_y);
		_landMark = rel_position;
		_customize = customize;
	}
}
