using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class GeometrizeVisualizer : MonoBehaviour
{
    [HideInInspector]
    public string versionString = "0.0.2 (CIRCLES ONLY)";

    [HideInInspector]
    public bool newVersionAvailable = false;
    // Input json file
    [Header("Input")]
    public TextAsset jsonFile;

    [Space(10)]
    [Header("Settings")]
    [Tooltip("1 = one in-game door")]
    [Range(0.01f, 1f)]
    public float size = 1f;
    public bool collidable = false;

    [Tooltip("Automatically clear all children before generating a new image")]
    public bool autoClear = true;

    [HideInInspector]
    [Header("Prefabs")]
    public GameObject cubePrefab;
    [HideInInspector]
    public GameObject spherePrefab;

    public void Convert()
    {

        if (autoClear)
        {
            clearChildren();
        }

        //This is an example JSON file
        /*
        {"shapes":
        [{"type":1, "data":[0,0,256,180],"color":[227,209,154,255],"score":0.19267},
        {"type":8, "data":[126,91,74,72],"color":[211,153,3,128],"score":0.167187},
        {"type":32, "data":[16,14,75],"color":[251,255,255,128],"score":0.152495},
        {"type":1, "data":[186,0,255,97],"color":[238,255,255,128],"score":0.140048},
        {"type":2, "data":[32,74,109,158,3],"color":[189,149,21,128],"score":0.133693},
        {"type":16, "data":[135,49,40,51,267],"color":[255,200,0,128],"score":0.126617}
        ]}
        */
        /*
        type: 1 = rectangle, 2 = rotated rectangle, 8 = ellipse, 16 = rotated ellipse, 32 = circle
        */
        //A rectangle is a cube prefab with a scale of (width, height, 1) and a position of (x, y, 0.01)
        //A rotated rectangle is a cube prefab with a scale of (width, height, 1) and a position of (x, y, 0.01) and a rotation of (angle, 0, 0)
        //A circle is a sphere prefab with a scale of (radius, radius, radius) and a position of (x, y, 0.01)
        //An ellipse is a sphere prefab with a scale of (radius, radius, radius) and a position of (x, y, 0.01) and a rotation of (angle, 0, 0)
        //A rotated ellipse is a sphere prefab with a scale of (radius, radius, radius) and a position of (x, y, 0.01) and a rotation of (angle, 0, 0)

        //We only care about the type, data, and color
        //In Unity these will be represented as GameObjects, Position and Scale, and Color


        // Parse the JSON data
        var shapeData = JsonConvert.DeserializeObject<ShapeData>(jsonFile.text);

        if (shapeData.shapes == null || shapeData.shapes.Count == 0)
        {
            Debug.Log("No shapes found");
            return;
        }

        var wichShape = 10; //inital distance from the canvas so that no clipping occurs
        // Loop through each shape
        foreach (var shape in shapeData.shapes)
        {
            // Now you can access shape properties as expected
            if (wichShape == 10)
            {
                createCanvas(shape.data, shape.color);
            }
            else
            {
                switch (shape.type)
                {
                    case 32:
                        createCircle(shape.data, shape.color, wichShape);
                        break;
                    case 8:
                        createEllipse(shape.data, shape.color, wichShape);
                        break;
                    case 16:
                        createRotatedEllipse(shape.data, shape.color, wichShape);
                        break;
                    case 1:
                        createCube(shape.data, shape.color, wichShape);
                        break;
                    case 2:
                        createRotatedCube(shape.data, shape.color, wichShape);
                        break;
                    default:
                        Debug.Log("Shape Type: " + shape.type + " not supported");
                        break;
                }
            }
            wichShape++;
        }

        //trigger recompilation of all scripts
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    private Transform canvas;

    float globalSizeMultiplier; //the value by wich to multiply so that the objects fit on the canvases new size

    void createCanvas(List<int> data, List<int> color)
    {
        //the canvas will have the height of 3. so we need to calculate the width and set the globalSizeMultiplier
        globalSizeMultiplier = (size * 3f) / data[3]; //when the height is 300 (pixels) the multiplier will be 0.01

        // Create a new cube
        GameObject cube = Instantiate(cubePrefab, transform);
        //Set parent    
        cube.transform.parent = transform;
        // Set the position
        cube.transform.localPosition = new Vector3(data[0], data[1], 0f);
        // Set the scale
        cube.transform.localScale = new Vector3(data[2] * globalSizeMultiplier, 3, 0.001f);
        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        cube.GetComponent<PrimitiveComponent>().Color = colorVar;
        cube.GetComponent<PrimitiveComponent>().Collidable = collidable;

        canvas = cube.transform;
    }

    void createCube(List<int> data, List<int> color, int position)
    {
        // Create a new cube
        GameObject cube = Instantiate(cubePrefab, transform);
        // Set the position
        //data[0] and data[1] are the x and y coordinates of the top left corner of the rectangle
        //data[2] and data[3] are the x and y coordinates of the bottom right corner of the rectangle

        float middleOfXCorners = ((data[2] - data[0]) / 2) + data[0];
        float middleOfYCorners = ((data[3] - data[1]) / 2) + data[1];
        Vector3 vectorToMiddle = new Vector3(middleOfXCorners * globalSizeMultiplier, middleOfYCorners * globalSizeMultiplier, 0);

        cube.transform.localPosition = new Vector3(canvas.localScale.x / 2, canvas.localScale.y / 2, 0) - vectorToMiddle + new Vector3(0, 0, 0.0001f * position);

        // Set the scale
        cube.transform.localScale = new Vector3((data[2] - data[0]) * globalSizeMultiplier, (data[3] - data[1]) * globalSizeMultiplier, 0.00001f);

        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        cube.GetComponent<PrimitiveComponent>().Color = colorVar;
        cube.GetComponent<PrimitiveComponent>().Collidable = collidable;
    }

    void createRotatedCube(List<int> data, List<int> color, int position)
    {
        // Create a new cube
        GameObject cube = Instantiate(cubePrefab, transform);
        // Set the position
        //data[0] and data[1] are the x and y coordinates of the top left corner of the rectangle
        //data[2] and data[3] are the x and y coordinates of the bottom right corner of the rectangle

        float middleOfXCorners = ((data[2] - data[0]) / 2) + data[0];
        float middleOfYCorners = ((data[3] - data[1]) / 2) + data[1];
        Vector3 vectorToMiddle = new Vector3(middleOfXCorners * globalSizeMultiplier, middleOfYCorners * globalSizeMultiplier, 0);

        cube.transform.localPosition = new Vector3(canvas.localScale.x / 2, canvas.localScale.y / 2, 0) - vectorToMiddle + new Vector3(0, 0, 0.0001f * position);

        // Set the scale
        cube.transform.localScale = new Vector3((data[2] - data[0]) * globalSizeMultiplier, (data[3] - data[1]) * globalSizeMultiplier, 0.00001f);

        //Set rotation
        cube.transform.localRotation = Quaternion.Euler(0, 0, data[4]);

        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        cube.GetComponent<PrimitiveComponent>().Color = colorVar;
        cube.GetComponent<PrimitiveComponent>().Collidable = collidable;
    }

    void createEllipse(List<int> data, List<int> color, int position)
    {
        // Create a new sphere
        GameObject sphere = Instantiate(spherePrefab, transform);
        // Set the position
        sphere.transform.localPosition = new Vector3(canvas.localScale.x / 2, canvas.localScale.y / 2, 0) - new Vector3(data[0] * globalSizeMultiplier, data[1] * globalSizeMultiplier, -(0.0001f * position));
        // Set the scale
        sphere.transform.localScale = new Vector3(data[2] * 2 * globalSizeMultiplier, 0.00001f, data[3] * 2 * globalSizeMultiplier);
        //Set rotation
        sphere.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        sphere.GetComponent<PrimitiveComponent>().Color = colorVar;
        sphere.GetComponent<PrimitiveComponent>().Collidable = collidable;
    }

    void createRotatedEllipse(List<int> data, List<int> color, int position)
    {
        // Create a new sphere
        GameObject sphere = Instantiate(spherePrefab, transform);
        // Set the position
        sphere.transform.localPosition = new Vector3(canvas.localScale.x / 2, canvas.localScale.y / 2, 0) - new Vector3(data[0] * globalSizeMultiplier, data[1] * globalSizeMultiplier, -(0.0001f * position));
        // Set the scale
        sphere.transform.localScale = new Vector3(data[2] * 2 * globalSizeMultiplier, 0.00001f, data[3] * 2 * globalSizeMultiplier);
        //Set rotation
        sphere.transform.localRotation = Quaternion.Euler(90f - data[4], 90, 90);
        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        sphere.GetComponent<PrimitiveComponent>().Color = colorVar;
        sphere.GetComponent<PrimitiveComponent>().Collidable = collidable;
    }

    void createCircle(List<int> data, List<int> color, int position)
    {
        // Create a new sphere
        GameObject sphere = Instantiate(spherePrefab, transform);
        // Set the position
        sphere.transform.localPosition = new Vector3(canvas.localScale.x / 2, canvas.localScale.y / 2, 0) - new Vector3(data[0] * globalSizeMultiplier, data[1] * globalSizeMultiplier, -(0.0001f * position));
        // Set the scale
        sphere.transform.localScale = new Vector3(data[2] * 2 * globalSizeMultiplier, 0.00001f, data[2] * 2 * globalSizeMultiplier);
        //Set rotation
        sphere.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Set the color
        Color colorVar = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
        colorVar.a = color[3] / 255f;

        sphere.GetComponent<PrimitiveComponent>().Color = colorVar;
        sphere.GetComponent<PrimitiveComponent>().Collidable = collidable;
    }

    public void clearChildren()
    {
        Transform parentTransform = transform;
        // Copy the children into an array
        Transform[] children = new Transform[parentTransform.childCount];
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            children[i] = parentTransform.GetChild(i);
        }

        // Destroy the children from the copied array
        foreach (Transform child in children)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    [System.Serializable]
    public class ShapeData
    {
        public List<Shape> shapes;
    }

    [System.Serializable]
    public struct Shape
    {
        public int type;
        public List<int> data;
        public List<int> color;
        public float score;
    }
}
