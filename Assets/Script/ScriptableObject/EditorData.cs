using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tool을 동작해서 저장한 data들을 실제 게임에 적용하기 위해 만든 저장소
/// </summary>
[CreateAssetMenu(fileName = "EditorData", menuName = "Bubble/EditorData")]
public class EditorData : ScriptableObject
{
    // board 정보
    public List<PathInformation> board;

    // row, col 정보
    public int row;
    public int col;

    // row col에 맞는 공간확보
    public void InitailzeBoard(int row, int col) => board = new List<PathInformation>(row * col);
}

