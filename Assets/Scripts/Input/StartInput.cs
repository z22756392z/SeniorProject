using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartInput : MonoBehaviour
{
	[Header("Asset References")]
	[SerializeField] private InputReader _inputReader = default;
	[SerializeField] private GameStateSO _gameStateSO = default;
	[Header("Scene Ready Event")]
    [SerializeField] private VoidEventChannelSO _onSceneReady = default; //Raised by SceneLoader when the scene is set to active
	private void OnEnable()
	{
		_onSceneReady.OnEventRaised += StartGame;
	}

	private void OnDisable()
	{
		_onSceneReady.OnEventRaised -= StartGame;
	}

    private void StartGame()
    {
		_inputReader.EnableGameplayInput();
		_gameStateSO.UpdateGameState(GameState.Gameplay);
	}
}
