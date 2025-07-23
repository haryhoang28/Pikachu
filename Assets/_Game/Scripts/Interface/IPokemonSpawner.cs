using System.Collections.Generic;
using UnityEngine;

public interface IPokemonSpawner
{
    Dictionary<MapCellType, PokemonType> PokemonTypeMap { get; } // In order to access from GridManager

    void SetPrefabs(Pokemon pokemonPrefab, GameObject obstaclePrefab);
    void Initialize(PokemonType[] pokemonTypes);
    Pokemon CreatePokemonAt(int x, int y, PokemonType type, Transform parentTransform, IGridManager gridManager);
    void CreateObstacleAt(int x, int y, Transform parentTransform, IGridManager gridManager);
}
