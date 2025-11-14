/// <summary>
/// Channel 정보를 담고 있는 Enum
/// </summary>
public enum ChannelInfo
{
    CheckBoardObject,
    InsertBoard,
    BossAttack,
    LifeText,
    RefillBubble,
    End,
}

/// <summary>
/// EventManager를 쉽게 등록하고 해지할 수 있게 만든 Interface
/// object가 value면 is keyword 사용 ref면 as keyword 사용
/// </summary>
public interface IChannel
{
    public void HandleEvent(ChannelInfo channel, object information = null);
}
