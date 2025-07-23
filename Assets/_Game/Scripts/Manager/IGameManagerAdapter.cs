public interface IGameManagerAdapter
    {
    bool IsGameState(GameState gameState);
    void ChangeGameState(GameState newState);
    }
