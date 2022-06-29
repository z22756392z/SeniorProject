using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuManager : MonoBehaviour
{
	[SerializeField] private UIPopup _popupPanel = default;
	[SerializeField] private UISettingsController _settingsPanel = default;
	[SerializeField] private UIMainMenu _mainMenuPanel = default;

	[SerializeField] private SaveSystem _saveSystem = default;

	[SerializeField] private InputReader _inputReader = default;


	[Header("Broadcasting on")]
	[SerializeField]
	private VoidEventChannelSO _startGameEvent = default;

	private IEnumerator Start()
	{
		_inputReader.EnableMenuInput();
		yield return new WaitForSeconds(0.4f); //waiting time for all scenes to be loaded 
		SetMenuScreen();
	}
	void SetMenuScreen()
	{
		_saveSystem.LoadSaveDataFromDisk();
		_mainMenuPanel.StartButtonAction += _startGameEvent.RaiseEvent;
		_mainMenuPanel.SettingsButtonAction += OpenSettingsScreen;
		_mainMenuPanel.ExitButtonAction += ShowExitConfirmationPopup;
	}

	public void OpenSettingsScreen()
	{
		_settingsPanel.gameObject.SetActive(true);
		_settingsPanel.Closed += CloseSettingsScreen;

	}
	public void CloseSettingsScreen()
	{
		_settingsPanel.Closed -= CloseSettingsScreen;
		_settingsPanel.gameObject.SetActive(false);
	}

	public void ShowExitConfirmationPopup()
	{
		_popupPanel.ConfirmationResponseAction += HideExitConfirmationPopup;
		_popupPanel.gameObject.SetActive(true);
		_popupPanel.SetPopup(PopupType.Quit);
	}
	void HideExitConfirmationPopup(bool quitConfirmed)
	{
		_popupPanel.ConfirmationResponseAction -= HideExitConfirmationPopup;
		_popupPanel.gameObject.SetActive(false);
		if (quitConfirmed)
		{
			Application.Quit();
		}
	}
	private void OnDestroy()
	{
		_popupPanel.ConfirmationResponseAction -= HideExitConfirmationPopup;
	}

}
