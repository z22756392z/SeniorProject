using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	Gameplay, //regular state: player moves, attacks, can perform actions
	Pause, //pause menu is opened, the whole game world is frozen
	Inventory, //when inventory UI open
	Dialogue,
	LocationTransition	, // LocationExit trigger, fade to black begins and control is removed
	Cutscene,
}

//[CreateAssetMenu(fileName = "GameState", menuName = "Gameplay/GameState", order = 51)]
public class GameStateSO : DescriptionBaseSO
{
	public GameState CurrentGameState => _currentGameState;
	
	[Header("Game states")]
	[SerializeField][ReadOnly] private GameState _currentGameState = default;
	[SerializeField][ReadOnly] private GameState _previousGameState = default;

	[Header("Broadcasting on")]
	[SerializeField] private BoolEventChannelSO _onCombatStateEvent = default;
	
	private List<Transform> _alertEnemies;

	private void Start()
	{
		_alertEnemies = new List<Transform>();
	}

	public void RemoveAlertEnemy(Transform enemy)
	{
		if (_alertEnemies.Contains(enemy))
		{
			_alertEnemies.Remove(enemy);

			if (_alertEnemies.Count == 0)
			{
				UpdateGameState(GameState.Gameplay);
			}
		}
	}

	public void UpdateGameState(GameState newGameState)
	{
		if (newGameState == CurrentGameState)
			return;


		_previousGameState = _currentGameState;
		_currentGameState = newGameState;
	}

	public void ResetToPreviousGameState()
	{
		if (_previousGameState == _currentGameState)
			return;
		
		GameState stateToReturnTo = _previousGameState;
		_previousGameState = _currentGameState;
		_currentGameState = stateToReturnTo;
	}
}
