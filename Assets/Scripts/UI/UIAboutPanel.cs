using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class UIAboutPanel : MonoBehaviour
{
	public UnityAction OnCloseAbout;

	[SerializeField] private InputReader _inputReader = default;
	[SerializeField] private TextAsset _aboutAsset;
	[SerializeField] private TextMeshProUGUI _aboutText = default;
	[SerializeField] private UICreditsRoller _aboutRoller = default;

	[Header("Listening on")]
	[SerializeField] private VoidEventChannelSO _aboutRollEndEvent = default;

	private string _aboutStrText;

	private void OnEnable()
	{
		_inputReader.MenuCloseEvent += CloseAboutScreen;
		SetAboutScreen();
	}

	private void OnDisable()
	{
		_inputReader.MenuCloseEvent -= CloseAboutScreen;
	}

	private void SetAboutScreen()
	{
		_aboutRoller.OnRollingEnded += EndRolling;
		FillAboutRoller();
		_aboutRoller.StartRolling();
	}

	public void CloseAboutScreen()
	{
		_aboutRoller.OnRollingEnded -= EndRolling;
		OnCloseAbout.Invoke();
	}

	private void FillAboutRoller()
	{
		_aboutStrText = _aboutAsset.text;
		SetAboutText();
	}

	private void SetAboutText()
	{
		_aboutText.text = _aboutStrText;
	}

	private void EndRolling()
	{
		if (_aboutRollEndEvent != null)
			_aboutRollEndEvent.RaiseEvent();
	}
}
