using UnityEngine;
using System;

public enum GameState { MainMenu, GamePlay, Setting, Finish, Revive, Shuffling }
public class GameManager : Singleton<GameManager>
{
    private static GameState _currentGameState;
    public static event Action<GameObject> OnGameObjectClicked;

    [Header("Game Time Settings")]
    [SerializeField] private float _gameDuration = 120f;
    private float _currentTime;

    [Header("Score Settings")]
    [SerializeField] private int _scorePerMatch = 10;
    private int _currentScore;

    [SerializeField]private Match2 _match2;

    private UIManager _uiManager;

    public int GetCurrentHintCount => _match2 != null ? _match2.CurrentHintCount : 0;
    public int GetMaxHintCount => _match2 != null ? _match2.MaxHintPerGame : 0;
    
    public int GetCurrentShuffleCount => _match2 != null ? _match2.CurrentShuffleCount : 0;
    public int GetMaxShuffleCount => _match2 != null ? _match2.MaxShufflePerGame : 0;
    
    public void ClearGame() => _match2.ClearGame();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Input.multiTouchEnabled = false;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        int maxScreenHeight = 1280;

        float ratio = (float)Screen.currentResolution.width / (float)Screen.currentResolution.height;

        if (ratio > maxScreenHeight)
        {
            Screen.SetResolution(Mathf.RoundToInt(ratio * (float)maxScreenHeight), maxScreenHeight, true);
        }
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
        if (_uiManager == null)
        {
            return;
        }
        _uiManager.OpenUI<CanvasMainMenu>();
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        HandleInput();

        if (_currentGameState == GameState.GamePlay)
        {
            _currentTime -= Time.deltaTime;
            _currentTime = Mathf.Max(0f, _currentTime);

            // Update TimeBar UI
            if (_uiManager != null && _uiManager.IsUIOpened<CanvasGamePlay>())
            {
                _uiManager.GetUI<CanvasGamePlay>().UpdateTimeBar(_currentTime, _gameDuration);
            }

            // Check if it was run out of time
            if (_currentTime <= 0f)
            {
                EndGame(false); // Game Over due to time = 0
            }
        }
    }

    public void StartGame()
    {
        if (_match2 == null)
        {
            Debug.LogError("[GameManager] Match2 reference not set in GameManager Inspector! Cannot start game.");
            return;
        }

        Debug.Log("[GameManager] Starting new game...");
        _currentTime = _gameDuration;

        _uiManager.CloseAll();
        _uiManager.OpenUI<CanvasGamePlay>();
        CanvasGamePlay canvasGamePlay = _uiManager.GetUI<CanvasGamePlay>();
        if (canvasGamePlay != null) 
        {
            canvasGamePlay.SetTimeBarMaxTime(_gameDuration);
            canvasGamePlay.UpdateTimeBar(_currentTime, _gameDuration);
            canvasGamePlay.UpdateScore(_currentScore);
        }
        _match2.InitializeGame();
        ChangeState(GameState.GamePlay);
    }
    

    public static void ChangeState(GameState state) => _currentGameState = state;

    public bool IsState(GameState state) => _currentGameState == state;

    public void IncreaseScore()
    {
        _currentScore += _scorePerMatch;
        Debug.Log($"[GameManager] Score increased to: {_currentScore}");
        if (_uiManager != null && _uiManager.IsUIOpened<CanvasGamePlay>())
        {
            _uiManager.GetUI<CanvasGamePlay>().UpdateScore(_currentScore);
        }
    }

    
    private void HandleInput()
    {
        Vector3 clickedPosition = Vector3.zero;
        bool clicked = false;
        if (Input.GetMouseButtonDown(0)) 
        {
            clickedPosition = Input.mousePosition;
            clicked = true;
        }

        if (clicked) 
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(clickedPosition);
            worldPoint.z = 0;
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector3.zero);
            OnGameObjectClicked?.Invoke(hit.collider != null ? hit.collider.gameObject : null);
        }
    }
    public void EndGame(bool levelComplete)
    {
        if (_currentGameState == GameState.Finish) return;
        ChangeState(GameState.Finish);
        if (levelComplete)
        {
            Debug.Log("[GameManager] Level Complete! All Pokemon cleared.");
            _uiManager.OpenUI<CanvasVictory>();
        }
        else
        {
            Debug.Log("[Game Manager] You Failed");
            _uiManager.OpenUI<CanvasFail>();
        }
    }

    public void OnHintButtonClicked()
    {
        _match2.OnHintButtonClicked();
    }
    public void OnShuffleButtonClicked()
    {
        _match2.OnShuffleButtonClicked();
    }
}
