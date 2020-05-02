using UnityEngine;
using System;
using System.Collections.Generic;

public class sys_Interactable : MonoBehaviour
{
    private static List<sys_Interactable> InteractableList = null;
    private UI_System comp_UISystem;
    private GameObject icon = null;

    private bool interactable = true;
    private Vector3 iconScale = Vector3.zero;

    private scr_Person user = null;

    private bool isSetUp = false;

    private float interactionHoldTime = 0.5f;


    public static void systemSetUp()
    {
        InteractableList = null;
    }



    private void Start() { setUp(); }
    public sys_Interactable setUp()
    {
        if (isSetUp) { return this; }
        if (InteractableList == null) { InteractableList = new List<sys_Interactable>(); }
        InteractableList.Add(this);
        comp_UISystem = GetComponent<UI_System>();

        icon = Instantiate((GameObject)Resources.Load("UI/UI_Selectable", typeof(GameObject)), transform.position, Quaternion.identity);
        icon.transform.localScale = Vector3.zero;
        isSetUp = true;

        return this;
    }

    private void Update()
    {
        icon.transform.localScale = Util.smoothChange(icon.transform.localScale, iconScale, 5, 1);
    }

    public GameObject getIcon() { return icon; }

    public static sys_Interactable getClosestInterableSystem(Vector3 location, float distanceLimit)
    {
        if (InteractableList == null) { return null; }

        float closestDistance = float.MaxValue;
        sys_Interactable closestInteractive = null;

        foreach (sys_Interactable interactable in InteractableList)
        {
            interactable.makeItShine(false);

            if (!interactable.interactable) { continue; }
            Vector3 interactiveLocation = interactable.icon.transform.position;

            float distance = (location - interactiveLocation).magnitude;
            if (distance < distanceLimit)
            {
                if (distance < closestDistance)
                {
                    closestDistance = (location - interactiveLocation).magnitude;
                    closestInteractive = interactable;
                }
            }
        }

        if (closestInteractive != null) { closestInteractive.makeItShine(true); }
        return closestInteractive;
    }

    public Vector3 getPosition() { return icon.transform.position; }
    public void setInteractable(bool interactable) { this.interactable = interactable; }
    public void makeItShine(bool shining) { iconScale = shining && interactable ? Vector3.one * 2 : Vector3.zero; }
    public void setIconPosition(Vector3 position) { icon.transform.position = position; }
    public scr_Person getUser() { return user; }
    public float getInteractionHoldTime() { return interactionHoldTime; }
    public void interact(scr_Person interactor, float keyPressedTime)
    {
        // * Error Cases ####################################################################################################################################
        if (interactor == null || comp_UISystem == null) { return; }

        // * Release ####################################################################################################################################
        if (keyPressedTime < 0)
        {
            setInteractable(true);
            interactor.hold(null);
            return;
        }

        // * Interact ####################################################################################################################################
        sys_Container comp_Container = comp_UISystem.getController().GetComponent<sys_Container>();
        sys_Item comp_Item = comp_UISystem.getController().GetComponent<sys_Item>();

        // Container ======================================================================
        if (comp_Container != null)
        {
            // Short Click **************************************************
            if (keyPressedTime < interactionHoldTime)
            {
                sys_Item holdingItem = interactor.getHoldingObject();
                user = interactor;

                // If there is nothing in the hand, Grab the current Container
                if (holdingItem == null)
                {
                    interactor.hold(comp_Item);
                    comp_Container.useUISAPI("grabbed", new List<UIS_Data>());
                    setInteractable(false);
                }

                // If there is something in the hand, put it into the Container
                else
                {
                    interactor.hold(null);
                    comp_Container.useUISAPI("addItem", new List<UIS_Data>() { new UIS_Data("argument", holdingItem.getUISE().getID(), UIS_Data_Type.Entity) });
                    comp_Container.addItem(holdingItem);
                }
            }

            // Key Held Long **************************************************
            else
            {
                comp_UISystem.activate();
                comp_Container.useUISAPI("clicked", new List<UIS_Data>());
            }
        }

        // Item ======================================================================
        else if (comp_Item != null)
        {
            interactor.hold(comp_Item);
            comp_Container.useUISAPI("clicked", new List<UIS_Data>());
            setInteractable(false);
        }

        // Normal ======================================================================
        else
        {
            comp_UISystem.activate();
            comp_Container.useUISAPI("clicked", new List<UIS_Data>());
        }
    }
}


