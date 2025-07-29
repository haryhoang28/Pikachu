using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon : MonoBehaviour
{
    [SerializeField] private PokemonType _type;
    [SerializeField] private SpriteRenderer _iconSprite;
    // Biến để lưu trữ màu gốc của Sprite Renderer
    private Color _originalColor;
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
        if (_iconSprite != null)
        {
            _iconSprite.color = Color.yellow; 
        }
        Debug.Log($"[Pokemon] {Type.typeName} at {transform.position} selected.");
    }

    
    public void Deselect()
    {
        if (_iconSprite != null)
        {
            _iconSprite.color = _originalColor; // Đặt lại màu gốc
        }
        Debug.Log($"[Pokemon] {Type.typeName} at {transform.position} deselected.");
    }

    internal void Highlight(float hintHighlightDuration, Color hintColor)
    {
        throw new NotImplementedException();
    }
}