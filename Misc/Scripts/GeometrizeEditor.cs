using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

[CustomEditor(typeof(Geometrize))]

public class GeometrizeEditor : Editor
{
    private bool? isNewVersionAvailable = null;

    void OnEnable()
    {
        // Start checking for a new version
        var task = (target as Geometrize)?.newVersionAvailable();
        task.ContinueWith(t => isNewVersionAvailable = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public override void OnInspectorGUI()
    {
        Geometrize geo = (Geometrize)target;

        //Version info in BOLD and blue text
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.26f, 0.62f, 0.28f, 1f);
        GUILayout.Label("<size=20><b>Geometrize (v" + geo.versionString + ")</b></size>", style);

        //If a new version is available, show a button to open the download page
        if (isNewVersionAvailable.HasValue && isNewVersionAvailable.Value)
        {
            if (GUILayout.Button("Download new version"))
            {
                Application.OpenURL("https://github.com/AlexInABox/mer-mosaic-generator/releases/latest");
            }
        }

        DrawDefaultInspector();


        if (GUILayout.Button("Convert Geometry"))
        {
            geo.convert();
        }

        if (GUILayout.Button("Clear Geometry"))
        {
            geo.clearChildren();
        }

        GUILayout.Label("Current amount of primitives: " + geo.transform.childCount);
    }
}
