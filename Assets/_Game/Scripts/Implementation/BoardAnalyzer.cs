using System.Collections.Generic;
using UnityEngine;

public class BoardAnalyzer : IBoardAnalyzer
{
    private readonly IGridManager _gridManager;
    private readonly IMatchFinder _matchFinder;

    public BoardAnalyzer(IGridManager gridManager, IMatchFinder matchFinder)
    {
        _gridManager = gridManager;
        _matchFinder = matchFinder;
    }

    public bool CheckEvenPokemonTypeCount()
    {
        Dictionary<PokemonType, int> typeCounts = new Dictionary<PokemonType, int>();
        for (int i = 1; i < _gridManager.GridWidth - 1; i++)
        {
            for (int j = 1; j < _gridManager.GridHeight - 1; j++)
            {
                Pokemon pokemon = _gridManager.GetPokemonAt(new Vector2Int(i, j));
                if (pokemon != null)
                {
                    if (typeCounts.ContainsKey(pokemon.Type))
                    {
                        typeCounts[pokemon.Type]++;
                    }
                    else
                    {
                        typeCounts.Add(pokemon.Type, 1);
                    }
                }
            }
            foreach (var entry in typeCounts)
            {
                if (entry.Value % 2 != 0)
                {
                    Debug.LogWarning($"[BoardAnalyzer] Pokemon Type '{entry.Key.typeName}' has an odd count: {entry.Value}.");
                    return false; // Tìm thấy một loại có số lượng lẻ
                }
            }
        }
        return true;
    }

    public bool HasPossibleMatches()
    {
        List<Vector2Int> pokemonPositions = new List<Vector2Int>();

        for (int x = 1; x < _gridManager.GridWidth - 1; x++)
        {
            for (int y = 1; y < _gridManager.GridHeight - 1; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                if (_gridManager.GetPokemonAt(currentPos) != null)
                {
                    pokemonPositions.Add(currentPos);
                }
            }
        }

        for (int i = 0; i < pokemonPositions.Count; i++)
        {
            for (int j = i + 1; j < pokemonPositions.Count; j++)
            {
                Vector2Int pos1 = pokemonPositions[i];
                Vector2Int pos2 = pokemonPositions[j];

                if (pos1 == pos2) continue;

                Pokemon p1 = _gridManager.GetPokemonAt(pos1);
                Pokemon p2 = _gridManager.GetPokemonAt(pos2);

                // Only try to find match if both positions have Pokemon and they are of the same type
                if (p1 != null && p2 != null && p1.Type == p2.Type)
                {
                    if (_matchFinder.TryFindMatch(pos1, pos2, out _, out _))
                    {
                        Debug.Log($"[BoardAnalyzer] Found a possible match between {pos1} and {pos2}.");
                        return true;
                    }
                }
            }
        }

        Debug.Log("[BoardAnalyzer] No possible matches found on the board. Board is stuck.");
        return false;
    }

}
