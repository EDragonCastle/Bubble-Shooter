using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 보스를 나타내는 클래스
/// </summary>
public class Boss : MonoBehaviour, IChannel
{
    public Image bossHPBar;
    private Image boss;
    public int bossLife = 9;
    private int maxBossLife;

    private void Awake()
    {
        boss = this.GetComponent<Image>();
        bossHPBar.type = Image.Type.Filled;
        bossHPBar.fillAmount = 1f;
        bossHPBar.fillMethod = Image.FillMethod.Horizontal;
        maxBossLife = bossLife;
    }

    private void OnEnable()
    {
        Enable();
    }

    private void OnDisable()
    {
        Disable();
    }

    /// <summary>
    /// Enable 시 실행되는 함수
    /// </summary>
    public void Enable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.BossAttack, HandleEvent);
        bossHPBar.fillAmount = 1f;
        bossLife = maxBossLife;
    }

    /// <summary>
    /// Disable 시 실행되는 함수
    /// </summary>
    public void Disable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.BossAttack, HandleEvent);
        bossLife = maxBossLife;
        bossHPBar.fillAmount = 1f;
    }

    /// <summary>
    /// 보스 공격시 실행되는 함수
    /// </summary>
    /// <param name="attackCount">공격 데미지</param>
    private void AttackBoss(int attackCount)
    {
        if(attackCount != 0)
        {
            // 데미지를 입혔으면 잠시 동안 맞은 것처럼 보이게 하기 위해 Color를 빨간색으로 나타냈다가 다시 되돌린다.
            Color originColor = boss.color;
            
            boss.DOColor(Color.red, 0.1f).SetEase(Ease.OutFlash, 2)
                .OnComplete(() => boss.color = originColor);

            // 보스 체력값을 나타내는 계산식
            bossLife -= attackCount;
            float currentHP = (float)bossLife / maxBossLife;

            // HPBar가 한번에 움직이는 것보다 자연스럽게 체력이 까이게 하기 위한 Dotween 함수
            bossHPBar.DOFillAmount(currentHP, 0.3f).SetEase(Ease.OutSine);
        }
        
        CurrentBossHP();
    }

    /// <summary>
    /// 현재 Boss 체력을 확인하는 함수
    /// </summary>
    private void CurrentBossHP()
    {
        // 죽으면 DeadBoss
        if (bossLife <= 0)
        {
            DeadBoss();
        }
    }

    /// <summary>
    /// 보스가 죽으면 실행하는 함수
    /// </summary>
    private void DeadBoss()
    {
        // Clear UI 생성
        var uiManger = Locator<UIManager>.Get();
        
        var gameClear = uiManger.GetUICloneObject(UIType.GameClear);
        var gameBoard = uiManger.GetUICloneObject(UIType.Board);
        var controller = uiManger.GetUICloneObject(UIType.Controller);

        controller.SetActive(false);
        gameBoard.SetActive(false);
        gameClear.SetActive(true);
    }

    /// <summary>
    /// Event Manager에 사용할 IChannel Interface
    /// </summary>
    /// <param name="channel">채널 정보</param>
    /// <param name="information">사용할 Object 내용</param>
    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.BossAttack:
                {
                    if (information is int attackCount)
                        AttackBoss(attackCount);
                }
                break;
        }
    }

}
