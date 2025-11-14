using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// 실제 유저의 조종을 담은 클래스
/// </summary>
public class Controller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IChannel
{
    public float degree = 20;
    private GameObject shooter;
    public TMP_Text lifeText;
    public GameObject pannel;
    public GameObject boss;

    private GameObject shootingObject;
    private GameObject addtiveObject;
    
    public Vector2 addtivePosition;

    private GuideLine guideObject;
    private Vector2 startPosition;
    private Vector2 dragPosition;

    // 위 아래 판별
    private bool isUpper;

    // 이 변수는 각도 범위에 있으면 드래그를 확인해서 아래로 갈지 위로 갈지 정하는 변수다.
    private bool isInnerDegree;
    private bool isShot;

    private bool isForward;

    private BubbleColor shootingColor;
    private BubbleColor addtiveColor;

    private void Awake()
    {
        var dataManager = Locator<DataManager>.Get();
        shooter = dataManager.GetShooter();
        // Controller에 Shooter를 어떻게 연결 시킬까?
        startPosition = shooter.transform.position;
        guideObject = shooter.GetComponent<GuideLine>();

        var factory = Locator<Factory>.Get();
        
        shootingColor = (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);
        shootingObject = factory.CreateObject(shootingColor, startPosition, false).GetTransform().gameObject;
        addtiveColor = (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);
        addtiveObject = factory.CreateObject(addtiveColor, startPosition + addtivePosition, false).GetTransform().gameObject;
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.LifeText, HandleEvent);
        eventManager.Subscription(ChannelInfo.RefillBubble, HandleEvent);
        boss.SetActive(true);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.LifeText, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.RefillBubble, HandleEvent);
        boss.SetActive(false);
    }

    /// <summary>
    /// Event Manager에 사용할 IChannel Interface
    /// </summary>
    /// <param name="channel">채널 정보</param>
    /// <param name="information">사용할 Object 내용</param>
    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch (channel)
        {
            case ChannelInfo.LifeText:
                {
                    string life = information as string;
                    lifeText.text = life;
                }
                break;
            case ChannelInfo.RefillBubble:
                {
                    if (information is bool isRefill)
                        pannel.SetActive(isRefill);
                }
                break;
        }
    }

    /// <summary>
    /// 터치했을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 mouseStartPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 direction = mouseStartPosition - startPosition;

        // 각도는 y축 기준이 아닌 x축 기준으로 구해서 위에 있는지 아래에 있는지 검사한다.
        float angle = Vector2.SignedAngle(Vector2.right, direction);

        // 해당 위치와 각도를 구한다.
        if(Mathf.Abs(angle) > 90)
        {
            if (angle > 0)
                angle = 180 - angle;
            else
                angle = -180 - angle;
        }

        // 요구 각도를 넘는지 확인한다.
        if(Mathf.Abs(angle) > degree)
        {
            // 위에 있는지 아래에 있는지 확인 후 저장한다.
            isUpper = mouseStartPosition.y > startPosition.y ? true : false;
            isForward = isUpper ? true : false;

            // 넘으면 그린다.
            guideObject.ShowLine();
            guideObject.UpdateLine(eventData.position, isForward);
            isInnerDegree = false;
        }
        else
        {
            dragPosition = mouseStartPosition;
            isInnerDegree = true;
        }
    }

    /// <summary>
    /// 터치하다 뗐을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 쏠 수 있으면 쏜다.
        if(isShot)
        {
            guideObject.HideLine();
            shootingObject.SetActive(false);
            guideObject.ShotObject(shootingColor);
            ShotSwapObject();
        }
        else
        {
            Debug.Log("Object 발사하지 않는다.");
        }

    }

    /// <summary>
    /// 드래그를 하고 있을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 direction = currentMousePosition - startPosition;

        float angle = Vector2.SignedAngle(Vector2.up, direction);
        float absoluteAngle = Mathf.Abs(angle);

        bool isDrawingLine = false;

        // 위아래 확인 하면서 각도를 확인한다.
        if (isUpper)
        {
            isDrawingLine = absoluteAngle <= 90 - degree ? true : false;
            isForward = true;
        }
        else
        {
            isDrawingLine = absoluteAngle >= 90 + degree ? true : false;
            isForward = false;
        }

        // 그릴지 말지 정한다.
        if(isDrawingLine)
        {
            guideObject.ShowLine();
            guideObject.UpdateLine(eventData.position, isForward);
            isShot = true;
        }
        else
        {
            guideObject.HideLine();
            isShot = false;
        }

    }

    /// <summary>
    /// 처음 드래그를 시작했을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInnerDegree)
            return;

        Vector2 mouseStartPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        isUpper = mouseStartPosition.y > dragPosition.y ? true : false;
    }

    private void ShotSwapObject()
    {
        var factory = Locator<Factory>.Get();

        // Color 변경
        shootingColor = addtiveColor;
        addtiveColor = (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(addtiveObject.transform.DOMove(startPosition, 0.1f));

        // Object 생성
        var settingMaterial = Locator<SettingMaterial>.Get();

        var tempIObject = factory.CreateObject(addtiveColor, startPosition + addtivePosition, false);
        var bubbleInformation = tempIObject.GetBubbleInformation();
        bubbleInformation.item = Item.None;
        tempIObject.SetBubbleInformation(bubbleInformation);
        GameObject tempObject = tempIObject.GetTransform().gameObject;
        
        settingMaterial.CreateMaterial(tempObject, bubbleInformation.item);

        shootingObject = addtiveObject;
        addtiveObject = tempObject;
        shootingObject.SetActive(true);

    }

    /// <summary>
    /// 서브로 저장된 object와 발사할 object를 바꾼다.
    /// </summary>
    public void SwapObject()
    {
        // 위치를 편하게 바꾸기 위해 Dotween을 사용한다.
        Sequence sequence = DOTween.Sequence();

        Vector3[] shootPath = new Vector3[]
        {
            startPosition + new Vector2(1,0),
            startPosition + addtivePosition,
          
        };

        Vector3[] addtivePath = new Vector3[]
        {
            startPosition + new Vector2(0,1),
            startPosition,
        };

        // 위치 변경
        sequence.Append(shootingObject.transform.DOPath(shootPath, 0.5f)).SetEase(Ease.Linear);
        sequence.Join(addtiveObject.transform.DOPath(addtivePath, 0.5f)).SetEase(Ease.Linear);

        // Color 변경
        BubbleColor tempColor = shootingColor;
        shootingColor = addtiveColor;
        addtiveColor = tempColor;
        
        // Object 변경
        GameObject tempObject = shootingObject;
        shootingObject = addtiveObject;
        addtiveObject = tempObject;
    }
}

// Line Width는 바꾸면 되니 상관없어.
// 지금해야 할 일을 적자.

// 터치했을 때 ->
// 현재 위치가 주어진 각도보다 크면 Guide를 그린다.
//                   각도보다 작으면 위치를 저장하고, 그리지 않아야해.
// 드래그를 시작했을 때 시작 위치 y를 비교해. y 값이 크면 direction은 그 방향 그대로 진행하면 되고,
//                                         y 값이 작으면 direction은 그 방향의 반대로 발사하게 해야해.
// 그리고 드래그를 하다가 각도보다 작거나 반대방향으로 갔다면 그리지 않아야해.

// 그린 상태에서 뗐을 때 Object 생성 아니라면 Object를 생성하지 않아야해.

// Object 위치에 대해서 DOTWeen을 해야하네.
