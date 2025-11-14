using UnityEngine;
using UnityEditor;

/// <summary>
/// Bubble을 만드는 Tool이다.
/// </summary>
[CustomEditor(typeof(BubbleToolkit))]
public class BubbleToolkitEditor : Editor
{
    private SerializedProperty bubblePrefab;
    private SerializedProperty maxRow;
    private SerializedProperty maxCol;
    private SerializedProperty parent;
    private SerializedProperty editData;

    private BubbleToolkit bubbleToolkit;


    private void OnEnable()
    {
        // Property Setting
        bubbleToolkit = (BubbleToolkit)target;
        bubblePrefab = serializedObject.FindProperty("bubblePrefab");
        editData = serializedObject.FindProperty("editData");
        maxRow = serializedObject.FindProperty("maxRow");
        maxCol = serializedObject.FindProperty("maxCol");
        parent = serializedObject.FindProperty("BubbleParent");
    }

    /// <summary>
    /// 실제 배치될 Tool
    /// </summary>
    public override void OnInspectorGUI()
    {
        // 최신 값으로 업데이트
        serializedObject.Update();

        // ----------------------------------------------------
        // 인스펙터 UI 그리기
        // ----------------------------------------------------

        // prefab 필드
        EditorGUILayout.PropertyField(bubblePrefab);
        EditorGUILayout.PropertyField(parent);
        EditorGUILayout.PropertyField(editData);

        // row와 col 필드
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Row", GUILayout.Width(35));
        maxRow.intValue = EditorGUILayout.IntField(maxRow.intValue);
        EditorGUILayout.LabelField("Col", GUILayout.Width(35));
        maxCol.intValue = EditorGUILayout.IntField(maxCol.intValue);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            // SetDimensions 메서드 호출
            bubbleToolkit.SetDimensions(maxRow.intValue, maxCol.intValue);

            // 변경사항 저장
            serializedObject.ApplyModifiedProperties();

            // 씬에 변경사항이 있음을 알림 (씬 저장을 위해)
            EditorUtility.SetDirty(bubbleToolkit);
        }
        // ----------------------------------------------------
        // 값 변경 감지 및 오브젝트 생성/파괴
        // ----------------------------------------------------

        // 값을 변경하는 버튼
        if (GUILayout.Button("Save Row Col Data"))
        {
            // SetDimensions 메서드 호출
            bubbleToolkit.SetDimensions(maxRow.intValue, maxCol.intValue);

            // 변경사항 저장
            serializedObject.ApplyModifiedProperties();

            // 씬에 변경사항이 있음을 알림 (씬 저장을 위해)
            EditorUtility.SetDirty(bubbleToolkit);
        }

        // ----------------------------------------------------
        // 수동 버튼 추가
        // ----------------------------------------------------
        EditorGUILayout.Space(10);

        // 보드 초기화 버튼
        if (GUILayout.Button("Clear Board"))
        {
            // Undo 시스템에 등록
            Undo.RecordObject(bubbleToolkit, "Clear Board");
            bubbleToolkit.ClearBoard();
        }

        // 경로 저장
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Save Path"))
        {
            // Undo 시스템에 등록
            Undo.RecordObject(bubbleToolkit, "Save Path");
            bubbleToolkit.SavePath();
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Refill Path"))
        {
            // Undo 시스템에 등록
            Undo.RecordObject(bubbleToolkit, "Refill Path");
            bubbleToolkit.RefillPath();
            EditorUtility.SetDirty(bubbleToolkit.editData);
        }

        // Default Inspector 그리기
        DrawDefaultInspector();
    }

    /// <summary>
    /// Ctrl + Alt + L 누르면 해당 Object를 Active를 활성화 비활성화 할 수 있다.
    /// </summary>
    [MenuItem("GameObject/Toggle Active %&l")]
    private static void ToggleActive()
    {
        // 선택된 모든 게임 오브젝트를 가져옵니다.
        foreach (GameObject go in Selection.gameObjects)
        {
            // 현재 활성화 상태를 반전시킵니다.
            Undo.RecordObject(go, "Toggle Active"); // Ctrl+Z를 위해 Undo를 기록합니다.
            go.SetActive(!go.activeSelf);
        }
    }
}
