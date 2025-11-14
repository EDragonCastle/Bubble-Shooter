using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tool에서 만든 Data를 관리하는 Manager
/// </summary>
public class DataManager
{
    private EditorData data;
    private List<List<Path>> pathList;
    private GameObject shooter;

    /// <summary>
    /// DataManager 생성자
    /// </summary>
    /// <param name="_scriptableData">EditData 값</param>
    public DataManager(EditorData _scriptableData)
    {
        data = _scriptableData;
        pathList = new List<List<Path>>();
        SortPathInformation();
    }

    /// <summary>
    /// Edit 정보를 가지고 경로 값에 따라 정렬해주는 함수
    /// </summary>
    private void SortPathInformation()
    {
        // data 보드를 돌면서 값을 넣어준다.
        for(int i = 0; i < data.board.Count; i++)
        {
            var currentPathInfo = data.board[i];

            if(currentPathInfo.path == 0 || currentPathInfo.index == 0)
                continue;
          
            while(pathList.Count <= currentPathInfo.index)
            {
                pathList.Add(new List<Path>());
            }

            // Path 정보를 넣는다.
            var newPath = new Path();
            newPath.arrayIndex = (i / data.col, i % data.col);
            newPath.position = currentPathInfo.position;
            newPath.pathIndex = currentPathInfo.path;

            pathList[currentPathInfo.index].Add(newPath);
        }

        // Path의 값에 맞게 정렬한다.
        for(int i = 0; i < pathList.Count; i++)
        {
            if(pathList[i].Count > 1)
            {
                pathList[i].Sort((a, b) => a.pathIndex.CompareTo(b.pathIndex));
            }
        }
    }

    // properties
    #region Properties
    public List<PathInformation> GetPathData() => data.board;
    public List<List<Path>> GetSortingPathData() => pathList;
    public GameObject GetShooter() => shooter;
    public void SetShooter(GameObject _shooter) => shooter = _shooter; 

    // total array index Properties
    public int GetRowData() => data.row;
    public int GetColData() => data.col;
    #endregion

    /// <summary>
    /// row와 col값을 가지고 실제 위치값을 출력해주는 함수
    /// </summary>
    /// <param name="row">row</param>
    /// <param name="col">col</param>
    /// <returns></returns>
    public Vector2 GetArrayPosition(int row, int col) 
    {
        // totalRow와 totalCol에서 실제 row와 col을 되돌린다.
        int originRow = (data.row - 1) / 2;
        int originCol = (data.col - 1) / 2;

        int gridRow = row - originRow;
        int gridCol = col - originCol;

        float xPosition = gridCol;
        float yOffset = Mathf.Sqrt(3) * 0.5f;

        // 홀짝인지 검사
        if(row % 2 != 0)
        {
            xPosition += 0.5f;
        }

        float yPosition = -gridRow * yOffset;

        Vector2 position = new Vector2(xPosition, yPosition);

        return position; 
    }


}

/// <summary>
/// Path 정보를 담은 구조체
/// </summary>
public struct Path
{
    public (int row, int col) arrayIndex;
    public Vector2 position;
    public int pathIndex;
}
