using System.Collections.Generic;
using UnityEngine;

public interface IMatchFinder
{
    bool TryFindMatch(Vector2Int tile1, Vector2Int tile2, out int bends, out List<Vector2Int> foundPath);
}