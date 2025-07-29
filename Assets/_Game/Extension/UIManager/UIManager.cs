using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private UICanvas[] uiResources;

    private Dictionary<System.Type, UICanvas> _uiCanvasPrefabs = new Dictionary<System.Type, UICanvas>();
    [Tooltip("Dictionary for actives UI")]
    private Dictionary<System.Type, UICanvas> _uiCanvasActives = new Dictionary<System.Type, UICanvas>();

    public Transform CanvasParentTF;

    private void Awake()
    {
        // Load UI prefabs from resources
        UICanvas[] prefabs = Resources.LoadAll<UICanvas>("UI/");
        for (int i = 0; i < prefabs.Length; i++)
        {
            _uiCanvasPrefabs.Add(prefabs[i].GetType(), prefabs[i]);
        }
    }

    public T OpenUI<T> () where T : UICanvas
    {
        UICanvas canvas = GetUI<T>();

        canvas.Setup();
        canvas.Open();

        return canvas as T;
    }

    public void CloseUI<T>(float delayTime) where T : UICanvas
    {
        if (IsUIOpened<T>())
        {
            GetUI<T>().Close(delayTime);
        }
    }
    public void CloseUIDirectly<T>() where T : UICanvas
    {
        if (IsUIOpened<T>())
        {
            _uiCanvasActives[typeof(T)].CloseDirectly();
        }
    } 

    public bool IsUIOpened<T>() where T : UICanvas
    {
        return IsUILoaded<T>() && _uiCanvasActives[typeof(T)].gameObject.activeSelf;
    }

    public bool IsUILoaded<T>() where T : UICanvas
    {
        Type type = typeof(T);
        return _uiCanvasActives.ContainsKey(type) && _uiCanvasActives[type] != null;
    }

    /// <summary>
    /// Get active canvas
    /// If UI isn't loaded then Instantiate Ui, else get that UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetUI<T>() where T : UICanvas
    {
        if (!IsUILoaded<T>())
        {
            T prefab = GetUIPrefab<T>();
            T canvas = Instantiate(prefab, CanvasParentTF);
            _uiCanvasActives[typeof (T)] = canvas;
        }

        return _uiCanvasActives[typeof(T)] as T;
    }

    //Close all UI
    //dong tat ca UI ngay lap tuc -> tranh truong hop dang mo UI nao dong ma bi chen 2 UI cung mot luc
    public void CloseAll()
    {
        foreach (var item in _uiCanvasActives)
        {
            if (item.Value != null && item.Value.gameObject.activeInHierarchy)
            {
                item.Value.Close(0);
            }
        }
    }
    /// <summary>
    /// Get prefab from Resources/UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T GetUIPrefab<T>() where T : UICanvas
    {
        if (!_uiCanvasPrefabs.ContainsKey(typeof(T)))
        {
            for (int i = 0; i < uiResources.Length; i++)
            {
                if (uiResources[i] is T)
                {
                    _uiCanvasPrefabs[typeof(T)] = uiResources[i];
                    break;
                }
            }
        }

        return _uiCanvasPrefabs[typeof(T)] as T;
    }

}
