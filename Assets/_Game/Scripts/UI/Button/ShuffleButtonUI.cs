using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShuffleButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _shuffleCountText;
    [SerializeField] private Button _button;
    private void Awake()
    {
        if (_shuffleCountText == null)
        {
            _shuffleCountText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        Match2.OnShuffleCountChanged += UpdateShuffleDisplay;
    }

    private void OnDisable()
    {
        Match2.OnShuffleCountChanged -= UpdateShuffleDisplay;
    }

    private void UpdateShuffleDisplay(int currentCount, int maxCount)
    {
        if (_shuffleCountText != null)
        {
            _shuffleCountText.text = $"{currentCount}/{maxCount}";
        }

        if (_button != null)
        {
            _button.interactable = (currentCount > 0);
        }
    }
}
