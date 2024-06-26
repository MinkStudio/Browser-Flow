using UnityEditor;
using UnityEngine;

public class BrowserFlow : EditorWindow
{
    [MenuItem("Window/Browser Flow/Custom Project Browser")]
    public static void ShowWindow()
    {
        // Show the custom window
        BrowserFlow window = (BrowserFlow)GetWindow(typeof(BrowserFlow));
        window.titleContent = new GUIContent("Custom Project Browser");
        window.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Move Project Browser"))
        {
            MoveProjectBrowser();
        }

        GUILayout.Label("This window allows you to manipulate the Project Browser.", EditorStyles.wordWrappedLabel);
    }

    private void MoveProjectBrowser()
    {
        // Get the Project Browser window
        System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (projectBrowserType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }

        EditorWindow projectBrowserWindow = EditorWindow.GetWindow(projectBrowserType);
        if (projectBrowserWindow == null)
        {
            Debug.LogError("Could not get ProjectBrowser window");
            return;
        }

        // Set the position and size of the Project Browser window
        Rect newRect = new Rect(100, 100, 800, 600);
        projectBrowserWindow.position = newRect;
        projectBrowserWindow.Close();

    }
}

