using UnityEngine;
using System.Collections.Generic;

public class sys_Input
{
    private static readonly float TIRGGER_DEADZONE = 0.06f;

    // Mouse Input
    public bool[] mouse_left = { false, false, false };
    public bool[] mouse_right = { false, false, false };
    public Vector3[] mouse_position = { Vector3.zero, Vector3.zero, Vector3.zero };
    public Vector3[] mouse_positionOnPlane = { Vector3.zero, Vector3.zero, Vector3.zero };
    public GameObject mouse_on = null;
    public List<GameObject> mouse_onAll = new List<GameObject>();

    public float look_hor = 0;
    public float look_ver = 0;

    // Keyboard Input
    public float move_hor = 0;
    public float move_ver = 0;

    // Gamepad Input
    public bool[] leftBumper = { false, false, false };
    public bool[] leftTriggered = { false, false, false };
    public float leftTrigger = 0;
    private float leftTrigger_pre = 0;

    public bool[] rightBumper = { false, false, false };
    public bool[] rightTriggered = { false, false, false };
    public float rightTrigger = 0;
    private float rightTrigger_pre = 0;

    public void inputCheck()
    {
        inputCheck_mouse();
        inputCheck_keyboard();
        inputCheck_gamepad();
        leftTrigger_pre = leftTrigger;
        rightTrigger_pre = rightTrigger;
    }

    public void inputCheck_mouse()
    {
        // --------------------Axis Check--------------------
        look_hor = Mathf.Max(new float[] { Input.GetAxis("Mouse X")});
        look_ver = Mathf.Max(new float[] { Input.GetAxis("Mouse Y")});

        // --------------------Mouse Button Check--------------------
        //Mouse Button Left
        mouse_left[0] = Input.GetMouseButtonDown(0);
        mouse_left[1] = Input.GetMouseButton(0);
        mouse_left[2] = Input.GetMouseButtonUp(0);

        //Mouse Button Right
        mouse_right[0] = Input.GetMouseButtonDown(1);
        mouse_right[1] = Input.GetMouseButton(1);
        mouse_right[2] = Input.GetMouseButtonUp(1);



        // --------------------Mouse-On Object--------------------
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);



        //All Mouse Hits
        mouse_onAll.Clear();
        RaycastHit[] mouseHits = Physics.RaycastAll(mouseRay, 100f);
        Vector3 mouseHitOnTop = Vector3.zero;

        bool[] checkList = new bool[mouseHits.Length];

        for (int i = 0; i < mouseHits.Length; i++) { checkList[i] = false; }
        for (int i = 0; i < mouseHits.Length; i++)
        {
            float shortestDistance = Mathf.Infinity;
            int shortestIndex = -1;
            for (int ii = 0; ii < mouseHits.Length; ii++)
            {
                if (checkList[ii]) { continue; }
                else if (mouseHits[ii].distance <= shortestDistance)
                {
                    //Mouse Hit on Top
                    if (i == 0) { mouseHitOnTop = mouseHits[ii].point; }

                    //Sorting Information Update
                    shortestDistance = mouseHits[ii].distance;
                    shortestIndex = ii;
                }
            }
            mouse_onAll.Add(Util.findTopParent(mouseHits[shortestIndex].transform.gameObject));
            checkList[shortestIndex] = true;
        }

        //Single Mouse Hit
        mouse_on = null;
        if (mouseHits.Length > 0)
        {
            mouse_on = Util.findTopParent(mouse_onAll[0].transform.gameObject);

            mouse_position[1] = mouse_position[0];
            mouse_position[0] = mouseHitOnTop;
            mouse_position[2] = mouse_position[0] - mouse_position[1]; 
        }



        // --------------------Mouse Coordinate on Ground--------------------
        Plane mouseCheckPlane = new Plane(Vector3.up, 0f);
        float mouseCheckDistance;

        if (mouseCheckPlane.Raycast(mouseRay, out mouseCheckDistance))
        {
            mouse_positionOnPlane[1] = mouse_positionOnPlane[0];
            mouse_positionOnPlane[0] = mouseRay.GetPoint(mouseCheckDistance);
            mouse_positionOnPlane[2] = mouse_positionOnPlane[0] - mouse_positionOnPlane[1];
        }
    }
    public void inputCheck_keyboard()
    {
        move_hor = Input.GetAxis("Horizontal");
        move_ver = Input.GetAxis("Vertical");
    }
    public void inputCheck_gamepad()
    {
        leftBumper[0] = Input.GetButtonDown("LB");
        leftBumper[1] = Input.GetButton("LB");
        leftBumper[2] = Input.GetButtonUp("LB");

        leftTrigger = Input.GetAxis("LT");
        leftTriggered[0] = leftTrigger_pre < TIRGGER_DEADZONE && leftTrigger >= TIRGGER_DEADZONE;
        leftTriggered[1] = leftTrigger >= TIRGGER_DEADZONE;
        leftTriggered[2] = leftTrigger_pre >= TIRGGER_DEADZONE && leftTrigger < TIRGGER_DEADZONE;

        rightBumper[0] = Input.GetButtonDown("RB");
        rightBumper[1] = Input.GetButton("RB");
        rightBumper[2] = Input.GetButtonUp("RB");

        rightTrigger = Input.GetAxis("RT");
        rightTriggered[0] = rightTrigger_pre < TIRGGER_DEADZONE && rightTrigger >= TIRGGER_DEADZONE;
        rightTriggered[1] = rightTrigger >= TIRGGER_DEADZONE;
        rightTriggered[2] = rightTrigger_pre >= TIRGGER_DEADZONE && rightTrigger < TIRGGER_DEADZONE;
    }
}
