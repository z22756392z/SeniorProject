using Mediapipe.Unity;
using UnityEngine;

public class LeftHandLandmarkController : MonoBehaviour
{
    [SerializeField] private HolisticLandmarkListAnnotation _holisticLandmarkListAnnotation = default;
    [SerializeField] private InventorySO _inventorySO = default;
    private void Awake()
    {
        //TODO: According to body part, modify tabs
        _holisticLandmarkListAnnotation._leftHandLandmarkListAnnotation.LandmarkCount = _inventorySO.GetItemsInCurrentInventory(InventoryTabType.AcupuncturePoint).Count;
    }
}
