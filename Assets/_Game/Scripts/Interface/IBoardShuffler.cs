using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoardShuffler
{
    IEnumerator ShuffleBoardRoutine(Transform parentTransform);
    
    void ShuffleList<T>(List<T> list);
}