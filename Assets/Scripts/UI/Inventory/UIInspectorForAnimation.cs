using UnityEngine;
using UnityEngine.Events;

class UIInspectorForAnimation : MonoBehaviour
{
    [SerializeField] private InventorySO _inventory = default;
    [SerializeField] private Animator _animator = default;
    [SerializeField] private string _onOpenInspectorAnimParameter = "OpenBook";
    [SerializeField] private string _onHideInspectorAnimParamter = "CloseBook";
    [SerializeField] private string _onFillInspectorAnimParameter1 = "FlippingR";
    [SerializeField] private string _onFillInspectorAnimParameter2 = "FlippingL";

    public event UnityAction AnimationEnded;
    public event UnityAction ContentChanged;
    private int _preItemIndex = -1;
    public void SetAnim(ItemSO itemToInspect)
    {
        int index = _inventory.Items.FindIndex(o => o.Item == itemToInspect);
        if (_preItemIndex == -1)
        {
            _animator.ResetTrigger(_onHideInspectorAnimParamter);
            _animator.ResetTrigger(_onFillInspectorAnimParameter1);
            _animator.ResetTrigger(_onFillInspectorAnimParameter2);
            _animator.SetTrigger(_onOpenInspectorAnimParameter);
        }
        else if (_preItemIndex < index)
        {
            _animator.SetTrigger(_onFillInspectorAnimParameter2);
        }
        else if (_preItemIndex > index)
        {
            _animator.SetTrigger(_onFillInspectorAnimParameter1);
        }
        else if(_preItemIndex == index)
        {
            _animator.SetTrigger(_onHideInspectorAnimParamter);
        }
        _preItemIndex = index;
    }

    public void SetAnim(string val)
    {
        _animator.SetTrigger(val);
    }

    public void OnAnimationEnded()
    {
        AnimationEnded?.Invoke();
    }

    public void OnChangeContent()
    {
        ContentChanged?.Invoke();
    }
}