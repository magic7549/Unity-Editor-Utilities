using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 하이어라키 뷰의 씬 이름을 클릭하면 빌드 목록에 포함된 씬 목록이 드롭다운 형식으로 표시
/// </summary>
[InitializeOnLoad]
public class HierarchySceneSwitchDropdown
{
    private const string FavoriteScenePrefix = "SceneSwitchDropdown_Favorite_";

    private const float StarIconWidth = 20f;

    static HierarchySceneSwitchDropdown()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
    }

    private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null || !IsSceneHeader(instanceID))
        {
            return;
        }

        Scene scene = GetSceneByHandle(instanceID);
        if (!scene.IsValid()) return;

        Event e = Event.current;
        bool isMouseOver = selectionRect.Contains(e.mousePosition);

        if (isMouseOver)
        {
            Color hoverColor = EditorGUIUtility.isProSkin
                ? new Color(0.5f, 0.5f, 0.5f, 0.25f)
                : new Color(0.0f, 0.0f, 0.0f, 0.1f);

            Rect hoverRect = selectionRect;
            hoverRect.width -= StarIconWidth;
            EditorGUI.DrawRect(hoverRect, hoverColor);

            EditorApplication.RepaintHierarchyWindow();
        }

        Rect starRect = new Rect(selectionRect.x + selectionRect.width - StarIconWidth, selectionRect.y, StarIconWidth, selectionRect.height);

        bool isFavorite = IsFavorite(scene.path);

        if (isMouseOver)
        {
            string starIcon = isFavorite ? "★" : "☆";

            if (GUI.Button(starRect, starIcon, EditorStyles.label))
            {
                ToggleFavorite(scene.path);
                e.Use(); 
            }
        }

        Rect sceneNameRect = selectionRect;
        sceneNameRect.width -= StarIconWidth; 

        if (GUI.Button(sceneNameRect, "", GUIStyle.none))
        {
            ShowSceneSwitchMenu(selectionRect, scene);
            e.Use();
        }

        Rect arrowRect = new Rect(selectionRect);
        arrowRect.xMin = arrowRect.xMax - StarIconWidth * 2;
        arrowRect.xMax -= StarIconWidth;
        GUI.Label(arrowRect, "▼");
    }

    private static void ShowSceneSwitchMenu(Rect position, Scene currentScene)
    {
        GenericMenu sceneMenu = new GenericMenu();

        List<EditorBuildSettingsScene> sortedScenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .OrderByDescending(s => IsFavorite(s.path))
            .ToList();

        bool hasFavorites = false;
        bool separatorAdded = false;

        foreach (var buildScene in sortedScenes)
        {
            bool isFavorite = IsFavorite(buildScene.path);

            if (hasFavorites && !isFavorite && !separatorAdded)
            {
                sceneMenu.AddSeparator("");
                separatorAdded = true;
            }

            string sceneName = Path.GetFileNameWithoutExtension(buildScene.path);
            string labelText = sceneName + (isFavorite ? "\t★" : "");
            GUIContent menuLabel = new GUIContent(labelText);

            sceneMenu.AddItem(
                menuLabel,
                currentScene.path == buildScene.path,
                () => SwitchToScene(buildScene.path)
            );

            if (isFavorite) hasFavorites = true;
        }

        sceneMenu.DropDown(position);
    }

    private static bool IsFavorite(string scenePath)
    {
        return EditorPrefs.GetBool(FavoriteScenePrefix + scenePath, false);
    }

    private static void ToggleFavorite(string scenePath)
    {
        bool currentStatus = IsFavorite(scenePath);
        EditorPrefs.SetBool(FavoriteScenePrefix + scenePath, !currentStatus);
    }

    private static void SwitchToScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }

    private static bool IsSceneHeader(int instanceID)
    {
        return EditorUtility.InstanceIDToObject(instanceID) == null;
    }

    private static Scene GetSceneByHandle(int handle)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.handle == handle)
            {
                return scene;
            }
        }
        return default;
    }
}