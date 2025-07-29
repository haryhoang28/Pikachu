using System.Numerics;
using UnityEngine;


public interface IBoardAnalyzer
{
    bool CheckEvenPokemonTypeCount();
    bool HasPossibleMatches();
    (Vector2Int, Vector2Int)? FindHintMatch();
}