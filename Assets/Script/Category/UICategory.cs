using UnityEngine;

/// <summary>
/// UI 정보를 담고 있는 Category
/// </summary>
public class UICategory
{ 
    #region UIObject
    private GameObject gameOver;
    private GameObject gameClear;
    private GameObject title;
    private GameObject board;
    private GameObject controller;
    #endregion

    /// <summary>
    /// UIPrefabCategory 생성자
    /// </summary>
    /// <param name="_title">Title object</param>
    /// <param name="_board">Board object</param>
    /// <param name="_gameOver">Gameover object</param>
    public UICategory(GameObject _title, GameObject _board, GameObject _gameOver, GameObject _gameClear, GameObject _controller)
    {
        title = _title;
        board = _board;
        gameOver = _gameOver;
        gameClear = _gameClear;
        controller = _controller;
    }

    /// <summary>
    /// UIPrefab 정보로 Object 출력
    /// </summary>
    /// <param name="prefabType">UIPrefab 정보</param>
    /// <returns>UIPrefab Object</returns>
    public GameObject GetUIPrefab(UIType prefabType)
    {
        GameObject prefab = null;

        switch (prefabType)
        {
            case UIType.Title:
                prefab = title;
                break;
            case UIType.Board:
                prefab = board;
                break;
            case UIType.Gameover:
                prefab = gameOver;
                break;
            case UIType.Controller:
                prefab = controller;
                break;
            case UIType.GameClear:
                prefab = gameClear;
                break;
        }

        return prefab;
    }
}

/// <summary>
/// UIPrefab 정보를 담당하고 있는 Enum
/// </summary>
public enum UIType
{
    Title,
    Board,
    Controller,
    Gameover,
    GameClear,
    End,
}
