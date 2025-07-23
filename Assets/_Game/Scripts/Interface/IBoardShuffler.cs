using System.Collections;
using UnityEngine;

public interface IBoardShuffler
{
    IEnumerator ShuffleBoardRoutine(Transform parentTransform);
}