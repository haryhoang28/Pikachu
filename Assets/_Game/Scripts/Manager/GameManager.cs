using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState { MainMenu, GamePlay, Setting, Finish, Revive, Shuffling }
public class GameManager : Singleton<GameManager>
{
    private static GameState _gameState;
    public static event Action<GameObject> OnGameObjectClicked;
    


    private void Update()
    {
        HandleInput();
    }
    public static void ChangeState(GameState state) => _gameState = state;

    public bool isState(GameState state) => _gameState == state;

    private void Awake()
    {
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
            if (hit.collider != null)
            {
                //Debug.Log($"[GameManager] Raycast hit: {hit.collider.gameObject.name} at {hit.transform.position}");
                OnGameObjectClicked?.Invoke(hit.collider.gameObject);
            }
            else
            {
                //Debug.Log($"[GameManager] Raycast hit nothing at {worldPoint}.");
                OnGameObjectClicked?.Invoke(null);
            }
        }
    }
}
