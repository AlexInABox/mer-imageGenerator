using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PixelArt : MonoBehaviour
{

    [HideInInspector]
    public string versionString = "1.2";
    [HideInInspector]
    [Header("Map Editor Reborn")]
    [Tooltip("If true, the color of each cube will be applied to the Primitive Component Script, rather than the Material. This is necessary for Map Editor Reborn.")]
    public bool MapEditorRebornUsage = true;
    [Header("Input")]
    public Texture2D inputImage; // Assign the input image in the Inspector

    [Space(10)]
    [Header("Settings")]
    [Tooltip("The quality at wich the input image will rendered. \n(1 = full quality, 0.001 = lowest quality)")]
    [Range(0.001f, 1f)]
    public float quality = 0.01f; // Adjust quality as needed

    [Tooltip("One unit equals the height of one in-game door. \n(You can always scale the mosaic in-game later on.)")]
    [Range(0.1f, 10f)]
    public float size = 1f; // Adjust size as needed


    [HideInInspector]
    public GameObject cubePrefab; // Assign the cube prefab in the Inspector

    [HideInInspector]
    [Header("Generate Mosaic")]
    [Tooltip("If true, the mosaic will be cleared before generating a new one")]
    public bool autoClean = true; // If true, the mosaic will be cleared before generating a new one

    [HideInInspector]
    [Range(0.01f, 1f)]
    public float sameColorMargin = 0.03f;

    [HideInInspector]
    public bool useLightsources = false;

    [HideInInspector]
    public bool showAdvancedSettings = false;

    void Reset(){
        cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Blocks/Primitives/Cube.prefab");
    }

    public void GenerateMosaic()
    {
        if (autoClean)
        {
            removeExistingCubes();
        }


        //Calculate the new width and height based on the set quality variable.
        int newWidth = Mathf.RoundToInt(inputImage.width * quality);
        int newHeight = Mathf.RoundToInt(inputImage.height * quality);

        Texture2D compressedImage = new Texture2D(newWidth, newHeight);

        // Copy pixel data from original texture to new resized texture
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Calculate the corresponding coordinates in the original texture
                int originalX = Mathf.RoundToInt((float)x / newWidth * inputImage.width);
                int originalY = Mathf.RoundToInt((float)y / newHeight * inputImage.height);

                Color pixelColor = inputImage.GetPixel(originalX, originalY);
                compressedImage.SetPixel(x, y, pixelColor);
            }
        }

        // Apply changes to the new texture
        compressedImage.Apply();

        //Now we have compressed the image. Lets start generating the mosaic.

        if (useLightsources)
        {
            GenerateLightsources(compressedImage);
        }
        else
        {
            GenerateCubes(compressedImage);
        }
    }

    private Vector3 startPosition;
    private float cubeSize;

    private void GenerateCubes(Texture2D image)
    {
        Color[] pixels = image.GetPixels();
        int width = image.width;
        int height = image.height;

        //Calculate the size a cube needs to be to fit the image
        cubeSize = (size * 3) / height; // 3 is the height of a cube to be as high as an in-game door

        // Get a reference to the parent GameObject (the object the script is attached to)
        Transform parentTransform = transform;

        // Calculate the starting position of the cubes
        startPosition = parentTransform.position - new Vector3((width * ((size * 3) / height) / 2f), (size * 3) / 2f, 0); //This vector points to the bottom left corner of the image

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = pixels[y * width + x];


                // Spawn a new cube
                GameObject cube = Instantiate(cubePrefab, startPosition + new Vector3(x * cubeSize, y * cubeSize, 0), Quaternion.identity, parentTransform);
                cube.transform.localScale = new Vector3(cubeSize, cubeSize, 0.001f);

                // Set the cube's color
                cube.GetComponent<PrimitiveComponent>().Color = pixelColor;
            }
        }

        //Optimize the mosaic (order matters because of some weird bug)
        optimizeHorizontally();
        optimizeVertically();


        print("Refreshing all cubes...");
        //trigger recompilation of all scripts
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    private int beginWithHorizontalOptimization()
    {
        optimizeHorizontally();
        optimizeVertically();
        return transform.childCount;
    }

    private int beginWithVerticalOptimization()
    {
        optimizeVertically();
        optimizeHorizontally();
        return transform.childCount;
    }

    private void optimizeHorizontally()
    {
        //Vertical optimization
        /*
            Go through every cube and check if the cube above has the same length, position and color.
            If so, delete the cube above, and extend the current cube upwards.
        */

        //Fetch all children
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        //put all cubes in a list by column
        List<List<GameObject>> cubesByColumn = new List<List<GameObject>>();
        float previousX = -1000000;
        foreach (Transform child in children)
        {
            if (child.localPosition.x != previousX)
            {
                cubesByColumn.Add(new List<GameObject>());
            }
            cubesByColumn[cubesByColumn.Count - 1].Add(child.gameObject);
            previousX = child.localPosition.x;
        }

        //NCheck and extend

        //For every element in a row check firstly if there is an element in the row above that has the same x position (then color, then length)
        //If so, delete the element above and extend the current element upwards

        for (int column = 0; column < cubesByColumn.Count; column++)
        {
            for (int i = 0; i < cubesByColumn[column].Count; i++)
            {

                GameObject currentCube;
                if (cubesByColumn[column][i])
                {
                    currentCube = cubesByColumn[column][i];
                }
                else
                {
                    continue;
                }

                //find all cubes to the right with the same y position (then color, then length)
                List<GameObject> validCubesToTheRight = new List<GameObject>();
                for (int a = column + 1; a < cubesByColumn.Count; a++)
                {

                    GameObject validCubeToTheRight = null;

                    foreach (GameObject cubeToTheRight in cubesByColumn[a])
                    {
                        if (cubeToTheRight.transform.localPosition.y == currentCube.transform.localPosition.y)
                        {
                            if (isRoughlyTheSameColor(cubeToTheRight.GetComponent<PrimitiveComponent>().Color, currentCube.GetComponent<PrimitiveComponent>().Color))
                            {
                                if (cubeToTheRight.transform.localScale.y == currentCube.transform.localScale.y)
                                {
                                    validCubeToTheRight = cubeToTheRight;
                                }
                            }
                        }
                    }
                    if (validCubeToTheRight)
                    {
                        validCubesToTheRight.Add(validCubeToTheRight);
                    }
                    else
                    {
                        break;
                    }
                }

                //If there are no valid cubes to the right, continue
                if (validCubesToTheRight.Count == 0)
                {
                    continue;
                }

                //Note the amount of valid cubes above
                int amountOfValidCubesToTheRight = validCubesToTheRight.Count;

                //Destroy all valid cubes above
                foreach (GameObject validCubeToTheRight in validCubesToTheRight)
                {
                    //Remove the valid cube above from the list
                    foreach (List<GameObject> columnList in cubesByColumn)
                    {
                        if (columnList.Contains(validCubeToTheRight))
                        {
                            columnList.Remove(validCubeToTheRight);
                            break;
                        }
                    }
                    DestroyImmediate(validCubeToTheRight);
                }

                //Extend the current cube to the right
                currentCube.transform.localScale += new Vector3(amountOfValidCubesToTheRight * cubeSize, 0, 0);

                //Move the current cube to the right
                currentCube.transform.position += new Vector3(amountOfValidCubesToTheRight * cubeSize / 2f, 0, 0);
            }
        }
    }

    private void optimizeVertically()
    {
        //Vertical optimization
        /*
            Go through every cube and check if the cube above has the same length, position and color.
            If so, delete the cube above, and extend the current cube upwards.
        */

        //Fetch all children
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        //put all cubes in a list by row
        List<List<GameObject>> cubesByRow = new List<List<GameObject>>();
        float previousY = -1000000;
        foreach (Transform child in children)
        {
            if (child.localPosition.y != previousY)
            {
                cubesByRow.Add(new List<GameObject>());
            }
            cubesByRow[cubesByRow.Count - 1].Add(child.gameObject);
            previousY = child.localPosition.y;
        }

        //NCheck and extend

        //For every element in a row check firstly if there is an element in the row above that has the same x position (then color, then length)
        //If so, delete the element above and extend the current element upwards

        for (int row = 0; row < cubesByRow.Count; row++)
        {
            for (int i = 0; i < cubesByRow[row].Count; i++)
            {

                GameObject currentCube;
                if (cubesByRow[row][i])
                {
                    currentCube = cubesByRow[row][i];
                }
                else
                {
                    continue;
                }

                //find all cubes above with the same x position (then color, then length)
                List<GameObject> validCubesAbove = new List<GameObject>();
                for (int a = row + 1; a < cubesByRow.Count; a++)
                {

                    GameObject validCubeAbove = null;

                    foreach (GameObject cubeAbove in cubesByRow[a])
                    {
                        if (cubeAbove.transform.localPosition.x == currentCube.transform.localPosition.x)
                        {
                            if (isRoughlyTheSameColor(cubeAbove.GetComponent<PrimitiveComponent>().Color, currentCube.GetComponent<PrimitiveComponent>().Color))
                            {
                                if (cubeAbove.transform.localScale.x == currentCube.transform.localScale.x)
                                {
                                    validCubeAbove = cubeAbove;
                                }
                            }
                        }
                    }
                    if (validCubeAbove)
                    {
                        validCubesAbove.Add(validCubeAbove);
                    }
                    else
                    {
                        break;
                    }
                }

                //If there are no valid cubes above, continue
                if (validCubesAbove.Count == 0)
                {
                    continue;
                }

                //Note the amount of valid cubes above
                int amountOfValidCubesAbove = validCubesAbove.Count;
                //Destroy all valid cubes above
                foreach (GameObject validCubeAbove in validCubesAbove)
                {
                    //Remove the valid cube above from the list
                    foreach (List<GameObject> rowList in cubesByRow)
                    {
                        if (rowList.Contains(validCubeAbove))
                        {
                            rowList.Remove(validCubeAbove);
                            break;
                        }
                    }
                    DestroyImmediate(validCubeAbove);
                }

                //Extend the current cube upwards
                currentCube.transform.localScale += new Vector3(0, amountOfValidCubesAbove * cubeSize, 0);

                //Move the current cube upwards
                currentCube.transform.position += new Vector3(0, (amountOfValidCubesAbove * cubeSize) / 2f, 0);
            }
        }
    }

    private void GeneratePixelPerfectMosaic()
    {
        Color[] pixels = inputImage.GetPixels();
        int width = inputImage.width;
        int height = inputImage.height;

        // Calcute the size a cube needs to be to fit the image
        float cubeSize = (size * 3) / height; // 3 is the height of a cube to be as high as an in-game door

        // Get a reference to the parent GameObject (the object the script is attached to)
        Transform parentTransform = transform;

        // Calculate the starting position of the cubes
        Vector3 startPosition = parentTransform.position - new Vector3(width / 2f, height / 2f, 0);
        //Draw a cube for every pixel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = pixels[y * width + x];

                // Spawn a new cube
                GameObject cube = Instantiate(cubePrefab, startPosition + new Vector3(x, y, 0), Quaternion.identity, parentTransform);
                cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
                // Set the cube's color
                if (MapEditorRebornUsage)
                {
                    cube.GetComponent<PrimitiveComponent>().Color = pixelColor;
                }
                else
                {

                    cube.GetComponent<Renderer>().material.color = pixelColor;
                }
            }
        }

        //refresh all the cubes if Map Editor Reborn is used
        if (MapEditorRebornUsage)
        {
            print("Refreshing all cubes...");
            //trigger recompilation of all scripts
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }

    private void GenerateLightsources(Texture2D image)
    {
        Color[] pixels = image.GetPixels();
        int width = image.width;
        int height = image.height;

        // Get a reference to the parent GameObject (the object the script is attached to)
        Transform parentTransform = transform;

        //Spawn a "canvas" for the lightsources (a primitive cube)

        GameObject canvas = Instantiate(cubePrefab, parentTransform.position, Quaternion.identity, parentTransform);
        //change the canvas color to black
        canvas.GetComponent<PrimitiveComponent>().Color = Color.black;

        //Create a lightsource prefab (a spotlight with the color red) and attach it to the parent of the canvas
        GameObject lightsourcePrefab = new GameObject("LightsourcePrefab");
        lightsourcePrefab.transform.parent = parentTransform;
        Light lightsourceLight = lightsourcePrefab.AddComponent<Light>();
        lightsourceLight.type = LightType.Spot;
        lightsourceLight.color = Color.red;
        lightsourceLight.range = 0.1f;
        lightsourceLight.spotAngle = 27f / 2;
        lightsourceLight.intensity = 100f;
        lightsourceLight.renderMode = LightRenderMode.ForcePixel;
        lightsourcePrefab.transform.localPosition = new Vector3(0f, 0f, -100f);

        // The starting position is the bottom left corner of the canvas (-0.5, -0.5, 0)
        Vector3 startPosition = parentTransform.position - new Vector3(0.5f, 0.5f, 0.09f);

        //For every pixel in the image, spawn a lightsource if the pixel is not black

        for (int y = 0; y < height; y++)
        {

            for (int x = 0; x < width; x++)
            {

                Color pixelColor = pixels[y * width + x];
                if (pixelColor != Color.black)
                {
                    //convert x and y to real scale
                    //if x 1 then realX should be 0.045
                    float realX = x * 0.0345f / 2f;
                    float realY = y * 0.0345f / 2f;

                    //print("pixelColor: " + pixelColor);
                    // Spawn a new lightsource
                    GameObject lightsource = Instantiate(lightsourcePrefab, startPosition + new Vector3(realX, realY, 0), Quaternion.identity, canvas.transform);
                    // Set the lightsource's color
                    lightsource.GetComponent<Light>().color = pixelColor;
                }
            }
        }

        //Destroy the prefab lightsource
        DestroyImmediate(lightsourcePrefab);
    }




    public void removeExistingCubes()
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


    private bool isRoughlyTheSameColor(Color color1, Color color2)
    {
        return Mathf.Abs(color1.r - color2.r) < sameColorMargin && Mathf.Abs(color1.g - color2.g) < sameColorMargin && Mathf.Abs(color1.b - color2.b) < sameColorMargin;
    }
}