using System;
using UnityEngine;

public class EditorTeleporter : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
	[SerializeField] private GameObject _cheatMenu;
	[SerializeField] private PathStorageSO _path;
	[SerializeField] private VoidEventChannelSO _onSceneReady;

	[Header("Broadcast on")]
	[SerializeField] private LoadEventChannelSO _loadLocationRequest;

	[SerializeField] private bool _enableCheatMenuInBuild = default;

	[SerializeField] private LocationSO _lastLocationTeleportedTo = default;

	private void OnEnable()
	{
		_inputReader.CheatMenuEvent += ToggleCheatMenu;
		_onSceneReady.OnEventRaised += Close;
	}

	private void OnDisable()
	{
		_inputReader.CheatMenuEvent -= ToggleCheatMenu;
		_onSceneReady.OnEventRaised -= Close;
	}

	private void Start()
	{
		_cheatMenu.SetActive(false);
		if (_enableCheatMenuInBuild) _inputReader.EnableCheatInput();
	}

	public void ToggleCheatMenu()
	{
		_cheatMenu.SetActive(!_cheatMenu.activeInHierarchy);
	}

	public void Teleport(LocationSO where, PathSO whichEntrance)
	{
		//Avoid reloading the same Location, which would result in an error
		if(where == _lastLocationTeleportedTo)
			return;

		_path.lastPathTaken = whichEntrance;
		_lastLocationTeleportedTo = where;
		_loadLocationRequest.RaiseEvent(where);
	}

	private void Close()
    {
		_cheatMenu.SetActive(false);
	}
}
