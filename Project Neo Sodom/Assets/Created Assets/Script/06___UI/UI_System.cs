using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_System : MonoBehaviour
{
    private bool activated = false;
    private UI_Window focusedWindow;
    private UI_Element focusedElement;
    private UI_Element focusedElement_pre;
    private controlSide side_pre;
    private UI_Element[] availableSelective;

    private UIS_Object controller;
    private sys_Interactable interactable;


    private void Start()
    {
        Transform controllerTransform = transform.Find("Controller");
        controller = controllerTransform == null ? null : controllerTransform.GetComponent<UIS_Object>();

        gameObject.AddComponent<sys_Interactable>();
        interactable = GetComponent<sys_Interactable>();
    }

    private void Update()
    {
        interactable.setIconPosition(transform.Find("Target").position);
        if (activated)
        {
            bool key_right = Input.GetKeyDown(KeyCode.LeftArrow);
            bool key_left = Input.GetKeyDown(KeyCode.RightArrow);
            bool key_up = Input.GetKeyDown(KeyCode.UpArrow);
            bool key_down = Input.GetKeyDown(KeyCode.DownArrow);

            bool key_clicked = Input.GetKeyDown(KeyCode.Space);

            controlSide side = controlSide.None;
            if (key_right) { side = controlSide.Right; }
            if (key_left) { side = controlSide.Left; }
            if (key_up) { side = controlSide.Up; }
            if (key_down) { side = controlSide.Down; }

            if (side != controlSide.None) { moveFocusedElement(side); }

            if (focusedElement != null)
            {
                if (key_clicked) { focusedElement.useUISAPI("clicked", new List<UIS_Data>()); }
            }
        }

        if (controller == null)
        {
            Destroy(interactable.getIcon());
            Destroy(gameObject);
        }
    }
    
    public void activate()
    {
        activated = true;
        if(controller != null) { controller.useUISAPI("clicked", new List<UIS_Data>()); }
    }
    public void deactivate() { activated = false; }

    public sys_Interactable getInteractable() { return interactable; }
    public UIS_Object getController() { return controller; }

    public void setFocus(UIS_Entity toFocus)
    {
        focusedWindow = toFocus.getOwner().GetComponent<UI_Window>();
        if (focusedWindow != null)
        {
            if (availableSelective != null) { foreach (UI_Element element in availableSelective) { element.unselect(); } }
            updateAvailableSelectiveList();
        }
        focusedElement = null;
        focusedElement_pre = null;
    }

    public void updateAvailableSelectiveList()
    {
        List<UI_Element> availableSelectiveList = new List<UI_Element>();
        foreach (UI_Element element in focusedWindow.GetComponentsInChildren<UI_Element>())
        {
            if (element.isSelectable()) { availableSelectiveList.Add(element); }
        }
        availableSelective = availableSelectiveList.ToArray();
    }

    private static readonly float recognizableAngle = 85f;
    private enum controlSide { Right, UpRight, Up, UpLeft, Left, DownLeft, Down, DownRight, None }
    private void moveFocusedElement(controlSide side)
    {
        // Exception Handling
        if (availableSelective == null || availableSelective.Length == 0) { return; }
        if (focusedElement == null)
        {
            foreach(UI_Element element in availableSelective) { if (element.isSelectable()) { focusedElement = element; focusedElement.select(); return; } }
        }

        //Step 0 : Set Up
        List<UI_Element> availableElements = new List<UI_Element>();
        float aimingSide = 180f - 45f * (int)side;

        if(focusedElement_pre != null)
        {
            if( (side == controlSide.Right && side_pre == controlSide.Left) ||
                (side == controlSide.Left && side_pre == controlSide.Right) ||
                (side == controlSide.Up && side_pre == controlSide.Down) ||
                (side == controlSide.Down && side_pre == controlSide.Up))
            {
                UI_Element save = focusedElement;

                focusedElement.unselect();
                focusedElement = focusedElement_pre;
                focusedElement.select();

                focusedElement_pre = save;
                side_pre = side;
                return;
            }
        }

        //Step 1 : Angle Check
        foreach(UI_Element element in availableSelective)
        {
            if (element != focusedElement)
            {
                Vector3 angleVector = (element.transform.localPosition - focusedElement.transform.localPosition);
                float angle = Mathf.Abs(Util.angleDifference(Util.vec2ToDir(angleVector), aimingSide));

                if (angle < recognizableAngle / 2) { availableElements.Add(element); }
            }
        }

        //Step 2 : Distance Check
        UI_Element closestElement = null;
        float closestDistance = float.MaxValue;

        foreach(UI_Element element in availableElements)
        {
            Vector3 distanceVector = (element.transform.localPosition - focusedElement.transform.localPosition);
            float distance = new Vector2(distanceVector.x, distanceVector.y).magnitude;

            if ( distance < closestDistance ) { closestElement = element; closestDistance = distance; }
        }

        //Step 3 : Finalization
        if (closestElement == null) { return; }
        focusedElement_pre = focusedElement;

        focusedElement.unselect();
        focusedElement = closestElement;
        focusedElement.select();
        side_pre = side;
    }
}
