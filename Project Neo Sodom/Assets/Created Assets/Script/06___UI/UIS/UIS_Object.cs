﻿using System.Collections.Generic;
using UnityEngine;
public class UIS_Object : MonoBehaviour
{
    [SerializeField] private bool DebuggingMode = false;
    [TextArea(10, 120)] [SerializeField] private string script = "";

    protected UI_System system = null;
    protected UIS_Object groundUISO = null;
    protected UIS_Object parentUISO = null;
    protected UIS_Entity UISE = null;

    private bool initialized = false;

    private UIS_Data data_Accessibility_Position;
    private UIS_Data data_position_x;
    private UIS_Data data_position_y;
    private UIS_Data data_position_z;

    private UIS_Data data_Accessibility_Focus;
    private UIS_Data data_focus;

    private UIS_Data data_Accessibility_Parent;
    private UIS_Data data_parent;

    private static UIS_Data data_deltaTime;

    private UIS_Data data_thisWindow;

    private bool initiated = false;
    private void Start() { initialize(); }
    public UIS_Object initialize()
    {
        if (!initiated)
        {
            initiated = true;

            //Parenting
            setParent((transform.parent == null) ? null : transform.parent.GetComponent<UIS_Object>());

            //Script Initialization
            if (groundUISO == this) { initializeAndComplieScript(); }

            // Data Initialization
            data_Accessibility_Position = new UIS_Data("ab_position", 0f, UIS_Data_Type.Numeric);
            data_position_x = new UIS_Data("x", 0f, UIS_Data_Type.Numeric);
            data_position_y = new UIS_Data("y", 0f, UIS_Data_Type.Numeric);
            data_position_z = new UIS_Data("z", 0f, UIS_Data_Type.Numeric);

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

                //Focus AB & Data Addition
                UISE.getData().Add(data_Accessibility_Focus);
                UISE.getData().Add(data_focus);

                //Parenting Data Addition
                data_parent.setData(parentUISO == null ? -1 : parentUISO.UISE.getID(), UIS_Data_Type.Entity);
                UISE.getData().Add(data_parent);

                //Window Data Addition
                UISE.getData().Add(data_thisWindow);
            }
        }
        return this;
    }

    private void Update()
    {
        if (initialized)
        {
            data_deltaTime.setData(Time.deltaTime, UIS_Data_Type.Numeric);
            step();

            if (UISE != null)
            {
                // Position Setting ------------------------------------------------------------------------------------------------------
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

                // Focus Setting ------------------------------------------------------------------------------------------------------
                if (data_Accessibility_Focus.getData().Equals(1f))
                {
                    if (system != null) { system.setFocus(UIS.getUISEfromUISD(data_focus)); }
                    data_Accessibility_Focus.setData(0f, UIS_Data_Type.Numeric);
                }

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

                        }
                    }
                    data_Accessibility_Parent.setData(0f, UIS_Data_Type.Numeric);
                }
            }
        }
        else
        {
            initialized = true;
            if (UISE != null)
            {
                UISE.executeAbility("main", new List<UIS_Data>());
            }
            create();
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

    public UIS_Entity setUISE(UIS_Entity UISE)
    {
        if(UISE == null) { return null; }

        data_Accessibility_Position = UISE.searchDataField("ab_position");
        data_position_x = UISE.searchDataField("x");
        data_position_y = UISE.searchDataField("y");
        data_position_z = UISE.searchDataField("z");
        data_Accessibility_Focus = UISE.searchDataField("ab_focus");
        data_focus = UISE.searchDataField("focus");
        data_parent = UISE.searchDataField("parent");
        data_deltaTime = UISE.searchDataField("deltaTime");
        data_thisWindow = UISE.searchDataField("thisWindow");
        this.UISE = UISE;

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

        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }
    public void delete()
    {
        destroy();
        if (UISE != null) { UISE.delete(); }

        gameObject.SetActive(false);
        Destroy(gameObject,1f);
    }

    public UIS_Entity duplicate(UI_System system)
    {
        UIS_Object duplicatedObject = Instantiate(gameObject).GetComponent<UIS_Object>();
        if (system != null) { duplicatedObject.transform.parent = system.transform; }
        duplicatedObject.transform.localRotation = Quaternion.identity;
        duplicatedObject.initialize();
        duplicatedObject.transform.parent = null;

        if (duplicatedObject.GetComponents<UI_Element>() != null)
        {
            //duplicatedObject.transform.parent = system.transform;
        }

        return duplicatedObject.getUISE();
    }
}
