using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMainMenu : UICanvas 
{ 
    public void PlayButton()
    {
        GameManager.Instance.StartGame();
    }

    public void SettingsButton()
    {
        UIManager.Instance.OpenUI<CanvasSettings>().SetState(this);
        GameManager.ChangeState(GameState.Setting);
    }
    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Exiting game");
    }
}
