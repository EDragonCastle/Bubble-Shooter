using UnityEngine;

/// <summary>
/// Title을 담당하는 클래스
/// </summary>
public class Title : MonoBehaviour
{
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void GameStart()
    {
        var uiManger = Locator<UIManager>.Get();
        var board = uiManger.GetUICloneObject(UIType.Board);
        var controller = uiManger.GetUICloneObject(UIType.Controller);

        controller.SetActive(true);
        board.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
