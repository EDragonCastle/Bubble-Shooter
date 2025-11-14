using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tool을 동작하게 되면 관련 Data와 동작하기 위해 만든 클래스
/// </summary>
public class BubbleToolkit : MonoBehaviour
{
    [HideInInspector]
    public GameObject bubblePrefab;
    [HideInInspector]
    public int maxRow;
    [HideInInspector]
    public int maxCol;
    [HideInInspector]
    public GameObject BubbleParent;
    [HideInInspector]
    public int maxPath;
    [HideInInspector]
    public EditorData editData;

    public List<List<GameObject>> boardPannel = new List<List<GameObject>>();

    /// <summary>
    /// Board Object 전체 삭제
    /// </summary>
    public void ClearBoard()
    {
        if (boardPannel == null) return;

        foreach (var rowList in boardPannel)
        {
            foreach (var bubble in rowList)
            {
                if (bubble != null)
                    DestroyImmediate(bubble);
            }
        }
        boardPannel.Clear();
    }

    /// <summary>
    /// Row Col에 따른 Object 생성
    /// </summary>
    /// <param name="newRow">새로운 Row 값</param>
    /// <param name="newCol">새로운 Col 값</param>
    public void SetDimensions(int newRow, int newCol)
    {
        if (newRow < 0) newRow = 0;
        if (newCol < 0) newCol = 0;

        if (maxRow == newRow && maxCol == newCol)
            return;

        ClearBoard();

        maxRow = newRow;
        maxCol = newCol;

        int totalRow = (2 * maxRow) + 1;
        int totalCol = (2 * maxCol) + 1;

        boardPannel = new List<List<GameObject>>(maxRow);
        editData.board = new List<PathInformation>(totalRow * totalCol);
        editData.row = totalRow;
        editData.col = totalCol;

        for(int i = 0; i < totalRow * totalCol; i++)
        {
            editData.board.Add(new PathInformation()); 
        }

        // 반지름, 지름 세팅
        float bubbleRadius = 0.5f;
        float bubbleDiameter = 1f;

        var spriteRenderer = bubblePrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            bubbleRadius = spriteRenderer.bounds.extents.x;
            bubbleDiameter = bubbleRadius * 2f;
        }
        else
        {
            var circleCollider = bubblePrefab.GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                // 원형 콜라이더의 반지름을 사용 (스케일도 고려)
                bubbleRadius = circleCollider.radius * bubblePrefab.transform.localScale.x;
                bubbleDiameter = bubbleRadius * 2f;
            }
        }

        float yOffset = Mathf.Sqrt(3) * bubbleRadius;

        for (int i = 0; i < totalRow; i++)
        {
            var rowList = new List<GameObject>(maxCol);
            for (int j = 0; j < totalCol; j++)
            {
                GameObject bubbleObject = GameObject.Instantiate(bubblePrefab, BubbleParent.transform);

                int gridRow = i - maxRow;
                int gridCol = j - maxCol;

                float xPos = gridCol * bubbleDiameter;

                if (i % 2 != 0)
                {
                    xPos += bubbleRadius;
                }

                float yPos = -gridRow * yOffset;
                bubbleObject.transform.localPosition = new Vector3(xPos, yPos, 0);

                // Position Component;
                var bubbleComponent = bubbleObject.GetComponent<Bubble>();
                var pathInfomation = new PathInformation();
                pathInfomation.position = new Vector2(xPos, yPos);
                bubbleComponent.SetPathInformation(pathInfomation);

                rowList.Add(bubbleObject);
            }
            boardPannel.Add(rowList);
        }
    }

    /// <summary>
    /// Active False 한 Object들은 삭제한다. 
    /// </summary>
    public void SavePath()
    {
        // 살아있는 Object를 그냥두고, 나머지는 삭제해서 null로 만든다.
        if (boardPannel == null) return;

        // for int i로 바꿔야 할 수도 있다.
        for(int i = 0; i < boardPannel.Count; i++) 
        {
            for(int j = 0; j < boardPannel[i].Count; j++)
            {
                if (!boardPannel[i][j].activeSelf)
                {
                    DestroyImmediate(boardPannel[i][j]);
                }
            }
        }
    }

    /// <summary>
    /// Refill 경로를 저장한다.
    /// </summary>
    public void RefillPath()
    {
        if (boardPannel == null) return;

        int totalCol = (2 * maxCol) + 1;

        for (int i = 0; i < boardPannel.Count; i++)
        {
            for(int j = 0; j < boardPannel[i].Count; j++)
            {
                if(boardPannel[i][j] != null)
                {
                    var bubbleComponent = boardPannel[i][j].GetComponent<Bubble>();
                    var pathInformation = bubbleComponent.GetPathInformation();

                    if (pathInformation.path == 0)
                        continue;
                    int index = i * totalCol + j;
                    editData.board[index] = pathInformation;
                }
            }
        }
    }

}
