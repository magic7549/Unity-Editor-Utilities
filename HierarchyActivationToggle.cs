using UnityEngine;
using UnityEditor;

/// <summary>
/// 하이어라키 뷰의 각 게임 오브젝트에 마우스를 올리면 활성화/비활성화 토글(체크박스)을 표시
/// </summary>
[InitializeOnLoad]
public static class HierarchyActivationToggle
{
    private const float ToggleWidth = 16f;

    static HierarchyActivationToggle()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null) return;

        Event e = Event.current;

        var hoverRect = new Rect(0f, selectionRect.y, selectionRect.xMax, selectionRect.height);

        if (hoverRect.Contains(e.mousePosition))
        {
            var toggleRect = new Rect(
                33f,
                selectionRect.y,
                ToggleWidth,
                selectionRect.height
            );

            bool currentActiveState = gameObject.activeSelf;
            bool newActiveState = EditorGUI.Toggle(toggleRect, currentActiveState);

            if (newActiveState != currentActiveState)
            {
                Undo.RecordObject(gameObject, (newActiveState ? "Enable" : "Disable") + " " + gameObject.name);
                gameObject.SetActive(newActiveState);
            }

            if (e.type == EventType.MouseMove)
            {
                EditorApplication.RepaintHierarchyWindow();
            }
        }
    }
}