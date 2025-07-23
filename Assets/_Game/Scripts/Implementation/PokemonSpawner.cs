using System;
using System.Collections.Generic;
using UnityEngine;

public class PokemonSpawner : IPokemonSpawner
{
    private Pokemon _pokemonPrefab;
    private GameObject _obstaclePrefab;
    private Dictionary<MapCellType, PokemonType> _pokemonTypeMap;
    public Dictionary<MapCellType, PokemonType> PokemonTypeMap => _pokemonTypeMap;

    public void CreateObstacleAt(int x, int y, Transform parentTransform, IGridManager gridManager)
    {
        if (_obstaclePrefab == null)
        {
            Debug.LogError("[PokemonSpawner] Obstacle Prefab is not assigned.");
            return;
        }

        Vector3 worldPosition = gridManager.GetWorldPositionCenter(x, y);
        UnityEngine.Object.Instantiate(_obstaclePrefab, worldPosition, Quaternion.identity, parentTransform);
    }

    public Pokemon CreatePokemonAt(int x, int y, PokemonType type, Transform parentTransform, IGridManager gridManager)
    {
        if (_pokemonPrefab == null)
        {
            Debug.LogError("[PokemonSpawner] Pokemon Prefab is not assigned.");
            return null;
        }
        Vector3 worldPosition = gridManager.GetWorldPositionCenter(x, y);
        Pokemon pokemon = UnityEngine.Object.Instantiate(_pokemonPrefab, worldPosition, Quaternion.identity, parentTransform);
        pokemon.OnInit(type);
        gridManager.SetPokemonAt(new Vector2Int(x, y), pokemon);
        return pokemon;
    }

    public void Initialize(PokemonType[] pokemonTypes)
    {
        _pokemonTypeMap = new Dictionary<MapCellType, PokemonType>();
        foreach (var pokemonType in pokemonTypes)
        {
            if (Enum.TryParse(pokemonType.typeId, out MapCellType cellType))
            {
                if (_pokemonTypeMap.ContainsKey(cellType))
                {
                    Debug.LogWarning($"[PokemonSpawner] Duplicate Pokemon ID '{pokemonType.typeId}' found");
                }
                else
                {
                    _pokemonTypeMap.Add(cellType, pokemonType);
                }
            }
            else
            {
                Debug.LogWarning($"[PokemonSpawner] PokemonType '{pokemonType.typeName}' has an invalid or unparseable typeId");
            }
        }
    }

    public void SetPrefabs(Pokemon pokemonPrefab, GameObject obstaclePrefab)
    {
        _pokemonPrefab = pokemonPrefab;
        _obstaclePrefab = obstaclePrefab;
    }
}