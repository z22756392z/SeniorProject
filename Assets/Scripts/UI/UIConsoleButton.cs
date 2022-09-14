using UnityEngine;

public class UIConsoleButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _ModalOnClose;
    [SerializeField] private GameObject _consoleButton;

    private void OnEnable()
    {
        if(_ModalOnClose != null)
            _ModalOnClose.OnEventRaised += OpenConsoleButton;
    }

    private void OnDisable()
    {
        if (_ModalOnClose != null)
            _ModalOnClose.OnEventRaised -= OpenConsoleButton;
    }

    void OpenConsoleButton()
    {
        _consoleButton.SetActive(true);
    }

    public void CloseConsoleButton()
    {
        _consoleButton.SetActive(false);
    }
}
