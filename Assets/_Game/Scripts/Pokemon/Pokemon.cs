using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon : MonoBehaviour
{
    [Header("Pokemon Settings")]
    [SerializeField] private PokemonType _type;
    [SerializeField] private SpriteRenderer _iconSprite;
    [SerializeField] private SpriteRenderer _backgroundSprite;
    
    [Header("Hint Settings")]
    private Color _originalColor;
    private Coroutine _highlightCoroutine;
    public PokemonType Type => _type;
    private void Awake()
    {
        if (_iconSprite == null)
        {
            _iconSprite = GetComponent<SpriteRenderer>();
            if (_iconSprite == null)
            {
                Debug.LogError("[Pokemon] SpriteRenderer component not found on this GameObject or assigned!");
            }
        }
        // Lưu lại màu gốc khi khởi tạo
        if (_iconSprite != null)
        {
            _originalColor = _iconSprite.color;
        }
    }
    public void OnInit(PokemonType type)
    {
        _type = type;
        if(_iconSprite != null)
        {
            _iconSprite.sprite = type.sprite;
        }
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        if (TryGetComponent(out SpriteRenderer spriteRenderer)) 
        {
            spriteRenderer.enabled = false;
        }
        gameObject.SetActive(false);
    }
    public void Select()
    {
        if (_backgroundSprite != null)
        {
            _backgroundSprite.color = Color.green; 
        }
        Debug.Log($"[Pokemon] {Type.typeName} at {transform.position} selected.");
    }

    
    public void Deselect()
    {
        if (_backgroundSprite != null)
        {
            _backgroundSprite.color = _originalColor; // Đặt lại màu gốc
        }
        Debug.Log($"[Pokemon] {Type.typeName} at {transform.position} deselected.");
    }

    public void Highlight(float hintHighlightDuration, Color hintColor)
    {
        if (_highlightCoroutine != null)
        {
            StopCoroutine(_highlightCoroutine);
        }
        _highlightCoroutine =StartCoroutine(HighLightRoutine(hintHighlightDuration, hintColor));
    }

    private IEnumerator HighLightRoutine(float hintHighlightDuration, Color hintColor)
    {
        if (_backgroundSprite != null)
        {
            Color prevColor = _backgroundSprite.color;
            _backgroundSprite.color = hintColor; // Đặt màu nền thành màu gợi ý
            yield return new WaitForSeconds(hintHighlightDuration);
            _backgroundSprite.color = prevColor; // Đặt lại màu nền về màu gốc
        }
        _highlightCoroutine = null; // Đặt lại coroutine để có thể bắt đầu một lần nữa nếu cần
    }
}