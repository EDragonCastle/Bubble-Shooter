using UnityEngine;

/// <summary>
/// 모든 Element 정보를 담고 있는 Class
/// </summary>
public class BubbleCategory
{
    #region Element Object
    private GameObject redElement;
    private GameObject blueElement;
    private GameObject greenElement;
    private GameObject yellowElement;
    #endregion

    /// <summary>
    /// ElementCategory 생성자
    /// </summary>
    /// <param name="red">red element object</param>
    /// <param name="blue">blue element object</param>
    /// <param name="green">green element object</param>
    /// <param name="yellow">yellow element object</param>
    public BubbleCategory(GameObject red, GameObject blue, GameObject green, GameObject yellow)
    {
        redElement = red;
        blueElement = blue;
        greenElement = green;
        yellowElement = yellow;
    }

    /// <summary>
    /// Category 정보에서 Object를 가져오는 함수
    /// </summary>
    /// <param name="_category">Color 정보</param>
    /// <returns>element Object</returns>
    public GameObject GetCategory(BubbleColor _category)
    {
        GameObject categoryObject = null;
        switch (_category)
        {
            case BubbleColor.Red:
                categoryObject = redElement;
                break;
            case BubbleColor.Blue:
                categoryObject = blueElement;
                break;
            case BubbleColor.Green:
                categoryObject = greenElement;
                break;
            case BubbleColor.Yellow:
                categoryObject = yellowElement;
                break;
            default:
                categoryObject = null;
                break;
        }

        if (categoryObject == null)
        {
            Debug.Log($"{_category} exist None Object");
            return null;
        }

        return categoryObject;
    }
}

/// <summary>
/// Eelement Color 정보를 담고 있는 Enum
/// </summary>
public enum BubbleColor
{
    Red,
    Blue,
    Green,
    Yellow,
    End,
}


// Board에서
/*
    // color 세팅이나 방문여부나 위치를 조절해준다. 
    // 여기서 핵심은 color인데, 위에서 이미 정보를 만들어서 굳이 필요할까? 필요하지 않다고 본다.
    var newElementInfo = new ElementInfo();

    newElementInfo.color = randIndex;
    newElementInfo.isVisits = false;
    newElementInfo.position = new Vector2(x, 0);
    newElementUIPos.anchoredPosition = upperStartPos;
 */