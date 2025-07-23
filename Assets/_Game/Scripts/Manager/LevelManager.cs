using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private string _savePath;

    private void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "match2_save.json");
        Debug.Log($"LevelManager Save Path: {_savePath}");

    }
    /// <summary>
    /// Lưu trạng thái hiện tại của lưới game vào file JSON.
    /// </summary>
    /// <param name="pokemonArray">Mảng Pokemon 2 chiều hiện tại trên lưới.</param>
    /// <param name="gridWidth">Chiều rộng của lưới.</param>
    /// <param name="gridHeight">Chiều cao của lưới.</param>
    /// <param name="fixedLayout">Layout map cố định, dùng để xác định chướng ngại vật khi lưu.</param>
    /// <param name="pokemonTypeMap">Dictionary ánh xạ từ MapCellType sang PokemonType để chuyển đổi typeId.</param>
    public void SaveGame(Pokemon[,] pokemonArray, int gridWidth, int gridHeight, MapCellType[,] fixedLayout, Dictionary<MapCellType, PokemonType> pokemonTypeMap)
    {
        GameSaveData data = new GameSaveData
        {
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            cellTypes = new List<MapCellType>(gridWidth * gridHeight)
        };

        // Gán kích thước mảng 2D cho fixedLayout. (Lưu ý: Bạn có thể cần truyền thêm nó vào nếu kích thước FIXED_MAP_LAYOUT có thể thay đổi động)
        int fixedLayoutWidth = fixedLayout.GetLength(0);
        int fixedLayoutHeight = fixedLayout.GetLength(1);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Pokemon pokemon = pokemonArray[x, y];
                if (pokemon != null)
                {
                    // Lấy MapCellType từ PokemonType.typeId
                    if (Enum.TryParse(pokemon.Type.typeId, out MapCellType cellType))
                    {
                        data.cellTypes.Add(cellType);
                    }
                    else
                    {
                        Debug.LogWarning($"Pokemon TypeId '{pokemon.Type.typeId}' at ({x},{y}) does not map to a MapCellType enum. Saving as Empty.");
                        data.cellTypes.Add(MapCellType.Empty);
                    }
                }
                else
                {
                    // Nếu ô trống (không có Pokemon), kiểm tra xem nó có phải là chướng ngại vật cố định không
                    // Đảm bảo rằng tọa độ x, y hợp lệ cho cả pokemonArray và fixedLayout
                    if (x < fixedLayoutWidth && y < fixedLayoutHeight)
                    {
                        MapCellType originalCellType = fixedLayout[x, y];
                        if ((int)originalCellType >= 1000) // Là chướng ngại vật
                        {
                            data.cellTypes.Add(originalCellType);
                        }
                        else
                        {
                            data.cellTypes.Add(MapCellType.Empty); // Ô trống hoàn toàn
                        }
                    }
                    else
                    {
                        // Trường hợp lưới hiện tại lớn hơn FIXED_MAP_LAYOUT, mặc định là Empty
                        data.cellTypes.Add(MapCellType.Empty);
                    }
                }
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        Debug.Log("Game Saved!");
    }

    /// <summary>
    /// Tải dữ liệu game từ file JSON.
    /// </summary>
    /// <param name="gridWidth">Output: Chiều rộng của lưới được tải.</param>
    /// <param name="gridHeight">Output: Chiều cao của lưới được tải.</param>
    /// <param name="cellTypes">Output: Danh sách các MapCellType của các ô trong lưới.</param>
    /// <returns>True nếu tải thành công, ngược lại False.</returns>
    public bool LoadGame(out int gridWidth, out int gridHeight, out List<MapCellType> cellTypes)
    {
        gridWidth = 0;
        gridHeight = 0;
        cellTypes = null;

        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

            gridWidth = data.gridWidth;
            gridHeight = data.gridHeight;
            cellTypes = data.cellTypes;

            Debug.Log("Game Loaded!");
            return true;
        }
        else
        {
            Debug.LogWarning("No save file found at: " + _savePath);
            return false;
        }
    }
}
