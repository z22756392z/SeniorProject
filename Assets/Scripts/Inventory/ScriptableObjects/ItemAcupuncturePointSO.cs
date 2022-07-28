using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ItemAcupuncturePoint", menuName = "Inventory/Acupuncture Point")]
public class ItemAcupuncturePointSO : ItemSO
{
	[SerializeField] private string _disease = default;
	[SerializeField] private int _landMark = default;
	[SerializeField] private Vector2 _offset = default;
	[SerializeField] private float _customize = default;

	public override string Disease => _disease;
	public override int LandMark => _landMark;
	public override Vector2 Offest => _offset;
	public override float Customize => _customize;

	public void Setup(string titleKeyID, string descritpionKeyID, ItemTypeSO itemType , string disease, float offset_x, float offset_y, int rel_position, float customize,GameObject AcupuncturePointPrefab)
    {
		_name = new LocalizedString("AcupuncturePoint", titleKeyID);
		_description = new LocalizedString("AcupuncturePoint", descritpionKeyID);
		_itemType = itemType;
		_prefab = AcupuncturePointPrefab;
		_disease = disease;
		_offset = new Vector2(offset_x, offset_y);
		_landMark = rel_position;
		_customize = customize;
	}
}
