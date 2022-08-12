using UnityEngine;

public class ItemCoreComponent : MonoBehaviour, ILogicUpdate
{
    protected ItemCore core;

    public virtual void Init(ItemCore core)
    {
        this.core = core;
    }

    protected virtual void Awake()
    {
        core = transform.parent.GetComponent<ItemCore>();

        if (core == null) Debug.LogError("There is no core on the parent");
        core.AddComponent(this);
    }

    public virtual void LogicUpdate()
    {

    }
}
