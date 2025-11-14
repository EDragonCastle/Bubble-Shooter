using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 각종 UI들을 관리하고 있는 Manager
/// </summary>
public class UIManager
{
    // ui category를 담고 있다.
    private UICategory category;

    // key: origine, value : clone object
    private Dictionary<GameObject, GameObject> uiObjects = null;
    

    #region UIManager Structor
    /// <summary>
    /// UIManager 생성자
    /// </summary>
    /// <param name="_category">UI category</param>
    public UIManager(UICategory _category)
    {
        category = _category;
        uiObjects = new Dictionary<GameObject, GameObject>();
    }
    #endregion

    /// <summary>
    /// UI Origine Prefab을 가져온다.
    /// </summary>
    /// <param name="type">UIType Enum Type</param>
    /// <returns>Prefab object</returns>
    public GameObject GetUIPrefab(UIType type)
    {
        var uiPrefab = category.GetUIPrefab(type);
        return uiPrefab;
    }

    /// <summary>
    /// UI Clone object를 가져온다.
    /// </summary>
    /// <param name="type">UIType Enum Type</param>
    /// <returns>Clone Object</returns>
    public GameObject GetUICloneObject(UIType type)
    {
        // Category에서 Origine Prefab을 가져온다.
        var uiPrefab = category.GetUIPrefab(type);
        GameObject uiObject = null;

        // origine Prefab Object를 통해 확인해서 있으면 Clone Object를 출력하고, 없다면 새로 만들어서 생성한다.
        if (uiObjects.ContainsKey(uiPrefab))
        {
            return uiObjects[uiPrefab];
        }
        else
        {
            uiObject = GameObject.Instantiate(uiPrefab);
            uiObjects.Add(uiPrefab, uiObject);
            return uiObject;
        }
    }

}
