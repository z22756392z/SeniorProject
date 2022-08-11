using UnityEngine;

public class UIInventoryInspector : MonoBehaviour
{
    [SerializeField] private UIInspectorDescription _inspectorDescription = default;
    [SerializeField] private ItemEventChannelSO _itemEventChannelSO = default;
    [SerializeField] private GameObject _inspector = default;

    private void OnEnable()
    {
        _itemEventChannelSO.OnEventRaised += FillInspector;
    }

    private void OnDisable()
    {
        _itemEventChannelSO.OnEventRaised -= FillInspector;
    }

    public void FillInspector(ItemSO itemToInspect/*, bool[] availabilityArray = null*/)
    {

        //bool isForCooking = (itemToInspect.ItemType.ActionType == ItemInventoryActionType.Cook);

        _inspectorDescription.FillDescription(itemToInspect);
        ShowInspector();
        //if (isForCooking && availabilityArray!= null)
        //{
        //_recipeIngredients.FillIngredients(itemToInspect.IngredientsList, availabilityArray);
        //_recipeIngredients.gameObject.SetActive(true);
        //}
        //else
        //_recipeIngredients.gameObject.SetActive(false);
    }

    public void ShowInspector()
    {
        _inspector.SetActive(true);
    }

    public void HideItemInformation()
    {
        _inspector.SetActive(false);
    }

}
