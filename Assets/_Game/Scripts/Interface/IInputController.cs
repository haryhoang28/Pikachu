using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInputController
{
    // Sự kiện để báo cho lớp điều phối (Match2) biết có đường đi và để vẽ debug
    event Action<List<Vector2Int>, Color, float> OnPathFoundForDebug;
    // Sự kiện khi 2 Pokemon được match thành công
    event Action<Pokemon, Pokemon> OnPokemonMatched;
    // Sự kiện khi click vào 2 Pokemon nhưng không tìm thấy match
    event Action OnNoMatchFound;
    void HandleClick(GameObject clickedGameObject);
    void EnableInput();
    void DisableInput();
}
