using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BrowserFlow : EditorWindow
{

    [MenuItem("Tools/Browser Flow/Browser Flow")]
    public static void ShowWindow()
    {
        // Show the custom window
        BrowserFlow BrowserFlow_window = (BrowserFlow)GetWindow(typeof(BrowserFlow));
        BrowserFlow_window.titleContent = new GUIContent("Browser Flow");
        BrowserFlow_window.Show();

        
    }

    private void OnGUI()
    {
       
        if (GUILayout.Button("Close Project Window"))
        {
            CloseProjectBrowser();
        }
        if (GUILayout.Button("Open Project Window"))
        {
            OpenProjectWindow();
        }

        GUILayout.Label("\n\n This window allows you to manipulate the Project Browser.", EditorStyles.wordWrappedLabel);
    }

    // toggle state of project window. and needed to have hotkey in unity editor
    [MenuItem("Tools/Browser Flow/Open-close &q")]
    static private void OpenCloseProjectBrowser()
    {
        if(IsProjectWindowOpen())
        {
            CloseProjectBrowser();
        }
        else
        {
            OpenProjectWindow();
        }

    }

    // check if project window is open or close.
    private static bool IsProjectWindowOpen()
    {
        System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (projectBrowserType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return false;
        }

        // check how many project window is open and if there is none, presumed window is close. it has some issue if there is more than one project window is opened.
        EditorWindow[] windows = Resources.FindObjectsOfTypeAll(projectBrowserType) as EditorWindow[];
        //Debug.Log(windows.Length);
        return windows != null && windows.Length > 0;
    }

    static private void CloseProjectBrowser()
    {
        //Get the Project Browser window
        System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (ProjectWindowType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }

        EditorWindow ProjectWindow = EditorWindow.GetWindow(ProjectWindowType);
        if (ProjectWindow == null)
        {
            Debug.LogError("Could not get ProjectBrowser window");
            return;
        }

        ProjectWindow.Close();

    }

    private static void OpenProjectWindow()
    {
        // Get the Project Browser window
        System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (ProjectWindowType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }


        EditorWindow ProjectWindow = EditorWindow.GetWindow(ProjectWindowType);
        if (ProjectWindow == null)
        {
            Debug.LogError("Could not get ProjectBrowser window");
            return;
        }

        ProjectWindow.Show();
        
    }
}


// '%' for CTRL (or CMD on macOS) | '#; for SHIFT | '&' for ALT | '_' for no modifiers like _g it's only g as hotkey