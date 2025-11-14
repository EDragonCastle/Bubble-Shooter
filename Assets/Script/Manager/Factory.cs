using UnityEngine;

/// <summary>
/// Puzzle Element Object를 담고 있는 Factory다.
/// </summary>
public class Factory
{
    // ElementColor와 Elment Category를 담은 Object Pool
    private ObjectPool<BubbleColor, BubbleCategory> objectPools;

    // Factory 생성자
    #region Factory Construct
    /// <summary>
    /// Factory를 생성한다.
    /// </summary>
    /// <param name="_category">Element Category가 필요하다.</param>
    /// <param name="parent">Object들을 한 곳에 보관할 수 있는 부모 값</param>
    public Factory(BubbleCategory _category, GameObject parent = null)
    {
        objectPools = new ObjectPool<BubbleColor, BubbleCategory>(_category, parent);
    }

    /// <summary>
    /// Factory를 생성한다.
    /// </summary>
    /// <param name="_category">Element Category가 필요하다.</param>
    /// <param name="poolInitSize">Object를 Size를 정할 최소 값</param>
    /// <param name="parent">Object들을 한 곳에 보관할 수 있는 부모 값</param>
    public Factory(BubbleCategory _category, int poolInitSize, GameObject parent = null )
    {
        objectPools = new ObjectPool<BubbleColor, BubbleCategory>(_category, poolInitSize, parent);
    }
    #endregion

    /// <summary>
    /// IObject를 Return하는 함수
    /// </summary>
    /// <param name="color">Enum Color</param>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    /// <param name="scale">크기</param>
    /// <param name="parent">부모 값</param>
    /// <returns>IUIElement (Puzzle Element)</returns>
    public IObject CreateObject(BubbleColor color, PathInformation pathInfo, Vector2 position, Quaternion rotation, Vector3 scale, Transform parent = null)
    {
        // Factory에서는 생성만 해야돼.

        // object Pool에서 color값을 통해 object를 가져온다.
        GameObject elementInfo = objectPools.Get(color);
        elementInfo.transform.SetParent(parent);
        IObject bubbleObject = elementInfo.GetComponent<IObject>();

        // rectTransform을 가져와서 위치, 회전, 크기를 조절한다.
        var bubbleTransform = bubbleObject.GetTransform();
        bubbleObject.SetPathInformation(pathInfo);
        bubbleTransform.position = position;
        bubbleTransform.rotation = rotation;
        bubbleTransform.localScale = scale;
       
        return bubbleObject;
    }

    /// <summary>
    /// IObject를 Return하는 함수
    /// </summary>
    /// <param name="color">Enum Color</param>
    /// <param name="parent">부모 값</param>
    /// <returns></returns>
    public IObject CreateObject(BubbleColor color, Transform parent = null)
    {
        GameObject elementInfo = objectPools.Get(color);
        elementInfo.transform.SetParent(parent);
        IObject bubbleObject = elementInfo.GetComponent<IObject>();
        var bubbleTransform = bubbleObject.GetTransform();
        bubbleTransform.rotation = Quaternion.identity;
        bubbleTransform.localScale = Vector3.one;
        return bubbleObject;
    }

    /// <summary>
    /// IObject를 Return하는 함수
    /// </summary>
    /// <param name="color">Enum Color</param>
    /// <param name="position">위치</param>
    /// <param name="parent">부모 값</param>
    /// <returns></returns>
    public IObject CreateObject(BubbleColor color, Vector2 position, Transform parent = null)
    {
        GameObject elementInfo = objectPools.Get(color);
        elementInfo.transform.SetParent(parent);
        IObject bubbleObject = elementInfo.GetComponent<IObject>();
        var bubbleTransform = bubbleObject.GetTransform();
        bubbleTransform.position = position;
        bubbleTransform.rotation = Quaternion.identity;
        bubbleTransform.localScale = Vector3.one;
        return bubbleObject;
    }

    /// <summary>
    /// IObject를 Return 하는 함수
    /// </summary>
    /// <param name="color">Enum Color</param>
    /// <param name="position">위치</param>
    /// <param name="isPhysics">collider 여부</param>
    /// <param name="parent">부모 값</param>
    /// <returns></returns>
    public IObject CreateObject(BubbleColor color, Vector2 position, bool isPhysics, Transform parent = null)
    {
        GameObject elementInfo = objectPools.Get(color);

        IObject bubbleObject = elementInfo.GetComponent<IObject>();
        var bubbleTransform = bubbleObject.GetTransform();
        bubbleTransform.position = position;
        bubbleTransform.rotation = Quaternion.identity;
        bubbleTransform.localScale = Vector3.one;

        var circleCollider2D = elementInfo.GetComponent<CircleCollider2D>();
        circleCollider2D.enabled = isPhysics;

        return bubbleObject;
    }


    /// <summary>
    /// Object 삭제이자 반납
    /// Object Pool을 이용해서 다시 되돌린다.
    /// </summary>
    /// <param name="_gameObject"></param>
    public void DestoryObject(GameObject _gameObject)
    {
        // object Pool을 이용해서 반납한다.
        objectPools.Return(_gameObject);
    }
}
