using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using System.Numerics;

public class Geometrize : MonoBehaviour
{
    [HideInInspector]
    public string versionString = "0.0.1";
    [HideInInspector]
    public bool newVersionAvailable = false;

    [Header("Input")]
    public Texture2D inputImage; // Assign the input image in the Inspector

    [Space(10)]
    [Header("Settings")]
    public int primitivesPerGeneration = 20;
    public int generations = 10;



    [Header("Allowed primitives")]
    public bool useCubes = true;
    public GameObject cubePrefab;
    public bool useSpheres = true;
    public GameObject spherePrefab;
    public bool useCylinders = true;
    public GameObject cylinderPrefab;
    public bool useCapsules = true;
    public GameObject capsulePrefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private Color[] pixelsInInputImage;
    private int pixelsInInputImageLength;
    private float[] pixelsInInputImageGrayScale;
    private int inputImageWidth;
    private int inputImageHeight;
    private Color averageColorOfInputImage;

    public void Generate()
    {

        //Get the input image and geometrize it
        //Geomentrization is the process of turning an image into a set of primitives
        //The primitives are then used to recreate the image
        //This is done by comparing the input image with the primitives and finding the best match
        //The best match is then used to recreate the image
        //This process is repeated until the image is fully recreated or the user stops the process

        //Fetch intial data that only needs to be fetched once
        //Data about the input image
        pixelsInInputImage = inputImage.GetPixels();
        pixelsInInputImageLength = pixelsInInputImage.Length;

        pixelsInInputImageGrayScale = new float[pixelsInInputImageLength];
        for (int i = 0; i < pixelsInInputImageLength; i++)
        {
            pixelsInInputImageGrayScale[i] = pixelsInInputImage[i].grayscale;
        }

        inputImageWidth = inputImage.width;
        inputImageHeight = inputImage.height;

        averageColorOfInputImage = new Color(0f, 0f, 0f, 0f);
        for (int i = 0; i < pixelsInInputImageLength; i++)
        {
            averageColorOfInputImage += pixelsInInputImage[i];
        }
        averageColorOfInputImage /= pixelsInInputImageLength;

        clearChildren();

        PrepareRatingVariables();

        DrawImage(inputImage);
    }


    private void DrawImage(Texture2D image)
    {
        //Create 100 version of the input image
        //Each version will have 1 randomly shaped, placed and colored primitives
        //The best version will be used to recreate the input image
        float newWidth = image.width / image.height;

        //Create a canvas to draw the image on
        GameObject canvas = Instantiate(cubePrefab);
        canvas.transform.parent = transform;

        //Set the canvas's position, rotation and scale
        canvas.transform.localPosition = UnityEngine.Vector3.zero;
        canvas.transform.localScale = new UnityEngine.Vector3(newWidth, 1f, 0.01f);

        //Set the canvas's color
        canvas.GetComponent<PrimitiveComponent>().Color = averageColorOfInputImage;
        canvas.GetComponent<Renderer>().material.color = averageColorOfInputImage;


        //Create a camera right in front of the canvas
        GameObject camera = new GameObject("Camera");
        camera.transform.parent = canvas.transform;
        camera.transform.localPosition = new UnityEngine.Vector3(0f, 0f, 80f);
        camera.transform.localRotation = UnityEngine.Quaternion.Euler(0f, -180f, 0f);

        //Attach a camera component to the camera
        camera.AddComponent<Camera>();
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().aspect = newWidth;
        camera.GetComponent<Camera>().orthographicSize = 0.5f;

        //Create a new parent object for the primitives
        Transform parent = transform;

        var watchALL = new System.Diagnostics.Stopwatch();

        //Create a list of random numbers
        int[] randomNum = new int[generations * primitivesPerGeneration];

        List<int> randomNumbersThatAreAllowed = new List<int>();
        if (useCubes) randomNumbersThatAreAllowed.Add(0);
        if (useSpheres) randomNumbersThatAreAllowed.Add(1);
        if (useCylinders) randomNumbersThatAreAllowed.Add(2);
        if (useCapsules) randomNumbersThatAreAllowed.Add(3);

        for (int i = 0; i < randomNum.Length; i++)
        {
            randomNum[i] = randomNumbersThatAreAllowed[UnityEngine.Random.Range(0, randomNumbersThatAreAllowed.Count)];
        }

        watchALL.Start();
        for (int g = 0; g < generations; g++)
        {
            GameObject generation = new GameObject("Generation " + g);
            generation.transform.parent = parent.transform;
            generation.transform.localPosition = UnityEngine.Vector3.zero;

            //create a list of all the versions
            List<GameObject> versions = new List<GameObject>();
            List<float> ratings = new List<float>();

            for (int i = 0; i < primitivesPerGeneration; i++)
            {
                var watchOne = new System.Diagnostics.Stopwatch();
                watchOne.Start();

                //Create a new child object for the current version
                GameObject version = new GameObject("Version " + i);
                version.transform.parent = generation.transform;
                version.transform.localPosition = UnityEngine.Vector3.zero;

                //Add the version to the list of versions
                versions.Add(version);

                GameObject primitive = null;

                //Create a primitive based on its random number in the list of random numbers
                switch (randomNum[(g * primitivesPerGeneration) + i])
                {
                    case 0:
                        primitive = Instantiate(cubePrefab);
                        break;
                    case 1:
                        primitive = Instantiate(spherePrefab);
                        break;
                    case 2:
                        primitive = Instantiate(cylinderPrefab);
                        break;
                    case 3:
                        primitive = Instantiate(capsulePrefab);
                        break;
                }

                primitive.transform.parent = version.transform;

                //Set the primitive's position, rotation and scale
                primitive.transform.localPosition = new UnityEngine.Vector3(UnityEngine.Random.Range(-(newWidth / 2f), newWidth / 2f), UnityEngine.Random.Range(-0.5f, 0.5f), 0.01f + (g * 0.001f));
                primitive.transform.rotation = UnityEngine.Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));
                primitive.transform.localScale = new UnityEngine.Vector3(UnityEngine.Random.Range(0.01f, 0.5f), UnityEngine.Random.Range(0.01f, 0.5f), 0.001f);


                //Set the primitive's color
                Color color = image.GetPixel(UnityEngine.Random.Range(0, image.width), UnityEngine.Random.Range(0, image.height));
                color.a = 0.5f;
                primitive.GetComponent<PrimitiveComponent>().Color = color;
                primitive.GetComponent<Renderer>().material.color = color;
                primitive.GetComponent<Renderer>().material.SetFloat("_Mode", 3);
                primitive.GetComponent<Renderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                primitive.GetComponent<Renderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                primitive.GetComponent<Renderer>().material.SetInt("_ZWrite", 0);
                primitive.GetComponent<Renderer>().material.DisableKeyword("_ALPHATEST_ON");
                primitive.GetComponent<Renderer>().material.EnableKeyword("_ALPHABLEND_ON");
                primitive.GetComponent<Renderer>().material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                primitive.GetComponent<Renderer>().material.renderQueue = 3000;

                //Get a rating for the current version
                float rating = GetRating(image, camera.GetComponent<Camera>());

                //Add the rating to the list of ratings
                ratings.Add(rating);

                //Make the primitive invisible
                primitive.GetComponent<Renderer>().enabled = false;

                watchOne.Stop();
                //print("Time for one version: " + watchOne.ElapsedMilliseconds + "ms");
            }


            //Find the best version
            float bestRating = 0f;
            float bestRatingPlace = 0f;
            GameObject previousBestGameobject = new GameObject();
            for (int i = 0; i < ratings.Count; i++)
            {

                if (ratings[i] > bestRating)
                {
                    bestRating = ratings[i];
                    bestRatingPlace = i;

                    //Destroy previous best version
                    DestroyImmediate(previousBestGameobject);

                    //Become the "previous best version"
                    previousBestGameobject = versions[(int)bestRatingPlace].transform.gameObject;
                }
                else
                {
                    //Destroy the current version
                    DestroyImmediate(versions[i].transform.gameObject);
                }
            }
            print("Best rating: " + bestRatingPlace + " with a rating of " + bestRating);
            //Make the best version visible again
            versions[(int)bestRatingPlace].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        }
        watchALL.Stop();

        print("Total time: " + watchALL.ElapsedMilliseconds + "ms");
    }

    //Variables for the GetRating function that should only be allocated once
    //we know the dimensions of the rendered image will always be the same so we can allocate the arrays once

    private RenderTexture renderTexture;
    private Texture2D renderedImage;
    private Rect defaultRect;
    //private float rating = 0f;
    private Color[] pixelsRenderedImage;
    private int vectorSize = Vector<float>.Count;
    private float[] imageValues;
    private float[] renderedValues;
    private float maxDifference;
    private float similarity;
    private Vector<float> vectorImage;
    private Vector<float> vectorRendered;


    private void PrepareRatingVariables()
    {
        //Create a new render texture
        renderTexture = new RenderTexture(inputImageWidth, inputImageHeight, 24);
        renderTexture.Create();

        //rect
        defaultRect = new Rect(0, 0, inputImageWidth, inputImageHeight);

        //Create a new texture2D
        renderedImage = new Texture2D(inputImageWidth, inputImageHeight, TextureFormat.RGB24, false);

        pixelsRenderedImage = new Color[pixelsInInputImageLength];

        imageValues = new float[vectorSize];
        renderedValues = new float[vectorSize];

        vectorImage = new Vector<float>(vectorSize);
        vectorRendered = new Vector<float>(vectorSize);
    }


    private float GetRating(Texture2D image, Camera camera)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        var compare = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        //Render the image
        camera.targetTexture = renderTexture;
        camera.Render();

        //Read the pixels from the image
        RenderTexture.active = renderTexture;
        renderedImage.ReadPixels(defaultRect, 0, 0);
        renderedImage.Apply();


        compare.Start();
        //Compare the pixels from the input image and the rendered image
        pixelsRenderedImage = renderedImage.GetPixels(); //3ms



        float rating = 0f;

        // Process pixel values in vectors of size vectorSize (e.g., 4 on typical platforms)
        for (int i = 0; i < pixelsInInputImageLength; i += vectorSize)
        {

            for (int j = 0; j < vectorSize; j++)
            {
                imageValues[j] = pixelsInInputImage[i + j].grayscale;
                renderedValues[j] = pixelsRenderedImage[i + j].grayscale;
            }


            vectorImage = new Vector<float>(imageValues);
            vectorRendered = new Vector<float>(renderedValues);

            var difference = Vector.Abs(Vector.Subtract(vectorImage, vectorRendered));
            rating += Vector.Dot(difference, Vector<float>.One);

        }

        // Handle any remaining pixels that are not processed in vectors
        for (int i = pixelsInInputImageLength - pixelsInInputImageLength % vectorSize; i < pixelsInInputImageLength; i++)
        {
            rating += Math.Abs(pixelsInInputImageGrayScale[i] - pixelsRenderedImage[i].grayscale);
        }

        //  Normalize the rating to a similarity value between 0 and 1
        maxDifference = pixelsInInputImageLength * 255f;
        similarity = 1f - (rating / maxDifference);
        compare.Stop();


        //Return the rating
        //print("Rating: " + rating);
        //print("Similarity: " + similarity);
        stopwatch.Stop();

        //print("Time for one compare: " + compare.ElapsedMilliseconds + "ms");
        //print("Time for one rating: " + stopwatch.ElapsedMilliseconds + "ms");
        return similarity;
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
}
