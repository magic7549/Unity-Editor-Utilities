using UnityEngine;
using UnityEditor;

/// <summary>
/// 하이어라키 뷰의 부모 자식 간 선으로 연결
/// </summary>
[InitializeOnLoad]
public static class HierarchyLines
{
    private static readonly Color LineColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private static readonly float LineWidth = 1f;

    static HierarchyLines()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null) return;

        if (gameObject.transform.parent == null) return;

        float indent = GetIndent(gameObject.transform);

        float horizontalLineY = selectionRect.y + selectionRect.height / 2;
        var horizontalLineRect = new Rect(indent, horizontalLineY, 7f, LineWidth);
        EditorGUI.DrawRect(horizontalLineRect, LineColor);

        if (IsLastChild(gameObject.transform))
        {
            var verticalLineRect = new Rect(indent, selectionRect.y, LineWidth, selectionRect.height / 2);
            EditorGUI.DrawRect(verticalLineRect, LineColor);
        }
        else
        {
            var verticalLineRect = new Rect(indent, selectionRect.y, LineWidth, selectionRect.height);
            EditorGUI.DrawRect(verticalLineRect, LineColor);
        }

        var parent = gameObject.transform.parent;
        while (parent != null && parent.parent != null)
        {
            if (!IsLastChild(parent))
            {
                float parentIndent = GetIndent(parent);
                var parentLineRect = new Rect(parentIndent, selectionRect.y, LineWidth, selectionRect.height);
                EditorGUI.DrawRect(parentLineRect, LineColor);
            }
            parent = parent.parent;
        }
    }

    private static float GetIndent(Transform transform)
    {
        return 52f + (GetDepth(transform) - 1) * 14f;
    }

    private static int GetDepth(Transform transform)
    {
        int depth = 0;
        while (transform.parent != null)
        {
            transform = transform.parent;
            depth++;
        }
        return depth;
    }

    private static bool IsLastChild(Transform transform)
    {
        var parent = transform.parent;
        if (parent == null) return true;
        return transform.GetSiblingIndex() == parent.childCount - 1;
    }
}