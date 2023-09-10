using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Geometrize))]

public class GeometrizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Geometrize geo = (Geometrize)target;

        //Version info in BOLD and blue text
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        GUILayout.Label("<size=20><b>Geometrizer - v" + geo.versionString + "</b></size>", style);

        //If a new version is available, show a button to open the download page
        if (geo.newVersionAvailable)
        {
            if (GUILayout.Button("Download new version"))
            {
                Application.OpenURL("https://github.com/AlexInABox/mer-mosaic-generator/releases/latest");
            }
        }

        DrawDefaultInspector();


        if (GUILayout.Button("Generate Geometry"))
        {
            geo.Generate();
        }

        if (GUILayout.Button("Clear Geometry"))
        {
            geo.clearChildren();
        }
    }
}
