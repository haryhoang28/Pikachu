using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Match2 : MonoBehaviour
{
    [Header("Grid Settings")]
    private readonly float _cellSize = 1f;
    private readonly Vector3 _origin = Vector3.zero;


    [Header("Pokemon Settings")]
    [SerializeField] private Pokemon _pokemonPrefab;
    [SerializeField] private PokemonType[] _pokemonTypes;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private float _pokemonDespawnDuration = 0.5f;
    [SerializeField] private int _innerGridWidth = 8;
    [SerializeField] private int _innerGridHeight = 8;

    [Header("Hint Booster Settings")] 
    [SerializeField] private float _hintHighlightDuration = 0.5f;
    [SerializeField] private Color _hintColor = Color.green;
    [SerializeField] private int _maxHintPerGame = 3;
    private int _hintRemaining;
    public static event Action<int, int> OnHintCountChanged;

    [Header("Shuffle Booster Settings")]
    [SerializeField] private int _maxShufflePerGame = 3;
    private int _shuffleRemaining;
    public static event Action<int, int> OnShuffleCountChanged;
    
    
    private IGridManager _gridManager;
    private IMatchFinder _matchFinder;
    private IInputController _inputController;
    private IPokemonSpawner _pokemonSpawner;
    private IBoardAnalyzer _boardAnalyzer;
    private IBoardShuffler _boardShuffler;

    public static event Action<int, int, float, Vector3> OnGridSystemReady;

    public int MaxHintPerGame => _maxHintPerGame;
    public int CurrentHintCount => _hintRemaining;
    
    public int MaxShufflePerGame => _maxShufflePerGame;
    public int CurrentShuffleCount => _shuffleRemaining;

    private void Awake()
    {
        // Initialize components
        _gridManager = new GridManager(_cellSize, _origin, _innerGridWidth, _innerGridHeight);
        _pokemonSpawner = new PokemonSpawner();
        _pokemonSpawner.SetPrefabs(_pokemonPrefab, _obstaclePrefab);
        _matchFinder = new MatchFinder(_gridManager);
        _boardAnalyzer = new BoardAnalyzer(_gridManager, _matchFinder); 
        _boardShuffler = new BoardShuffler(_gridManager, _pokemonSpawner, _boardAnalyzer);
        _inputController = new InputController(_gridManager, _matchFinder);

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

    public void InitializeGame()
    {
        ClearGame();

        _pokemonSpawner.Initialize(_pokemonTypes);
        MapCellType[,] randomMapLayout = GenerateRandomMapLayout(_innerGridWidth, _innerGridHeight, _pokemonTypes);
        _gridManager.InitializeGrid(randomMapLayout, _pokemonSpawner.PokemonTypeMap, _obstaclePrefab, _pokemonPrefab, transform, _pokemonSpawner);
        if (!_boardAnalyzer.CheckEvenPokemonTypeCount())
        {
            Debug.LogWarning("[Match2] Generated map does not have even counts for all Pokemon types. Reshuffling or regenerating recommended.");
        }

        OnGridSystemReady?.Invoke(_gridManager.InnerGridWidth, _gridManager.InnerGridHeight, _gridManager.CellSize, _gridManager.Origin);
        _inputController.EnableInput();
        _hintRemaining = _maxHintPerGame;
        _shuffleRemaining = _maxShufflePerGame;
        Debug.Log($"[Match2] Game initialized. Hints available: {_hintRemaining}");

        OnHintCountChanged?.Invoke(_hintRemaining, _maxHintPerGame);
        OnShuffleCountChanged?.Invoke(_shuffleRemaining, _maxShufflePerGame);
    }
    public void ClearGame()
    {
        DisableInput();
        _gridManager.ClearAllPokemonsAndObstacles(transform);
    }
    private void DisableInput() => _inputController.DisableInput();

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
        _boardShuffler.ShuffleList(pokemonsToPlace);
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
                        layout[x, y] = MapCellType.Empty; // If cannot parse, set to Empty
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

    private void ShuffleBoard()
    {
        Debug.Log("[Match2] ShuffleBoard called. Disabling input temporarily.");
        _inputController.DisableInput();
        StartCoroutine(_boardShuffler.ShuffleBoardRoutine(transform));
    }

    private void HandleNoMatchFound()
    {
        //StartCoroutine(CheckForShuffleAfterDelay(0.1f)); 
    }

    private void HandlePokemonMatched(Pokemon pokemon1, Pokemon pokemon2)
    {
        GameManager.Instance.IncreaseScore();

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

    
    private IEnumerator CheckForShuffleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance.IsState(GameState.GamePlay))
        {
            if (_gridManager.GetActivePokemonCount() == 0)
            {
                Debug.Log("[Match2] All Pokemon cleared. Notifying GameManager for level complete!");
                GameManager.Instance.EndGame(true);
                yield break;
            }

            if (!_boardAnalyzer.HasPossibleMatches())
            {
                Debug.Log("[Match2] No possible matches left. Initiating shuffle!");
                GameManager.ChangeState(GameState.Shuffling);
                yield return StartCoroutine(_boardShuffler.ShuffleBoardRoutine(transform));
                GameManager.ChangeState(GameState.GamePlay);
                Debug.Log("[Match2] Shuffle complete. GamePlay state resumed.");
                _inputController.EnableInput();
                // SAU KHI XÁO TRỘN, kiểm tra lại. Nếu vẫn không có nước đi, GAME OVER
                if (!_boardAnalyzer.HasPossibleMatches())
                {
                    Debug.LogWarning("[Match2] No possible matches left AFTER SHUFFLE. Game Over!");
                    GameManager.Instance.EndGame(false);
                }
            }
        }
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

    public void OnHintButtonClicked()
    {
        if (GameManager.Instance.IsState(GameState.GamePlay) && _hintRemaining > 0)
        {
            Debug.Log("[Match2] Hint button clicked!");

            // Tìm một cặp Pokemon có thể ăn được
            (Vector2Int pos1, Vector2Int pos2)? hint = _boardAnalyzer.FindHintMatch();

            if (hint.HasValue)
            {
                _hintRemaining--; 
                OnHintCountChanged?.Invoke(_hintRemaining, _maxHintPerGame);
                Debug.Log($"[Match2] Hint provided for positions: {hint.Value.pos1} and {hint.Value.pos2}. Hints remaining: {_hintRemaining}");

                Pokemon p1 = _gridManager.GetPokemonAt(hint.Value.pos1);
                Pokemon p2 = _gridManager.GetPokemonAt(hint.Value.pos2);

                if (p1 != null)
                {
                    p1.Highlight(_hintHighlightDuration, _hintColor);
                }
                else
                {
                    Debug.LogWarning($"[Match2] Pokemon at {hint.Value.pos1} is null after FindHintMatch returned it. This shouldn't happen.");
                }

                if (p2 != null)
                {
                    p2.Highlight(_hintHighlightDuration, _hintColor);
                }
                else
                {
                    Debug.LogWarning($"[Match2] Pokemon at {hint.Value.pos2} is null after FindHintMatch returned it. This shouldn't happen.");
                }

                // Deactivate input so that player can focus on hint 
                StartCoroutine(DisableInputTemporarily(_hintHighlightDuration));
                OnHintCountChanged?.Invoke(_hintRemaining, _maxHintPerGame);

            }
            else
            {
                Debug.Log("[Match2] No possible hint found at this time. Board might need shuffling or game is over.");
            }
        }
        else if (_hintRemaining <= 0)
        {
            Debug.Log("[Match2] No hints left for this game!");
        }
    }

    public void OnShuffleButtonClicked()
    {
        if (GameManager.Instance.IsState(GameState.GamePlay) && _shuffleRemaining > 0)
        {
            Debug.Log("[Match2] Shuffle button clicked!");
            _shuffleRemaining--;
            OnShuffleCountChanged?.Invoke(_shuffleRemaining, _maxShufflePerGame);
            ShuffleBoard();
            StartCoroutine(DisableInputTemporarily(0.1f));
        }
        else if (_shuffleRemaining <= 0)
        {
            Debug.Log("[Match2] No shuffles left for this game!");
        }
    }
    
    private IEnumerator DisableInputTemporarily(float duration)
    {
        _inputController.DisableInput();
        yield return new WaitForSeconds(duration);
        if (GameManager.Instance.IsState(GameState.GamePlay))
        {
            _inputController.EnableInput();
        }
    }
}
