





// use this script to get feild of unity project window study this in later time




using UnityEditor;
using UnityEngine;

public class ListProjectBrowserFields : EditorWindow
{
    [MenuItem("Tools/List ProjectBrowser Fields")]
    public static void ShowWindow()
    {
        // Show the custom window
        ListProjectBrowserFields window = (ListProjectBrowserFields)GetWindow(typeof(ListProjectBrowserFields));
        window.titleContent = new GUIContent("ProjectBrowser Fields");
        window.Show();

        // List the fields in the ProjectBrowser class
        ListFields();
    }

    private void OnGUI()
    {
        GUILayout.Label("Check the Console for a list of ProjectBrowser fields.", EditorStyles.wordWrappedLabel);
    }

    private static void ListFields()
    {
        // Get the ProjectBrowser type
        System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (projectBrowserType == null)
        {
            Debug.LogError("ProjectBrowser type not found");
            return;
        }

        // List all the fields in the ProjectBrowser class
        var fields = projectBrowserType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            Debug.Log($"Field: {field.Name}, Type: {field.FieldType}");
        }
    }
}


