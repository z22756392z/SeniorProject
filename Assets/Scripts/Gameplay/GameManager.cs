using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameStateSO _gameState = default;

	[Header("Inventory")]
	[SerializeField] private InventorySO _inventory = default;

	private void Start()
	{
		StartGame();
	}

	void StartGame()
	{
		_gameState.UpdateGameState(GameState.Gameplay);
	}
}
