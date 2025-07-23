using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using Random = UnityEngine.Random;


public class Match2 : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private Vector3 _origin = Vector3.zero;


    [Header("Pokemon Settings")]
    [SerializeField] private Pokemon _pokemonPrefab;
    [SerializeField] private PokemonType[] _pokemonTypes;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private float _pokemonDespawnDuration = 0.5f;
    [SerializeField] private int _innerGridWidth = 8;
    [SerializeField] private int _innerGridHeight = 8;
    
    [Header("Game End Settings")]
    [SerializeField] private float _gameDuration = 120f; // Tổng thời gian chơi (ví dụ: 120 giây)
    private float _currentTime; // Thời gian còn lại


    private IGridManager _gridManager;
    private IMatchFinder _matchFinder;
    private IInputController _inputController;
    private IPokemonSpawner _pokemonSpawner;
    private IGameManagerAdapter _gameManagerAdapter;
    private IBoardAnalyzer _boardAnalyzer;
    private IBoardShuffler _boardShuffler;

    private LevelManager _levelManager;
    public static event Action<int, int, float, Vector3> OnGridSystemReady;


    private void Awake()
    {
        _levelManager = FindObjectOfType<LevelManager>();
        if (_levelManager == null)
        {
            Debug.LogWarning("[Match2] LevelManager not found.");
        }

        // Initialize components
        // Pass FIXED_MAP_LAYOUT to GridManager for initial setup and obstacle check
        _gridManager = new GridManager(_cellSize, _origin, _innerGridWidth, _innerGridHeight);
        _pokemonSpawner = new PokemonSpawner();
        _pokemonSpawner.SetPrefabs(_pokemonPrefab, _obstaclePrefab);
        _gameManagerAdapter = new GameManagerAdapter();
        _matchFinder = new MatchFinder(_gridManager);
        _boardAnalyzer = new BoardAnalyzer(_gridManager, _matchFinder); 
        _boardShuffler = new BoardShuffler(_gridManager, _pokemonSpawner, _boardAnalyzer);

        _inputController = new InputController(_gridManager, _matchFinder, _gameManagerAdapter);

        // Subscribe to events from InputController
        if (_inputController is InputController concreteInputController)
        {
            concreteInputController.OnPathFoundForDebug += DrawDebugPath;
            concreteInputController.OnPokemonMatched += HandlePokemonMatched;
            concreteInputController.OnNoMatchFound += HandleNoMatchFound; // Subscribe to new event
        }
    }
    private void OnEnable()
    {
        GameManager.OnGameObjectClicked += _inputController.HandleClick;
    }
    private void OnDisable()
    {
        GameManager.OnGameObjectClicked -= _inputController.HandleClick;

        if (_inputController is InputController concreteInputController)
        {
            concreteInputController.OnPathFoundForDebug -= DrawDebugPath;
            concreteInputController.OnPokemonMatched -= HandlePokemonMatched;
            concreteInputController.OnNoMatchFound -= HandleNoMatchFound;
        }
    }

    private void Start()
    {
        _gameManagerAdapter.ChangeGameState(GameState.GamePlay);
        _pokemonSpawner.Initialize(_pokemonTypes);

        MapCellType[,] randomMapLayout = GenerateRandomMapLayout(_innerGridWidth, _innerGridHeight, _pokemonTypes);
        
        _gridManager.InitializeGrid(randomMapLayout, _pokemonSpawner.PokemonTypeMap, _obstaclePrefab, _pokemonPrefab, transform, _pokemonSpawner);

        if (!_boardAnalyzer.CheckEvenPokemonTypeCount())
        {
            Debug.LogWarning("[Match2] Generated map does not have even counts for all Pokemon types. Reshuffling or regenerating recommended.");
        }

        OnGridSystemReady?.Invoke(_gridManager.InnerGridWidth, _gridManager.InnerGridHeight, _gridManager.CellSize, _gridManager.Origin);
        _inputController.EnableInput();

        _currentTime = _gameDuration; // Khởi tạo bộ đếm thời gian
        
    }
    private void Update()
    {
        if (_gameManagerAdapter.IsGameState(GameState.GamePlay))
        {
            _currentTime -= Time.deltaTime;
            // TODO: Cập nhật UI hiển thị thời gian ở đây (nếu có)

            if (_currentTime <= 0f)
            {
                _currentTime = 0f; // Đảm bảo thời gian không âm
                EndGame(false); // Game Over do hết thời gian
            }
        }
    }
    private MapCellType[,] GenerateRandomMapLayout(int innerGridWidth, int innerGridHeight, PokemonType[] availablePokemonTypes)
    {
        MapCellType[,] layout = new MapCellType[innerGridWidth, innerGridHeight];
        List<PokemonType> pokemonsToPlace = new List<PokemonType>();

        // Lọc ra các MapCellType tương ứng với PokemonType có sẵn
        List<MapCellType> availableCellTypes = new List<MapCellType>();
        foreach (var pType in availablePokemonTypes)
        {
            if (Enum.TryParse(pType.typeId, out MapCellType cellType) && (int)cellType >= 1 && (int)cellType <= 10)
            {
                availableCellTypes.Add(cellType);
            }
        }

        if (availableCellTypes.Count == 0)
        {
            Debug.LogError("[Match2] No valid PokemonType found to generate random map!");
            return layout; // Trả về layout trống
        }

        int totalCells = innerGridWidth * innerGridHeight;
        int totalPokemonCount = totalCells;

        if (totalCells % 2 != 0)
        {
            totalPokemonCount--;
        }

        int pairsToGenerate = totalPokemonCount / 2;
        for (int i = 0; i < pairsToGenerate; i++)
        {
            PokemonType randomType = availablePokemonTypes[Random.Range(0, availablePokemonTypes.Length)];
            pokemonsToPlace.Add(randomType);
            pokemonsToPlace.Add(randomType); // Thêm cặp
        }
        ShuffleList(pokemonsToPlace);
        int pokemonIndex = 0;
        for (int x = 0; x < innerGridWidth; x++)
        {
            for (int y = 0; y < innerGridHeight; y++)
            {
                if (pokemonIndex < pokemonsToPlace.Count)
                {
                    if (Enum.TryParse(pokemonsToPlace[pokemonIndex].typeId, out MapCellType cellType))
                    {
                        layout[x, y] = cellType;
                    }
                    else
                    {
                        Debug.LogError($"[Match2] Invalid MapCellType for PokemonType: {pokemonsToPlace[pokemonIndex].typeId}");
                        layout[x, y] = MapCellType.Empty; // Mặc định là Empty nếu có lỗi
                    }
                    pokemonIndex++;
                }
                else
                {
                    layout[x, y] = MapCellType.Empty; // Ô còn lại là Empty nếu có
                }
            }
        }
        return layout;
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

    private void HandleNoMatchFound()
    {
        StartCoroutine(CheckForShuffleAfterDelay(0.1f)); 
    }

    private void HandlePokemonMatched(Pokemon pokemon1, Pokemon pokemon2)
    {
        Vector2Int pos1 = _gridManager.GetPokemonGridPosition(pokemon1);
        Vector2Int pos2 = _gridManager.GetPokemonGridPosition(pokemon2);
        pokemon1.OnDespawn();
        Destroy(pokemon1.gameObject, _pokemonDespawnDuration);
        _gridManager.ClearPokemonAt(pos1);


        pokemon2.OnDespawn();
        Destroy(pokemon2.gameObject, _pokemonDespawnDuration);
        _gridManager.ClearPokemonAt(pos2);

        StartCoroutine(CheckForShuffleAfterDelay(_pokemonDespawnDuration + 0.1f));
    }

    private IEnumerator HandleMatchCompletion(Vector2Int pos1, Vector2Int pos2)
    {
        yield return new WaitForSeconds(_pokemonDespawnDuration);

        // KIỂM TRA ĐIỀU KIỆN HOÀN THÀNH LEVEL: Nếu tất cả Pokemon đã được xóa
        if (_gridManager.GetActivePokemonCount() == 0)
        {
            EndGame(true); // Level Complete!
            yield break; // Thoát coroutine
        }

        // Sau khi xử lý khớp, kiểm tra xem còn nước đi nào không
        // (Nếu game vẫn chưa kết thúc)
        if (_gameManagerAdapter.IsGameState(GameState.GamePlay)) // Kiểm tra lại trạng thái
        {
            StartCoroutine(CheckForShuffleAfterDelay(0.1f)); // Đảm bảo độ trễ nhỏ sau khi xử lý khớp
        }

        _gameManagerAdapter.ChangeGameState(GameState.GamePlay); // Kích hoạt lại input
    }

    private IEnumerator CheckForShuffleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_gameManagerAdapter.IsGameState(GameState.GamePlay))
        {
            if (!_boardAnalyzer.HasPossibleMatches())
            {
                if (_gridManager.GetActivePokemonCount()==0)
                {
                    _gameManagerAdapter.ChangeGameState(GameState.Finish);
                }
                Debug.Log("[Match2] No possible matches left. Initiating shuffle!");
                _gameManagerAdapter.ChangeGameState(GameState.Shuffling);
                yield return StartCoroutine(_boardShuffler.ShuffleBoardRoutine(transform)); // Wait for shuffle to complete
                _gameManagerAdapter.ChangeGameState(GameState.GamePlay);
                Debug.Log("[Match2] Shuffle complete. GamePlay state resumed.");
                _inputController.EnableInput();

                // SAU KHI XÁO TRỘN, kiểm tra lại. Nếu vẫn không có nước đi, GAME OVER
                if (!_boardAnalyzer.HasPossibleMatches())
                {
                    Debug.LogWarning("[Match2] No possible matches left AFTER SHUFFLE. Game Over!");
                    EndGame(false); // Game Over do không còn nước đi
                }
            }
        }
    }
    private void EndGame(bool levelComplete)
    {
        // Tránh gọi lại EndGame nếu đã ở trạng thái kết thúc
        if (_gameManagerAdapter.IsGameState(GameState.Finish)) // Chỉ kiểm tra trạng thái Finish
        {
            return;
        }

        if (levelComplete)
        {
            Debug.Log("[Match2] Level Complete! All Pokemon cleared.");
            _gameManagerAdapter.ChangeGameState(GameState.Finish); // Sử dụng Finish cho level complete
            // TODO: Hiển thị UI "Level Complete" ở đây
        }
        else
        {
            Debug.Log("[Match2] Game Over!"); // Có thể do hết thời gian hoặc không còn nước đi
            _gameManagerAdapter.ChangeGameState(GameState.Finish); // Sử dụng Finish cho game over
            // TODO: Hiển thị UI "Game Over" ở đây
        }

        _inputController.DisableInput(); // Vô hiệu hóa input khi game kết thúc
    }

    private void DrawDebugPath(List<Vector2Int> matchedPath, Color color, float duration)
    {
        if (matchedPath == null || matchedPath.Count < 2) return;

        for (int i = 0; i < matchedPath.Count - 1; i++)
        {
            Vector3 start = _gridManager.GetWorldPositionCenter(matchedPath[i].x, matchedPath[i].y);
            Vector3 end = _gridManager.GetWorldPositionCenter(matchedPath[i + 1].x, matchedPath[i + 1].y);
            Debug.DrawLine(start, end, color, duration);
        }
        Debug.Log($"[Match2 Debug] Drawing path with {matchedPath.Count} points. Color: {color}, Duration: {duration}");
    }
}
 