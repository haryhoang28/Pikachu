using System;
using System.Collections.Generic;
using UnityEngine;

public class InputController : IInputController
{
    private IGridManager _gridManager;
    private IMatchFinder _matchFinder;

    private Pokemon _firstSelectedPokemon;
    private Vector2Int _firstSelectedPosition; 

    public event Action<List<Vector2Int>, Color, float> OnPathFoundForDebug;
    public event Action<Pokemon, Pokemon> OnPokemonMatched;
    public event Action OnNoMatchFound;
    private bool _inputEnabled = true;

    public InputController(IGridManager gridManager, IMatchFinder matchFinder)
    {
        _gridManager = gridManager;
        _matchFinder = matchFinder;
    }

    public void EnableInput()
    {
        _inputEnabled = true;
        Debug.Log("[InputController] Input Enabled.");
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        // Đảm bảo bỏ chọn bất kỳ Pokemon nào đang được chọn khi input bị vô hiệu hóa
        ResetSelection();
        Debug.Log("[InputController] Input Disabled.");
    }

    public void HandleClick(GameObject clickedGameObject)
    {
        if (!_inputEnabled)
        {
            Debug.Log("[InputController] Input is currently disabled.");
            return;
        }

        // Only process clicks when the game is in gameplay state
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        Pokemon clickedPokemon = clickedGameObject?.GetComponent<Pokemon>();

        if (clickedPokemon != null)
        {
            Vector2Int clickedGridPos = _gridManager.GetPokemonGridPosition(clickedPokemon);

            // If this is the first Pokemon being selected
            if (_firstSelectedPokemon == null)
            {
                // Assign the clicked Pokemon as the first selected Pokemon
                _firstSelectedPokemon = clickedPokemon;
                _firstSelectedPosition = clickedGridPos; // Save the position of the first selected Pokemon
                _firstSelectedPokemon.Select();
                Debug.Log($"[InputController] First Pokemon selected: {clickedPokemon.Type.typeName} at {_firstSelectedPosition}");
            }
            else // If a Pokemon is already selected
            {
                // If the clicked Pokemon is the same as the first selected one, deselect it
                if (clickedPokemon == _firstSelectedPokemon)
                {
                    Debug.Log("[InputController] Same Pokemon clicked again. Deselecting.");
                    ResetSelection(); 
                    return;
                }

                // If the clicked Pokemon has a different type than the first selected one, reset selection
                if (_firstSelectedPokemon.Type.typeId != clickedPokemon.Type.typeId)
                {
                    Debug.Log($"[InputController] Pokemon types do not match: {_firstSelectedPokemon.Type.typeName} vs {clickedPokemon.Type.typeName}.");
                    OnNoMatchFound?.Invoke(); // Kích hoạt sự kiện không tìm thấy match
                    ResetSelection(); 
                    return;
                }
                // The second Pokemon is different and matches the type of the first selected one. then try to find a match
                
                Vector2Int pos1 = _firstSelectedPosition; 
                Vector2Int pos2 = clickedGridPos;

                List<Vector2Int> path;
                bool checkMatch = _matchFinder.TryFindMatch(pos1, pos2, out int bends, out path);

                if (checkMatch)
                {
                    // Match found, proceed with the match logic
                    // Change color of the second Pokemon to indicate selection
                    clickedPokemon.Select();
                    Debug.Log($"[InputController] Match found between {pos1} and {pos2}!");
                    OnPathFoundForDebug?.Invoke(path, Color.green, 1f); // Kích hoạt sự kiện debug đường đi
                    OnPokemonMatched?.Invoke(_firstSelectedPokemon, clickedPokemon); // Kích hoạt sự kiện khớp nối thành công
                    // Sau khi match thành công, luôn reset lựa chọn
                    ResetSelection(); 
                }
                else
                {
                    Debug.Log($"[InputController] No valid path found between {pos1} and {pos2}.");
                    OnNoMatchFound?.Invoke(); // Kích hoạt sự kiện không tìm thấy match
                    ResetSelection(); 
                }
            }
        }
        else
        {
            Debug.Log($"[InputController] Clicked on an empty space or non-Pokemon object. Deselecting any previously selected Pokemon.");
            ResetSelection();
        }
    }

    

    private void ResetSelection()
    {
        if (_firstSelectedPokemon != null)
        {
            _firstSelectedPokemon.Deselect(); 
            _firstSelectedPokemon = null;
            _firstSelectedPosition = Vector2Int.zero;
            Debug.Log("[InputController] Selection reset.");
        }
    }
}