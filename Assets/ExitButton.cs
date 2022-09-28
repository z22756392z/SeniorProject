using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader = default;

    public void OnExitButtonClicked()
    {
        _inputReader.PauseFromButton();
    }
}
