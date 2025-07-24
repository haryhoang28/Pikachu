using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasGamePlay : UICanvas
{
    [SerializeField] TextMeshProUGUI scoreText;

    public override void Setup()
    {
        base.Setup();
        UpdateScore(0);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
    public void SettingsButton()
    {
        UIManager.Instance.OpenUI<CanvasSettings>().SetState(this);
    }
}
