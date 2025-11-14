using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Object가 어디에 위치될지 알려주는 클래스
/// </summary>
public class GuideLine : MonoBehaviour
{
    public int maxBounce = 3;
    public float maxLength = 100f;
    public float duration = 0.1f;

    public GameObject guideObject;
    private LineRenderer lineRenderer;

    private Vector2Int[] oddOffsets;
    private Vector2Int[] evenOffsets;

    private Vector2 targetPosition;
    private Vector2Int targetIndex;
    private void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        oddOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
        };

        evenOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 0),
        };
        var dataManger = Locator<DataManager>.Get();
        dataManger.SetShooter(this.gameObject);
    }

    /// <summary>
    /// 발사되는 위치를 Line으로 알려준다.
    /// </summary>
    public void ShowLine()
    {
        lineRenderer.enabled = true;
        guideObject.SetActive(true);
    }

    /// <summary>
    /// 선을 계속 그리면서 알려주는 함수
    /// </summary>
    /// <param name="mousePosition">마우스 위치</param>
    /// <param name="isForward">정방향 역방향</param>
    public void UpdateLine(Vector2 mousePosition, bool isForward)
    {
        // 마우스 위치를 카메라가 보고있는 위치로 옮긴다.
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = mouseWorldPosition - (Vector2)this.transform.position;

        // 방향 계산 후 그린다.
        direction = isForward ? direction.normalized : -direction.normalized;

        DrawGuideLine(direction);
    }

    /// <summary>
    /// 발사되는 위치를 알려주는 Line을 숨긴다.
    /// </summary>
    public void HideLine()
    {
        lineRenderer.enabled = false;
        guideObject.SetActive(false);
    }

    /// <summary>
    /// Line을 그리기 위해 필요한 함수
    /// </summary>
    /// <param name="direction">방향</param>
    private void DrawGuideLine(Vector2 direction)
    {
        // 선을 그리기 위한 필요한 좌표 data
        var linePoints = new List<Vector3>();

        Vector2 currentPosition = this.transform.position;
        Vector2 currentDirection = direction;

        linePoints.Add(currentPosition);

        // 튕기는 횟수만큼 검사를 한다.
        for(int i = 0; i < maxBounce + 1; i++)
        {
            // ray를 쏴서 확인한다.
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, maxLength);
            
            // collider 에 부딪혔다면 실행
            if(hit.collider != null)
            {
                // 어떤 collider에 닿았다.
                linePoints.Add(hit.point);

                // wall이면 튕군다.
                if (hit.collider.CompareTag("Wall"))
                {
                    currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point + currentDirection * 0.01f;
                }
                else
                {
                    // object라면
                    // 닿은 object의 index의 position을 확인한다.
                    var bubble = hit.collider.gameObject.GetComponent<Bubble>();

                    if (bubble != null)
                    {
                        // 버블 정보를 가져온다.
                        var bubbleInformation = bubble.GetPathInformation();
                        // 닿아있는 bubble의 index와 실제 위치를 가져온다.
                        var bubbleBoardIndex = bubbleInformation.boardIndex;
                        var bubblePosition = bubbleInformation.position;

                        // 근데 해당 index를 알게 되었어. 이걸 통해 position을 알 수 있으려나?
                        targetIndex = SearchIndex(bubblePosition, bubbleBoardIndex, hit.point);

                        var dataManager = Locator<DataManager>.Get();
                        targetPosition = dataManager.GetArrayPosition(targetIndex.x, targetIndex.y);
                        guideObject.SetActive(true);
                        guideObject.transform.localPosition = targetPosition;
                    }
                    else
                    {
                        guideObject.SetActive(false);
                        targetPosition = default;
                    }

                    break;
                }
            }
            else
            {
                linePoints.Add(currentPosition + currentDirection * maxLength);
                break;
            }
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    // 여기서 생성이 맞냐 아니냐는 Guide Line에서 hit collider를 사용하면서 가지고 있는 point들이 있어서 그걸 이용하면 된다.
    public void ShotObject(BubbleColor color)
    {
        var positionCount = lineRenderer.positionCount;
        Vector3[] points = new Vector3[positionCount];

        lineRenderer.GetPositions(points);
        // Shot한 Object는 Controll에 있는 Object와 다른 것이다. O
        var factory = Locator<Factory>.Get();
        var bubbleObject = factory.CreateObject(color, points[0]);

        // Path Information 생성
        var pathInfomation = new PathInformation();
        pathInfomation.position = targetPosition;
        pathInfomation.boardIndex = targetIndex;
        pathInfomation.path = int.MinValue;
        bubbleObject.SetPathInformation(pathInfomation);

        // Bubble Information 생성
        var bubbleInformation = new BubbleInformation();
        bubbleInformation.bubbleColor = color;
        bubbleInformation.item = Item.None;
        bubbleObject.SetBubbleInformation(bubbleInformation);

        var settingMaterial = Locator<SettingMaterial>.Get();
        settingMaterial.CreateMaterial(bubbleObject.GetTransform().gameObject, bubbleInformation.item);

        var bubbleTransform = bubbleObject.GetTransform();
        Sequence sequence = DOTween.Sequence();

        for (int i = 1; i < positionCount - 1; i++)
        {
            sequence.Append(bubbleTransform.DOMove(points[i], duration)).SetEase(Ease.Linear);
        }

        sequence.Append(bubbleTransform.DOMove(targetPosition, duration)).SetEase(Ease.Linear)
            .OnComplete(()=> {
                var eventManager = Locator<EventManager>.Get();
                eventManager.Notify(ChannelInfo.InsertBoard, bubbleObject);
            });
    }


    /// <summary>
    /// 실제 Board 판에서 index 위치를 찾는 함수다.
    /// </summary>
    /// <param name="originPosition">원본 위치</param>
    /// <param name="originIndex">원본 index</param>
    /// <param name="point">점</param>
    /// <returns></returns>
    private Vector2Int SearchIndex(Vector2 originPosition, Vector2 originIndex, Vector2 point)
    {
        // 이제 hit.point를 가지고 만들어야 한다.
        // 원 기준으로 6등분으로 나누고 원점과 원의 둘레에 닿아있는 hitPoint를 이용한다.
        // 극좌표계 기준 이런식으로 나뉜다.
        // 1. [row][col+1] : 330 ~ 30 
        // 2. [row-1][col] : 30 ~ 90
        // 3. [row-1][col-1] : 90 ~ 150
        // 4. [row][col-1] : 150 ~ 210
        // 5. [row+1][col-1] : 210 ~ 270
        // 6. [row+1][col] : 270 ~ 330
        // originePosition이랑 point를 가지고 각도를 구한다. 각도가 0~360도에 따라서 위에 있는 menual 대로 진행하면 된다.
        

        // 방향을 구한다.
        Vector2 direction = point - originPosition;

        // 각도를 구하고
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 음수면 양수로 바꿔준다.
        if(angle < 0)
            angle += 360;

        // 각도를 60으로 나눠 sector를 구한다. + 30을 한 이유는 0으로 하면 0~60 60~120 120~180으로 검사하기 때문에 30도를 회전시킨다.
        int sector = Mathf.FloorToInt((angle + 30) / 60f);
        // oddOffset index을 넘기면 안 되니 나머지값을 이용한다.
        sector %= oddOffsets.Length;

        if (sector < 0)
            sector = 0;

        Vector2Int offset = default;

        // 벌집모양 array에서 홀짝에 따라 row, col 값이 달라져서 홀짝 판단을 해야한다.
        if (originIndex.x % 2 != 0)
            offset = oddOffsets[sector];
        else
            offset = evenOffsets[sector];

        // 그만큼 index를 더하고
        Vector2Int checkIndex = new Vector2Int((int)originIndex.x + offset.x, (int)originIndex.y + offset.y); 
        
        // index 두개를 넣어서 board에 있는 object가 있는지 없는지 확인한다.
        var eventManager = Locator<EventManager>.Get();
        var checkBoard = new CheckinBoard();
        checkBoard.boardIndex = checkIndex;
        // 여기서 board index를 가지고 object가 있는지 없는지 판단한다.
        eventManager.Notify(ChannelInfo.CheckBoardObject, checkBoard);

        // isNullObject booltype으로 비어 있으면 그 자리로 출력 비어 있지 않다면 뭔가를 해야한다.
        if(checkBoard.isNullObject)
        {
            return checkIndex;
        }
        else
        {
            // 있다면 검사한다.
            bool isLeft = ((90f < angle) && (angle < 270)) ? true : false;

            if (isLeft)
                sector++;
            else
                sector--;

            if (sector < 0)
                sector = 0;

            sector %= oddOffsets.Length;

            if (originIndex.x % 2 != 0)
                offset = oddOffsets[sector];
            else
                offset = evenOffsets[sector];

            checkIndex = new Vector2Int((int)originIndex.x + offset.x, (int)originIndex.y + offset.y);
            return checkIndex;
        }
    }
}

/// <summary>
/// board 판에서 index를 넣으면 object가 있는지 없는지 알려주는 struct
/// </summary>
public class CheckinBoard
{
    public Vector2Int boardIndex;
    public bool isNullObject;
}