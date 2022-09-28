using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
	[SerializeField] private UIPopup _popupPanel = default;
	[SerializeField] private UISettingsController _settingsPanel = default;
	[SerializeField] private UIAboutPanel _aboutPanel = default;
	[SerializeField] private UIMainMenu _mainMenuPanel = default;

	[SerializeField] private SaveSystem _saveSystem = default;

	[SerializeField] private InputReader _inputReader = default;
	[SerializeField] private Animator _animator = default;

	[Header("Effect")]
	[SerializeField] private List<GameObject> effects = new List<GameObject>();

	[Header("Broadcasting on")]
	[SerializeField]
	private VoidEventChannelSO _startGameEvent = default;
	private bool isStartClicked = false;
	private IEnumerator Start()
	{
		_inputReader.EnableMenuInput();
		yield return new WaitForSeconds(0.4f); //waiting time for all scenes to be loaded 
		SetMenuScreen();
	}
	void SetMenuScreen()
	{
		_saveSystem.LoadSaveDataFromDisk();
		_mainMenuPanel.StartButtonAction += OnStartIntroAnim;
		_mainMenuPanel.SettingsButtonAction += OpenSettingsScreen;
		_mainMenuPanel.AboutButtonAction += OpenAboutScreen;
		_mainMenuPanel.ExitButtonAction += ShowExitConfirmationPopup;
	}

	public void StartGame()
    {
		_startGameEvent.RaiseEvent();
	}

	public void OnStartIntroAnim()
    {
        if (!isStartClicked)
        {
			isStartClicked = true;
			_animator.SetTrigger("IntoMain");
		}		
	}
	void CloseEffect()
    {
        foreach (var item in effects)
        {
			item.SetActive(false);
        }
	}

	void OpenEffect()
    {
		foreach (var item in effects)
		{
			item.SetActive(true);
		}
	}

	public void OpenSettingsScreen()
	{
		CloseEffect();
		_settingsPanel.gameObject.SetActive(true);
		_settingsPanel.Closed += CloseSettingsScreen;

	}
	public void CloseSettingsScreen()
	{
		OpenEffect();
		_settingsPanel.Closed -= CloseSettingsScreen;
		_settingsPanel.gameObject.SetActive(false);
	}

	public void OpenAboutScreen()
    {
		CloseEffect();
		_aboutPanel.gameObject.SetActive(true);

		_aboutPanel.OnCloseAbout += CloseAboutScreen;
	}

	public void CloseAboutScreen()
	{
		OpenEffect();
		_aboutPanel.OnCloseAbout -= CloseAboutScreen;
		_aboutPanel.gameObject.SetActive(false);
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
