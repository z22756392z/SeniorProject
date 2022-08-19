using UnityEngine;
using UnityEngine.Events;

public class UIMainMenu : MonoBehaviour
{
	public UnityAction StartButtonAction;
	public UnityAction SettingsButtonAction;
	public UnityAction AboutButtonAction;
	public UnityAction ExitButtonAction;

	public void StartButton()
	{
		StartButtonAction.Invoke();
	}

	public void SettingsButton()
	{
		SettingsButtonAction.Invoke();
	}

	public void AboutButton()
    {
		AboutButtonAction.Invoke();
	}

	public void ExitButton()
	{
		ExitButtonAction.Invoke();
	}
}
