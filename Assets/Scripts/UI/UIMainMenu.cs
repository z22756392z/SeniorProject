using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
	[SerializeField] private Button _startButton = default;

	public UnityAction StartButtonAction;
	public UnityAction SettingsButtonAction;
	public UnityAction ExitButtonAction;

	public void StartButton()
	{
		StartButtonAction.Invoke();
	}

	public void SettingsButton()
	{
		SettingsButtonAction.Invoke();
	}

	public void ExitButton()
	{
		ExitButtonAction.Invoke();
	}
}
