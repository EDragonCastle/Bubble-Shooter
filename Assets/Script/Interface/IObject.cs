using UnityEngine;

/// <summary>
/// Bubble을 생성할 Object Interface
/// </summary>
public interface IObject
{
    public Transform GetTransform();
    public PathInformation GetPathInformation();
    public void SetPathInformation(PathInformation information);

    public BubbleInformation GetBubbleInformation();
    public void SetBubbleInformation(BubbleInformation information);
}
