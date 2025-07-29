using System.Collections.Generic;
using UnityEngine; 

public interface IGridManager
{
    int GridWidth { get; }
    int GridHeight { get; }
    int InnerGridWidth { get; }
    int InnerGridHeight { get; }
    float CellSize { get; }
    Vector3 Origin { get; }

    Pokemon GetPokemonAt(Vector2Int gridPosition);
    void SetPokemonAt(Vector2Int gridPosition, Pokemon pokemon);
    void ClearPokemonAt(Vector2Int gridPosition); 

    Vector3 GetWorldPositionCenter(int x, int y);
    Vector2Int WorldToGrid(Vector3 worldPosition);
    Vector2Int GetPokemonGridPosition(Pokemon pokemon);
    bool IsObstacleInInnerGrid(int x, int y);
    bool IsPointValidAndClear(Vector2Int point, Vector2Int pos1, Vector2Int pos2);

    void InitializeGrid(MapCellType[,] fixedMapLayout, Dictionary<MapCellType, PokemonType> pokemonTypeMap, GameObject obstaclePrefab, Pokemon pokemonPrefab, Transform parentTransform, IPokemonSpawner pokemonSpawner);
    void LoadGridData(List<MapCellType> loadedCellTypes, Dictionary<MapCellType, PokemonType> pokemonTypeMap, GameObject obstaclePrefab, Pokemon pokemonPrefab, Transform parentTransform, IPokemonSpawner pokemonSpawner);
    void ClearAllPokemonsAndObstacles(Transform parentTransform);

    int GetActivePokemonCount();
}
