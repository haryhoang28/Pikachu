using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : IMatchFinder
{
    private readonly IGridManager _gridManager;

    public MatchFinder(IGridManager gridManager)
    {
        _gridManager = gridManager;
    }

    public bool TryFindMatch(Vector2Int tile1, Vector2Int tile2, out int bends, out List<Vector2Int> foundPath)
    {
        bends = 0;
        foundPath = new List<Vector2Int>();

        // Lấy Pokemon từ mảng lưới mở rộng
        if (tile1.x < 0 || tile1.x >= _gridManager.GridWidth || tile1.y < 0 || tile1.y >= _gridManager.GridHeight ||
            tile2.x < 0 || tile2.x >= _gridManager.GridWidth || tile2.y < 0 || tile2.y >= _gridManager.GridHeight)
        {
            Debug.LogError($"[MatchFinder] One or both input tiles ({tile1}, {tile2}) are out of the extended grid bounds.");
            return false;
        }

        Pokemon pokemon1 = _gridManager.GetPokemonAt(tile1);
        Pokemon pokemon2 = _gridManager.GetPokemonAt(tile2);

        if (pokemon1 == null || pokemon2 == null)
        {
            // Debug.LogWarning($"[MatchFinder] One or both selected positions ({tile1}, {tile2}) do not contain a Pokemon.");
            return false;
        }
        if (pokemon1.Type != pokemon2.Type)
        {
            // Debug.Log($"[MatchFinder] Pokemons are not of the same type: {pokemon1.Type.typeName} vs {pokemon2.Type.typeName}.");
            return false;
        }
        if (tile1 == tile2)
        {
            // Debug.Log($"[MatchFinder] Selected the same tile: {tile1}.");
            return false;
        }

        // Debug.Log($"[MatchFinder] Attempting to find path from {tile1} to {tile2} (Type: {pokemon1.Type.typeName})");
        bool matchFound = false;

        if (CheckLine(tile1, tile2, out foundPath))
        {
            bends = 0;
            matchFound = true;
            // Debug.Log($"[MatchFinder] Found 0-bend path. Path length: {foundPath.Count}");
        }
        else if (CheckOneBend(tile1, tile2, out foundPath))
        {
            bends = 1;
            matchFound = true;
            // Debug.Log($"[MatchFinder] Found 1-bend path. Path length: {foundPath.Count}");
        }
        else if (CheckTwoBend(tile1, tile2, out foundPath))
        {
            bends = 2;
            matchFound = true;
            // Debug.Log($"[MatchFinder] Found 2-bend path. Path length: {foundPath.Count}");
        }

        return matchFound;
    }

    private bool CheckLine(Vector2Int pos1, Vector2Int pos2, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        // Debug.Log($"[MatchFinder - CheckLine] Checking line from {pos1} to {pos2}");

        if (pos1.y == pos2.y) // Horizontal line
        {
            int step = (pos1.x < pos2.x) ? 1 : -1;
            for (int x = pos1.x + step; x != pos2.x; x += step)
            {
                Vector2Int currentPoint = new Vector2Int(x, pos1.y);
                if (!_gridManager.IsPointValidAndClear(currentPoint, pos1, pos2)) return false;
                path.Add(currentPoint);
            }
            return true;
        }
        if (pos1.x == pos2.x) // Vertical line
        {
            int step = (pos1.y < pos2.y) ? 1 : -1;
            for (int y = pos1.y + step; y != pos2.y; y += step)
            {
                Vector2Int currentPoint = new Vector2Int(pos1.x, y);
                if (!_gridManager.IsPointValidAndClear(currentPoint, pos1, pos2)) return false;
                path.Add(currentPoint);
            }
            return true;
        }
        return false;
    }

    private bool CheckOneBend(Vector2Int pos1, Vector2Int pos2, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();

        Vector2Int bendPoint1 = new Vector2Int(pos2.x, pos1.y);
        List<Vector2Int> segment1Path_b1, segment2Path_b1;

        if (_gridManager.IsPointValidAndClear(bendPoint1, pos1, pos2))
        {
            if (CheckLine(pos1, bendPoint1, out segment1Path_b1) && CheckLine(bendPoint1, pos2, out segment2Path_b1))
            {
                path.Add(pos1);
                AddRangeUnique(path, segment1Path_b1);
                if (!path.Contains(bendPoint1)) path.Add(bendPoint1);
                AddRangeUnique(path, segment2Path_b1);
                if (!path.Contains(pos2)) path.Add(pos2);
                return true;
            }
        }

        Vector2Int bendPoint2 = new Vector2Int(pos1.x, pos2.y);
        List<Vector2Int> segment1Path_b2, segment2Path_b2;

        if (_gridManager.IsPointValidAndClear(bendPoint2, pos1, pos2))
        {
            if (CheckLine(pos1, bendPoint2, out segment1Path_b2) && CheckLine(bendPoint2, pos2, out segment2Path_b2))
            {
                path.Add(pos1);
                AddRangeUnique(path, segment1Path_b2);
                if (!path.Contains(bendPoint2)) path.Add(bendPoint2);
                AddRangeUnique(path, segment2Path_b2);
                if (!path.Contains(pos2)) path.Add(pos2);
                return true;
            }
        }
        path = null;
        return false;
    }

    private bool CheckTwoBend(Vector2Int pos1, Vector2Int pos2, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        for (int x1 = 0; x1 < _gridManager.GridWidth; x1++)
        {
            for (int y1 = 0; y1 < _gridManager.GridHeight; y1++)
            {
                Vector2Int p1 = new Vector2Int(x1, y1);

                if (!_gridManager.IsPointValidAndClear(p1, pos1, pos2)) continue;

                List<Vector2Int> path_pos1_p1;
                if (CheckLine(pos1, p1, out path_pos1_p1))
                {
                    for (int x2 = 0; x2 < _gridManager.GridWidth; x2++)
                    {
                        for (int y2 = 0; y2 < _gridManager.GridHeight; y2++)
                        {
                            Vector2Int p2 = new Vector2Int(x2, y2);

                            if (!(_gridManager.IsPointValidAndClear(p2, pos1, pos2))) continue;

                            List<Vector2Int> path_p1_p2;
                            if (CheckLine(p1, p2, out path_p1_p2))
                            {
                                List<Vector2Int> path_p2_pos2;
                                if (CheckLine(p2, pos2, out path_p2_pos2))
                                {
                                    path.Clear();
                                    path.Add(pos1);
                                    AddRangeUnique(path, path_pos1_p1);
                                    if (!path.Contains(p1)) path.Add(p1);
                                    AddRangeUnique(path, path_p1_p2);
                                    if (!path.Contains(p2)) path.Add(p2);
                                    AddRangeUnique(path, path_p2_pos2);
                                    if (!path.Contains(pos2)) path.Add(pos2);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        path = null;
        return false;
    }

    private void AddRangeUnique(List<Vector2Int> targetList, List<Vector2Int> sourceList)
    {
        if (sourceList == null) return;
        foreach (var item in sourceList)
        {
            if (!targetList.Contains(item))
            {
                targetList.Add(item);
            }
        }
    }
}