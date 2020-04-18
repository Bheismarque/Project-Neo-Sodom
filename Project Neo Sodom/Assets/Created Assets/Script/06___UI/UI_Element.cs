using UnityEngine;

public class UI_Element : UIS_Object
{
    [SerializeField] private bool selectable = false;
    private bool selected = false;
    private UIS_Data data_selectedPopOutValue;
    private float selected_popOut_val = 0f;

    [SerializeField] [HideInInspector] private Vector2 scale = Vector2.one;
    [SerializeField] [HideInInspector] public bool componentsSatisfied;
    [SerializeField] [HideInInspector] private Transform right = null;
    [SerializeField] [HideInInspector] private Transform left = null;
    [SerializeField] [HideInInspector] private Transform up = null;
    [SerializeField] [HideInInspector] private Transform down = null;
    [SerializeField] [HideInInspector] private Transform center = null;
    [SerializeField] [HideInInspector] private Transform upperRight = null;
    [SerializeField] [HideInInspector] private Transform upperLeft = null;
    [SerializeField] [HideInInspector] private Transform lowerRight = null;
    [SerializeField] [HideInInspector] private Transform lowerLeft = null;

    [SerializeField] [HideInInspector] private Vector2 originalScale = Vector2.one;

    [SerializeField] [HideInInspector] private Vector2 originalScale_edge = Vector2.one;

    [SerializeField] [HideInInspector] private float originalScale_rightEgde = 0.5f;
    [SerializeField] [HideInInspector] private float originalScale_leftEgde = 0.5f;
    [SerializeField] [HideInInspector] private float originalScale_upEgde = 0.5f;
    [SerializeField] [HideInInspector] private float originalScale_downEgde = 0.5f;

    [SerializeField] [HideInInspector] private Vector2 originalScale_center = Vector2.zero;

    private Vector2 goalScale = Vector2.zero;

    private MeshRenderer[] renderers = new MeshRenderer[18];
    private Material instanciatedMaterial = null;

    private UI_Window window = null;

    protected override void setUpDetail()
    {
        data_selectedPopOutValue = new UIS_Data("popOutValue", 0.03f, UIS_Data_Type.Numeric);
        UISE.getData().Add(data_selectedPopOutValue);
    }

    protected override void create()
    {
        window = GetComponentInParent<UI_Window>();
        goalScale = scale;
        scale.x = 0;
        scale.y = originalScale_edge.y / originalScale.y;

        int n = 0;
        Transform[] components = new Transform[] { right, left, up, down, center, upperRight, upperLeft, lowerRight, lowerLeft };
        instanciatedMaterial = Instantiate(right.GetChild(0).GetComponent<MeshRenderer>().material);
        foreach (Transform component in components)
        {
            renderers[n] = component.GetChild(0).GetComponent<MeshRenderer>(); renderers[n++].material = instanciatedMaterial;
            renderers[n] = component.GetChild(1).GetComponent<MeshRenderer>(); renderers[n++].material = instanciatedMaterial;
        }
        resize();
    }

    private bool doneScaling = false;
    protected override void step()
    {
        //Scaling ========================================================================================================================
        if (deleted)
        {
            bool startScaling;
            if (transform.parent != null && transform.parent.childCount > 1 && transform.parent.GetChild(0) == transform)
            {
                startScaling = transform.parent.GetChild(1).GetComponent<UI_Element>().doneScaling;
            }
            else { startScaling = true; }

            doneScaling = false;
            if (startScaling)
            {
                scale.y = Util.smoothChange(scale.y, 0, 5f, 1);
                if (scale.y / goalScale.y < .2f)
                {
                    scale.x = Util.smoothChange(scale.x, 0, 10f, 1);
                    doneScaling = scale.x / goalScale.x < .2f;
                }
            }
        }
        else
        {
            bool startScaling;
            if (transform.parent != null && transform.parent.childCount > 1 && transform.parent.GetChild(0) != transform)
            {
                startScaling = transform.parent.GetChild(0).GetComponent<UI_Element>().doneScaling;
            }
            else { startScaling = true; }

            doneScaling = false;
            if (startScaling)
            {
                scale.x = Util.smoothChange(scale.x, goalScale.x, 5f, 1);
                if (scale.x / goalScale.x > .95f)
                {
                    scale.y = Util.smoothChange(scale.y, goalScale.y, 10f, 1);
                    doneScaling = scale.y / goalScale.y > .95f;
                }
            }
            else { scale = Vector2.zero; }
        }
        resize();

        //Selection ========================================================================================================================
        Vector3 localPosition = transform.localPosition;
        
        localPosition.z -= -selected_popOut_val;
        selected_popOut_val = Util.smoothChange(selected_popOut_val, selected? (float)data_selectedPopOutValue.getData(): 0, 2, 1);
        localPosition.z += -selected_popOut_val;

        transform.localPosition = localPosition;


        //Rendering ========================================================================================================================
        float windowDepth = 0;
        if(window != null) { windowDepth = (window.getSystem().transform.position - window.transform.position).magnitude; }
        instanciatedMaterial.renderQueue = 3000 + (int)((transform.parent!=null? - windowDepth + transform.localPosition.z : 0) * -50);
    }

    public void setWindow(UI_Window window) { this.window = window; }

    public Vector2 getScale() { return scale; }
    public void setScale(Vector2 scale) { this.scale = scale; }

    public bool isSelectable() { return selectable && !deleted; }

    public void select() { selected = true; }
    public void unselect() { selected = false; }


    public void registerUIWindowComponents()
    {
        //Component Count Checking
        if (transform.childCount != 9) { return; }

        //Component Present Checking
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            switch (child.name)
            {
                case "Right": right = child; break;
                case "Left": left = child; break;
                case "Up": up = child; break;
                case "Down": down = child; break;
                case "Center": center = child; break;
                case "UpperRight": upperRight = child; break;
                case "UpperLeft": upperLeft = child; break;
                case "LowerRight": lowerRight = child; break;
                case "LowerLeft": lowerLeft = child; break;
            }
        }

        originalScale_edge.x = right.localScale.x + left.localScale.x;
        originalScale_edge.y = up.localScale.y + down.localScale.y;

        originalScale_rightEgde = right.localScale.x;
        originalScale_leftEgde = left.localScale.x;
        originalScale_upEgde = up.localScale.y;
        originalScale_downEgde = down.localScale.y;

        originalScale_center.x = center.localScale.x;
        originalScale_center.y = center.localScale.y;

        originalScale.x = originalScale_center.x + originalScale_edge.x;
        originalScale.y = originalScale_center.y + originalScale_edge.y;

        scale = Vector2.one;
    }

    public void resize()
    {
        if (componentsSatisfied)
        {
            Vector2 edgeScale = Vector2.one;
            Vector2 centerScale = Vector2.zero;

            // X Scale Calculation
            float scaleCheck_x = scale.x * originalScale.x;

            edgeScale.x = (scaleCheck_x < originalScale_edge.x) ? scaleCheck_x / originalScale_edge.x : 1;
            centerScale.x = (scaleCheck_x > originalScale_edge.x) ? (scaleCheck_x - originalScale_edge.x) / originalScale_center.x : 0;

            // Y Scale Calculation
            float scaleCheck_y = scale.y * originalScale.y;

            edgeScale.y = (scaleCheck_y < originalScale_edge.y) ? scaleCheck_y / originalScale_edge.y : 1;
            centerScale.y = (scaleCheck_y > originalScale_edge.y) ? (scaleCheck_y - originalScale_edge.y) / originalScale_center.y : 0;

            // Reflect the Calculation
            center.localScale = new Vector3(centerScale.x * originalScale_center.x, centerScale.y * originalScale_center.y,1f);

            right.localScale = new Vector3(edgeScale.x * originalScale_rightEgde, center.localScale.y, 1f);
            left.localScale = new Vector3(edgeScale.x * originalScale_leftEgde, center.localScale.y, 1f);
            up.localScale = new Vector3(center.localScale.x, edgeScale.y * originalScale_upEgde, 1f);
            down.localScale = new Vector3(center.localScale.x, edgeScale.y * originalScale_downEgde, 1f);

            upperRight.localScale = new Vector3(right.localScale.x, up.localScale.y, 1f);
            upperLeft.localScale = new Vector3(left.localScale.x, up.localScale.y, 1f);
            lowerRight.localScale = new Vector3(right.localScale.x, down.localScale.y, 1f);
            lowerLeft.localScale = new Vector3(left.localScale.x, down.localScale.y, 1f);

            // Position Update
            float constantScale = 1;

            center.localPosition = Vector3.zero;

            right.localPosition = new Vector3(center.localScale.x + right.localScale.x, 0, 0) * (constantScale / 2);
            left.localPosition = new Vector3(center.localScale.x + left.localScale.x, 0, 0) * (-constantScale / 2);
            up.localPosition = new Vector3(0, center.localScale.y + up.localScale.y, 0) * (constantScale / 2);
            down.localPosition = new Vector3(0, center.localScale.y + down.localScale.y, 0) * (-constantScale / 2);

            upperRight.localPosition = new Vector3(right.localPosition.x, up.localPosition.y, 0);
            upperLeft.localPosition = new Vector3(left.localPosition.x, up.localPosition.y, 0);
            lowerRight.localPosition = new Vector3(right.localPosition.x, down.localPosition.y, 0);
            lowerLeft.localPosition = new Vector3(left.localPosition.x, down.localPosition.y, 0);
        }
    }
}