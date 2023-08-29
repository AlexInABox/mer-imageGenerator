using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CubeMosaicGenerator : MonoBehaviour
{

    [HideInInspector]
    public string version = "1.0.4";
    [HideInInspector]
    public bool newVersionAvailable = false;

    [Header("Map Editor Reborn")]
    [Tooltip("If true, the color of each cube will be applied to the Primitive Component Script, rather than the Material. This is necessary for Map Editor Reborn.")]
    public bool MapEditorRebornUsage = true;
    [Space(20)]
    public Texture2D inputImage; // Assign the input image in the Inspector
    public GameObject cubePrefab; // Assign the cube prefab in the Inspector

    public float cubeSize = 0.1f; // Adjust cube size as needed
    [Header("Spacing")]
    public bool useSpacing = false; // If true, the spacing between cubes will be adjusted to fit the target height
    [Tooltip("Spacing between cubes. Adjust spacing as needed.")]
    public float spacing = 0.1f; // Adjust spacing between cubes
    [Header("Target Height")]
    public bool useTargetHeight = true; // If true, the mosaic will be scaled to the target height
    [Tooltip("One unit equals the height of one in-game door. Adjust the target height of the mosaic to fit the height of your door. (Otherwise, one door equals the height of 3 primitive cubes.)")]
    public float targetHeightAsDoors = 1f; // Adjust the height of the mosaic

    [Header("Generate Mosaic")]
    [Tooltip("If true, the mosaic will be cleared before generating a new one")]
    public bool autoClean = true; // If true, the mosaic will be cleared before generating a new one

    [Tooltip("If true, the script will try to merge cubes that are roughly the same color and next to each other in order to reduce the amount primitives.")]
    public bool optimization = true; // If true, the mosaic will be optimized vertically

    [Tooltip("The lower the value, the more similar the colors have to be to be considered the same color. (0.03 is a good value)")]
    [Range(0.01f, 1f)]
    public float sameColorMargin = 0.03f;


    public void Start()
    {
        //TODO: Check for new version
    }
    public void GenerateMosaic()
    {

        if (autoClean)
        {
            removeExistingCubes();
        }
        if (useTargetHeight)
        {
            //calculate the amount of cubes needed to fit the target height
            float targetHeight = targetHeightAsDoors * 3f / cubeSize;
            int newHeight = Mathf.RoundToInt(targetHeight);

            //calculate the width of the mosaic based on the aspect ratio of the input image
            int newWidth;

            int heightFaktor = Mathf.RoundToInt((float)inputImage.height / (float)newHeight);
            newWidth = Mathf.RoundToInt((float)inputImage.width / (float)heightFaktor);

            // Create a new uncompressed texture with the desired size
            Texture2D newImage = new Texture2D(newWidth, newHeight);

            // Copy pixel data from original texture to new resized texture
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Calculate the corresponding coordinates in the original texture
                    int originalX = Mathf.RoundToInt((float)x / newWidth * inputImage.width);
                    int originalY = Mathf.RoundToInt((float)y / newHeight * inputImage.height);

                    Color pixelColor = inputImage.GetPixel(originalX, originalY);
                    newImage.SetPixel(x, y, pixelColor);
                }
            }

            // Apply changes to the new texture
            newImage.Apply();

            // Generate the cubes
            GenerateCubes(newImage);
        }
        else
        {
            GenerateCubes(inputImage);
        }
    }

    private void GenerateCubes(Texture2D image)
    {
        Color[] pixels = image.GetPixels();
        int width = image.width;
        int height = image.height;

        spacing = useSpacing ? cubeSize + spacing : cubeSize;

        // Get a reference to the parent GameObject (the object the script is attached to)
        Transform parentTransform = transform;


        // Calculate the starting position of the cubes
        Vector3 startPosition = parentTransform.position - new Vector3(width * spacing / 2f, height * spacing / 2f, 0);

        for (int y = 0; y < height; y++)
        {
            GameObject previousCube = null;
            Color previousColor = Color.clear;
            int sameColorCount = 0;

            for (int x = 0; x < width; x++)
            {
                Color pixelColor = pixels[y * width + x];

                if (previousCube == null)
                {
                    // Spawn a new cube
                    GameObject cube = Instantiate(cubePrefab, startPosition + new Vector3(x * spacing, y * spacing, 0), Quaternion.identity, parentTransform);
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

                    // Set the previous cube
                    previousCube = cube;

                    // Set the previous color
                    previousColor = pixelColor;

                    // Reset the counter
                    sameColorCount = 0;
                }
                else
                {
                    if (isRoughlyTheSameColor(pixelColor, previousColor))
                    {
                        // Count the number of same-colored pixels to prepare for extending the cube
                        sameColorCount++;
                    }
                    else
                    {
                        if (sameColorCount > 0)
                        {
                            // Extend the previous cube
                            ExtendCube(previousCube, sameColorCount);

                            // Spawn a new cube
                            GameObject cube = Instantiate(cubePrefab, startPosition + new Vector3(x * spacing, y * spacing, 0), Quaternion.identity, parentTransform);
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


                            // Set the previous cube
                            previousCube = cube;

                            // Set the previous color
                            previousColor = pixelColor;

                            // Reset the counter
                            sameColorCount = 0;
                        }
                        else
                        {
                            // Spawn a new cube
                            GameObject cube = Instantiate(cubePrefab, startPosition + new Vector3(x * spacing, y * spacing, 0), Quaternion.identity, parentTransform);
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

                            // Set the previous cube
                            previousCube = cube;

                            // Set the previous color
                            previousColor = pixelColor;

                            // Reset the counter
                            sameColorCount = 0;
                        }
                    }
                }
            }
            if (sameColorCount > 0)
            {
                // Extend the previous cube
                ExtendCube(previousCube, sameColorCount);
            }
        }

        //Vertical optimization
        /*
            Go through every cube and check if the cube above has the same length, position and color.
            If so, delete the cube above, and extend the current cube upwards.
        */

        //Fetch all children
        Transform[] children = new Transform[parentTransform.childCount];
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            children[i] = parentTransform.GetChild(i);
        }

        //put all cubes in a list by row
        List<List<GameObject>> cubesByRow = new List<List<GameObject>>();
        foreach (Transform child in children)
        {
            int row = Mathf.RoundToInt((child.position.y - startPosition.y) / spacing);
            if (cubesByRow.Count <= row)
            {
                cubesByRow.Add(new List<GameObject>());
            }
            cubesByRow[row].Add(child.gameObject);
        }
        print("cubesByRow.Count: " + cubesByRow.Count);


        if (optimization)
        {
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
                    currentCube.transform.localScale += new Vector3(0, amountOfValidCubesAbove * spacing, 0);

                    //Move the current cube upwards
                    currentCube.transform.position += new Vector3(0, amountOfValidCubesAbove * spacing / 2f, 0);
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

    private void ExtendCube(GameObject cube, int length)
    {
        // Extend the previous cube's scale based on the number of same-colored pixels
        cube.transform.localScale += new Vector3(cubeSize * length, 0, 0);

        // Move the previous cube's position based on the number of same-colored pixels
        cube.transform.position += new Vector3(cubeSize * length / 2f, 0, 0);
    }
}