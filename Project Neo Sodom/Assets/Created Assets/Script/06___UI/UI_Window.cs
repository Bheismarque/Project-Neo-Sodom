using UnityEngine;
using System.Collections.Generic;

public class UI_Window : UIS_Object
{
    private Vector3 currentPosition = Vector3.zero;
    private Vector3 desiredPosition = Vector3.zero;
    
    private Vector3 distanceFromParentWindow = new Vector3(-.00f,.05f,-.1f);

    protected override void create()
    {
        if (transform.parent != null && transform.parent.GetComponent<UI_Window>() != null)
        {
            distanceFromParentWindow = transform.localPosition;
        }
    }

    protected override void step()
    {
        positionUpdate();
    }

    private void positionUpdate()
    {
        if(parentUISO != null)
        {
            //Current Position Retreive
            currentPosition = transform.position;

            //Desired Position Retreive
            Transform previousParent = transform.parent;
            transform.parent = parentUISO.transform;
            transform.localPosition = distanceFromParentWindow;

            desiredPosition = transform.position;

            //Clear out Heirarchy
            transform.parent = previousParent;
            transform.position = currentPosition;
            currentPosition = Util.smoothChange(currentPosition, desiredPosition, 10, 1);

            //Position Update
            transform.position = currentPosition;
            transform.rotation = parentUISO.transform.rotation;
        }
    }

    public Vector3 getCurrentPosition() { return currentPosition; }
    public void setDistanceFromParentWindow(Vector3 distanceFromParentWindow) { this.distanceFromParentWindow = distanceFromParentWindow; }
}