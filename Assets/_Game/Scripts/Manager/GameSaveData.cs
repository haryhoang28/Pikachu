using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int gridWidth;
    public int gridHeight;
    public List<MapCellType> cellTypes; // Flattened 2D array

    public GameSaveData()
    {
        cellTypes = new List<MapCellType>();
    }
}