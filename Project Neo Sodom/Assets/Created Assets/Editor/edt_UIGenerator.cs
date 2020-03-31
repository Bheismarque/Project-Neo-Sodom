using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class edt_UIGenerator : EditorWindow
{
    //Constants
    private const string SAVE_DIRECTORY = "Assets/Created Assets/Object/0CO - UI Object";

    //Statics
    private static EditorWindow WINDOW = null;
    private static float WINDOW_WIDTH = 0;
    private static float WINDOW_HEIGHT = 0;

    private static float PIXEL_PER_UNIT = 640;

    private static string NAME = "";
    private static string RESOURCE_DIRECTORY = "";

    private static Texture2D MainTexture = null;
    private static Texture2D LookUpTexture = null;

    [MenuItem("UI Object/UI Object Generator")]
    public static void ShowWindow()
    {
        WINDOW = GetWindow<edt_UIGenerator>("UI Object Generator");
        WINDOW_WIDTH = WINDOW.position.width - 6;
        WINDOW_HEIGHT = WINDOW.position.height - 6;
        WINDOW.Show();
    }

    void OnGUI()
    {
        WINDOW_WIDTH = position.width - 6;
        WINDOW_HEIGHT = position.height - 6;

        GUILayout.Label("UI Object Generator", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        NAME = EditorGUILayout.TextField("UI Object Name", NAME);
        PIXEL_PER_UNIT = EditorGUILayout.Slider("Pixel Per Unit", PIXEL_PER_UNIT, 1, 1280);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        TextureField("Main Texture", MainTexture);
        if (GUILayout.Button("Load Main Texture")) { MainTexture = loadTexture(); }
        if (GUILayout.Button("Load LookUp Texture")) { LookUpTexture = loadTexture(); }

        if(MainTexture != null) { RESOURCE_DIRECTORY = SAVE_DIRECTORY + "/" + NAME + " Resource/"; }

        if (GUILayout.Button("Generate"))
        {
            if (MainTexture.width != 0 && MainTexture.height != 0 && !NAME.Equals("")) { UIObjectGenerate(); }
        }
    }

    private static Texture2D TextureField(string name, Texture2D texture)
    {
        //Texture Draw
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = WINDOW_WIDTH;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(WINDOW_WIDTH), GUILayout.Height(WINDOW_WIDTH));
        GUILayout.EndVertical();
        return result;
    }
    static private Texture2D loadTexture()
    {
        // Step 1 ------------------- Set Up
        RESOURCE_DIRECTORY = "";
        Texture2D texture = new Texture2D(0, 0);

        // Step 2 ---- --------------- File Load
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            //Load File
            var fileContent = File.ReadAllBytes(path);
            texture.LoadImage(fileContent);
        }

        if (texture.width == 0) { texture = null; }

        return texture;
    }

    private class Point
    {
        public int x, y;
        public Point(int x, int y) { this.x = x; this.y = y; }
    }
    static private void UIObjectGenerate()
    {
        // Step 2 ------------------- Folder Creation
        if (Directory.Exists(RESOURCE_DIRECTORY)) { Directory.Delete(RESOURCE_DIRECTORY, true); }
        AssetDatabase.CreateFolder(SAVE_DIRECTORY, NAME + " Resource");

        // Dividing Guide Points
        Point[] points = new Point[4];
        int pointIndex = 0;
        for(int y = 0; y < LookUpTexture.height; y++)
        {
            for(int x = 0; x < LookUpTexture.width; x++)
            {
                Color color = LookUpTexture.GetPixel(x, y);
                if(color.r == 1 && color.a  != 0) { points[pointIndex++] = new Point(x, y); }
            }
        }
        int leftMargin = points[0].x + 1;
        int rightMargin = MainTexture.width - points[1].x;
        int center_width = MainTexture.width - rightMargin - leftMargin;

        int lowerMargin = points[0].y + 1;
        int upperMargin = MainTexture.height - points[2].y;
        int center_height = MainTexture.height - upperMargin - lowerMargin;

        // Pannel Texture Creation
        Vector2[,] pannels = new Vector2[3,3];
        Point[,] pannelCoords = new Point[,] {  {new Point(0,0), new Point(points[0].x+1,0), new Point(points[1].x,0) },
                                                {new Point(0,points[0].y+1), new Point(points[0].x+1,points[0].y+1), new Point(points[1].x,points[0].y+1) },
                                                {new Point(0,points[3].y), new Point(points[0].x+1,points[3].y), new Point(points[1].x,points[3].y) }};

        string[,] pannelNames = new string[,] { { "LowerLeft", "Down", "LowerRight" }, { "Left", "Center", "Right" }, { "UpperLeft", "Up", "UpperRight" } };

        Vector2 lowerLeft = pannels[0, 0] = new Vector2(leftMargin, lowerMargin);
        Vector2 down = pannels[0, 1] = new Vector2(center_width, lowerMargin);
        Vector2 lowerRight = pannels[0, 2] = new Vector2(rightMargin, lowerMargin);

        Vector2 left = pannels[1, 0] = new Vector2(leftMargin, center_height);
        Vector2 center = pannels[1, 1] = new Vector2(center_width, center_height);
        Vector2 right = pannels[1, 2] = new Vector2(rightMargin, center_height);

        Vector2 upperLeft = pannels[2, 0] = new Vector2(leftMargin, upperMargin);
        Vector2 up = pannels[2, 1] = new Vector2(center_width, upperMargin);
        Vector2 upperRight = pannels[2, 2] = new Vector2(rightMargin, upperMargin);

        // Pannel Texture Creation
        Texture2D texture = registerTexture(NAME, MainTexture);
        Material material = registerMaterial(NAME, texture);
        GameObject UIObject = new GameObject(NAME);
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                // Plane Mesh Construct
                GameObject pannel = new GameObject(pannelNames[y, x]);

                registerMesh(pannelNames[y, x],pannel, material,
                             new Vector2(texture.width, texture.height),
                             new Vector2(pannels[y,x].x, pannels[y,x].y),
                             pannelCoords[y, x]);

                pannel.transform.parent = UIObject.transform;
            }
        }

        // Save the Pixel Object
        AssetDatabase.SaveAssets();
        PrefabUtility.SaveAsPrefabAsset(UIObject, SAVE_DIRECTORY + "/" + NAME + ".prefab");
        DestroyImmediate(UIObject);
    }

    private static Texture2D registerTexture(string name, Texture2D texture)
    {
        string textureDirectory = RESOURCE_DIRECTORY + name + "_texture.png";
        File.WriteAllBytes(textureDirectory, texture.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(textureDirectory);
        textureImporter.isReadable = true;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        AssetDatabase.ImportAsset(textureDirectory, ImportAssetOptions.ForceUpdate);

        texture = (Texture2D)AssetDatabase.LoadAssetAtPath(textureDirectory, typeof(Texture2D));
        texture.Apply();
        AssetDatabase.ImportAsset(textureDirectory, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return texture;
    }

    static private Material registerMaterial(string name, Texture2D pannelTexture)
    {
        //Material
        Material material = new Material(Shader.Find("Unlit/Transparent"));
        string materialDirectory = RESOURCE_DIRECTORY + name + "_material.mat";

        AssetDatabase.CreateAsset(material, materialDirectory);
        material = (Material)AssetDatabase.LoadAssetAtPath(materialDirectory, typeof(Material));

        //Albedo
        material.SetTexture("_MainTex", pannelTexture);

        //Material Save
        AssetDatabase.ImportAsset(materialDirectory, ImportAssetOptions.ForceUpdate);

        return material;
    }

    static private void registerMesh(string name, GameObject pannel, Material material, Vector2 textureSize, Vector2 pannelSize, Point pannelCoord)
    {
        GameObject[] pannels = new GameObject[2];
        int sideIndex = 2;
        for (int side = 0; side < sideIndex; side++)
        {
            //Mesh Basic Information
            string tempName = "";
            if (side == 0) { tempName = "outside mesh"; }
            if (side == 1) { tempName = "inside mesh"; }

            pannels[side] = new GameObject(tempName);
            pannels[side].AddComponent<MeshFilter>();
            pannels[side].AddComponent<MeshRenderer>();

            // Step 0 ---------- Mesh Setup
            Mesh mesh = new Mesh(); mesh.Clear();
            MeshFilter meshFilter = pannels[side].GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = pannels[side].GetComponent<MeshRenderer>();

            // Step 1 ---------- Vertices
            Vector3 p0 = new Vector3(-.5f, -.5f, 0);
            Vector3 p1 = new Vector3(.5f, -.5f, 0);
            Vector3 p2 = new Vector3(-.5f, .5f, 0);
            Vector3 p3 = new Vector3(.5f, .5f, 0);
            Vector3[] vertices = { p0, p1, p2, p3 };
            /*Apply*/
            mesh.vertices = vertices;


            // Step 2 ---------- Triangles
            int[] triangles = null;
            if (side == 0) { triangles = new int[] { 0, 1, 3, 2, 0, 3 }; }
            if (side == 1) { triangles = new int[] { 3, 1, 0, 3, 0, 2 }; }
            /*Apply*/
            mesh.triangles = triangles;



            // Step 3 ---------- Normals
            Vector3[] normals = null;
            if (side == 0) { normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down }; }
            if (side == 1) { normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up }; }
            /*Apply*/
            mesh.normals = normals;


            // Step 4 ---------- UV
            Vector2 point0 = new Vector2((pannelCoord.x) / textureSize.x,
                                         (pannelCoord.y) / textureSize.y);

            Vector2 point1 = new Vector2((pannelCoord.x + pannelSize.x) / textureSize.x,
                                         (pannelCoord.y) / textureSize.y);

            Vector2 point2 = new Vector2((pannelCoord.x) / textureSize.x,
                                         (pannelCoord.y + pannelSize.y) / textureSize.y);

            Vector2 point3 = new Vector2((pannelCoord.x + pannelSize.x) / textureSize.x,
                                         (pannelCoord.y + pannelSize.y) / textureSize.y);

            /*Apply*/
            mesh.uv = new Vector2[] { point0, point1, point2, point3 };

            //Save The Data
            meshFilter.mesh = mesh;
            pannels[side].transform.parent = pannel.transform;
            pannels[side].transform.localScale = new Vector3(1f, 1, 1f);

            //Save
            string inOrOut = "";
            if (side == 0) { inOrOut = "_out"; }
            if (side == 1) { inOrOut = "_in"; }
            string meshDirectory = RESOURCE_DIRECTORY + "_mesh" + inOrOut + "_" + name + ".asset";

            AssetDatabase.CreateAsset(mesh, meshDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshDirectory, typeof(Mesh));

            //Finish-up
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            AssetDatabase.ImportAsset(meshDirectory, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        pannel.transform.localScale = new Vector3(pannelSize.x / PIXEL_PER_UNIT, pannelSize.y / PIXEL_PER_UNIT, 1f);
    }
}