using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace BrowserFlow
{
    public class BrowserFlow : EditorWindow
    {
        // keep the key for Editor preference.
        private const string GridSizeKey = "ProjectWindow_GridSize";
        private const string LastFolderKey = "ProjectWindow_LastFolder";

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

        // placeholder at this point 
        // Not needed yet. it doesn't needed yet maybe for future update that i want add setting page.
        // [MenuItem("Tools/Browser Flow/Browser Flow")]
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

            // set last visited folder
            string savedFolder = EditorPrefs.GetString(LastFolderKey, "");
            if (!string.IsNullOrEmpty(savedFolder))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savedFolder);
                if (obj != null)
                {
                    int id = obj.GetInstanceID();

                    var entityIdType = System.Type.GetType("UnityEngine.EntityId, UnityEngine.CoreModule");
                    var setFolderSelection = ProjectWindowType.GetMethod(
                        "SetFolderSelection",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        new System.Type[] { entityIdType.MakeArrayType(), typeof(bool) },
                        null);

                    if (entityIdType != null && setFolderSelection != null)
                    {
                        // Convert int -> EntityId via its implicit operator
                        var implicitOp = entityIdType.GetMethod("op_Implicit", new System.Type[] { typeof(int) });
                        object entityId = implicitOp.Invoke(null, new object[] { id });

                        // Build a properly-typed EntityId[] array
                        var idArray = System.Array.CreateInstance(entityIdType, 1);
                        idArray.SetValue(entityId, 0);

                        var pw = ProjectWindow; // capture for the closure
                        EditorApplication.delayCall += () =>
                        {
                            setFolderSelection.Invoke(pw, new object[] { idArray, true });
                        };
                    }
                }
            }


            //var methods = ProjectWindowType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            //foreach (var m in methods)
            //{
            //    if (m.Name.ToLower().Contains("folder") || m.Name.ToLower().Contains("select"))
            //    {
            //        var pars = string.Join(", ", System.Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name));
            //        Debug.Log($"[BrowserFlow] Method: {m.Name}({pars})");
            //    }
            //}


            //var entityIdType = System.Type.GetType("UnityEngine.EntityId, UnityEngine.CoreModule");
            //if (entityIdType == null)
            //{
            //    // try alternate assembly names if the above fails
            //    Debug.Log("[BrowserFlow] EntityId type not found under CoreModule, trying UnityEditor");
            //    entityIdType = System.Type.GetType("UnityEngine.EntityId, UnityEditor");
            //}

            //Debug.Log($"[BrowserFlow] EntityId type found: {entityIdType != null}");

            //if (entityIdType != null)
            //{
            //    foreach (var ctor in entityIdType.GetConstructors())
            //    {
            //        var pars = string.Join(", ", System.Array.ConvertAll(ctor.GetParameters(), p => p.ParameterType.Name));
            //        Debug.Log($"[BrowserFlow] EntityId ctor: ({pars})");
            //    }

            //    foreach (var m in entityIdType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            //    {
            //        if (m.Name.Contains("Implicit") || m.Name.Contains("op_"))
            //        {
            //            var pars = string.Join(", ", System.Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name));
            //            Debug.Log($"[BrowserFlow] EntityId operator: {m.Name}({pars}) -> {m.ReturnType.Name}");
            //        }
            //    }
            //}
            //Debug.Log(EditorPrefs.GetString(LastFolderKey));

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

            var selectedPathField = ProjectWindowType.GetField("m_SelectedPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (selectedPathField != null)
            {
                string selectedPath = selectedPathField.GetValue(ProjectWindow) as string;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    EditorPrefs.SetString(LastFolderKey, selectedPath);
                }
            }

            // log fileds
            //var fields = ProjectWindowType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            //foreach (var f in fields)
            //{
            //    if (f.Name.ToLower().Contains("folder") || f.Name.ToLower().Contains("path") || f.Name.ToLower().Contains("select"))
            //    {
            //        object val = f.GetValue(ProjectWindow);
            //        Debug.Log($"[BrowserFlow] Field: {f.Name} ({f.FieldType.Name}) = {val}");
            //    }
            //}


            ProjectWindow.Close();
            //ProjectWindow = null;
            //Debug.Log(EditorPrefs.GetString(LastFolderKey));
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
            //Debug.Log(EditorPrefs.GetString(LastFolderKey));
        }
        #endregion

    }


    public class ProjectBrowserFocus : ScriptableObject
    {
        public bool FocusedCheck = false;
    }


    // '%' for CTRL (or CMD on macOS) | '#; for SHIFT | '&' for ALT | '_' for no modifiers like _g it's only g as hotkey
}
