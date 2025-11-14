using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 실제 Bubble Board를 관리하고 있는 핵심 클래스
/// </summary>
public class BubbleBoard : MonoBehaviour, IChannel
{
    public int life = 22;
    // board 판
    private List<List<IObject>> board;

    // 첫 생성될 Object 위치를 저장해둔 List
    private List<Path> firstPathList;

    // 마지막 Object 위치
    private List<Path> lastPathList;

    // Object를 모아둘 곳
    public GameObject parent;
    public float duration = 0.01f;
    private int maxLife;

    private Factory factory;
    private DataManager dataManager;

    private List<List<Path>> sortingPath;
    private List<Vector2Int> oddIndex;
    private List<Vector2Int> evenIndex;

    private HashSet<Vector2Int> removeHashSet;
    private List<Vector2Int> futureRemoveList;
    private List<(int index, int path)> saveRemoveIndexList;

    private void Awake()
    {
        dataManager = Locator<DataManager>.Get();

        int rowData = dataManager.GetRowData();
        int colData = dataManager.GetColData();
        sortingPath = dataManager.GetSortingPathData();

        // List를 생성하기 위한 초기 단계
        board = new List<List<IObject>>();
        saveRemoveIndexList = new List<(int index, int path)>();
        removeHashSet = new HashSet<Vector2Int>();
        futureRemoveList = new List<Vector2Int>();
        oddIndex = new List<Vector2Int> {
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
        };

        evenIndex = new List<Vector2Int> {
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 0),
        };

        maxLife = life;
        // 일단 Board 에서 null로 채워 넣는다.
        for (int i = 0; i < rowData; i++)
        {
            var rowList = new List<IObject>();
            for (int j = 0; j < colData; j++)
            {
                rowList.Add(null);
            }
            board.Add(rowList);
        }

        firstPathList = new List<Path>();
        lastPathList = new List<Path>();
        factory = Locator<Factory>.Get();

        for (int i = 1; i < sortingPath.Count; i++)
        {
            // 처음 위치와 마지막 Path 위치를 저장해둔다.
            firstPathList.Add(sortingPath[i][0]);
            lastPathList.Add(sortingPath[i][sortingPath[i].Count-1]);
        }

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
    /// Event Manager에 사용할 IChannel Interface
    /// </summary>
    /// <param name="channel">채널 정보</param>
    /// <param name="information">사용할 Object 내용</param>
    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch (channel)
        {
            case ChannelInfo.CheckBoardObject:
                CheckinBoard checkBoard = information as CheckinBoard;
                if (checkBoard != null)
                {
                    var index = checkBoard.boardIndex;

                    int rowData = dataManager.GetRowData();
                    int colData = dataManager.GetColData();

                    if (index.x < 0 || index.y < 0 || rowData <= index.x || colData <= index.y)
                    {
                        break;
                    }

                    if (board[index.x][index.y] == null)
                        checkBoard.isNullObject = true;
                    else
                        checkBoard.isNullObject = false;
                }
                break;

            case ChannelInfo.InsertBoard:
                IObject boardObject = information as IObject;
                CheckDestoryObject(boardObject);
                DestoryBubble();
                //CreateBubble();
                TestCreateBubble();
                //RefillObject(); 추후에 하자. 지금은 기능 먼저다.
                CheckLife();
                break;
        }
    }

    /// <summary>
    /// Enable이 됐을 떄 실행하는 함수
    /// </summary>
    private void Enable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.CheckBoardObject, HandleEvent);
        eventManager.Subscription(ChannelInfo.InsertBoard, HandleEvent);
        life = maxLife;
        eventManager.Notify(ChannelInfo.LifeText, life.ToString());
        //CreateBubble();

        Invoke("TestCreateBubble", 2f);
    }

    /// <summary>
    /// Disable이 실행됐을 떄 실행하는 함수
    /// </summary>
    private void Disable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.CheckBoardObject, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.InsertBoard, HandleEvent);
        AllDestoryBubble();
    }

    #region Bubble Witch Saga의 Refill 방식

    private void TestCreateBubble()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.RefillBubble, true);
        TestRefill(eventManager);
    }

    private void TestRefill(EventManager eventManager)
    {
        if(!IsEmpty())
        {
            eventManager.Notify(ChannelInfo.RefillBubble, false);
            return;
        }

        Sequence stepSequence = DOTween.Sequence();
        var settingMaterial = Locator<SettingMaterial>.Get();

        for(int i = 0; i < firstPathList.Count; i++)
        {
            // 마지막 Object를 확인해서 비어 있으면 진행 차 있으면 넘긴다.
            if (!IsEmptyLastObject(i))
                continue;

            // board 첫 번째 Path에 object를 생성했다. 다음 Path로 이동시킨다.
            MoveObject(i);
            stepSequence.Join(TestAnimationMove(i));

            // random object 출력
            int randIndex = UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);
            // index 찾기
            var index = firstPathList[i].arrayIndex;
            // Object 생성
            var newObject = factory.CreateObject((BubbleColor)randIndex, firstPathList[i].position, parent.transform);
            newObject.GetTransform().position = firstPathList[i].position;

            // PathInformation Setting 
            var pathInfomation = new PathInformation();
            pathInfomation.position = firstPathList[i].position;
            pathInfomation.boardIndex = new Vector2(index.row, index.col);
            pathInfomation.path = 1;
            pathInfomation.index = i + 1;
            newObject.SetPathInformation(pathInfomation);

            // Bubble Information Setting
            var bubbleInformation = new BubbleInformation();
            bubbleInformation.bubbleColor = (BubbleColor)randIndex;
            randIndex = UnityEngine.Random.Range(0, 10);
            bubbleInformation.item = RandomItemBox(randIndex);
            newObject.SetBubbleInformation(bubbleInformation);

            // Material Setting
            settingMaterial.CreateMaterial(newObject.GetTransform().gameObject, bubbleInformation.item);

            // board판에 object 넣기
            board[index.row][index.col] = newObject;
        }

        // sequence가 종료되면 Refill 끝났다고 알려준다.
        stepSequence.OnComplete(() => {
            TestRefill(eventManager);
        });
    }

    private Sequence TestAnimationMove(int index)
    {
        Sequence sequence = DOTween.Sequence();

        for (int j = 1; j < sortingPath[index + 1].Count; j++)
        {
            var currentPath = sortingPath[index + 1][j];
            var previousPath = sortingPath[index +1][j - 1];

            var bubbleObject = board[currentPath.arrayIndex.row][currentPath.arrayIndex.col];

            if (bubbleObject != null)
            {
                var bubbleTransform = bubbleObject.GetTransform();
                bubbleTransform.position = previousPath.position;
                var targetPosition = new Vector3(currentPath.position.x, currentPath.position.y, 0);

                // Sequence에 정방향으로 출력한다.
                sequence.Join(bubbleTransform.DOMove(targetPosition, duration).SetEase(Ease.Linear));
            }
        }

        return sequence;
    }
    #endregion


    #region Bubble 생성 구 버전
    /// <summary>
    /// Bubble의 생성을 담당하는 함수
    /// </summary>
    private void CreateBubble()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.RefillBubble, true);

        var settingMaterial = Locator<SettingMaterial>.Get();

        bool isEmpty = IsEmpty();

        // 자연스러운 움직임을 구현하기 위해 DOTween을 사용한다.
        Sequence masterSequence = DOTween.Sequence();
        while (isEmpty)
        {
            // 각 시작 부분에서 Object 생성한다. 
            for (int i = 0; i < firstPathList.Count; i++)
            {
                // 마지막 Object를 확인해서 비어 있으면 진행 차 있으면 넘긴다.
                if (!IsEmptyLastObject(i))
                    continue;

                // board 첫 번째 Path에 object를 생성했다. 다음 Path로 이동시킨다.
                MoveObject(i);
                masterSequence.Join(AnimationMoveObject(i));

                // random object 출력
                int randIndex = UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);
                // index 찾기
                var index = firstPathList[i].arrayIndex;
                // Object 생성
                var newObject = factory.CreateObject((BubbleColor)randIndex, firstPathList[i].position, parent.transform);
                newObject.GetTransform().position = firstPathList[i].position;

                // PathInformation Setting 
                var pathInfomation = new PathInformation();
                pathInfomation.position = firstPathList[i].position;
                pathInfomation.boardIndex = new Vector2(index.row, index.col);
                pathInfomation.path = 1;
                pathInfomation.index = i + 1;
                newObject.SetPathInformation(pathInfomation);

                // Bubble Information Setting
                var bubbleInformation = new BubbleInformation();
                bubbleInformation.bubbleColor = (BubbleColor)randIndex;
                randIndex = UnityEngine.Random.Range(0, 10);
                bubbleInformation.item = RandomItemBox(randIndex);
                newObject.SetBubbleInformation(bubbleInformation);

                // Material Setting
                settingMaterial.CreateMaterial(newObject.GetTransform().gameObject, bubbleInformation.item);

                // board판에 object 넣기
                board[index.row][index.col] = newObject;
            }

            isEmpty = IsEmpty();
        }
        // sequence가 종료되면 Refill 끝났다고 알려준다.
        masterSequence.OnComplete(() => {
            eventManager.Notify(ChannelInfo.RefillBubble, false);
        });
    }

    // Path 경로대로 Object가 이동하게 한다.
    private Sequence AnimationMoveObject(int index)
    {
        Sequence sequence = DOTween.Sequence();

        for (int j = 1; j < sortingPath[index + 1].Count; j++)
        {
            var currentPath = sortingPath[index + 1][j];

            var bubbleObject = board[currentPath.arrayIndex.row][currentPath.arrayIndex.col];

            if (bubbleObject != null)
            {
                var bubbleTransform = bubbleObject.GetTransform();
                var targetPosition = new Vector3(currentPath.position.x, currentPath.position.y, 0);

                // Sequence에 정방향으로 출력한다.
                sequence.Append(bubbleTransform.DOMove(targetPosition, duration).SetEase(Ease.Linear));
            }
        }

        return sequence;
    }

    #endregion


    /// <summary>
    /// Random Index를 통해 item을 생성한다.
    /// </summary>
    /// <param name="randomNumber">random 값</param>
    /// <returns>Item</returns>
    private Item RandomItemBox(int randomNumber)
    {
        Item item = Item.None;
        if (randomNumber < 3)
            item = Item.Attack;
        else if (randomNumber < 6)
            item = Item.Bomb;
        else
            item = Item.None;

        return item;
    }

    
    /// <summary>
    /// object data를 움직인다.
    /// </summary>
    /// <param name="index"></param>
    private void MoveObject(int index)
    {
        // 역순으로 한다.
        for (int j = sortingPath[index + 1].Count - 1; j > 0; j--)
        {
            // 지금 이게 다음으로 갈 index
            var currentIndex = sortingPath[index + 1][j].arrayIndex;
            // 이전 위치 index와 현재 index
            var previousIndex = sortingPath[index + 1][j-1].arrayIndex;
            var currentPosition = sortingPath[index + 1][j].position;

            // 현재 object가 null일 수도 들어있을 수도 있지만 저장해둔다. -> Board를 이동한 것이다.
            var tempObject = board[previousIndex.row][previousIndex.col];

            // pathInfo도 바꿔줘야한다.
            if (tempObject != null)
            {
                var pathInfo = tempObject.GetPathInformation();
                pathInfo.position = currentPosition;
                pathInfo.boardIndex = new Vector2(currentIndex.row, currentIndex.col);
                pathInfo.path = j + 1;
                pathInfo.index = index + 1;
                tempObject.SetPathInformation(pathInfo);
            }

            board[currentIndex.row][currentIndex.col] = tempObject;
            board[previousIndex.row][previousIndex.col] = null;
        }
    }



    /// <summary>
    /// 첫번째 위치 object를 생성하는 함수
    /// </summary>
    /// <param name="index">index 값</param>
    private void FirstIndexCreateObject(int index)
    {
        // random object 출력
        var randIndex = UnityEngine.Random.Range(0, (int)BubbleColor.End - 1);
        // index 찾기
        var firstIndex = firstPathList[index].arrayIndex;
        // Object 생성
        var newObject = factory.CreateObject((BubbleColor)randIndex, firstPathList[index].position, parent.transform);
        newObject.GetTransform().position = firstPathList[index].position;

        // PathInformation Setting
        var pathInfomation = new PathInformation();
        pathInfomation.position = firstPathList[index].position;
        pathInfomation.boardIndex = new Vector2(firstIndex.row, firstIndex.col);
        pathInfomation.path = 1;
        pathInfomation.index = index + 1;
        newObject.SetPathInformation(pathInfomation);

        // Bubble Information Setting
        var bubbleInformation = new BubbleInformation();
        bubbleInformation.bubbleColor = (BubbleColor)randIndex;
        randIndex = UnityEngine.Random.Range(0, 10);
        bubbleInformation.item = RandomItemBox(randIndex);
        newObject.SetBubbleInformation(bubbleInformation);

        // board판에 object 넣기
        board[firstIndex.row][firstIndex.col] = newObject;
    }

    /// <summary>
    /// Path의 마지막이 비어있는지 차있는지 확인하는 함수
    /// </summary>
    /// <returns>bool type</returns>
    private bool IsEmpty()
    {
        for (int i = 0; i < lastPathList.Count; i++)
        {
            var lastIndex = lastPathList[i].arrayIndex;

            if (board[lastIndex.row][lastIndex.col] == null)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 마지막 object가 비어있는지 비어있지 않은지 확인하는 함수
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool IsEmptyLastObject(int index)
    {
        var lastIndex = lastPathList[index].arrayIndex;

        return board[lastIndex.row][lastIndex.col] == null ? true : false;
    }


    /// <summary>
    /// Bubble을 삭제를 담당하는 함수
    /// </summary>
    private void DestoryBubble()
    {
        foreach(var removeIndex in futureRemoveList)
        {
            factory.DestoryObject(board[removeIndex.x][removeIndex.y].GetTransform().gameObject);
            board[removeIndex.x][removeIndex.y] = null;
        }
        futureRemoveList.Clear();

        TravelConnectionBubble();
    }

    /// <summary>
    /// 모든 Bubble을 삭제하는 함수
    /// </summary>
    private void AllDestoryBubble()
    {
        for(int i = 0; i < board.Count; i++)
        {
            for(int j = 0; j < board[i].Count; j++)
            {
                if(board[i][j] != null)
                {
                    factory.DestoryObject(board[i][j].GetTransform().gameObject);
                    board[i][j] = null;
                }
            }
        }
    }

    /// <summary>
    /// 삭제 후 다음 index에서 천장에 닿는 지 안 닿는지 확인해서 안 닿으면 삭제 닿으면 삭제하지 않는 것을 판단하는 함수
    /// </summary>
    private void TravelConnectionBubble()
    {
        // Sort를 안해도 된다. 다음것만 확인하면 되니까
        // removeIndex의 다음것을 확인해야 해야 하네.
     
        foreach (var value in saveRemoveIndexList)
        {
            if (value.index == 0)
                continue;

            int length = sortingPath[value.index].Count;

            removeHashSet.Clear();
            if (length - 1 <= value.path)
                length -= 1;
            else
                length = value.path + 1;

            // index 값을 가져온다.
            var pathIndex = sortingPath[value.index][length].arrayIndex;
            Vector2Int index = new Vector2Int(pathIndex.row, pathIndex.col);

            int answer = 0;
            // 홀수 짝수에 따라 들어가는게 다르다.
            if (index.x % 2 != 0)
            {
                TravelDFS(index, true, ref answer);
            }
            else
            {
                TravelDFS(index, false, ref answer);
            }
            
            if(answer == 0)
            {
                // removeHashSet을 돌면서 삭제 해야 된다.
                // 1이면 천장에 닿았다는 것이니 삭제할 필요 없다.
                TravelDestoryObject();
            }
        }

        saveRemoveIndexList.Clear();
    }

    /// <summary>
    /// object를 둘러보고 삭제할 object가 있으면 삭제한다.
    /// </summary>
    private void TravelDestoryObject()
    {
        foreach (var removeIndex in removeHashSet)
        {
            factory.DestoryObject(board[removeIndex.x][removeIndex.y].GetTransform().gameObject);
            board[removeIndex.x][removeIndex.y] = null;
        }
        removeHashSet.Clear();
    }

    /// <summary>
    /// Board를 순회하는 핵심 함수
    /// </summary>
    /// <param name="index">index 값</param>
    /// <param name="isOdd">홀짝여부</param>
    /// <param name="answer">answer 값</param>
    private void TravelDFS(Vector2Int index, bool isOdd, ref int answer)
    {
        // out of range 방지
        if (index.x < 0 || index.y < 0 || board.Count <= index.x || board[0].Count <= index.y)
            return;
        
        // null error 방지
        if (board[index.x][index.y] == null)
            return;

        // 천장에 닿는지 확인 현재는 1이 천장
        if (index.x <= 1)
        {
            // 천장에 닿은 것이다.
            answer = 1;
            return;
        }

        var bubbleObject = board[index.x][index.y];
        var bubbleInformation = bubbleObject.GetBubbleInformation();

        // 방문했으면 Return 한다.
        if (bubbleInformation.isVisit)
            return;

        removeHashSet.Add(index);
        bubbleInformation.isVisit = true;
        bubbleObject.SetBubbleInformation(bubbleInformation);

        if (index.x % 2 != 0)
        {
            for (int i = 0; i < oddIndex.Count; i++)
            {
                if (answer == 1)
                    return;
                TravelDFS(index + oddIndex[i], true, ref answer);
            }
        }
        else
        {
            for (int i = 0; i < evenIndex.Count; i++)
            {
                if (answer == 1)
                    return;
                TravelDFS(index + evenIndex[i], false, ref answer);
            }
        }

        bubbleInformation.isVisit = false;
        bubbleObject.SetBubbleInformation(bubbleInformation);
    }

    /// <summary>
    /// 삭제할 object가 있는지 확인하는 함수
    /// </summary>
    /// <param name="checkObject">target object</param>
    private void CheckDestoryObject(IObject checkObject)
    {
        var pathInforamtion = checkObject.GetPathInformation();
        var bubbleInformation = checkObject.GetBubbleInformation();
        var boardIndex = pathInforamtion.boardIndex;
        removeHashSet.Clear();

        // out of Range도 어떻게 해결해야 한다.
        // 조건문 설치해서 List를 추가한다. row+1이 추가되면서 col도 추가되는거지.

        board[(int)boardIndex.x][(int)boardIndex.y] = checkObject;

        // 이제 삭제하면 되는데 board.index가 짝수인지 홀수인지 확인해야한다
        bool isDestory = false;
        if (boardIndex.x % 2 != 0)
        {
            for (int i = 0; i < oddIndex.Count; i++)
            {
                Vector2Int searchIndex = default;
                searchIndex.x = (int)boardIndex.x + oddIndex[i].x;
                searchIndex.y = (int)boardIndex.y + oddIndex[i].y;

                // tourList를 매번 초기화를 해야 하나?
                DFS(searchIndex, true, bubbleInformation.bubbleColor);

                if (!isDestory)
                {
                    // 어차피 여기서 삭제하게 된다.
                    if (removeHashSet.Count >= 3)
                    {
                        foreach (var removeIndex in removeHashSet)
                        {
                            RemoveIndex(removeIndex);
                        }

                        isDestory = true;
                        removeHashSet.Clear();
                    }
                }
                else
                {
                    foreach (var removeIndex in removeHashSet)
                    {
                        RemoveIndex(removeIndex);
                    }

                    isDestory = true;
                    removeHashSet.Clear();
                }
            }
        }
        else
        {
            for (int i = 0; i < evenIndex.Count; i++)
            {
                Vector2Int searchIndex = default;
                searchIndex.x = (int)boardIndex.x + evenIndex[i].x;
                searchIndex.y = (int)boardIndex.y + evenIndex[i].y;

                DFS(searchIndex, false, bubbleInformation.bubbleColor);
            }

            if (!isDestory)
            {
                if (removeHashSet.Count >= 3)
                {
                    foreach (var removeIndex in removeHashSet)
                    {
                        RemoveIndex(removeIndex);
                    }

                    isDestory = true;
                    removeHashSet.Clear();
                }
            }
            else
            {
                foreach (var removeIndex in removeHashSet)
                {
                    RemoveIndex(removeIndex);
                }

                isDestory = true;
                removeHashSet.Clear();
            }
        }

    }

    /// <summary>
    /// 삭제되는 object의 index를 가지고 board에서 삭제한다.
    /// </summary>
    /// <param name="removeIndex"></param>
    private void RemoveIndex(Vector2Int removeIndex)
    {
        var removeBubbleInformation = board[removeIndex.x][removeIndex.y].GetBubbleInformation();
        removeBubbleInformation.isDead = true;
        // 삭제하면서 item 값이 attack이면 보스에게 공격한다.
        if(removeBubbleInformation.item == Item.Attack)
        {
            var eventManager = Locator<EventManager>.Get();
            eventManager.Notify(ChannelInfo.BossAttack, 1);
        }    
        board[removeIndex.x][removeIndex.y].SetBubbleInformation(removeBubbleInformation);
        futureRemoveList.Add(removeIndex);

        // 삭제된 index와 path 저장.
        var removeBubblePathInformation = board[removeIndex.x][removeIndex.y].GetPathInformation();
        int index = removeBubblePathInformation.index;
        int path = removeBubblePathInformation.path;
        saveRemoveIndexList.Add((index, path));
    }

    /// <summary>
    /// 발사된 object를 순회하면서 같은 color가 3개 이상있는지 없는지 확인하는 함수
    /// </summary>
    /// <param name="index">index 값</param>
    /// <param name="isOdd">홀짝 여부</param>
    /// <param name="targetColor">target Object</param>
    private void DFS(Vector2Int index, bool isOdd, BubbleColor targetColor)
    {
        // 1. index를 확인해야 한다. out of range error 방지
        // 2. 그 후 board가 비어 있는지 확인
        // 3. 그 다음 방문여부를 확인하고 삭제여부 확인
        if (index.x < 0 || index.y < 0 || board.Count <= index.x || board[0].Count <= index.y)
            return;

        if (board[index.x][index.y] == null)
            return;

        var bubbleObject = board[index.x][index.y];
        var bubbleInformation = bubbleObject.GetBubbleInformation();

        // 색이 다르거나, 방문했고, 죽은 상태라면 return 한다.
        if (bubbleInformation.bubbleColor != targetColor || bubbleInformation.isVisit || bubbleInformation.isDead)
            return;

        // 위에 해당하지 않는다면
        bubbleInformation.isVisit = true;
        bubbleObject.SetBubbleInformation(bubbleInformation);

        // remove hashSet에 추가
        removeHashSet.Add(index);

        // index에 따라 방향이 다르다.
        if(index.x % 2 != 0)
        {
            for (int i = 0; i < oddIndex.Count; i++)
            {
                Vector2Int searchIndex = default;
                searchIndex.x = (int)index.x + oddIndex[i].x;
                searchIndex.y = (int)index.y + oddIndex[i].y;

                DFS(searchIndex, true, targetColor);
            }
        }
        else
        {
            for (int i = 0; i < evenIndex.Count; i++)
            {
                Vector2Int searchIndex = default;
                searchIndex.x = (int)index.x + evenIndex[i].x;
                searchIndex.y = (int)index.y + evenIndex[i].y;

                DFS(searchIndex, false, targetColor);
            }
        }

        bubbleInformation.isVisit = false;
        bubbleObject.SetBubbleInformation(bubbleInformation);

    }

    /// <summary>
    /// 현재 life 상태를 확인한다.
    /// </summary>
    private void CheckLife()
    {
        // Boss 체력 상태 확인해야 해.
        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.BossAttack, 0);

        // Gameover UI 생성
        if (life <= 0)
        {
            life = 0;
            var uiManager = Locator<UIManager>.Get();
            var gameOver = uiManager.GetUICloneObject(UIType.Gameover);
            var controller = uiManager.GetUICloneObject(UIType.Controller);
            var board = uiManager.GetUICloneObject(UIType.Board);

            controller.SetActive(false);
            board.SetActive(false);
            gameOver.SetActive(true);
        }
        else
            life -= 1;

        eventManager.Notify(ChannelInfo.LifeText, life.ToString());
    }
}
