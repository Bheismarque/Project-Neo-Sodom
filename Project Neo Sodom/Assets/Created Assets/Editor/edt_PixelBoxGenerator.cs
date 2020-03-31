using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class edt_PixelBoxGenerator : EditorWindow
{
    //Constants
    private const string SAVE_DIRECTORY = "Assets/Created Assets/Object/0CO - Pixel Object";

    //Constant Colors
    private static readonly Color COLOR_IGNORE = Color.black;

    //Statics
    private static EditorWindow WINDOW = null;
    private static float WINDOW_WIDTH = 0;
    private static float WINDOW_HEIGHT = 0;

    private static float PIXEL_PER_UNIT = 64;

    private static string NAME = "";
    private static string RESOURCE_DIRECTORY = "";

    private static bool DOUBLE_SIDED = true;
    private static Material MATERIAL = null;
    private static Texture2D BLUEPRINT = null;
    private static Texture2D TEXTURE_ALBEDO = null;
    private static Texture2D TEXTURE_NORMAL = null;
    private static Texture2D TEXTURE_EMISSION = null;

    private static List<MeshFilter> PIXEL_OBJECT_MESHES_IN = null;
    private static List<MeshFilter> PIXEL_OBJECT_MESHES_OUT = null;

    private static float NORMAL_INTENCITY = 3f;

    private static Counter COUNTER = null;
    private class Counter { public int number = 0; }

    //UI
    #region
    [MenuItem("Pixel Object/Pixel Object Generator")]
    public static void ShowWindow()
    {
        WINDOW = GetWindow<edt_PixelBoxGenerator>("Pixel Object Generator");
        WINDOW_WIDTH = WINDOW.position.width - 6;
        WINDOW_HEIGHT = WINDOW.position.height - 6;
        WINDOW.Show();
    }

    void OnGUI()
    {
        WINDOW_WIDTH = position.width - 6;
        WINDOW_HEIGHT = position.height - 6;

            GUILayout.Label("Pixel Object Generator", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            NAME = EditorGUILayout.TextField("Pixel Object Name", NAME);
            DOUBLE_SIDED = EditorGUILayout.Toggle("Double Sided", DOUBLE_SIDED);
            PIXEL_PER_UNIT = EditorGUILayout.Slider("Pixel Per Unit", PIXEL_PER_UNIT, 1, 124);
            NORMAL_INTENCITY = EditorGUILayout.Slider("Normal Map Intensity", NORMAL_INTENCITY, 0, 10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            TextureField("Blueprint", BLUEPRINT);
            if (GUILayout.Button("Load Blueprint"))
            {
                loadBlueprint();
            }

            if (GUILayout.Button("Generate"))
            {
                if (BLUEPRINT.width != 0 && BLUEPRINT.height != 0 && !NAME.Equals("")) { pixelObjectGenerate(BLUEPRINT); }
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

    static private void loadBlueprint()
    {
        // Step 1 ------------------- Set Up
        RESOURCE_DIRECTORY = "";
        BLUEPRINT = new Texture2D(0, 0);

        // Step 2 ---- --------------- File Load
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            //Load File
            var fileContent = File.ReadAllBytes(path);
            BLUEPRINT.LoadImage(fileContent);
            BLUEPRINT.filterMode = FilterMode.Point;
        }

        if (BLUEPRINT.width == 0) { BLUEPRINT = null; return; }
        else
        {
            //Folder Creation
            RESOURCE_DIRECTORY = SAVE_DIRECTORY + "/" + NAME + " Resource/";
        }
    }
    #endregion

    //Main Logic
    #region
    private static void pixelObjectGenerate( Texture2D blueprint )
    {
        // Step 1 ------------------- Initalize
        MATERIAL = null;
        TEXTURE_ALBEDO = null;
        TEXTURE_NORMAL = null;
        TEXTURE_EMISSION = null;

        PIXEL_OBJECT_MESHES_IN = new List<MeshFilter>();
        PIXEL_OBJECT_MESHES_OUT = new List<MeshFilter>();

        COUNTER = new Counter();

        // Step 2 ------------------- Folder Creation
        if (Directory.Exists(RESOURCE_DIRECTORY)) { Directory.Delete(RESOURCE_DIRECTORY, true); }
        AssetDatabase.CreateFolder(SAVE_DIRECTORY, NAME + " Resource");

        // Step 3 ------------------- Blueprint Process
        processBlueprint(blueprint);

        // Step 4 ------------------- Sub-Blueprints Seperations
        SubBlueprint[] subBlueprints = cutSubBlueprint(blueprint);


        // Step 5 ------------------- Save Material
        //Material
        MATERIAL = new Material(Shader.Find("Standard"));
        string materialDirectory = RESOURCE_DIRECTORY + NAME + "_material.mat";

        AssetDatabase.CreateAsset(MATERIAL, materialDirectory);
        MATERIAL = (Material)AssetDatabase.LoadAssetAtPath(materialDirectory, typeof(Material));

        //Albedo
        MATERIAL.SetTexture("_MainTex", TEXTURE_ALBEDO);

        //Normal
        MATERIAL.shaderKeywords = new string[1] { "_NORMALMAP" };
        MATERIAL.SetTexture("_BumpMap", TEXTURE_NORMAL);

        //Emission
        MATERIAL.EnableKeyword("_EMISSION");
        MATERIAL.SetTexture("_EmissionMap", TEXTURE_EMISSION);
        MATERIAL.SetColor("_EmissionColor", Color.white);
        MATERIAL.SetVector("_EmissionColor", Color.white * 5f);
        MATERIAL.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

        //Cutout Mode
        MATERIAL.SetFloat("_Mode", 1);
        MATERIAL.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        MATERIAL.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        MATERIAL.SetInt("_ZWrite", 1);
        MATERIAL.EnableKeyword("_ALPHATEST_ON");
        MATERIAL.DisableKeyword("_ALPHABLEND_ON");
        MATERIAL.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        MATERIAL.renderQueue = 2450;
        AssetDatabase.ImportAsset(materialDirectory, ImportAssetOptions.ForceUpdate);


        // Step 6 ------------------- Pixel Part Construction
        PixelPart[] pixelParts = new PixelPart[subBlueprints.Length];
        for (int i = 0; i < pixelParts.Length; i++)
        {
            pixelParts[i] = generatePart(subBlueprints[i]);
        }

        attachColorMatch(pixelParts);
        constructParts(pixelParts[0], null);

        GameObject pixelObject = new GameObject(NAME);
        pixelObject.AddComponent<scr_PixelObjectController>();
        pixelObject.GetComponent<scr_PixelObjectController>().setPixelObjectMaterial(MATERIAL);
        pixelObject.GetComponent<scr_PixelObjectController>().setOriginalPixelObjectMaterial(MATERIAL);

        combineMeshes(pixelObject, pixelParts[0]);


        // Step 7 ------------------- Save the Pixel Object
        AssetDatabase.SaveAssets();
        PrefabUtility.SaveAsPrefabAsset
            (
            pixelObject,
            SAVE_DIRECTORY + "/" + NAME + ".prefab"
            );
        DestroyImmediate(pixelObject);

        // Step 8 ------------------- Reset
        MATERIAL = null;
        TEXTURE_ALBEDO = null;
        TEXTURE_NORMAL = null;
        TEXTURE_EMISSION = null;

        PIXEL_OBJECT_MESHES_IN = null;
        PIXEL_OBJECT_MESHES_OUT = null;

    }
    #endregion

    //Blueprint Process
    #region
    private static Texture2D registerTexture( Texture2D map, string path, bool isNormal)
    {
        File.WriteAllBytes(path, map.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
        if ( isNormal ) { textureImporter.textureType = TextureImporterType.NormalMap; }
        textureImporter.isReadable = true;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        map = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        map.Apply();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return map;
    }

    private static void processBlueprint( Texture2D blueprint )
    {
        //---------------- Step 1 - Albedo Map Creation
        Texture2D AlbedoMap = new Texture2D(blueprint.width, blueprint.height);
        for (int iy = 0; iy < blueprint.height; iy++)
        {
            for (int ix = 0; ix < blueprint.width; ix++)
            {
                Color curColor = blueprint.GetPixel(ix, iy);
                AlbedoMap.SetPixel(ix, iy, curColor);
                if (curColor.Equals(COLOR_IGNORE)) { AlbedoMap.SetPixel(ix, iy, Color.clear); }
                if (curColor.r == 1)
                {
                    int count = 0;
                    Vector3 c = new Vector3(0, 0, 0);
                    for (int cy = -1; cy <= 1; cy++)
                    {
                        for (int cx = -1; cx <= 1; cx++)
                        {
                            if ( cx == 0 && cy == 0) { continue; }
                            int cix = Mathf.Clamp(ix + cx, 0, blueprint.width);
                            int ciy = Mathf.Clamp(iy + cy, 0, blueprint.height);

                            Color checkColor = blueprint.GetPixel(cix, ciy);
                            if (checkColor.r != 1 && checkColor.a != 0 && !checkColor.Equals(COLOR_IGNORE))
                            {
                                c.x += checkColor.r;
                                c.y += checkColor.g;
                                c.z += checkColor.b;
                                count++;
                            }
                        }
                    }
                    c /= count;

                    if (curColor.a != 0) { AlbedoMap.SetPixel(ix, iy, new Color(c.x, c.y, c.z, 1)); }
                }
            }
        }
        AlbedoMap.Apply();
        TEXTURE_ALBEDO = registerTexture(AlbedoMap, RESOURCE_DIRECTORY + NAME + "_texture_albedo.png", false);

        //---------------- Step 2 - Normal Map Creation
        Texture2D NormalMap = new Texture2D(blueprint.width, blueprint.height);
        for (int y = 0; y < blueprint.height; y++)
        {
            for (int x = 0; x < blueprint.width; x++)
            {
                bool noNormalCalculation = false;

                Color curColor = blueprint.GetPixel(x, y);
                if (curColor.a == 0 || curColor.Equals(Color.black) || curColor.r == 1 )
                {
                    noNormalCalculation = true;
                }

                Vector3 normal = new Vector3(0, 0, 0);

                float[,] brightness = new float[3,3];
                float brightness_L = 0;
                float brightness_R = 0;
                float brightness_D = 0;
                float brightness_U = 0;

                for (int iy = -1; iy <= 1; iy++)
                {
                    for (int ix = -1; ix <= 1; ix++)
                    {
                        int index_x = x + ix;
                        int index_y = y + iy;
                        int bright_x = ix + 1;
                        int bright_y = iy + 1;

                        curColor = blueprint.GetPixel(index_x, index_y);
                        if (curColor.r == 1) { noNormalCalculation = true; }

                        brightness[bright_y, bright_x] = (curColor.a + curColor.g + curColor.b) / 3;

                        if (index_x < 0 || index_x >= blueprint.width || 
                            index_y < 0 || index_y >= blueprint.height)
                        {
                            brightness[bright_y, bright_x] = 0;
                        }

                    }
                }

                float normalCorrection = 6;
                for (int i = 0; i < 3; i++)
                {
                    brightness_L += ((brightness[i, 0] - brightness[1, 1]) / normalCorrection) * NORMAL_INTENCITY;
                    brightness_R += ((brightness[i, 2] - brightness[1, 1]) / normalCorrection) * NORMAL_INTENCITY;
                    brightness_D += ((brightness[0, i] - brightness[1, 1]) / normalCorrection) * NORMAL_INTENCITY;
                    brightness_U += ((brightness[2, i] - brightness[1, 1]) / normalCorrection) * NORMAL_INTENCITY;
                }

                normal.x = brightness_R - brightness_L;
                normal.y = brightness_U - brightness_D;
                normal.z = 1;
                normal.Normalize();

                if (noNormalCalculation)
                {
                    NormalMap.SetPixel(x, y, new Color(0.5f, 0.5f, 1));
                }
                else
                {
                    NormalMap.SetPixel(x, y, new Color(-normal.x + 0.5f, -normal.y + 0.5f, normal.z, 1));
                }
            }
        }
        NormalMap.Apply();
        TEXTURE_NORMAL = registerTexture(NormalMap, RESOURCE_DIRECTORY + NAME + "_texture_normal.png", true);

        //---------------- Step 3 - Emission Map Creation
        Texture2D EmissionMap = new Texture2D(blueprint.width, blueprint.height);
        for (int iy = 0; iy < blueprint.height; iy++)
        {
            for (int ix = 0; ix < blueprint.width; ix++)
            {
                Color curColor = blueprint.GetPixel(ix, iy);
                if (curColor.a != 1 && curColor.a != 0)
                {
                    Color emissionColor = curColor;
                    emissionColor.a = 1;
                    EmissionMap.SetPixel(ix, iy, emissionColor);
                }
                else
                {
                    EmissionMap.SetPixel(ix, iy, Color.black);
                }
            }
        }
        EmissionMap.Apply();
        TEXTURE_EMISSION = registerTexture(EmissionMap, RESOURCE_DIRECTORY + NAME + "_texture_emission.png", false);
    }
    #endregion

    //Sub-Blueprints Seperation
    #region
    private class Point
    {
        public float x, y;
        public Point(float x, float y) { this.x = x; this.y = y; }
        public string toString() { return "[" + x + "," + y + "]"; }
    }

    private class SubBlueprint
    {
        private Vector2 location;
        private Texture2D blueprint;
        private Texture2D subBlueprint;
        private Texture2D[] sideTextures;

        private int width;
        private int height;
        private int depth;

        public SubBlueprint(Texture2D blueprint, Texture2D subBlueprint, Vector2 location)
        {
            this.blueprint = blueprint;
            this.subBlueprint = subBlueprint;
            this.location = location;
        }

        public void setWidth(int width) { this.width = width; }
        public int getWidth() { return width; }

        public void setHeight(int height) { this.height = height; }
        public int getHeight() { return height; }

        public void setDepth(int depth) { this.depth = depth; }
        public int getDepth() { return depth; }

        public void setLocation(Vector2 location) { this.location = location; }
        public Vector2 getLocation() { return location; }

        public void setBlueprint(Texture2D blueprint) { this.blueprint = blueprint; }
        public Texture2D getBlueprint() { return blueprint; }

        public void setSubBlueprint(Texture2D subBlueprint) { this.subBlueprint = subBlueprint; }
        public Texture2D getSubBlueprint() { return subBlueprint; }

        public void setSideTextures(Texture2D[] sideTextures) { this.sideTextures = sideTextures; }
        public Texture2D[] getSideTextures() { return sideTextures; }
    }

    static private SubBlueprint[] cutSubBlueprint(Texture2D blueprint)
    {
        //---------------- Step 0 - Set Up Variables
        int width = blueprint.width;
        int height = blueprint.height;

        List<SubBlueprint> subBlueprints = new List<SubBlueprint>();

        //---------------- Step 1 - Pixel Check List Setup
        bool[,] pixelCheckList = new bool[height, width];
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (blueprint.GetPixel(x, y).a != 0 && !pixelCheckList[y, x])
                {
                    SubBlueprint subBlueprint = cutSubBlueprint(blueprint, pixelCheckList, x, y);
                    subBlueprintsDetailSet(subBlueprint);
                    subBlueprints.Add(subBlueprint);
                }
            }
        }

        return subBlueprints.ToArray();
    }

    static private SubBlueprint cutSubBlueprint(Texture2D blueprint, bool[,] checkList, int x, int y)
    {
        List<Point> pointSet = new List<Point>();

        Stack<Point> stack = new Stack<Point>();
        stack.Push(new Point(x, y));
        while(stack.Count != 0)
        {
            Point curPoint = stack.Pop();
            int cur_x = (int)curPoint.x;
            int cur_y = (int)curPoint.y;

            List<Point> checkPoints = new List<Point>();

            for (int iy = -1; iy <= 1; iy++)
            {
                for (int ix = -1; ix <= 1; ix++)
                {
                    int cx = cur_x + ix;
                    int cy = cur_y + iy;

                    if (cx < 0 || cx >= blueprint.width) { continue; }
                    if (cy < 0 || cy >= blueprint.height) { continue; }
                    if (blueprint.GetPixel(cx, cy).a == 0) { continue; }
                    if (checkList[cy, cx]) { continue; }

                    checkPoints.Add(new Point(cx, cy));
                    pointSet.Add(new Point(cx, cy));
                    checkList[cy, cx] = true;
                }
            }

            Point[] checkPointArr = checkPoints.ToArray();
            for (int i = 0; i < checkPointArr.Length; i++) { stack.Push(checkPointArr[i]); }
        }
        Point[] coords = pointSet.ToArray();


        //Find Min/Max x, y coords of the set
        int x_min = blueprint.width;
        int y_min = blueprint.height;
        int x_max = -1;
        int y_max = -1;

        for (int i = 0; i < coords.Length; i++)
        {
            if (coords[i].x < x_min) { x_min = (int)coords[i].x; }
            if (coords[i].x > x_max) { x_max = (int)coords[i].x; }
            if (coords[i].y < y_min) { y_min = (int)coords[i].y; }
            if (coords[i].y > y_max) { y_max = (int)coords[i].y; }
        }


        //Construct the Sub-Blueprint
        int tex_width = x_max - x_min + 1;
        int tex_height = y_max - y_min + 1;

        Color[] emptyColor = new Color[tex_width * tex_height];
        for (int i = 0; i < emptyColor.Length; i++) { emptyColor[i] = new Color(0, 0, 0, 0); }

        Texture2D subBluePrint = new Texture2D(tex_width, tex_height);
        subBluePrint.SetPixels(emptyColor);

        //Set the Empty Color in Sub-Blueprint
        for (int iy = 0; iy < tex_height; iy++)
        {
            for (int ix = 0; ix < tex_width; ix++)
            {
                subBluePrint.SetPixel(ix, iy, Color.clear);
            }
        }

        //Populate the Sub-Blueprint
        for (int i = 0; i < coords.Length; i++)
        {
            int ori_x = (int)coords[i].x;
            int ori_y = (int)coords[i].y;

            subBluePrint.SetPixel(ori_x - x_min, ori_y - y_min, blueprint.GetPixel(ori_x, ori_y));
        }

        //Configurate the Sub-Blueprint
        subBluePrint.filterMode = FilterMode.Point;
        subBluePrint.Apply();

        //Return the Sub-Blueprint
        return new SubBlueprint(blueprint, subBluePrint, new Vector2(x_min, y_min));
    }

    static private SubBlueprint subBlueprintsDetailSet(SubBlueprint subBlueprint)
    {
        //---------------- Step 0 - Set Up Variables
        Texture2D[] sideTextures = new Texture2D[6];

        int width = subBlueprint.getSubBlueprint().width;
        int height = subBlueprint.getSubBlueprint().height;

        int box_width = 0;
        int box_height = 0;
        int box_depth = 0;

        Vector2 cursor = new Vector2(0, height);

        //---------------- Step 1 - Find the Height
        while (true)
        {
            bool hit = false;
            for (int i = (int)cursor.x; i >= 0; i--)
            {
                if (subBlueprint.getSubBlueprint().GetPixel((int)cursor.x - i, (int)cursor.y).a != 0) { hit = true; break; }
            }
            for (int i = height - (int)cursor.y; i >= 0; i--)
            {
                if (subBlueprint.getSubBlueprint().GetPixel((int)cursor.x, (int)cursor.y + i).a != 0) { hit = true; break; }
            }
            if (hit) { break; }

            cursor.x++;
            cursor.y--;
            box_height++;
        }

        //---------------- Step 2 - Find the Width
        cursor.x = box_height;
        cursor.y = height-1;

        while (true)
        {
            if (subBlueprint.getSubBlueprint().GetPixel((int)cursor.x,(int)cursor.y).a == 0)
            {
                bool clear = true;
                for ( int i = 0; i < box_height; i++ )
                {
                    if (subBlueprint.getSubBlueprint().GetPixel((int)cursor.x, height - i).a != 0) { clear = false; }
                }
                if (clear) { break; }
            }

            cursor.x++;
            box_width++;
        }

        //---------------- Step 3 - Find the Depth
        cursor.x = 0;
        cursor.y = height - box_height - 1;
        
        while (true)
        {
            if (subBlueprint.getSubBlueprint().GetPixel((int)cursor.x, (int)cursor.y).a == 0)
            {
                bool clear = true;
                for (int i = 0; i < box_height; i++)
                {
                    if (subBlueprint.getSubBlueprint().GetPixel(i, (int)cursor.y).a != 0) { clear = false; }
                }
                if (clear) { break; }
            }

            cursor.y--;
            box_depth++;
        }

        //---------------- Step 4 - Cut out into Pieces
        for ( int sideIndex = 0; sideIndex < 6; sideIndex++ )
        {
            int n;
            int crop_x = 0, crop_y = 0;
            int crop_w = 0, crop_h = 0;

            // Crop Location
                n = 0;
                /*Top*/     if (sideIndex == n) { crop_x = box_height;                  crop_y = height - box_height;               }n++;
                /*Right*/   if (sideIndex == n) { crop_x = box_height + box_width;      crop_y = height - box_height - box_depth;   }n++;
                /*Left*/    if (sideIndex == n) { crop_x = 0;                           crop_y = height - box_height - box_depth;   }n++;
                /*Foward*/  if (sideIndex == n) { crop_x = box_height*2 + box_width;    crop_y = height - box_height - box_depth;   }n++;
                /*Backward*/if (sideIndex == n) { crop_x = box_height;                  crop_y = height - box_height - box_depth;   }n++;
                /*Bottom*/  if (sideIndex == n) { crop_x = box_height*2 + box_width;    crop_y = height - box_height*2 - box_depth; }n++;
            

            // Crop Size
                n = 0;
                /*Top*/     if (sideIndex == n) { crop_w = box_width;   crop_h = box_height;    }n++;
                /*Right*/   if (sideIndex == n) { crop_w = box_height;  crop_h = box_depth;     }n++;
                /*Left*/    if (sideIndex == n) { crop_w = box_height;  crop_h = box_depth;     }n++;
                /*Foward*/  if (sideIndex == n) { crop_w = box_width;   crop_h = box_depth;     }n++;
                /*Backward*/if (sideIndex == n) { crop_w = box_width;   crop_h = box_depth;     }n++;
                /*Bottom*/  if (sideIndex == n) { crop_w = box_width;   crop_h = box_height;    }n++;


            // Crop
                Color[] extraction = subBlueprint.getSubBlueprint().GetPixels(crop_x, crop_y, crop_w, crop_h);
                Texture2D tex = new Texture2D(crop_w, crop_h);
                tex.SetPixels(extraction);
                tex.filterMode = FilterMode.Point;
                tex.Apply(true);

                sideTextures[sideIndex] = tex;
        }

        //---------------- Step 5 - Finishing Off
        subBlueprint.setWidth(box_width);
        subBlueprint.setHeight(box_height);
        subBlueprint.setDepth(box_depth);
        subBlueprint.setSideTextures(sideTextures);

        return subBlueprint;
    }
    #endregion

    //Pixel Part Construction
    #region
    static void meshConstruct(GameObject pixelPartObject, SubBlueprint subBlueprint, Texture emmisionMap)
    {
        GameObject[] pixelPartObjects = new GameObject[2];

        Texture2D texture_subBlueprint = subBlueprint.getSubBlueprint();
        Texture2D texture_blueprint = subBlueprint.getBlueprint();
        int bluePrint_width = texture_blueprint.width;
        int bluePrint_height = texture_blueprint.height;

        int width = subBlueprint.getWidth();
        int height = subBlueprint.getHeight();
        int depth = subBlueprint.getDepth();
        float w, h, d; // Shorten Froms

        int sideIndex = 1; if (DOUBLE_SIDED) { sideIndex = 2; }
        for (int side = 0; side < sideIndex; side++)
        {
            //Mesh Basic Information
            string name = "";
            if ( side == 0 ) { name = "outside mesh"; }
            if ( side == 1 ) { name = "inside mesh"; }

            pixelPartObjects[side] = new GameObject(name);
            pixelPartObjects[side].AddComponent<MeshFilter>();
            pixelPartObjects[side].AddComponent<MeshRenderer>();

            // Step 0 ---------- Mesh Setup
            Mesh mesh = new Mesh(); mesh.Clear();
            MeshFilter meshFilter = pixelPartObjects[side].GetComponent<MeshFilter>();
            MeshRenderer renderer = pixelPartObjects[side].GetComponent<MeshRenderer>();

            // Step 1 ---------- Vertices
            w = width / PIXEL_PER_UNIT;
            h = height / PIXEL_PER_UNIT;
            d = depth / PIXEL_PER_UNIT;

            Vector3 p0 = new Vector3(-w / 2, -d / 2, -h / 2);
            Vector3 p1 = new Vector3(w / 2, -d / 2, -h / 2);
            Vector3 p2 = new Vector3(-w / 2, -d / 2, h / 2);
            Vector3 p3 = new Vector3(w / 2, -d / 2, h / 2);
            Vector3 p4 = new Vector3(-w / 2, d / 2, -h / 2);
            Vector3 p5 = new Vector3(w / 2, d / 2, -h / 2);
            Vector3 p6 = new Vector3(-w / 2, d / 2, h / 2);
            Vector3 p7 = new Vector3(w / 2, d / 2, h / 2);
            Vector3[] vertices =
            {
                p0,p1,p2,p3, // Down
                p4,p5,p6,p7, // Up
                p3,p2,p7,p6, // Front
                p0,p1,p4,p5, // Back
                p1,p3,p5,p7, // Right
                p2,p0,p6,p4, // Left
            };
            /*Apply*/
            mesh.vertices = vertices;


            // Step 2 ---------- Triangles
            List<int> triangles = new List<int>();
            int[][] quads =
                {
                    new int[]{ 1, 0, 3, 2 }, // Down
                    new int[]{ 4, 5, 6, 7 }, // Up
                    new int[]{ 8, 9, 10, 11 }, // Front
                    new int[]{ 12, 13, 14, 15 }, // Back
                    new int[]{ 16, 17, 18, 19 }, // Right
                    new int[]{ 20, 21, 22, 23 }, // Left
                };

            for (int i = 0; i < quads.Length; i++)
            {
                int[] triLeftDown = { quads[i][0], quads[i][2], quads[i][1] };
                int[] triRightUp = { quads[i][2], quads[i][3], quads[i][1] };
                if (side == 0)
                {
                    for (int ii = 0; ii < 3; ii++) { triangles.Add(triLeftDown[ii]); }
                    for (int ii = 0; ii < 3; ii++) { triangles.Add(triRightUp[ii]); }
                }
                if (side == 1)
                {
                    for (int ii = 2; ii >= 0; ii--) { triangles.Add(triLeftDown[ii]); }
                    for (int ii = 2; ii >= 0; ii--) { triangles.Add(triRightUp[ii]); }
                }
            }
            /*Apply*/
            mesh.triangles = triangles.ToArray();



            // Step 3 ---------- Normals
            Vector3[] normals = null;
            if (side == 0)
            {
                normals = new Vector3[]
                {
                    Vector3.down,Vector3.down,Vector3.down,Vector3.down, //Down
                    Vector3.up,Vector3.up,Vector3.up,Vector3.up, //Up
                    Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward, //Front
                    Vector3.back,Vector3.back,Vector3.back,Vector3.back, //Back
                    Vector3.right,Vector3.right,Vector3.right,Vector3.right, //Right
                    Vector3.left,Vector3.left,Vector3.left,Vector3.left, //Left
                };
            }
            if (side == 1)
            {
                normals = new Vector3[]
                {
                    Vector3.up,Vector3.up,Vector3.up,Vector3.up, //Up
                    Vector3.down,Vector3.down,Vector3.down,Vector3.down, //Down
                    Vector3.back,Vector3.back,Vector3.back,Vector3.back, //Back
                    Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward, //Front
                    Vector3.left,Vector3.left,Vector3.left,Vector3.left, //Left
                    Vector3.right,Vector3.right,Vector3.right,Vector3.right, //Right
                };
            }
            /*Apply*/
            mesh.normals = normals;


            // Step 4 ---------- UV
            w = width;
            h = height;
            d = depth;

            Vector2 point0 = new Vector2(h + w + h, 0);
            Vector2 point1 = new Vector2(h + w + h + w, 0);

            Vector2 point2 = new Vector2(0, h);
            Vector2 point3 = new Vector2(h, h);
            Vector2 point4 = new Vector2(h + w, h);
            Vector2 point5 = new Vector2(h + w + h, h);
            Vector2 point6 = new Vector2(h + w + h + w, h);

            Vector2 point7 = new Vector2(0, h + d);
            Vector2 point8 = new Vector2(h, h + d);
            Vector2 point9 = new Vector2(h + w, h + d);
            Vector2 point10 = new Vector2(h + w + h, h + d);
            Vector2 point11 = new Vector2(h + w + h + w, h + d);

            Vector2 point12 = new Vector2(h, h + d + h);
            Vector2 point13 = new Vector2(h + w, h + d + h);

            Vector2[] uv =
            {
                point1,point0,point6,point5, // Down
                point8,point9,point12,point13, // Up
                point5,point6,point10,point11, // Front
                point3,point4,point8,point9, // Back
                point4,point5,point9,point10, // Right
                point2,point3,point7,point8, // Left
            };

            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] += subBlueprint.getLocation();
                uv[i].x /= texture_blueprint.width;
                uv[i].y /= texture_blueprint.height;
            }
            /*Apply*/
            mesh.uv = uv;


            //Save The Data
            meshFilter.mesh = mesh;
            if (side == 0) { PIXEL_OBJECT_MESHES_OUT.Add(meshFilter); }
            if (side == 1) { PIXEL_OBJECT_MESHES_IN.Add(meshFilter); }
            pixelPartObjects[side].transform.parent = pixelPartObject.transform;
            /*
            string sideStr = "";
            if (side == 0) { sideStr = "_out"; }
            if (side == 1) { sideStr = "_in"; }

            string meshDirectory = RESOURCE_DIRECTORY + NAME + "_" + sideStr + COUNTER.number + "_mesh.asset";

            AssetDatabase.CreateAsset(mesh, meshDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshDirectory, typeof(Mesh));


            //Finish-up
            meshFilter.mesh = mesh;
            renderer.material = MATERIAL;
            pixelPartObjects[side].transform.parent = pixelPartObject.transform;

            AssetDatabase.ImportAsset(meshDirectory, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            */
        }
        COUNTER.number++;
    }


    //Client is responsible for the argument to be in certain order
    enum side { Top, Right, Left, Forward, Backward, Bottom };
    static private PixelPart generatePart(SubBlueprint subBlueprint)
    {
        //Object
        PixelPart pixelPart = new PixelPart(new GameObject("sex"));
        GameObject pixelPartObject = pixelPart.getObject();
        meshConstruct(pixelPartObject, subBlueprint, null);

        float boxSize_width = subBlueprint.getWidth();
        float boxSize_height = subBlueprint.getHeight();
        float boxSize_depth = subBlueprint.getDepth();

        //Attach Color Registeration
        Texture2D[] sideTextures = subBlueprint.getSideTextures();
        for (int sideIndex = 0; sideIndex < 6; sideIndex++)
        {
            Texture2D side = sideTextures[sideIndex];
            int width = side.width;
            int height = side.height;

            bool[,] pixelCheckList = new bool[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color curColor = side.GetPixel(x, y);
                    if (curColor.r == 1 && !pixelCheckList[y,x])
                    {
                        //Attach Color Width, Height, Location Set
                        float adjust_x = 0.5f;
                        float adjust_y = 0.5f;

                        pixelCheckList[y, x] = true;
                        if (x + 1 < width && side.GetPixel(x + 1, y).Equals(curColor)) { adjust_x += 0.5f; pixelCheckList[y, x + 1] = true; }
                        if (y + 1 < height && side.GetPixel(x, y + 1).Equals(curColor)) { adjust_y += 0.5f; pixelCheckList[y + 1, x] = true; }
                        if (x + 1 < width && y + 1 < height && side.GetPixel(x + 1, y + 1).Equals(curColor)) { pixelCheckList[y + 1, x + 1] = true; }

                        /*
                        float adjust_x = 0;
                        float adjust_y = 0;
                        for (int i = 0; i < 2; i++ )
                        {
                            bool attachSizeCheck_x = true;
                            bool attachSizeCheck_y = true;

                            if (x + i >= width) { break; }
                            if (y + i >= height) { break; }

                            for (int ii = i; ii >= 0; ii--)
                            {
                                int cx = x + ii;
                                int cy = y + i;
                                if (!side.GetPixel(cx, cy).Equals(curColor)) { attachSizeCheck_x = false; break; }
                                else { pixelCheckList[cy, cx] = true; }
                            }
                            for (int ii = i; ii >= 0; ii--)
                            {
                                int cx = x + i;
                                int cy = y + ii;
                                if (!side.GetPixel(cx, cy).Equals(curColor)) { attachSizeCheck_y = false; break; }
                                else { pixelCheckList[cy, cx] = true; }
                            }

                            if (attachSizeCheck_x) { adjust_x += 0.5f; }
                            if (attachSizeCheck_y) { adjust_y += 0.5f; }
                            else { break; }
                        }
                        */
                        float loc_x = x + adjust_x - (width/2f);
                        float loc_y = y + adjust_y - (height/2f);

                        //Attach Color Registeration
                        Vector3 coordsPosition;
                        Vector3 sideVector;
                        switch (sideIndex)
                        {
                            case 0: coordsPosition = new Vector3( loc_x, boxSize_depth / 2, loc_y); sideVector = new Vector3( 0, 1, 0); break;
                            case 1: coordsPosition = new Vector3( boxSize_width / 2, loc_y, loc_x); sideVector = new Vector3( 1, 0, 0); break;
                            case 2: coordsPosition = new Vector3(-boxSize_width / 2, loc_y,-loc_x); sideVector = new Vector3(-1, 0, 0); break;
                            case 3: coordsPosition = new Vector3(-loc_x, loc_y, boxSize_height / 2); sideVector = new Vector3( 0, 0, 1); break;
                            case 4: coordsPosition = new Vector3( loc_x, loc_y,-boxSize_height / 2); sideVector = new Vector3( 0, 0,-1); break;
                            case 5: coordsPosition = new Vector3(-loc_x,-boxSize_depth / 2, loc_y); sideVector = new Vector3( 0,-1, 0); break;

                            default: coordsPosition = new Vector3(0, 0, 0); sideVector = new Vector3( 0, 0, 0); break;
                        }

                        ColorCoordinates newColorCoord = new ColorCoordinates(pixelPart, sideIndex, sideVector, curColor);
                            GameObject coords = new GameObject("Attach Color " + pixelPart.getColorCoords().Count);
                            coords.transform.position = coordsPosition/PIXEL_PER_UNIT;
                            coords.transform.parent = pixelPartObject.transform;
                            newColorCoord.setCoords(coords);

                        pixelPart.addColorCoords(newColorCoord);
                    }
                }
            }
        }

        //Finishing Setting
        pixelPartObject.transform.position = new Vector3(0, 0, 0);

        return pixelPart;
    }
    #endregion

    //Attach Color Matching
    #region
    private class PixelPart
    {
        private Vector3 size;
        private GameObject obj;

        private List<ColorCoordinates> colorCoords;

        public PixelPart( GameObject obj)
        {
            this.obj = obj;
            colorCoords = new List<ColorCoordinates>();
        }

        public void setSize( float width, float height, float depth) { size = new Vector3(width, height, depth); }
        public float width() { return size.x; }
        public float height() { return size.y; }
        public float depth() { return size.z; }

        public void setObject(GameObject obj) { this.obj = obj; }
        public GameObject getObject() { return obj; }

        public void addColorCoords(ColorCoordinates colorCoord) { colorCoords.Add(colorCoord); }
        public List<ColorCoordinates> getColorCoords() { return colorCoords; }
    }

    private class ColorCoordinates
    {
        private PixelPart part;
        private ColorCoordinates attachedTo;
        private int side;
        private Vector3 sideVector;

        private Color color;
        private GameObject coords;

        public ColorCoordinates(PixelPart part, int side, Vector3 sideVector, Color color)
        {
            this.part = part;
            this.side = side;
            this.sideVector = sideVector;
            this.color = color;

            attachedTo = null;
            coords = null;
        }

        public void setPart(PixelPart part) { this.part = part; }
        public PixelPart getPart() { return part; }

        public void setAttachedTo(ColorCoordinates attachedTo) { this.attachedTo = attachedTo; }
        public ColorCoordinates getAttachedTo() { return attachedTo; }

        public void setSide(int side) { this.side = side; }
        public int getSide() { return side; }

        public void setSideVector(Vector3 sideVector) { this.sideVector = sideVector; }
        public Vector3 getSideVector() { return sideVector; }

        public void setColor(Color color) { this.color = color; }
        public Color getColor() { return color; }
        public Vector3 getColorVector() { return new Vector3(color.r,color.g,color.b)*255; }

        public void setCoords(GameObject coords) { this.coords = coords; }
        public GameObject getCoords() { return coords; }
    }

    static void attachColorMatch( PixelPart[] pixelParts )
    {
        for (int i = 0; i < pixelParts.Length; i++)
        {
            for (int ii = 0; ii < pixelParts.Length; ii++)
            {
                if (i == ii) { continue; }
                
                ColorCoordinates[] colorCoords_cur = pixelParts[i].getColorCoords().ToArray();
                ColorCoordinates[] colorCoords_cmp = pixelParts[ii].getColorCoords().ToArray();

                for (int n = 0; n < colorCoords_cur.Length; n++)
                {
                    for (int nn = 0; nn < colorCoords_cmp.Length; nn++)
                    {
                        ColorCoordinates colorCoord_cur = colorCoords_cur[n];
                        ColorCoordinates colorCoord_cmp = colorCoords_cmp[nn];
                        
                        if (colorCoord_cur.getColor().Equals(colorCoord_cmp.getColor()))
                        {
                            colorCoord_cur.setAttachedTo(colorCoord_cmp);
                        }
                    }
                }
            }
        }
    }

    static GameObject constructParts(PixelPart pixelPart, PixelPart pixelPart_from)
    {
        ColorCoordinates[] colorCoords = pixelPart.getColorCoords().ToArray();

        if (colorCoords.Length > 0)
        {
            ColorCoordinates colorCoord_fm = null;
            ColorCoordinates colorCoord_to = null;

            for (int i = 0; i < colorCoords.Length; i++)
            {
                PixelPart pixelPart_send = colorCoords[i].getAttachedTo().getPart();

                if (pixelPart_send == pixelPart_from)
                {
                    colorCoord_fm = colorCoords[i];
                    colorCoord_to = colorCoords[i].getAttachedTo();
                }
                else
                {
                    constructParts(pixelPart_send, pixelPart);
                }
            }

            if (pixelPart_from != null)
            {
                PixelPart pixelPart_fm = colorCoord_fm.getPart();
                PixelPart pixelPart_to = colorCoord_to.getPart();

                GameObject object_fm = pixelPart_fm.getObject();
                GameObject object_to = pixelPart_to.getObject();

                switch (colorCoord_to.getSide())
                {
                    case 0:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(0, 0, 1), 180f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 0, 1),-90f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 0, 1), 90f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(1, 0, 0), 90f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(1, 0, 0),-90f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(0, 0, 1), 0f); break;
                        }
                        break;
                    case 1:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(0, 0, 1), 90f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 0, 1), 180f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 0, 1), 0f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(0, 1, 0),-90f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(0, 1, 0), 90f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(0, 0, 1),-90f); break;
                        }
                        break;
                    case 2:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(0, 0, 1),-90f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 0, 1), 0f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 0, 1), 180f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(0, 1, 0), 90f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(0, 1, 0),-90f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(0, 0, 1), 90f); break;
                        }
                        break;
                    case 3:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(1, 0, 0),-90f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 1, 0), 90f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 1, 0),-90f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(0, 1, 0), 180f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(0, 1, 0), 0f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(1, 0, 0), 90f); break;
                        }
                        break;
                    case 4:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(1, 0, 0), 90f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 1, 0),-90f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 1, 0), 90f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(0, 1, 0), 0f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(0, 1, 0), 180f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(1, 0, 0),-90f); break;
                        }
                        break;
                    case 5:
                        switch (colorCoord_fm.getSide())
                        {
                            case 0: object_fm.transform.Rotate(new Vector3(0, 0, 1), 0f); break;
                            case 1: object_fm.transform.Rotate(new Vector3(0, 0, 1), 90f); break;
                            case 2: object_fm.transform.Rotate(new Vector3(0, 0, 1),-90f); break;
                            case 3: object_fm.transform.Rotate(new Vector3(1, 0, 0),-90f); break;
                            case 4: object_fm.transform.Rotate(new Vector3(1, 0, 0), 90f); break;
                            case 5: object_fm.transform.Rotate(new Vector3(0, 0, 1), 180f); break;
                        }
                        break;
                }


                Vector3 object_colorCoordObject_fm = colorCoord_fm.getCoords().transform.position;
                Vector3 object_colorCoordObject_to = colorCoord_to.getCoords().transform.position;
                Vector3 pixelPartLocationAdjust = object_colorCoordObject_to - object_colorCoordObject_fm;

                object_fm.transform.position += pixelPartLocationAdjust;
                object_fm.transform.parent = object_to.transform;
            }
        }

        //Delete Used Attach Color Pivot GameObjects
        for (int i = 0; i < colorCoords.Length; i++)
        {
            DestroyImmediate(colorCoords[i].getCoords());
        }

        //Return GameObject
        return pixelPart.getObject();
    }
    #endregion

    //Pixel Object Construction
    #region
    private static void combineMeshes( GameObject pixelObject, PixelPart pixelPart )
    {
        int sideIndex = 1; if ( DOUBLE_SIDED ) { sideIndex++; }
        for ( int side = 0; side < sideIndex; side++)
        {
            string name = "";
            if ( side == 0 ) { name = "Outter Mesh"; }
            if ( side == 1 ) { name = "Inner Mesh"; }
            GameObject model = new GameObject(name);

            MeshFilter[] meshFilters = null;
            if (side == 0) { meshFilters = PIXEL_OBJECT_MESHES_OUT.ToArray(); }
            if (side == 1) { meshFilters = PIXEL_OBJECT_MESHES_IN.ToArray(); }

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                DestroyImmediate(meshFilters[i].gameObject);
                i++;
            }

            MeshFilter meshFilter = model.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.CombineMeshes(combine);
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = model.AddComponent<MeshRenderer>();
            meshRenderer.material = MATERIAL;


            //Save
            string inOrOut = "";
            if (side == 0) { inOrOut = "_out"; }
            if (side == 1) { inOrOut = "_in"; }
            string meshDirectory = RESOURCE_DIRECTORY + NAME + "_mesh" + inOrOut + ".asset";

            AssetDatabase.CreateAsset(mesh, meshDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshDirectory, typeof(Mesh));


            //Finish-up
            meshFilter.mesh = mesh;
            meshRenderer.material = MATERIAL;

            AssetDatabase.ImportAsset(meshDirectory, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            model.transform.parent = pixelObject.transform;
        }

        DestroyImmediate(pixelPart.getObject());
    }
    #endregion
}
