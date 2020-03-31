using UnityEngine;
using UnityEditor;

public class scr_PixelObjectController : MonoBehaviour
{
    [SerializeField] [HideInInspector] private Material pixelObjectMaterial_original = null;
    [SerializeField] [HideInInspector] private Material pixelObjectMaterial = null;
    [SerializeField] [HideInInspector] private Color emission_color = Color.white;
    [SerializeField] [HideInInspector] private float emission_intensity = 1f;
    [SerializeField] [HideInInspector] private bool isUnique = false;

    private void Start()
    {
        //makeMaterialUnique();
    }

    //Material
    public Material getOriginalPixelObjectMaterial() { return pixelObjectMaterial_original; }
    public void setOriginalPixelObjectMaterial(Material pixelObjectMaterial_original) { this.pixelObjectMaterial_original = pixelObjectMaterial_original; }

    public Material getPixelObjectMaterial()
    {
        pixelObjectMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        return pixelObjectMaterial;
    }
    public void setPixelObjectMaterial(Material pixelObjectMaterial) { this.pixelObjectMaterial = pixelObjectMaterial; }

    //Emission
    public Color getEissionColor() { return emission_color; }
    public void setEissionColor(Color color)
    {
        getPixelObjectMaterial();
        pixelObjectMaterial.EnableKeyword("_EMISSION");
        pixelObjectMaterial.SetColor("_EmissionColor", color);
    }

    public float getEmissionIntensity() { return emission_intensity; }
    public void setEmissionIntensity(float intensity)
    {
        pixelObjectMaterial.SetVector("_EmissionColor", getEissionColor()*intensity);
    }

    //UniqueNess
    public bool getUniqueness() { return isUnique; }
    public void makeMaterialUnified()
    {
        setMaterial(gameObject, pixelObjectMaterial_original);
    }

    public void makeMaterialUnique()
    {
        Material material;

        Texture TEXTURE_ALBEDO = pixelObjectMaterial_original.GetTexture("_MainTex");
        Texture TEXTURE_NORMAL = pixelObjectMaterial_original.GetTexture("_BumpMap");
        Texture TEXTURE_EMISSION = pixelObjectMaterial_original.GetTexture("_EmissionMap");

        //Material
        material = new Material(Shader.Find("Standard"));

        //Albedo
        material.SetTexture("_MainTex", TEXTURE_ALBEDO);

        //Normal
        material.shaderKeywords = new string[1] { "_NORMALMAP" };
        material.SetTexture("_BumpMap", TEXTURE_NORMAL);

        //Emission
        material.EnableKeyword("_EMISSION");
        material.SetTexture("_EmissionMap", TEXTURE_EMISSION);
        material.SetColor("_EmissionColor", Color.white);
        material.SetVector("_EmissionColor", Color.white * 5f);
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

        //Cutout Mode
        material.SetFloat("_Mode", 1);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.EnableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 2450;

        //Apply Material
        setMaterial(gameObject, material);

        setEissionColor(getEissionColor());
        setEmissionIntensity(getEmissionIntensity());
    }

    private void setMaterial(GameObject target, Material material)
    {
        pixelObjectMaterial = material;
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
        if (meshRenderer != null) { meshRenderer.material = material; }

        int childNum = target.transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            setMaterial(target.transform.GetChild(i).gameObject, material);
        }
    }
}