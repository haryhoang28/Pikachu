using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGamePlay : UICanvas
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _hintCountText;
    [SerializeField] private Button _hintButton;

    [Header("Time Bar UI Settings")]
    [SerializeField] private TimeBar _timeBar;

    public override void Setup()
    {
        base.Setup();
        UpdateScore(0);
    }
    public void SettingsButton()
    {
        UIManager.Instance.OpenUI<CanvasSettings>().SetState(this);
    }

    public void UpdateHintCount(int currentCount, int maxCount)
    {
        

        // Vô hiệu hóa hoặc kích hoạt lại nút gợi ý
        if (_hintButton != null)
        {
            _hintButton.interactable = (currentCount > 0); // Nếu currentCount > 0 thì nút sẽ tương tác được
        }
        else
        {
            Debug.LogWarning("[CanvasGamePlay] Hint Button reference is null. Please assign it in the Inspector.");
        }
    }

    public void UpdateTimeBar(float currentTime, float maxTime)
    {
        if (_timeBar != null) 
        {
            _timeBar.SetTime(currentTime);
        }
        else
        {
            Debug.LogWarning("[CanvasGamePlay] TimeBar reference not set in Inspector!");
        }
    }

    public void SetTimeBarMaxTime(float maxTime)
    {
        if (_timeBar != null)
        {
            _timeBar.SetMaxTime(maxTime);
        }
    }

    public void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }
    public void OnHintButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClicked();
        }
        else
        {
            Debug.LogWarning("[CanvasGamePlay] Match2 instance is null. Cannot request hint.");
        }
    }
}
