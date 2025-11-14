using UnityEngine;

/// <summary>
/// Game Clear를 담당하는 클래스
/// </summary>
public class GameClear : MonoBehaviour
{
    /// <summary>
    /// 버튼 클릭시 Title로 가는 함수
    /// </summary>
    public void Title()
    {
        var uiManager = Locator<UIManager>.Get();
        var gameClear = uiManager.GetUICloneObject(UIType.GameClear);
        gameClear.SetActive(false);
        var title = uiManager.GetUICloneObject(UIType.Title);
        title.SetActive(true);
    }

    /// <summary>
    /// 버튼 클릭시 Restart를 하는 함수
    /// </summary>
    public void Restart()
    {
        var uiManager = Locator<UIManager>.Get();
        var gameClear = uiManager.GetUICloneObject(UIType.GameClear);
        var controller = uiManager.GetUICloneObject(UIType.Controller);
        var gameBoard = uiManager.GetUICloneObject(UIType.Board);

        gameClear.SetActive(false);
        controller.SetActive(true);
        gameBoard.SetActive(true);
    }
}
