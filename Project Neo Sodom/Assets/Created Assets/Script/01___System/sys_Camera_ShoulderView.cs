using UnityEngine;
using System.Collections;

public class sys_Camera_ShoulderView : MonoBehaviour
{
    private Transform target = null;
    private scr_Person player = null;

    [SerializeField] private GameObject UI_AimLazor = null;
    private float UI_AimLazor_alpha = 0f;
    private float UI_AimLazor_alpha_timer = 0f;

    [SerializeField] private float POV_normal = 80;
    [SerializeField] private float POV_aim = 70;
    [SerializeField] private float POV_aimProper = 90;
    [SerializeField] private float POV_UI = 80;

    private float POV = 60;

    private bool sideInverted = false;

    [SerializeField] private Vector3 distanceFromTarget_normal = new Vector3(0.1f, 0.5f, 0.1f);
    [SerializeField] private Vector3 distanceFromTarget_aim = new Vector3(0.1f, 0.5f, 0.1f);
    [SerializeField] private Vector3 distanceFromTarget_aimProper = new Vector3(0.1f, 0.5f, 0.1f);
    [SerializeField] private Vector3 distanceFromTarget_UI = new Vector3(0.1f, 0.1f, 0.1f);
    private Vector3 distanceFromTarget = new Vector3(0.1f, 0.5f, 0.1f);

    [SerializeField] private Vector2 panningSpeed_normal = new Vector3(10, 10);
    [SerializeField] private Vector2 panningSpeed_aim = new Vector3(10, 10);
    [SerializeField] private Vector2 panningSpeed_aimProper = new Vector3(10, 10);
    [SerializeField] private Vector2 panningSpeed_firstPerson = new Vector3(10, 10);
    private Vector2 panningSpeed = new Vector3(10, 10);

    [SerializeField] private float panningSmoothness_normal = 5f;
    [SerializeField] private float panningSmoothness_aim = 5f;
    [SerializeField] private float panningSmoothness_aimProper = 5f;
    [SerializeField] private float panningSmoothness_firstPerson = 1f;
    private float panningSmoothness = 5f;

    private GameObject lookAtObject = null;
    private Vector3 position_goal = Vector3.zero;
    private Vector3 position = Vector3.zero;

    private Vector2 angle_goal = Vector2.zero;
    private Vector2 angle = Vector2.zero;

    private Vector2 angle_recoil = Vector2.zero;
    private Vector2 angle_recoil_goal = Vector2.zero;

    private void Awake()
    {
        //Initialize
        God.CAMERA = this;

        //Camera LookAt Object Set
        lookAtObject = new GameObject("Camera LookAt Object");
        lookAtObject.transform.localPosition = new Vector3(0, 0, 0);

        //UI
        UI_AimLazor = Instantiate(UI_AimLazor);

        //Reset
        reset(90);
    }

    private void Update()
    {
        Vector2 panSpeed;
        panSpeed.x =-God.input.look_hor*1.3f;
        panSpeed.y = God.input.look_ver*1.3f;
        panSpeed *= Util.NORMAL_FRAMERATE*God.gameTime;

        pan(panSpeed);

        infomationSetting();
        playerSetting();
        cameraMove();
        UI();
        finishUp();

        GetComponent<Camera>().fieldOfView = POV;
    }

    private void infomationSetting()
    {
        position = transform.position;
    }

    private void playerSetting()
    {
        float POV_goal = POV_normal;
        Vector3 distanceFromTarget_goal = distanceFromTarget_normal;
        Vector2 panningSpeed_goal = panningSpeed_normal;
        float panningSmoothness_goal = panningSmoothness_normal;

        if ( God.PLAYER != null)
        {
            target = God.PLAYER.transform;
        }
        else
        {
            target = null;
            player = null;
        }
        if ( target != null )
        {
            player = target.GetComponent<scr_Person>();
            target = player.getBone("head");
            
            if ( player != null )
            {
                if ( player.isAimming())
                {
                    POV_goal = POV_aim;
                    distanceFromTarget_goal = distanceFromTarget_aim;
                    panningSpeed_goal = panningSpeed_aim;
                    panningSmoothness_goal = panningSmoothness_aim;
                }
                if (player.isAimmingProper())
                {
                    POV_goal = POV_aimProper;
                    distanceFromTarget_goal = distanceFromTarget_aimProper;
                    panningSpeed_goal = panningSpeed_aimProper;
                    panningSmoothness_goal = panningSmoothness_aimProper;
                }

                if (player.isInFirstPerson())
                {
                    distanceFromTarget_goal = distanceFromTarget_UI;
                    POV_goal = POV_UI;
                    panningSpeed_goal = panningSpeed_firstPerson;
                    panningSmoothness_goal = panningSmoothness_firstPerson;
                }

                if (player.key_cameraSideInvert) { sideInverted = !sideInverted; }
            }
        }

        POV = Util.smoothChange(POV, POV_goal, 15, 1);
        distanceFromTarget = Util.smoothChange(distanceFromTarget, distanceFromTarget_goal, 5, 1);
        panningSpeed = Util.smoothChange(panningSpeed, panningSpeed_goal, 5, 1);
        panningSmoothness = Util.smoothChange(panningSmoothness, panningSmoothness_goal, 5, 1);
    }

    private void cameraMove()
    {
        //Angle
        angle.x = Util.smoothAngleChange(angle.x, angle_goal.x, panningSmoothness, 1);
        angle.y = Util.smoothAngleChange(angle.y, angle_goal.y, panningSmoothness, 1);

        angle_recoil = Util.smoothChange(angle_recoil, angle_recoil_goal, 2, 1);
        angle += angle_recoil;

        lookAtObject.transform.position = transform.position + Util.dirToVec3(angle);
        transform.LookAt(lookAtObject.transform);

        //Position
        setGoalPosition(angle);
        position = Util.smoothChange(position, position_goal, panningSmoothness, 1);
    }

    private Transform targetSave = null;
    public void saveTarget() { targetSave = target; }
    public void loadTarget() { target = targetSave; }

    private void UI()
    {
        if (target != null)
        {
            float UI_AimLazor_length = 500f;

            if (player != null )
            {
                if (player.isAimmingProper())
                {
                    //UI Configure
                    UI_AimLazor_length = 500f;
                    float UI_AimLazor_alpha_goal = 0.4f;
                    if (UI_AimLazor_alpha < UI_AimLazor_alpha_goal && UI_AimLazor_alpha_timer <= 0)
                    {
                        UI_AimLazor_alpha += (UI_AimLazor_alpha_goal / 5) * God.deltaTime;
                    }
                    else
                    {
                        UI_AimLazor_alpha_timer -= God.deltaTime;
                    }
                    if (player.isShooting())
                    {
                        UI_AimLazor_alpha = 0;
                        UI_AimLazor_alpha_timer = 0.25f;
                    }

                    if(player.getGun() != null)
                    {
                        // Raycast
                        Vector3 gunPosition = player.getGun().getGunPoint().position;
                        Vector3 gunRotation = player.getGun().getAngle();
                        RaycastHit hitPoint;
                        if (Physics.Raycast(gunPosition, gunRotation, out hitPoint, 500, Util.BulletLayerMask))
                        {
                            UI_AimLazor_length = hitPoint.distance;
                        }
                    }
                }
                else
                {
                    UI_AimLazor_alpha = 0;
                    UI_AimLazor_alpha_timer = 0.5f;
                }
            }
            else { UI_AimLazor_alpha = 0; }

            Color UI_AimLazor_color = UI_AimLazor.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
            UI_AimLazor_color.a = UI_AimLazor_alpha;
            UI_AimLazor.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = UI_AimLazor_color;

            if (player.getGun() == null) { return; }
            UI_AimLazor.transform.position = player.getGun().getGunPoint().transform.position;
            UI_AimLazor.transform.rotation = player.getGun().transform.rotation;

            float thickness = 0.01f;
            UI_AimLazor.transform.localScale = new Vector3(UI_AimLazor_length, thickness, thickness);
        }
    }

    private void finishUp()
    {
        transform.position = position;
    }

    public void reset(float resetAngle)
    {
        angle_goal.x = resetAngle;
        angle_goal.y = 0;
        cameraMove();
    }

    public void pan(Vector2 panAngles)
    {
        angle_goal.x = Util.angleCorrection(angle_goal.x + panAngles.x);
        angle_goal.y = Util.angleCorrection(angle_goal.y + panAngles.y);

        if ( angle_goal.y > 180 ) { angle_goal.y -= 360; }
        if ( angle_goal.y > 80 ) { angle_goal.y = 80; }
        if ( angle_goal.y <-80 ) { angle_goal.y =-80; }
    }

    private float inverted = 1;
    private void setGoalPosition(Vector2 angles)
    {
        //Position
        Vector3 d = distanceFromTarget;

        Vector3 cameraAwayDistance = Vector3.zero;
        if (player != null)// && player.isAimming())
        {
            Vector3 unit_x = Util.dirToVec3(new Vector2(angles.x, angles.y));
            cameraAwayDistance += unit_x * d.x;

            Vector3 unit_y = Util.dirToVec3(new Vector2(angles.x, angles.y - 90));
            cameraAwayDistance += unit_y * d.y;

            //Shoulder Side
            Vector3 unit_z = Util.dirToVec3(angles.x + 90);

            //Invert
            float invertedGoal = 1;
            if (sideInverted) { invertedGoal = -0.7f; }
            inverted = Util.smoothChange(inverted, invertedGoal,10,1);
            d.z *= inverted;

            //Apply
            cameraAwayDistance += unit_z * d.z;
        }
        else
        {
            cameraAwayDistance.x = Mathf.Cos(angles.x * Mathf.Deg2Rad) * d.x + Mathf.Cos((angles.x + 90) * Mathf.Deg2Rad) * d.z;
            cameraAwayDistance.z = Mathf.Sin(angles.x * Mathf.Deg2Rad) * d.x + Mathf.Sin((angles.x + 90) * Mathf.Deg2Rad) * d.z;
            cameraAwayDistance.y = -d.y;
        }

        if ( target != null )
        {
            position_goal = target.transform.position + new Vector3(0,0,0);

            RaycastHit hit;
            if (Physics.Raycast(target.transform.position, -cameraAwayDistance, out hit, cameraAwayDistance.magnitude, Util.CameraLayerMask))
            {
                position_goal -= cameraAwayDistance * (hit.distance / cameraAwayDistance.magnitude);
            }
            else
            {
                position_goal -= cameraAwayDistance;
            }
        }
    }

    //Getter & Setter
    public void setTarget(GameObject target) { this.target = target.transform; }
    public GameObject getTarget() { return target.gameObject; }

    public void setPOV(float POV) { this.POV = POV; }
    public float getPOV() { return POV; }

    public void setDistanceFromTarget(Vector3 distanceFromTarget) { this.distanceFromTarget = distanceFromTarget; }
    public void setDistanceFromTarget(float x, float y, float z) { distanceFromTarget = new Vector3(x, y, z); }
    public Vector3 getDistanceFromTarget() { return distanceFromTarget; }

    public void setPanningSpeed(Vector2 panningSpeed) { this.panningSpeed = panningSpeed; }
    public void setPanningSpeed(float x, float y) { panningSpeed = new Vector2(x, y); }
    public Vector2 getPanningSpeed() { return panningSpeed; }

    public void setPanningSmoothness(float panningSmoothness) { this.panningSmoothness = panningSmoothness; }
    public float getPanningSmoothness() { return panningSmoothness; }

    public Vector2 getAngle() { return angle; }

    public Vector2 getRecoil() { return angle_recoil; }
    public void setRecoil(Vector2 recoil) { angle_recoil_goal = recoil; }
}
