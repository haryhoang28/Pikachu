using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : IGridManager
{
    [Header("Grid Settings")]
    private Pokemon[,] _pokemonArray;
    private float _cellSize;
    private Vector3 _origin;
    private MapCellType[,] _currentMapLayout;
    private int _activePokemonCount;

    public int GridWidth {  get; private set; }

    public int GridHeight { get; private set; }

    public int InnerGridWidth { get; private set; }

    public int InnerGridHeight { get; private set; }

    public float CellSize => _cellSize;

    public Vector3 Origin => _origin;
    public int GetActivePokemonCount() => _activePokemonCount;

    public GridManager(float cellSize, Vector3 origin, int innerWidth, int innerHeight)
    {
        _cellSize = cellSize;
        _origin = origin;
        InnerGridWidth = innerWidth;
        InnerGridHeight = innerHeight;
        GridWidth = InnerGridWidth + 2;
        GridHeight = InnerGridHeight + 2;
        _currentMapLayout = new MapCellType[GridWidth, GridHeight]; 
        _pokemonArray = new Pokemon[GridWidth, GridHeight];
        _activePokemonCount = 0;
    }

    public void ClearAllPokemonsAndObstacles(Transform parentTransform)
    {
        if (_pokemonArray == null) return;

        // Clear references in _pokemonArray
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (_pokemonArray[x, y] != null)
                {
                    _pokemonArray[x, y].OnDespawn(); // Assuming Pokemon has this method for visual effect
                    UnityEngine.Object.Destroy(_pokemonArray[x, y].gameObject);
                    _pokemonArray[x, y] = null;
                }
            }
        }
        // Destroy any remaining obstacles in scene
        // This assumes obstacles are direct children of parentTransform and are not in _pokemonArray
        List<GameObject> objectsToDestroy = new List<GameObject>();
        foreach (Transform child in parentTransform)
        {
            if (child.GetComponent<Pokemon>() == null && child.CompareTag("Obstacle")) // Ensure it's not a Pokemon and is an obstacle
            {
                objectsToDestroy.Add(child.gameObject);
            }
        }
        foreach (GameObject obj in objectsToDestroy)
        {
            UnityEngine.Object.Destroy(obj);
        }
        _activePokemonCount = 0;
    }

    public void ClearPokemonAt(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
        {
            Debug.LogError($"[GridManager] Attempted to clear Pokemon at invalid grid position: {gridPosition}");
            return;
        }
        if (_pokemonArray[gridPosition.x, gridPosition.y] != null)
        {
            _pokemonArray[gridPosition.x, gridPosition.y] = null;
            _activePokemonCount--; // Giảm số lượng Pokemon đang hoạt động
            Debug.LogError(_activePokemonCount);
            Debug.Log($"[GridManager] Pokemon cleared at {gridPosition}. Active Pokemon count: {_activePokemonCount}");
        }
    }

    public Pokemon GetPokemonAt(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition)) return null;
        return _pokemonArray[gridPosition.x, gridPosition.y];
    }

    public void SetPokemonAt(Vector2Int gridPosition, Pokemon pokemon)
    {
        if (!IsValidGridPosition(gridPosition))
        {
            Debug.LogError($"[GridManager] Attempted to set Pokemon at invalid grid position: {gridPosition}");
            return;
        }
        _pokemonArray[gridPosition.x, gridPosition.y] = pokemon;
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < GridWidth &&
               gridPosition.y >= 0 && gridPosition.y < GridHeight;
    }

    public Vector2Int GetPokemonGridPosition(Pokemon pokemon)
    {
        if (pokemon == null) return new Vector2Int(-1, -1);
        Vector2Int gridPos = WorldToGrid(pokemon.transform.position);
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogError($"[GridManager] Pokemon's world position leads to an out-of-bounds grid position: {gridPos}. World Pos: {pokemon.transform.position}");
            return new Vector2Int(-1, -1);
        }
        return gridPos;
    }

    public Vector3 GetWorldPositionCenter(int x, int y)
    {
        return new Vector3(x * _cellSize + _cellSize * 0.5f, y * _cellSize + _cellSize * 0.5f, 0) + _origin;
    }

    public void InitializeGrid(MapCellType[,] fixedMapLayout, Dictionary<MapCellType, PokemonType> pokemonTypeMap, GameObject obstaclePrefab, Pokemon pokemonPrefab, Transform parentTransform, IPokemonSpawner pokemonSpawner)
    {
        // Xóa tất cả các đối tượng Pokemon hiện có trên sân khấu để tránh trùng lặp
        ClearExistingPokemonObjects(parentTransform); // Truyền parentTransform nếu cần Destroy con
        // Điền dữ liệu cho toàn bộ mảng _pokemonArray, bao gồm cả các ô biên
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                
                // Khởi tạo các ô biên (x=0, x=GridWidth-1, y=0, y=GridHeight-1)
                if (x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1)
                {
                    _pokemonArray[x, y] = null; // Các ô biên không chứa Pokemon
                    _currentMapLayout[x, y] = MapCellType.Empty;
                    // TODO: Tạo các đối tượng Block cho biên nếu cần hiển thị
                }
                else
                {
                    // Ánh xạ tọa độ từ fixedMapLayout (0-indexed) sang mảng nội bộ (1-indexed)
                    MapCellType cellType = fixedMapLayout[x - 1, y - 1];
                    _currentMapLayout[x, y] = cellType; // Lưu layout cho các lần kiểm tra sau

                    if ((int)cellType >= 1 && (int)cellType <= 10) // Đây là loại Pokemon (giá trị enum 1-10)
                    {
                        if (pokemonTypeMap.TryGetValue(cellType, out PokemonType pokemonType))
                        {
                            // Tạo đối tượng Pokemon và lưu tham chiếu của nó
                            Pokemon newPokemon = pokemonSpawner.CreatePokemonAt(x, y, pokemonType, parentTransform, this);
                            _pokemonArray[x, y] = newPokemon; // Đặt Pokemon vào mảng
                        }
                        else
                        {
                            Debug.LogWarning($"[GridManager] No PokemonType found for MapCellType: {cellType} at ({x},{y}). Setting to null.");
                            _pokemonArray[x, y] = null;
                        }
                    }
                    else if (cellType == MapCellType.Block)
                    {
                        // Xử lý việc tạo chướng ngại vật (nếu bạn có prefab chướng ngại vật)
                        if (obstaclePrefab != null)
                        {
                            // Ví dụ: GameObject obstacle = GameObject.Instantiate(obstaclePrefab, GetWorldPositionCenter(x, y), Quaternion.identity, parentTransform);
                        }
                        _pokemonArray[x, y] = null; 
                    }
                    else 
                    {
                        _pokemonArray[x, y] = null;
                    }
                }
            }
        }

        // *** Sau khi InitializeGrid hoàn tất việc điền _pokemonArray, chúng ta sẽ tính lại số lượng***
        RecalculateActivePokemonCount();
        Debug.Log($"[GridManager] Grid initialized. Total active Pokemon (recalculated): {_activePokemonCount}");
    }

    private void RecalculateActivePokemonCount()
    {
        int count = 0;
        // Duyệt qua phần lưới bên trong (không bao gồm các ô biên)
        for (int x = 1; x < GridWidth - 1; x++)
        {
            for (int y = 1; y < GridHeight - 1; y++)
            {
                // Kiểm tra không null, có GameObject và đang active (nếu Pokemon bị tắt)
                if (_pokemonArray[x, y] != null && _pokemonArray[x, y].gameObject != null && _pokemonArray[x, y].gameObject.activeSelf)
                {
                    count++;
                }
            }
        }
        _activePokemonCount = count;
        Debug.Log($"[GridManager] Recalculated active Pokemon count: {_activePokemonCount}");
    }

    public void ClearExistingPokemonObjects(Transform parentTransform)
    {
        if (_pokemonArray == null) return;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (_pokemonArray[x, y] != null)
                {
                    if (_pokemonArray[x, y].gameObject != null)
                    {
                        GameObject.Destroy(_pokemonArray[x, y].gameObject);
                    }
                    _pokemonArray[x, y] = null;
                }
            }
        }
    }
    public bool IsObstacleInInnerGrid(int x, int y)
    {
        if (_currentMapLayout == null)
        {
            Debug.LogWarning("[GridManager] _fixedMapLayout is null. Cannot check for obstacles.");
            return false;
        }

        int innerX = x - 1;
        int innerY = y - 1;
        if (innerX >= 0 && innerX < InnerGridWidth && innerY >= 0 && innerY < InnerGridHeight)
        {
            return (int)_currentMapLayout[innerX, innerY] >= 1000;
        }
        return false;
    }

    public bool IsPointValidAndClear(Vector2Int point, Vector2Int pos1, Vector2Int pos2)
    {
        if (!IsValidGridPosition(point)) return false;
        if (point == pos1 || point == pos2) return true;

        bool isVirtualBorderCell =
            (point.x == 0 || point.x == GridWidth - 1 ||
             point.y == 0 || point.y == GridHeight - 1);

        if (isVirtualBorderCell) return true;

        return _pokemonArray[point.x, point.y] == null && !IsObstacleInInnerGrid(point.x, point.y);
    }

    public void LoadGridData(List<MapCellType> loadedCellTypes, Dictionary<MapCellType, PokemonType> pokemonTypeMap, GameObject obstaclePrefab, Pokemon pokemonPrefab, Transform parentTransform, IPokemonSpawner pokemonSpawner)
    {
        ClearAllPokemonsAndObstacles(parentTransform);

        // Grid dimensions should already be set by Match2's load logic or constructor
        _pokemonArray = new Pokemon[GridWidth, GridHeight];

        pokemonSpawner.SetPrefabs(pokemonPrefab, obstaclePrefab); // Ensure spawner has prefabs
        pokemonSpawner.Initialize(pokemonTypeMap.Values.ToArray()); // Pass array of PokemonTypes

        int index = 0;
        for (int y = 0; y < InnerGridHeight; y++) // Iterate through inner grid only based on loaded data
        {
            for (int x = 0; x < InnerGridWidth; x++)
            {
                MapCellType cellType = loadedCellTypes[index];
                int mappedX = x + 1; // Adjust for border
                int mappedY = y + 1; // Adjust for border

                if ((int)cellType >= 1000)
                {
                    pokemonSpawner.CreateObstacleAt(mappedX, mappedY, parentTransform, this);
                }
                else if ((int)cellType >= 1 && (int)cellType <= 10)
                {
                    if (pokemonTypeMap.TryGetValue(cellType, out PokemonType pokemonType))
                    {
                        pokemonSpawner.CreatePokemonAt(mappedX, mappedY, pokemonType, parentTransform, this);
                    }
                }
                index++;
            }
        }
        // Fill border cells with null
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if ((x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1) && _pokemonArray[x, y] == null)
                {
                    _pokemonArray[x, y] = null;
                }
            }
        }
    }
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 gridPosition = (worldPosition - _origin) / _cellSize;
        int x = Mathf.FloorToInt(gridPosition.x);
        int y = Mathf.FloorToInt(gridPosition.y);
        return new Vector2Int(x, y);
    }

    
}
