using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hintCountText;
    [SerializeField] private Button _button;
    [SerializeField] private Match2 _match2;
    private void Awake()
    {
        if (_hintCountText == null)
        {
            _hintCountText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        Match2.OnHintCountChanged += UpdateHintDisplay;
    }

    private void OnDisable()
    {
        Match2.OnHintCountChanged -= UpdateHintDisplay;
    }

    private void UpdateHintDisplay(int currentCount, int maxCount)
    {
        if (_hintCountText != null)
        {
            _hintCountText.text = $"Hints: {currentCount}/{maxCount}";
        }

        if (_button != null)
        {
            _button.interactable = (currentCount > 0);
        }
    }

    private void OnBUttonClicked()
    {
        if (_match2 != null)
        {
            _match2.OnHintButtonClicked();
        }
        else
        {
            Debug.LogWarning("[HintButtonUI] Match2 reference is null. Cannot request hint.");
        }
    }
}
