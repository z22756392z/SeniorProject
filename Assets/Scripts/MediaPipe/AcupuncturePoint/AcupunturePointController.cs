using UnityEngine;

public class AcupunturePointController : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _acupunturePointUITitleEvent = default;
    private bool _hide = false;
    public void OnHideAcupunturePointButtonClicked()
    {
        _hide = !_hide;
        _acupunturePointUITitleEvent.RaiseEvent(_hide);
    }
}
