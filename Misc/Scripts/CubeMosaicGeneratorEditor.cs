using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeMosaicGenerator))]

public class CubeMosaicGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CubeMosaicGenerator cmg = (CubeMosaicGenerator)target;

        //Version info in BOLD and blue text
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        GUILayout.Label("<size=20><b>Cube Mosaic Generator - v" + cmg.version + "</b></size>", style);

        //If a new version is available, show a button to open the download page
        if (cmg.newVersionAvailable)
        {
            if (GUILayout.Button("Download new version"))
            {
                Application.OpenURL("https://github.com/AlexInABox/mer-mosaic-generator/releases/latest");
            }
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Mosaic"))
        {
            cmg.GenerateMosaic();
        }

        if (GUILayout.Button("Clear Mosaic"))
        {
            cmg.removeExistingCubes();
        }

        //Text that shows the amount of children in the parent object
        GUILayout.Label("Number of cubes: " + cmg.transform.childCount);
    }
}
