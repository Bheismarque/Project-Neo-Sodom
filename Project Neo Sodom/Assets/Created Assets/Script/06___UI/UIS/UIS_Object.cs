using System.Collections.Generic;
using UnityEngine;
public class UIS_Object : MonoBehaviour
{
    [SerializeField] private bool DebuggingMode = false;
    [TextArea(10, 120)] [SerializeField] private string script = "";

    protected UI_System system = null;
    protected UIS_Object groundUISO = null;
    protected UIS_Object parentUISO = null;
    protected UIS_Entity UISE = null;

    private bool isStarted = false;

    private UIS_Data data_Accessibility_Position;
    private UIS_Data data_position_x;
    private UIS_Data data_position_y;
    private UIS_Data data_position_z;

    private UIS_Data data_Accessibility_LocalPosition;
    private UIS_Data data_position_lx;
    private UIS_Data data_position_ly;
    private UIS_Data data_position_lz;

    private UIS_Data data_Accessibility_Focus;
    private UIS_Data data_focus;

    private UIS_Data data_Accessibility_Parent;
    private UIS_Data data_parent;

    private static UIS_Data data_deltaTime;

    private UIS_Data data_thisWindow;

    private bool isSetUp = false;
    private void Start() { setUp(); }
    public UIS_Object setUp()
    {
        if (isSetUp) { return this; }
        isSetUp = true;

        //Parenting
        setParent((transform.parent == null) ? null : transform.parent.GetComponent<UIS_Object>());

        //Script Initialization
        if (groundUISO == this) { initializeAndComplieScript(); }

        // Data Initialization
        data_Accessibility_Position = new UIS_Data("ab_position", 0f, UIS_Data_Type.Numeric);
        data_position_x = new UIS_Data("x", 0f, UIS_Data_Type.Numeric);
        data_position_y = new UIS_Data("y", 0f, UIS_Data_Type.Numeric);
        data_position_z = new UIS_Data("z", 0f, UIS_Data_Type.Numeric);

        data_Accessibility_LocalPosition = new UIS_Data("ab_localPosition", 0f, UIS_Data_Type.Numeric);
        data_position_lx = new UIS_Data("lx", 0f, UIS_Data_Type.Numeric);
        data_position_ly = new UIS_Data("ly", 0f, UIS_Data_Type.Numeric);
        data_position_lz = new UIS_Data("lz", 0f, UIS_Data_Type.Numeric);

        data_Accessibility_Focus = new UIS_Data("ab_focus", -1, UIS_Data_Type.Entity);
        data_focus = new UIS_Data("focus", -1, UIS_Data_Type.Entity);

        data_Accessibility_Parent = new UIS_Data("ab_parent", -1, UIS_Data_Type.Entity);
        data_parent = new UIS_Data("parent", -1, UIS_Data_Type.Entity);

        data_deltaTime = new UIS_Data("deltaTime", 0f, UIS_Data_Type.Numeric);

        UI_Window parentWindow;
        parentWindow = transform.GetComponent<UI_Window>();
        parentWindow = parentWindow != null ? parentWindow : transform.GetComponentInParent<UI_Window>();
        data_thisWindow = new UIS_Data("thisWindow", parentWindow == null ? -1 : parentWindow.getUISE()== null? -1 : parentWindow.getUISE().getID(), UIS_Data_Type.Entity);

        if (UISE != null)
        {
            UISE.debugMode = DebuggingMode;

            //Configuration Data Addition
            UISE.getData().Add(new UIS_Data("name", gameObject.name, UIS_Data_Type.String));
            UISE.getData().Add(new UIS_Data("systemName", GetComponentInParent<UI_System>() == null? "null" : GetComponentInParent<UI_System>().name, UIS_Data_Type.String));
            UISE.getData().Add(UIS.globalEntity);
            UISE.getData().Add(data_deltaTime);

            //Position AB & Data Addition
            UISE.getData().Add(data_Accessibility_Position);
            data_position_x.setData(transform.position.x, UIS_Data_Type.Numeric);
            data_position_y.setData(transform.position.y, UIS_Data_Type.Numeric);
            data_position_z.setData(transform.position.z, UIS_Data_Type.Numeric);

            UISE.getData().Add(data_position_x);
            UISE.getData().Add(data_position_y);
            UISE.getData().Add(data_position_z);

            UISE.getData().Add(data_Accessibility_LocalPosition);
            data_position_x.setData(transform.localPosition.x, UIS_Data_Type.Numeric);
            data_position_y.setData(transform.localPosition.y, UIS_Data_Type.Numeric);
            data_position_z.setData(transform.localPosition.z, UIS_Data_Type.Numeric);

            UISE.getData().Add(data_position_lx);
            UISE.getData().Add(data_position_ly);
            UISE.getData().Add(data_position_lz);

            //Focus AB & Data Addition
            UISE.getData().Add(data_Accessibility_Focus);
            UISE.getData().Add(data_focus);

            //Parenting Data Addition
            UISE.getData().Add(data_Accessibility_Parent);
            data_parent.setData(parentUISO == null ? -1 : parentUISO.UISE.getID(), UIS_Data_Type.Entity);
            UISE.getData().Add(data_parent);

            //Window Data Addition
            UISE.getData().Add(data_thisWindow);

            setUpDetail();
        }
        return this;
    }

    protected virtual void setUpDetail() {}

    private void Update()
    {
        // START %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        if (isSetUp && !isStarted)
        {
            isStarted = true;
            //Only run "Main" if it isn't duplicated or replaced.
            if (UISE != null && !duplicated && !replaced)
            {
                UISE.executeAbility("main", new List<UIS_Data>());
            }
            create();
        }

        // UPDATE %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        if (isStarted)
        {
            data_deltaTime.setData(Time.deltaTime, UIS_Data_Type.Numeric);
            step();

            if (UISE != null)
            {
                // Parenting ------------------------------------------------------------------------------------------------------
                if (data_Accessibility_Parent.getData().Equals(1f))
                {
                    UIS_Entity parentEntity = UIS.getUISEfromUISD(data_parent);
                    if (parentEntity != null)
                    {
                        UIS_Object parentObject = parentEntity.getOwner();

                        if (parentUISO != parentObject)
                        {
                            setParent(parentObject);

                            UI_Element element = GetComponent<UI_Element>();
                            UI_Window parentWindow = parentObject.GetComponent<UI_Window>();
                            if (element != null && parentWindow != null)
                            {
                                // Transform Set
                                element.transform.parent = parentObject.transform;
                                element.transform.localRotation = Quaternion.identity;

                                // Window Set
                                element.setWindow(parentWindow);
                                data_thisWindow.setData(parentWindow == null ? -1 : parentWindow.getUISE() == null ? -1 : parentWindow.getUISE().getID(), UIS_Data_Type.Entity);

                                // System Set
                                parentObject.system.updateAvailableSelectiveList();
                            }
                            system = parentObject.system;
                        }
                    }
                    data_Accessibility_Parent.setData(0f, UIS_Data_Type.Numeric);
                }


                // Global Position Setting ------------------------------------------------------------------------------------------------------
                if (data_Accessibility_Position.getData().Equals(1f))
                {
                    transform.position = new Vector3((float)data_position_x.getData(),
                                                     (float)data_position_y.getData(),
                                                     (float)data_position_z.getData());
                    data_Accessibility_Position.setData(0f, UIS_Data_Type.Numeric);
                }
                else
                {
                    data_position_x.setData(transform.position.x, UIS_Data_Type.Numeric);
                    data_position_y.setData(transform.position.y, UIS_Data_Type.Numeric);
                    data_position_z.setData(transform.position.z, UIS_Data_Type.Numeric);
                }


                // Local Position Setting ------------------------------------------------------------------------------------------------------
                if (data_Accessibility_LocalPosition.getData().Equals(1f))
                {
                    transform.localPosition = new Vector3((float)data_position_lx.getData(),
                                                          (float)data_position_ly.getData(),
                                                          (float)data_position_lz.getData());
                    data_Accessibility_LocalPosition.setData(0f, UIS_Data_Type.Numeric);
                }
                else
                {
                    data_position_lx.setData(transform.localPosition.x, UIS_Data_Type.Numeric);
                    data_position_ly.setData(transform.localPosition.y, UIS_Data_Type.Numeric);
                    data_position_lz.setData(transform.localPosition.z, UIS_Data_Type.Numeric);
                }


                // Focus Setting ------------------------------------------------------------------------------------------------------
                if (data_Accessibility_Focus.getData().Equals(1f))
                {
                    if (system != null) { system.setFocus(UIS.getUISEfromUISD(data_focus)); }
                    data_Accessibility_Focus.setData(0f, UIS_Data_Type.Numeric);
                }
            }
        }
    }

    public UI_System getSystem() { return system; }
    public UIS_Object getGroundUSIO() { return groundUISO; }
    public UIS_Object getParent() { return parentUISO; }
    public void setParent(UIS_Object parentUISO)
    {
        if (parentUISO == null) { groundUISO = this; system = transform.GetComponentInParent<UI_System>(); return; }
        this.parentUISO = parentUISO;
        system = parentUISO.system;
        groundUISO = parentUISO.groundUISO;
    }

    public void initializeAndComplieScript()
    {
        UISE = UIS.compile(this, script, DebuggingMode);

        foreach (Transform child in transform)
        {
            UIS_Object childUISO = child.GetComponent<UIS_Object>();
            if (childUISO != null){ childUISO.initializeAndComplieScript(); }
        }
    }

    public void useUISAPI(string API, List<UIS_Data> arguments) { if (UISE != null) { UISE.executeAbility(API, arguments); } }
    protected virtual void create() { }
    protected virtual void step() { }

    private bool replaced = false;
    public UIS_Entity setUISE(UIS_Entity UISE)
    {
        if (UISE == null) { return null; }

        data_Accessibility_Position = UISE.searchDataField("ab_position");
        data_position_x = UISE.searchDataField("x");
        data_position_y = UISE.searchDataField("y");
        data_position_z = UISE.searchDataField("z");

        data_Accessibility_LocalPosition = UISE.searchDataField("ab_localPosition");
        data_position_lx = UISE.searchDataField("lx");
        data_position_ly = UISE.searchDataField("ly");
        data_position_lz = UISE.searchDataField("lz");

        data_Accessibility_Focus = UISE.searchDataField("ab_focus");
        data_focus = UISE.searchDataField("focus");

        data_Accessibility_Parent = UISE.searchDataField("ab_parent");
        data_parent = UISE.searchDataField("parent");

        data_thisWindow = UISE.searchDataField("thisWindow");

        this.UISE = UISE;
        this.UISE.setOwner(this);
        replaced = true;

        return UISE;
    }

    public UIS_Entity getUISE() { return UISE; }
    public void printOnConsole(System.Object toPrint) { print(toPrint); }

    public enum UIS_Controllability { location }

    protected bool deleted = false;

    private void destroy()
    {
        deleted = true;
        UIS_Object[] childUISOs = transform.GetComponentsInChildren<UIS_Object>();
        foreach (UIS_Object childUISO in childUISOs)
        {
            if (childUISO == this) { continue; }
            childUISO.delete();
        }
    }
    public void skin()
    {
        destroy();
        if(UISE != null) { UISE.setOwner(null); }
        Destroy(gameObject, (transform.GetComponent<UI_Element>() || transform.GetComponent<UI_Window>()) ? 1 : 0);
    }
    public void delete()
    {
        destroy();
        if (UISE != null) { UISE.delete(); }
        Destroy(gameObject, (transform.GetComponent<UI_Element>() || transform.GetComponent<UI_Window>()) ? 1 : 0);
    }

    private bool duplicated = false;
    public UIS_Entity duplicate(UI_System system)
    {
        UIS_Object duplicatedObject = Instantiate(gameObject).GetComponent<UIS_Object>();
        if (system != null) { duplicatedObject.transform.parent = system.transform; }
        duplicatedObject.transform.localRotation = Quaternion.identity;
        duplicatedObject.setUp();
        duplicatedObject.transform.parent = null;
        duplicatedObject.duplicated = true;

        return duplicatedObject.getUISE();
    }
}
