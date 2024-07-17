using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;


public class BrowserFlow : EditorWindow
{
    // keep the key for Editor preference.
    private const string GridSizeKey = "ProjectWindow_GridSize";

    private static bool FocusTrigger = true;

    private static ProjectBrowserFocus ProjectWinState;
    private static EditorWindow ProjectWindow;

    // class constructor
    static BrowserFlow()
    {
        // make Scriptable object to can track focus change even when window close
        ProjectWinState = ScriptableObject.CreateInstance<ProjectBrowserFocus>();

        // create and initiate update event (i think it's for unity editor itself) that update with each frame.
        EditorApplication.update += UIUpdate;
    }

    // Not needed yet. it doesn't needed yet maybe for future update that i want add setting page.
    //[MenuItem("Tools/Browser Flow/Browser Flow")]
    public static void ShowWindow()
    {
        // Show the custom window
        BrowserFlow BrowserFlow_window = (BrowserFlow)GetWindow(typeof(BrowserFlow));
        BrowserFlow_window.titleContent = new GUIContent("Browser Flow");
        BrowserFlow_window.Show();
    }

    #region State of Project Window

    /// <summary>
    /// toggle state of project window. and needed to have hot-key in unity editor
    /// </summary>
    [MenuItem("Tools/Browser Flow/1 - Open-Close %SPACE")]
    private static void OpenCloseProjectBrowser()
    {
        if (IsProjectWindowOpen())
        {
            CloseProjectBrowser();
        }
        else
        {
            OpenProjectWindow();
        }
    }

    /// <summary>
    /// if user want to keep project window open and don't close it 
    /// </summary>
    [MenuItem("Tools/Browser Flow/2 - Focus On-Off %q")]
    private static void FocusLoseHandler()
    {
        FocusTrigger = !FocusTrigger;
    }

    /// <summary>
    /// check if project window is open or close.
    /// </summary>
    /// <returns> Return State(Open/Close) of Project Window</returns>
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

        return windows != null && windows.Length > 0;
    }

    #endregion

    #region Open Project Window
    /// <summary>
    /// open(show) Project window and set last grid size that user choose
    /// </summary>
    public static void OpenProjectWindow()
    {
        // Get the Project Browser window
        System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (ProjectWindowType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }


        ProjectWindow = EditorWindow.GetWindow(ProjectWindowType);
        if (ProjectWindow == null)
        {
            Debug.LogError("Could not get ProjectBrowser window");
            return;
        }

        //Set Grid size 
        var GridSizeField = ProjectWindowType.GetField("m_StartGridSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (GridSizeField != null)
        {
            int GridSize = EditorPrefs.GetInt(GridSizeKey, 16); // Default icon size is 16
            GridSizeField.SetValue(ProjectWindow, GridSize);
        }
        else
        {
            Debug.Log("m_StartGridSize is not found");
        }

        ProjectWindow.Show();
        ProjectWinState.FocusedCheck = true;
    }
    #endregion

    #region Close Project Window 
    /// <summary>
    /// Close project window and save it's grid size and last path user was in in editor preference
    /// </summary>
    private static void CloseProjectBrowser()
    {
        //Get the Project Browser window
        System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (ProjectWindowType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }

        ProjectWindow = EditorWindow.GetWindow(ProjectWindowType);
        if (ProjectWindow == null)
        {
            Debug.LogError("Could not get ProjectBrowser window");
            return;
        }

        // get and save grid size of project window
        var GridSizeField = ProjectWindowType.GetField("m_StartGridSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (GridSizeField != null) // check to be sure grid size not empty and check if project window exist
        {
            int GridSize = (int)GridSizeField.GetValue(ProjectWindow);
            EditorPrefs.SetInt(GridSizeKey, GridSize);
        }
        else
        {
            Debug.Log("m_StartGridSize is not found");
        }

        ProjectWindow.Close();
        //ProjectWindow = null;
    }

    #endregion

    #region UI Update

    /// <summary>
    /// This is event that fire with each UI update
    /// </summary>
    private static void UIUpdate()
    {

        // check for if project window has focus or not if not close project window
        if (ProjectWindow != null)
        {
            if (!ProjectWindow.IsFocused() && ProjectWinState.FocusedCheck && FocusTrigger)
            {
                CloseProjectBrowser();
            }
        }
    }
    #endregion

}


public class ProjectBrowserFocus : ScriptableObject
{
    public bool FocusedCheck = false;
}


// '%' for CTRL (or CMD on macOS) | '#; for SHIFT | '&' for ALT | '_' for no modifiers like _g it's only g as hotkey