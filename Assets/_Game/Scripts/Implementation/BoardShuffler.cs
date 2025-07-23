using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardShuffler : IBoardShuffler
{
    private readonly IGridManager _gridManager;
    private readonly IPokemonSpawner _pokemonSpawner;
    private readonly IBoardAnalyzer _boardAnalyzer; // Để kiểm tra sau khi shuffle

    public BoardShuffler(IGridManager gridManager, IPokemonSpawner pokemonSpawner, IBoardAnalyzer boardAnalyzer)
    {
        _gridManager = gridManager;
        _pokemonSpawner = pokemonSpawner;
        _boardAnalyzer = boardAnalyzer;
    }

    public IEnumerator ShuffleBoardRoutine(Transform parentTransform)
    {
        if (_gridManager.GetActivePokemonCount() == 0)
        {
            yield break;
        }
        Debug.Log("[BoardShuffler] Starting board shuffle...");

        List<PokemonType> existingPokemonTypes = new List<PokemonType>();
        for (int x = 1; x < _gridManager.GridWidth - 1; x++)
        {
            for (int y = 1; y < _gridManager.GridHeight - 1; y++)
            {
                Pokemon pokemon = _gridManager.GetPokemonAt(new Vector2Int(x, y));
                if (pokemon != null)
                {
                    existingPokemonTypes.Add(pokemon.Type);
                }
            }
        }

        _gridManager.ClearAllPokemonsAndObstacles(parentTransform);
        yield return new WaitForSeconds(0.1f); // Give a small visual pause/delay after clearing

        List<Vector2Int> emptyInnerPositions = new List<Vector2Int>();
        for (int x = 1; x < _gridManager.GridWidth - 1; x++)
        {
            for (int y = 1; y < _gridManager.GridHeight - 1; y++)
            {
                if (!_gridManager.IsObstacleInInnerGrid(x, y))
                {
                    emptyInnerPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (existingPokemonTypes.Count > emptyInnerPositions.Count)
        {
            Debug.LogError("[BoardShuffler] More Pokemon types than empty slots after clearing! Truncating.");
            // This case indicates an error in initial setup or obstacle clearing, or game design.
            // For now, we'll truncate the list of types.
            existingPokemonTypes = existingPokemonTypes.Take(emptyInnerPositions.Count).ToList();
        }
        else if (existingPokemonTypes.Count < emptyInnerPositions.Count)
        {
            Debug.LogWarning("[BoardShuffler] More empty slots than Pokemon types. Some slots will remain empty.");
        }


        bool boardIsSolvable = false;
        int maxShuffleAttempts = 100;
        int currentAttempt = 0;

        while (!boardIsSolvable && currentAttempt < maxShuffleAttempts)
        {
            currentAttempt++;
            ShuffleList(existingPokemonTypes); // Re-shuffle types

            // Temporarily populate the grid
            // Clear current _pokemonArray state without destroying GameObjects for this simulation
            for (int x = 0; x < _gridManager.GridWidth; x++)
            {
                for (int y = 0; y < _gridManager.GridHeight; y++)
                {
                    _gridManager.ClearPokemonAt(new Vector2Int(x, y));
                }
            }

            int typeIndex = 0;
            foreach (Vector2Int pos in emptyInnerPositions)
            {
                if (typeIndex < existingPokemonTypes.Count)
                {
                    _pokemonSpawner.CreatePokemonAt(pos.x, pos.y, existingPokemonTypes[typeIndex], parentTransform, _gridManager);
                    typeIndex++;
                }
            }

            boardIsSolvable = _boardAnalyzer.HasPossibleMatches();
            if (!boardIsSolvable && currentAttempt < maxShuffleAttempts)
            {
                Debug.Log($"[BoardShuffler] Board still stuck after shuffle attempt {currentAttempt}. Re-shuffling...");
                _gridManager.ClearAllPokemonsAndObstacles(parentTransform); // Clear physical objects to re-spawn
                yield return new WaitForSeconds(0.05f); // Small delay between attempts
            }
        }

        if (!boardIsSolvable)
        {
            Debug.LogError("[BoardShuffler] Failed to find a solvable board after multiple shuffles! Game might be unplayable.");
            // Potentially end game or force a new level.
        }

        Debug.Log("[BoardShuffler] Board shuffle complete.");
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[randomIndex];
            list[randomIndex] = list[i];
            list[i] = temp;
        }
    }
}