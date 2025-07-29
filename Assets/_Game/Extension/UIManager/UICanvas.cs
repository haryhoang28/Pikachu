using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    [SerializeField] bool isDestroyOnClose = false;
    private void Awake()
    {
        // Xu ly tai tho
        RectTransform rect = GetComponent<RectTransform>();
        float ratio = (float)Screen.width / (float)Screen.height;
        if (ratio > 2.1f)
        {
            Vector2 leftBottom = rect.offsetMin;
            Vector2 rightTop = rect.offsetMax;

            leftBottom.y = 0f;
            rightTop.y = -100f;
            rect.offsetMin = leftBottom;
            rect.offsetMax = rightTop;

        }
    }
    /// <summary>
    /// Call before canvas was acvtivated
    /// </summary>
    public virtual void Setup()
    {

    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Shut canvas down after time (s)
    /// </summary>
    /// <param name="time"></param>
    public virtual void Close(float time) 
    {
        Invoke(nameof(CloseDirectly), time);
    }


    /// <summary>
    /// Shut down canvas directly
    /// </summary>
    public virtual void CloseDirectly() 
    {
        if (isDestroyOnClose)
        {
            Destroy(gameObject);
        }
        else 
        {
            gameObject.SetActive(false);
        }
        
    }
}
