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

        // Chỉ xử lý input nếu trò chơi đang ở trạng thái GamePlay
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        Pokemon clickedPokemon = clickedGameObject?.GetComponent<Pokemon>();

        if (clickedPokemon != null)
        {
            Vector2Int clickedGridPos = _gridManager.GetPokemonGridPosition(clickedPokemon);

            // Kiểm tra xem đây có phải là lần chọn đầu tiên hay thứ hai
            if (_firstSelectedPokemon == null)
            {
                // Đây là Pokemon đầu tiên được chọn
                _firstSelectedPokemon = clickedPokemon;
                _firstSelectedPosition = clickedGridPos; // Lưu vị trí của Pokemon đầu tiên
                _firstSelectedPokemon.Select(); // 
                Debug.Log($"[InputController] First Pokemon selected: {clickedPokemon.Type.typeName} at {_firstSelectedPosition}");
            }
            else // Một Pokemon đã được chọn trước đó
            {
                // Nếu người chơi click lại vào cùng một Pokemon đã chọn
                if (clickedPokemon == _firstSelectedPokemon)
                {
                    Debug.Log("[InputController] Same Pokemon clicked again. Deselecting.");
                    ResetSelection(); 
                    return;
                }

                // Nếu hai Pokemon có loại khác nhau, đây không phải là một cặp hợp lệ
                if (_firstSelectedPokemon.Type.typeId != clickedPokemon.Type.typeId)
                {
                    Debug.Log($"[InputController] Pokemon types do not match: {_firstSelectedPokemon.Type.typeName} vs {clickedPokemon.Type.typeName}.");
                    OnNoMatchFound?.Invoke(); // Kích hoạt sự kiện không tìm thấy match
                    ResetSelection(); 
                    return;
                }

                Vector2Int pos1 = _firstSelectedPosition; // Sử dụng vị trí đã lưu
                Vector2Int pos2 = clickedGridPos;

                List<Vector2Int> path;
                bool checkMatch = _matchFinder.TryFindMatch(pos1, pos2, out int bends, out path);

                if (checkMatch)
                {
                    Debug.Log($"[InputController] Match found between {pos1} and {pos2}!");
                    OnPathFoundForDebug?.Invoke(path, Color.green, 0.5f); // Kích hoạt sự kiện debug đường đi
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