using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _backgroundSprite;
    

    private void OnEnable()
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

    private void CenterCameraAndBackground(int innerGridWidth, int innerGridHeight, float cellSize, Vector3 origin)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.Log("Main camera not found");
            return;
        }

        mainCamera.orthographic = true;
        float centerX = innerGridWidth / 2f + 0.5f;
        float centerY = innerGridWidth / 2f + 0.5f;

        Vector3 gridWorldCenter = origin + new Vector3(centerX, centerY, 0);
        mainCamera.transform.position = new Vector3(gridWorldCenter.x, gridWorldCenter.y, mainCamera.transform.position.z);

        float totalGridWorldWidth = innerGridWidth * cellSize;
        float totalGridWorldHeight = innerGridHeight * cellSize;

        float screenAspect = (float) Screen.width / Screen.height;

        float padding = cellSize * 1f;
        float desireedWorldWidth = totalGridWorldWidth + padding * 2;
        float desiredWorldHeight = totalGridWorldHeight + padding * 2;

        float sizeBasedOnWidth = desireedWorldWidth / (2f * mainCamera.aspect);
        float sizeBasedOnHeight = desiredWorldHeight / 2f;

        mainCamera.orthographicSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight);

        if (_backgroundSprite != null && _backgroundSprite.sprite != null)
        {
            // Đặt vị trí của background tại tâm của lưới
            _backgroundSprite.transform.position = new Vector3(gridWorldCenter.x, gridWorldCenter.y, _backgroundSprite.transform.position.z);

            float bgSpriteWidth = _backgroundSprite.sprite.bounds.size.x;
            float bgSpriteHeight = _backgroundSprite.sprite.bounds.size.y;

            float cameraVisibleWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
            float cameraVisibleHeight = mainCamera.orthographicSize * 2f;

            float scaleX = cameraVisibleWidth / bgSpriteWidth;
            float scaleY = cameraVisibleHeight / bgSpriteHeight;

            float scale = Mathf.Max(scaleX, scaleY);

            _backgroundSprite.transform.localScale = new Vector3(scale, scale, 1f);
            _backgroundSprite.transform.position = new Vector3(
                _backgroundSprite.transform.position.x,
                _backgroundSprite.transform.position.y,
                mainCamera.transform.position.z + 10f 
            );
        }
    }
}

