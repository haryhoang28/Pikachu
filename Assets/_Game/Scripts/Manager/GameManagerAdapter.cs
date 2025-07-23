using System;
using UnityEngine;

public class GameManagerAdapter : IGameManagerAdapter
{
    public void ChangeGameState(GameState newState)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[GameManagerAdapter] GameManager.Instance is null. Cannot change game state.");
            return;
        }
        GameManager.ChangeState(newState);
    }

    public bool IsGameState(GameState gameState)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[GameManagerAdapter] GameManager.Instance is null. Assuming not in GamePlay state.");
            return false;
        }
        return GameManager.Instance.isState(gameState);
    }
    
}