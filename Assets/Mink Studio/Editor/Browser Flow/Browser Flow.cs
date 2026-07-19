using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace BrowserFlow
{
    public class BrowserFlow : EditorWindow
    {
        // Keep the key for Editor preference to save last grid size user choosed.
        private const string GridSizeKey = "ProjectWindow_GridSize";
        // Keep the key for Editor preference to save last folder path user was in.
        private const string LastFolderKey = "ProjectWindow_LastFolder";

        // Prevent project window from closing when user want to keep it open.
        private static bool FocusTrigger = true;

        // To track focus state of project window.
        private static ProjectBrowserFocus ProjectWinState;
        // Instanc of EditorWindow type to be populated with project window instance.
        private static EditorWindow ProjectWindow;


        #region Constructor
        // Class constructor of Browser flow script
        static BrowserFlow()
        {
            // Make Scriptable object to can track focus change even when window close
            ProjectWinState = ScriptableObject.CreateInstance<ProjectBrowserFocus>();

            // Create and initiate update event (i think it's for unity editor itself) that update with each frame.
            EditorApplication.update += UIUpdate;
        }
        #endregion

        // placeholder at this point 
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
        /// Toggle open/close state of project window. and it have hot-key(ctrl+spcae) in unity editor
        /// </summary>
        [MenuItem("Tools/Browser Flow/1 - Open-Close %SPACE")] // add sub menu in Unity editor in Tools menu that run function below
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
        /// If user want to keep project window open and don't be close when project window lost focus
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
            // Unity editor limitions that i have to use reflection to get project window type because it's not public class. so i have to get it by name.
            // there was limition that(can't remember the reason) i can't hide project menu so each time i have to close it or after each opening there was added to number of project window that open.
            // maybe my code is wrong, must check it again after that i finded solution to set path for project windows
            System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            if (projectBrowserType == null) // most likely not going to happen.
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
        /// Open(show) Project window and set last grid size that user choose and set last folder user visit
        /// </summary>
        public static void OpenProjectWindow()
        {
            // Get the Project Browser window
            System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");

            // i pretty sure no need of this. i think this was to find name of fields that used by unity editor for project window and make sure of existence of it
            // maybe it's better to keep it because maybe in future or old versio of unity they change it
            if (ProjectWindowType == null) 
            {
                Debug.LogError("ProjectBrowser type not found");
                return;
            }

            //this line get instace of project window
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

            //TODO : i should check what will happend if i put this after file path setter
            ProjectWindow.Show();

            // Set last visited folder
            string savedFolder = EditorPrefs.GetString(LastFolderKey, "");
            // this statement make sure savedfolder string be actual folder path not path to seleceted file or empty string. and check if folder is valid folder in project.
            if (!string.IsNullOrEmpty(savedFolder) && AssetDatabase.IsValidFolder(savedFolder)) // AssetDatabase : it's the API that manages everything about your project's Assets folder
            {
                // make the get id if folder in unity.
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savedFolder);
                if (obj != null)
                {
                    int id = obj.GetInstanceID();
                    var entityIdType = System.Type.GetType("UnityEngine.EntityId, UnityEngine.CoreModule");
                    var showFolderContents = ProjectWindowType.GetMethod(
                        "ShowFolderContents",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        new System.Type[] { entityIdType, typeof(bool) },
                        null);

                    if (entityIdType != null && showFolderContents != null)
                    {
                        var implicitOp = entityIdType.GetMethod("op_Implicit", new System.Type[] { typeof(int) });
                        object entityId = implicitOp.Invoke(null, new object[] { id });

                        var pw = ProjectWindow;

                        // delay by a tick in editor to make sure "showFolderContents" window layout be ready
                        //TODO : check without delay to see how relible it is or what will happened
                        EditorApplication.delayCall += () =>
                        {
                            showFolderContents.Invoke(pw, new object[] { entityId, true });
                        };
                    }
                }
            }

            ProjectWinState.FocusedCheck = true;
        }
        #endregion

        #region Close Project Window 
        /// <summary>
        /// Close project window and save it's grid size and last folder user was in.
        /// </summary>
        private static void CloseProjectBrowser()
        {
            //Get the Project Browser window type
            System.Type ProjectWindowType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            if (ProjectWindowType == null)
            {
                Debug.LogError("ProjectBrowser type not found");
                return;
            }

            //get Project window instance
            ProjectWindow = EditorWindow.GetWindow(ProjectWindowType);
            if (ProjectWindow == null)
            {
                Debug.LogError("Could not get ProjectBrowser window");
                return;
            }

            // Get and save grid size of project window
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

            //get last folder user was in and save it to EditorPrefs2
            var getActiveFolderPath = ProjectWindowType.GetMethod("GetActiveFolderPath"
                ,System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (getActiveFolderPath != null)
            {
                string activeFolder = getActiveFolderPath.Invoke(ProjectWindow, null) as string;
                //Debug.Log($"[BrowserFlow] GetActiveFolderPath = '{activeFolder}'");
                if (!string.IsNullOrEmpty(activeFolder))
                {
                    EditorPrefs.SetString(LastFolderKey, activeFolder);
                }
            }

            ProjectWindow.Close();
        }

        #endregion

        #region UI Update

        /// <summary>
        /// This is event that fire with each UI update(editor UI fps)
        /// </summary>
        private static void UIUpdate()
        {
            // Check for if project window has focus or not if not close project window
            if (ProjectWindow != null)
            {
                if (ProjectWindow != EditorWindow.focusedWindow && ProjectWinState.FocusedCheck && FocusTrigger)
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


    //TODO : '%' for CTRL (or CMD on macOS) | '#; for SHIFT | '&' for ALT | '_' for no modifiers like _g it's only g as hotkey
}   //TODO : if more than one projet window opened, they will close one by one first.