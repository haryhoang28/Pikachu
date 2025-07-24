using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasFail : UICanvas
{
    [SerializeField] TextMeshProUGUI scoreText;

    public void SetBestScore(int score)
    {
        scoreText.text = score.ToString();
    }
    public void MainMenuButton()
    {
        Close(0);
        UIManager.Instance.OpenUI<CanvasMainMenu>();
    }
}
