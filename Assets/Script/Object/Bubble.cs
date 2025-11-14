using UnityEngine;

/// <summary>
/// Board에 저장될 실제 Object
/// </summary>
public class Bubble : MonoBehaviour, IObject
{
    [SerializeField]
    private PathInformation pathInformation;

    [SerializeField]
    private BubbleInformation bubbleInformation;

    public Transform GetTransform()
    {
        return this.transform;
    }

    // PathInformation Properties
    public PathInformation GetPathInformation() => pathInformation;
    public void SetPathInformation(PathInformation information) => pathInformation = information;

    // BubbleInformation Properties
    public BubbleInformation GetBubbleInformation() => bubbleInformation;
    public void SetBubbleInformation(BubbleInformation information) => bubbleInformation = information;
}

/// <summary>
/// Path 정보를 담은 구조체
/// </summary>
[System.Serializable]
public struct PathInformation
{
    // 여기는 이동할 경로를 적는 공간
    public int index;
    public int path;

    // 실제 Board position과 index
    public Vector2 position;
    public Vector2 boardIndex;

}

/// <summary>
/// Bubble의 정보를 담은 구조체
/// </summary>
[System.Serializable]
public struct BubbleInformation
{
    public BubbleColor bubbleColor;
    public Item item;
    public bool isVisit;
    public bool isDead;
}


/// <summary>
/// Item 정보
/// </summary>
public enum Item
{
    None,
    Attack,
    Bomb,
    End,
}