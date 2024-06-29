using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BrowserFlow : EditorWindow
{
    // kep the key for Editor preference.
    private const string GridSizeKey = "ProjectWindow_GridSize";
    private const string LastSelectedPathKey = "ProjectWindow_LastSelectedPath";


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

    }

    #region State of Project Window

    /// <summary>
    /// toggle state of project window. and needed to have hot-key in unity editor
    /// </summary>
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

    #region Close Project Window 

    /// <summary>
    /// Close project window and save it's grid size and last path user was in in editor preference
    /// </summary>
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

        //close the window
        ProjectWindow.Close();

    }// CloseProjectBrowser function

    #endregion 


    #region Open Project Window

    /// <summary>
    /// open(show) Project window and set last grid size that user choose and go to last path user was in.
    /// </summary>
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

        //Set Grid size 
        var GridSizeField = ProjectWindowType.GetField("m_StartGridSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (GridSizeField != null)
        {
            int iconSize = EditorPrefs.GetInt(GridSizeKey, 16); // Default icon size is 16
            GridSizeField.SetValue(ProjectWindow, iconSize);
        }
        else
        {
            Debug.Log("m_StartGridSize is not found");
        }


        ProjectWindow.Show();
        
    }
    #endregion

}


// '%' for CTRL (or CMD on macOS) | '#; for SHIFT | '&' for ALT | '_' for no modifiers like _g it's only g as hotkey