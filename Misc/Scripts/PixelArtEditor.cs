using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PixelArt))]

public class PixelArtEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PixelArt cmg = (PixelArt)target;

        //Version info in BOLD and blue text
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.9f, 0f, 0.2f, 1f);
        GUILayout.Label("<size=20><b>PixelArt - (v" + cmg.versionString + ")</b></size>", style);

        DrawDefaultInspector();

        //if the quality value is greater than 0.8 show a warning
        if (cmg.quality >= 0.1f)
        {
            EditorGUILayout.HelpBox("A quality value of 0.1 or higher is not recommended. This will result in a very high amount of primitives. Also Unity might crash or stall!", MessageType.Warning);
        }


        if (GUILayout.Button("Generate Mosaic"))
        {
            cmg.GenerateMosaic();
        }

        if (GUILayout.Button("Clear Mosaic"))
        {
            cmg.removeExistingCubes();
        }

        //Text that shows the amount of children in the parent object
        if (cmg.transform.childCount > 0)
        {
            if (cmg.useLightsources)
            {
                GUILayout.Label("Current amount of lightsources: " + cmg.transform.GetChild(0).childCount);
            }
            else
            {
                GUILayout.Label("Current amount of primitives: " + cmg.transform.childCount);
            }
        }

        //Foldout for the advanced settings
        cmg.showAdvancedSettings = EditorGUILayout.Foldout(cmg.showAdvancedSettings, "Advanced Settings");

        if (cmg.showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            //Show the advanced settings if the foldout is open
            cmg.cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cmg.cubePrefab, typeof(GameObject), true);
            cmg.useLightsources = EditorGUILayout.Toggle("Use Lightsources", cmg.useLightsources);

            cmg.sameColorMargin = EditorGUILayout.Slider("Same Color Margin", cmg.sameColorMargin, 0.01f, 1f);

            EditorGUI.indentLevel--;
        }

        if (cmg.useLightsources)
        {
            EditorGUILayout.HelpBox("Lightsources are still experimental and therefore might not work properly in-game!", MessageType.Error);
        }
    }
}
