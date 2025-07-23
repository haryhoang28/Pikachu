using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _backgroundSprite;
    
    //public static event Action<int, int, float, Vector3> OnGridSizeChanged;

    private void OnEnalble()
    {
        Match2.OnGridSystemReady += HandleGridSystemReady;
    }

    private void OnDisable()
    {
        Match2.OnGridSystemReady -= HandleGridSystemReady;
    }

    private void HandleGridSystemReady(int gridWidth, int gridHeight, float cellSize, Vector3 origin)
    {
        CenterCameraAndBackground(gridWidth, gridHeight, cellSize, origin);
    }

    private void CenterCameraAndBackground(int gridWidth, int gridHeight, float cellSize, Vector3 origin)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.Log("Main camera not found");
            return;
        }

        mainCamera.orthographic = true;
        float totalGridWidth = gridWidth * cellSize;
        float totalGridHeight = gridHeight * cellSize;

        Vector3 gridWorldCenter = origin + new Vector3(totalGridWidth / 2f - cellSize / 2f, totalGridHeight / 2f - cellSize / 2f, 0);
        mainCamera.transform.position = new Vector3(gridWorldCenter.x, gridWorldCenter.y, mainCamera.transform.position.z);
        
        float screenAspect =(float) Screen.width / Screen.height;
        float targetGridAspect = totalGridHeight / totalGridWidth;

        float padding = cellSize * 1f;
        float paddingGridWidth = totalGridWidth + padding;
        float paddingGridHeight = totalGridHeight + padding;

        if (screenAspect >= targetGridAspect)
        {
            mainCamera.orthographicSize = paddingGridHeight / 2f;
        }
        else
        {
            mainCamera.orthographicSize = paddingGridWidth / (2f * screenAspect);
        }
        
        if (_backgroundSprite != null && _backgroundSprite.sprite != null)
        {
            // Đặt vị trí của background tại tâm của lưới
            _backgroundSprite.transform.position = new Vector3(gridWorldCenter.x, gridWorldCenter.y, _backgroundSprite.transform.position.z);

            // Lấy kích thước gốc của sprite background (theo đơn vị thế giới)
            float bgSpriteWidth = _backgroundSprite.sprite.bounds.size.x;
            float bgSpriteHeight = _backgroundSprite.sprite.bounds.size.y;

            // Lấy kích thước thế giới mà camera đang nhìn thấy
            float cameraVisibleWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
            float cameraVisibleHeight = mainCamera.orthographicSize * 2f;

            // Tính toán tỉ lệ scale cần thiết cho background
            // Để đảm bảo background bao phủ toàn bộ màn hình, chúng ta lấy tỉ lệ lớn hơn
            float scaleX = cameraVisibleWidth / bgSpriteWidth;
            float scaleY = cameraVisibleHeight / bgSpriteHeight;

            float scale = Mathf.Max(scaleX, scaleY);
            // Áp dụng scale
            _backgroundSprite.transform.localScale = new Vector3(scale, scale, 1f);

            // Đảm bảo background nằm phía sau tất cả các Pokemon và Tilemap (z-order)
            // Có thể điều chỉnh giá trị Z hoặc sử dụng Sorting Layer/Order in Layer của SpriteRenderer.
            if (_backgroundSprite.transform.position.z >= mainCamera.transform.position.z)
            {
                _backgroundSprite.transform.position = new Vector3(
                    _backgroundSprite.transform.position.x,
                    _backgroundSprite.transform.position.y,
                    mainCamera.transform.position.z + 10f // Đặt nó xa hơn camera về phía sau
                );
            }
        }
    }
}

