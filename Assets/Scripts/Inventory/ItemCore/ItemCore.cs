using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemCore : MonoBehaviour
{
    private readonly List<ItemCoreComponent> CoreComponents = new List<ItemCoreComponent>();
    [SerializeField] private InventorySO _inventorySO = default;
    [HideInInspector] public ItemStack _itemStack;
    public Color HandnessColor = Color.white;
    public Color LeftHandColor;
    protected UIInventoryItem InventoryItem { get => inventoryItem ??= GetCoreComponent<UIInventoryItem>(); }
    private UIInventoryItem inventoryItem;
    private bool _isSetup = false;
    //Raised by ListAnnotations -- InstantiateChild
    private void Awake()
    {
        var comps = GetComponentsInChildren<ItemCoreComponent>();

        foreach (var component in comps)
        {
            AddComponent(component);
        }
    }

    public void SetupItemCore(int index)
    {
        _isSetup = true;
        if (_inventorySO.Items.Count - 1< index) return;
        List<ItemStack> itemStacks = _inventorySO.GetItemsInCurrentInventory(InventoryTabType.AcupuncturePoint);
        if (itemStacks.Count < index) return;
        _itemStack = itemStacks[index];

        foreach (var component in CoreComponents)
        {
            component.Init(this);
        }
    }
    //Raised by ListAnnotations -- ApplyColor
    public void ApplyColorToUI(Color color)
    {
        HandnessColor = color;
        if (InventoryItem != null)
        {
            InventoryItem.ApplyColorItemImage(HandnessColor);
        }
    }

    public void Update()
    {
        if (!_isSetup) return;
        foreach (ItemCoreComponent component in CoreComponents)
        {
            component.LogicUpdate();
        }
    }

    public void AddComponent(ItemCoreComponent component)
    {
        if (!CoreComponents.Contains(component))
        {
            CoreComponents.Add(component);
        }
    }

    public T GetCoreComponent<T>() where T : ItemCoreComponent
    {
        var comp = CoreComponents.OfType<T>().FirstOrDefault();

        if (comp == null) Debug.LogWarning($"{typeof(T)} not found on {transform.parent.name}");
        return comp;
    }

    public T GetCoreComponent<T>(ref T value) where T : ItemCoreComponent
    {
        value = GetCoreComponent<T>();
        return value;
    }
}
