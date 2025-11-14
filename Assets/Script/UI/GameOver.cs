using UnityEngine;

/// <summary>
/// Game Over을 담당한 Class
/// </summary>
public class GameOver : MonoBehaviour
{
    /// <summary>
    /// 버튼 클릭시 Title 화면으로 가기 위해 만든 함수 
    /// </summary>
    public void Title()
    {
        var uiManager = Locator<UIManager>.Get();
        var gameOver = uiManager.GetUICloneObject(UIType.Gameover);
        gameOver.SetActive(false);
        var title = uiManager.GetUICloneObject(UIType.Title);
        title.SetActive(true);
    }


    /// <summary>
    /// 버튼 클릭시 Restart로 가기 위해 만든 함수
    /// </summary>
    public void Restart()
    {
        var uiManager = Locator<UIManager>.Get();
        var gameOver = uiManager.GetUICloneObject(UIType.Gameover);
        var controller = uiManager.GetUICloneObject(UIType.Controller);
        var gameBoard = uiManager.GetUICloneObject(UIType.Board);

        gameOver.SetActive(false);
        controller.SetActive(true);
        gameBoard.SetActive(true);
    }
}
