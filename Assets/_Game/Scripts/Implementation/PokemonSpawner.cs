using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonSpawner : IPokemonSpawner
{
    private Pokemon _pokemonPrefab;
    private GameObject _obstaclePrefab;
    
    private readonly float _dropDuration = 0.5f;
    private readonly float _dropHeight = 3f;
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
        foreach (var type in pokemonTypes)
        {
            if (Enum.TryParse(type.typeId, out MapCellType mapCellType))
            {
                if (!_pokemonTypeMap.ContainsKey(mapCellType))
                {
                    _pokemonTypeMap[mapCellType] = type;
                }
                else
                {
                    Debug.LogWarning($"Duplicate Pokemon ID '{type.typeId}' found");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid typeId for '{type.typeName}'");
            }
        }
    }

    public void SetPrefabs(Pokemon pokemonPrefab, GameObject obstaclePrefab)
    {
        _pokemonPrefab = pokemonPrefab;
        _obstaclePrefab = obstaclePrefab;
    }
    
    public void SetPokemonSprite(Pokemon pokemon, PokemonType type)
    {
        if (pokemon == null)
        {
            Debug.LogError("[PokemonSpawner] Active Pokemon is null.");
            return;
        }
        if (type == null)
        {
            Debug.LogError("[PokemonSpawner] PokemonType is null.");
            return;
        }
        pokemon.OnInit(type);
    }

    public void SpawnPokemonWithDrop(Vector3 targetPosition, Transform parent)
    {
        Vector3 startPosition = targetPosition + Vector3.up * _dropHeight;
        UnityEngine.Object.Instantiate(_pokemonPrefab.gameObject,
            startPosition,
            Quaternion.identity,
            parent);
    }

    private IEnumerator DropPokemonRoutine(Vector3 targetPosition, Transform parent)
    {
        Vector3 startPosition = targetPosition + Vector3.up * _dropHeight;
        Pokemon pokemon = UnityEngine.Object.Instantiate(_pokemonPrefab,
            startPosition,
            Quaternion.identity,
            parent);
        float elapsedTime = 0f;
        while (elapsedTime < _dropDuration)
        {
            pokemon.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / _dropDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        pokemon.transform.position = targetPosition;
    }
}